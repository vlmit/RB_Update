using System;
using System.Collections.ObjectModel;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Результат выполнения IKrExecutor'a
    /// </summary>
    public interface IKrExecutionResult
    {
        /// <summary>
        /// Ошибки и предупреждения, возникшие при выполнении
        /// Может заполняться как самим Executor'ом, 
        /// так и пользовательским кодом
        /// </summary>
        ValidationResult Result { get; }

        /// <summary>
        /// Список идентификаторов шаблонов, у которых 
        /// SqlУсловие И C#Условие == true 
        /// </summary>
        ReadOnlyCollection<Guid> ConfirmedIDs { get; }

        /// <summary>
        /// Статус выполнения
        /// </summary>
        KrExecutionStatus Status { get; }

        /// <summary>
        /// ID шаблона, на котором прервалась обработка
        /// Актуально для Status == Interrupt...
        /// </summary>
        Guid? InterruptedStageID { get; }
    }
}
