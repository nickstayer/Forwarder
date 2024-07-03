namespace Forwarder;

public class Consts
{
    // Для доступа к почте из приложения необходимо создать пароль по ссылке:
    // https://id.yandex.ru/security/app-passwords
    // subject format: Пересылка;Адрес1,Адрес2;Тема
    public const string MESSAGE_COUNTDOWN = "Проверка почты через: ";
    public const string KEYWORD = "Пересылка";
    public const string WARNING = "ВНИМАНИЕ! БУДЬТЕ ОСТОРОЖНЫ, ДАННОЕ ПИСЬМО ПОЛУЧЕНО ОТ ВНЕШНЕГО ОТПРАВИТЕЛЯ!";
    //public const string SMTP_SERVER_ISOD = "post.mvd.ru";
    //public const int SMTP_PORT_ISOD = 587;
    //public const string IMAP_SERVER_ISOD = "post.mvd.ru";
    //public const int IMAP_PORT_ISOD = 143;
    public const string NO_SETTINGS_FILE = "Отсутствует файл конфигурации";
    public const string LOGS_DIR_NAME = "Logs";
    public const string FILE_LOG_NAME = "log.log";
    public const string LOG_DATE_FORMAT_FOR_FILE_NAME = "ddMMyyyy";
}
