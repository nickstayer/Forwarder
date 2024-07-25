using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System.Net;
using System.Text;

namespace Forwarder;

public class Post
{
    private readonly Settings _settings;
    private readonly Logger _logger;
    public Post(Settings settings, Logger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public void SendMail(string[] toAddresses, string subject, string body = null, string[] files = null)
    {
        using (var client = new SmtpClient())
        {
            if (_settings.UseSsl)
            {
                client.Connect(_settings.SmtpServer, _settings.SmtpPort, true); //yandex
            }

            else
            {
                DisableCertificateValidation();
                client.Connect(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls); //isod
            }

            var username = _settings.UsernameWithoutDog ? _settings.User.Split("@")[0] : _settings.User;
            client.Authenticate(username, _settings.Password);


            var bodyBldr = new BodyBuilder();
            bodyBldr.TextBody = body;
            bodyBldr.HtmlBody = body;
            if (files != null)
            {
                foreach (string file in files)
                {
                    bodyBldr.Attachments.Add(file);
                }
            }
            var msg = new MimeMessage()
            {
                Subject = subject,
                Body = bodyBldr.ToMessageBody(),
            };
            foreach (var address in toAddresses)
            {
                msg.To.Add(MailboxAddress.Parse(address));
            }
            msg.From.Add(new MailboxAddress(_settings.User, _settings.User));
            client.Send(msg);
        }
    }

    private void DisableCertificateValidation()
    {
        ServicePointManager.ServerCertificateValidationCallback +=
            (s, cert, chain, sslPolicyErrors) => true;
    }


    public List<Message> GetMails(DateTime onDate, string attachmentsPath)
    {
        var messages = new List<Message>();
        
        using (var client = new ImapClient())
        {
            
            if (_settings.UseSsl)
            {
                client.Connect(_settings.ImapServer, _settings.ImapPort, true); //yandex
            }

            else
            {
                DisableCertificateValidation();
                client.Connect(_settings.ImapServer, _settings.ImapPort, SecureSocketOptions.StartTls); //isod
            }
                
            var username = _settings.UsernameWithoutDog ? _settings.User.Split("@")[0] : _settings.User;
            client.Authenticate(username, _settings.Password);
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);
            var uids = client.Inbox.Search(SearchQuery.DeliveredOn(onDate));
            var summaries = client.Inbox.Fetch(uids, MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

            if (summaries != null && summaries.Count > 0)
            {
                foreach (var summury in summaries)
                {
                    var attachments = new List<string>();
                    foreach (var attachment in summury.Attachments.OfType<BodyPartBasic>())
                    {
                        try
                        {
                            var part = (MimePart)client.Inbox.GetBodyPart(summury.UniqueId, attachment);
                            if (!Directory.Exists(attachmentsPath))
                            {
                                Directory.CreateDirectory(attachmentsPath);
                            }
                            var file = Path.Combine(attachmentsPath, part.FileName);
                            if (!File.Exists(file))
                            {
                                using var strm = File.Create(file);
                                part.Content.DecodeTo(strm);
                            }
                            attachments.Add(file);
                        }
                        catch (Exception)
                        {
                            _logger.Write($"Не удалось получить вложения письма с темой \"{summury.Envelope.Subject}\", " +
                                $"отправитель: {summury.Envelope.From.Mailboxes.FirstOrDefault()?.Address}");
                            continue;
                        }
                    }
                    var message = new Message();
                    message.Id = summury.Envelope.MessageId;
                    message.Subject = summury.Envelope.Subject;
                    var msg = inbox.GetMessage(summury.UniqueId);
                    message.Body = GetTextFromBodyParts(msg.Body);
                    message.Attachments = attachments;
                    messages.Add(message);
                    message.From = summury.Envelope.From.Mailboxes.FirstOrDefault()?.Address;
                }
            }
            return messages;
        }
    }

    public static string GetTextFromBodyParts(MimeEntity body)
    {
        if (body is TextPart textPart)
        {
            return textPart.Text;
        }
        else if (body is Multipart multipart)
        {
            var builder = new StringBuilder();
            foreach (var part in multipart)
            {
                var text = GetTextFromBodyParts(part);
                if (!string.IsNullOrEmpty(text))
                {
                    builder.AppendLine(text);
                }
            }
            return builder.ToString();
        }
        return string.Empty;
    }

    public static string GetStringAddresses(string[] addresses)
    {
        var builder = new StringBuilder();
        for (int i=0;i<addresses.Length;i++)
        {
            builder.Append(addresses[i]);
            if(i < addresses.Length - 1)
                builder.Append(", ");
        }
        return builder.ToString();
    }
}