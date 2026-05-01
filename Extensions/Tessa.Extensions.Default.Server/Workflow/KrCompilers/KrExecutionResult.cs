using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Результат выполнения IKrExecutor'a
    /// </summary>
    public class KrExecutionResult : IKrExecutionResult
    {
        #region constructor

        public KrExecutionResult(
            ValidationResult result,
            IList<Guid> confirmedIDs,
            KrExecutionStatus status,
            Guid? interruptedStageID = null)
        {
            this.Result = result;
            this.ConfirmedIDs = new ReadOnlyCollection<Guid>(confirmedIDs);
            this.Status = status;
            this.InterruptedStageID = interruptedStageID;
        }

        public KrExecutionResult(
            ValidationResult result,
            ReadOnlyCollection<Guid> confirmedIDs,
            KrExecutionStatus status,
            Guid? interruptedStageID = null)
        {
            this.Result = result;
            this.ConfirmedIDs = confirmedIDs;
            this.Status = status;
            this.InterruptedStageID = interruptedStageID;
        }

        #endregion

        #region implementation

        /// <summary>
        /// Ошибки и предупреждения, возникшие при выполнении
        /// Может заполняться как самим Executor'ом,
        /// так и пользовательским кодом
        /// </summary>
        public ValidationResult Result { get; }

        /// <summary>
        /// Список идентификаторов шаблонов, у которых
        /// SqlУсловие И C#Условие == true
        /// </summary>
        public ReadOnlyCollection<Guid> ConfirmedIDs { get;  }

        /// <summary>
        /// Статус выполнения
        /// </summary>
        public KrExecutionStatus Status { get; }

        /// <summary>
        /// ID шаблона, на котором прервалась обработка
        /// Актуально для Status == Interrupt...
        /// </summary>
        public Guid? InterruptedStageID { get; }

        #endregion
    }
}
