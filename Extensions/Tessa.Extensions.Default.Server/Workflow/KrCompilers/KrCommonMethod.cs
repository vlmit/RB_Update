using System;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    public class KrCommonMethod : IKrCommonMethod
    {
        #region constructor

        public KrCommonMethod(
            Guid id,
            string name,
            string source = null)
        {
            this.ID = id;
            this.Name = name;
            this.Source = source ?? string.Empty;
        }

        #endregion

        #region properties

        /// <summary>
        /// Уникальный идентификатор карточки KrCommonMethod
        /// </summary>
        public Guid ID { get; }

        /// <summary>
        /// Имя метода, подставляемое в генерируемом коде
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Тело метода, подставляемое в генерируемый код
        /// </summary>
        public string Source { get; private set; }

        #endregion

    }
}
