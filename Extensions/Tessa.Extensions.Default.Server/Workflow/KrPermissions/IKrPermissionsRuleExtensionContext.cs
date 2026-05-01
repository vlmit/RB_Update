using System;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    /// <summary>
    /// Контекст расширения проверки прав по правилам доступа
    /// </summary>
    public interface IKrPermissionsRuleExtensionContext : IKrPermissionsManagerContext
    {
        /// <summary>
        /// Идентификатор правила доступа
        /// </summary>
        Guid RuleID { get; }

        /// <summary>
        /// Флаг определяет, что данное правило прав доступа не учитывается при расчете прав.
        /// Данные флаг устанавливается в расширениях <see cref="IKrPermissionsRuleExtension"/>
        /// </summary>
        bool Cancel { get; set; } 
    }
}