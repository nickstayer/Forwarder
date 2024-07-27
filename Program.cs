namespace Forwarder;

public class Program
{
    public static void Main()
    {
        var logger = Logger.UpdateLogFileName();
        var settingsFile = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        var processedMessages = new List<string>();
        var processedMessagesFromLog = logger.GetMessagesIdFromLog();
        processedMessages.AddRange(processedMessagesFromLog);
        Settings? settings;
        try
        {
            logger.Write($"Получаю настройки из {settingsFile}");
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
            var mailer = new Post(settings, logger, processedMessages);
            var folderAttach = Path.Combine(Directory.GetCurrentDirectory(), "Attachments");
            List<Message> messages = new List<Message>();

            try
            {
                logger.Write($"Загружаю письма");
                messages = mailer.GetMails(DateTime.Now, folderAttach);
                logger.Write($"Загрузил новых: {messages?.Count}");
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
                                    $"Ваше сообщение \"{subject}\" для {Post.GetStringAddresses(addresses)} доставлено на сервер пересылки");
                            var addr = string.Join(",", addresses);
                            logger.Write($"От {message.From} на {addr} с темой \"{subject}\". {message.Id}");
                        }
                        catch (Exception ex)
                        {
                            logger.Write($"Возникла ошибка при отправке почты: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Write($"Ошибка при обработки письма с темой {message?.Subject}: {ex}");
                    continue;
                }
            }
            logger.Write($"Таймаут минут: {settings.PeriodMinutes}");
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
}