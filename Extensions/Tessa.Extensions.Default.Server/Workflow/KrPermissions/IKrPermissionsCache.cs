using System;
using System.Collections.Generic;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    /// <summary>
    /// Кеш настроек правил доступа
    /// </summary>
    public interface IKrPermissionsCache
    {
        /// <summary>
        /// Версия кеша правил доступа
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Метод для получения всех настреок правил доступа с распределением по ID
        /// </summary>
        /// <returns>Возвращает все настройки правил доступа.</returns>
        IDictionary<Guid, IKrPermissionRuleSettings> GetAll();

        /// <summary>
        ///  Метод для получения настроек правила доступа с по ID
        /// </summary>
        /// <param name="ruleID">Идентификатор правила доступа</param>
        /// <returns>Возвращает настройки правила доступа, или null, если по переданному идентификатору не найдено правила доступа.</returns>
        IKrPermissionRuleSettings GetRuleByID(Guid ruleID);

        /// <summary>
        /// Метод для получения расширенных настроек правил доступа по типу и состоянию.
        /// </summary>
        /// <param name="typeID">Тип карточки или документа</param>
        /// <param name="state">Состояние карточки. Может быть не задано</param>
        /// <returns>Возвращает список расширенных настроек правил доступа.</returns>
        IEnumerable<IKrPermissionRuleSettings> GetExtendedRules(Guid typeID, KrState? state);

        /// <summary>
        /// Метод для получения настроек правил доступа по типу и состоянию, которые указаны как обязательные.
        /// </summary>
        /// <param name="typeID">Тип карточки или документа</param>
        /// <param name="state">Состояние карточки. Может быть не задано</param>
        /// <returns>Возвращает список настроек правил доступа, которые указаны как обязательные.</returns>
        IEnumerable<IKrPermissionRuleSettings> GetRequiredRules(Guid typeID, KrState? state);

        /// <summary>
        /// Метод для получения настроек правил доступа по типу и состоянию. Состояние может быть не обязательным
        /// </summary>
        /// <param name="typeID">Тип карточки или документа</param>
        /// <param name="state">Состояние карточки. Может быть не задано</param>
        /// <returns>Возвращает список настроек правил доступа для данного типа документа и состояния</returns>
        IEnumerable<IKrPermissionRuleSettings> GetRulesByTypeAndState(Guid typeID, KrState? state);
    }
}
