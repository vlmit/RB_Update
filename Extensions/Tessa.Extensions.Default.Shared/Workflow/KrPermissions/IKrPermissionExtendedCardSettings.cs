using System;
using System.Collections.Generic;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    /// <summary>
    /// Описание объекта с расширенными настройками к карточке
    /// </summary>
    public interface IKrPermissionExtendedCardSettings
    {
        /// <summary>
        /// Метод для установки доступа
        /// </summary>
        /// <param name="isAllowed">Определяет, должен ли данный флаг быть доступным или нет</param>
        /// <param name="sectionID">Идентификатор секции, для которой устанавливаются права</param>
        /// <param name="fields">Идентификатор полей, для которых устанавливаются права. Если не заданы, то доступ устанаваливается для всей секции</param>
        void SetCardAccess(
            bool isAllowed,
            Guid sectionID,
            ICollection<Guid> fields);

        /// <summary>
        /// Метод для получения расширенных настроек для секций карточки
        /// </summary>
        /// <returns>Возвращает список настроек по секциям карточки</returns>
        ICollection<IKrPermissionSectionSettings> GetCardSettings();

        /// <summary>
        /// Метод для получения расширенных настроек для секций заданий по типам заданий
        /// </summary>
        /// <returns>Возвразает список расширенных настроек для секций заданий по типам заданий</returns>
        Dictionary<Guid, ICollection<IKrPermissionSectionSettings>> GetTasksSettings();

        /// <summary>
        /// Метод возвращает настройки видимости контролов, блоков и вкладок
        /// </summary>
        /// <returns>Возвращает настройки видимости контролов, блоков и вкладок</returns>
        ICollection<KrPermissionVisibilitySettings> GetVisibilitySettings();

        /// <summary>
        /// Метод для получения настроек доступа к файлам.
        /// </summary>
        /// <returns>Возвращает настройки доступа к файлам.</returns>
        ICollection<KrPermissionFileSettings> GetFileSettings();
    }
}
