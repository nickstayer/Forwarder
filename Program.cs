using MailKit;

namespace Forwarder;

public class Program
{
    private static List<string> _sentMessages = new List<string>();
    public static void Main()
    {
        var settingsFile = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        var logger = Logger.UpdateLogFileName();
        Settings? settings;

        //settings.PeriodMinutes = 15;
        //settings.SmtpServer = "post.mvd.ru";
        //settings.SmtpPort = 587;
        //settings.ImapServer = "post.mvd.ru";
        //settings.ImapPort = 143;
        //settings.BodyTextDefault = "Дубовсков Н.М. +79085781423";
        //settings.User = "ndubovskov@mvd.ru";
        //settings.Password = "Ghjhjxtcndj@0";
        //settings.SerializeSettings(settingsFile);
        //settings.Options = MailKit.Security.SecureSocketOptions.StartTls;
        //settings.UseSsl = false;
        //settings.UsernameWithoutDog = false;

        try
        {
            settings = Settings.DeserializeSettings(settingsFile);
        }
        catch(Exception ex)
        {
            logger.Write($"Ошибка десериализации настроек: {ex}");
            Console.ReadLine();
            return;
        }
        while (true)
        {
            logger = Logger.UpdateLogFileName();
            if (settings == null)
            {
                logger.Write($"Ошибка десериализации настроек");
                Console.ReadLine();
                return;
            }
            var mailer = new Post(settings);
            var folderAttach = Path.Combine(Directory.GetCurrentDirectory(), "Attachments");
            List<Message> messages = new List<Message>();

            try
            {
                messages = mailer.GetMails(DateTime.Now, folderAttach);
            }
            catch (Exception ex)
            {
                logger.Write($"Ошибка: {ex.Message}");
            }

            foreach (var message in messages)
            {
                try
                {
                    if (string.IsNullOrEmpty(message.Subject))
                        continue;
                    if (message.Subject.ToLower().Contains(Consts.KEYWORD.ToLower()))
                    {
                        if (!_sentMessages.Contains(message.Id))
                        {
                            var addrSubj = GetAddressAndSubject(message.Subject);
                            if (addrSubj.Item1.Length == 0)
                            {
                                continue;
                            }
                            string[] addresses = addrSubj.Item1;
                            string subject = addrSubj.Item2;
                            var body = string.IsNullOrEmpty(message.Body) ? settings.BodyTextDefault : message.Body.Replace(Consts.WARNING, "");
                            try
                            {
                                mailer.SendMail(addresses, subject, body, message.Attachments.ToArray());
                                if (!string.IsNullOrEmpty(message.From))
                                    mailer.SendMail(new string[] { message.From }, 
                                        "Отчет о доставке",
                                        $"Ваше сообщение \"{subject}\" доставлено получателю: {Post.GetStringAddresses(addresses)}");
                                _sentMessages.Add(message.Id);
                                var addr = string.Join(",", addresses);
                                logger.Write($"От {message.From} на {addr} с темой \"{subject}\". {message.Id}");
                            }
                            catch (Exception ex)
                            {

                                logger.Write($"Возникла ошибка при отправке почты: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Write($"Ошибка при обработки письма с темой {message?.Subject}: {ex}");
                    continue;
                }
            }
            Thread.Sleep(TimeSpan.FromMinutes(settings.PeriodMinutes));
        }
    }

    private static (string[],string) GetAddressAndSubject(string subject)
    {
        var arr = subject.Split(";");
        (string[], string) result = (new string[] { }, string.Empty);
        if(arr.Length == 3)
        {
            string[] addresses = GetAddresses(arr[1]);
            result = (addresses, arr[2].Trim());
        }
        return result;
    }

    private static string[] GetAddresses(string addresses)
    {
        var arr = addresses.Split(",").Select(x => x.Trim()).ToArray();
        return arr;
    }

    public static void CountDownMinutesToStart(int minutes)
    {
        if (minutes > 0)
        {
            var timerSize = (int)Math.Log10(minutes) + 1;
            Console.Write(Consts.MESSAGE_COUNTDOWN);
            for (var i = minutes; i > 0; i--)
            {

                Console.CursorLeft = Consts.MESSAGE_COUNTDOWN.Length;
                int digitCount = (int)Math.Log10(i) + 1;
                var gapsCount = timerSize - digitCount >= 0 ? timerSize - digitCount : 0;
                var gaps = new string(' ', gapsCount);
                Console.Write($"{i}{gaps}");
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
            Console.WriteLine();
        }
    }
}