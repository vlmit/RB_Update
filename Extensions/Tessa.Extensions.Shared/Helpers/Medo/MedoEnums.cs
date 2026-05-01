using System.ComponentModel;

namespace Tessa.Extensions.Shared.Helpers.Medo
{
    /// <summary>
    /// виды штампов
    /// </summary>
    public enum StampType
    {
        [Description("Регистрационный штамп")]
        Reg = 0,

        [Description("Штамп подписи")]
        Sign = 1
    }

    /// <summary>
    /// Перечисление допустимых видов сообщений. Каждый вид определяет допустимый набор элементов в сообщении
    /// </summary>
    public enum MedoMessageType
    {
        /// <summary>
        /// Сообщение содержит атрибутику документа
        /// </summary>
        [Description("Документ")]
        Document = 0,

        /// <summary>
        /// Сообщение информирует пользователей СЭД АП РФ о ходе процесса прохождения документа в ФОИВ  (в пилотном проекте не используется)
        /// </summary>
        [Description("Уведомление")]
        Notification = 1,

        /// <summary>
        /// Подтверждение о приеме сообщения
        /// </summary>
        [Description("Квитанция")]
        Acknowledgment = 2,

        /// <summary>
        /// Транспортный контейнер документа в электронном виде
        /// </summary>
        [Description("Транспортный контейнер")]
        ShippingContainer = 3
    }

    /// <summary>
    /// статусы отправки сообщения по мэдо
    /// </summary>
    public enum MedoState
    {
        [Description("Новое")]
        New = 0,

        [Description("Отправлено")]
        Send = 1,

        [Description("Получено")]
        Recive = 2,

        [Description("Отклонено")]
        Reject = 3,

        [Description("Ошибка формирования")]
        FormationError = 4,

        [Description("На доработке")]
        OnRework = 5
    }

    /// <summary>
    /// Тип уведомления МЭДО
    /// </summary>
    public enum NoticeType
    {
        [Description("Зарегистрирован")]
        Registered = 0,

        [Description("Отказано в регистрации")]
        RegDenied = 1,

        [Description("Назначен исполнитель")]
        ExecuterAssign = 2,

        [Description("Доклад подготовлен")]
        ReportPrepared = 3,

        [Description("Доклад направлен")]
        ReportSend = 4,

        [Description("Исполнение")]
        Execute = 5,

        [Description("Опубликование")]
        Publish = 6,

        [Description("Государственная регистрация")]
        StateReg = 7,

        [Description("Документ")]
        Document = 8
    }

    public enum XsdVersion
    {
        //Не поддерживаемая версия
        NoSupportVersion = -1,

        [Description("2.7")] //Может быть указана как "2.7.0"
        OldVersion = 0,
        
        [Description("2.7.1")]
        NewVersion = 1,

        [Description("2.2")]
        Version22 = 2,

        [Description("2.5")]
        Version25 = 3,

        [Description("2.6")]
        Version26 = 4,

        [Description("2.0")]
        Version20 = 5
    }

    public enum XmlType
    {
        [Description("passport")]
        Passport = 0,
        
        [Description("message")]
        Message = 1
    }
}