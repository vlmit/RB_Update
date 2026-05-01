using System;
using System.Collections.Generic;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;

namespace Tessa.Extensions.Default.Server.Workflow.KrObjectModel
{
    /// <summary>
    /// Реализация объекта исполнителя для этапа с множественными исполнителями.
    /// Работает поверх хранилища <see cref="KrConstants.KrPerformersVirtual"/>.
    /// </summary>
    public sealed class MultiPerformer: Performer
    {
        #region constructors

        /// <summary>
        /// Конструктор копирования. Запечатанность не переносится.
        /// </summary>
        /// <param name="performer"></param>
        public MultiPerformer(
            MultiPerformer performer)
            : base(new Dictionary<string, object>(10))
        {
            this.Init(KrConstants.RowID, performer.RowID);
            this.Init(KrConstants.KrPerformersVirtual.SQLApprover,  performer.IsSql);
            this.Init(KrConstants.KrPerformersVirtual.PerformerID, performer.PerformerID);
            this.Init(KrConstants.KrPerformersVirtual.PerformerName,  performer.PerformerName);
            this.Init(KrConstants.StageRowID, performer.StageRowID);
            this.Init(KrConstants.Order, 0);
        }

        /// <summary>
        /// Установить хранилище для декорирования
        /// </summary>
        /// <param name="storage"></param>
        public MultiPerformer(
            Dictionary<string, object> storage)
            : base(storage)
        {
        }
        
        /// <summary>
        /// Для использования внутри пользовательского кода
        /// </summary>
        /// <param name="rowID"></param>
        /// <param name="performerID"></param>
        /// <param name="performerName"></param>
        /// <param name="stageRowID"></param>
        /// <param name="isSql"></param>
        public MultiPerformer(
            Guid rowID,
            Guid performerID,
            string performerName,
            Guid stageRowID,
            bool isSql = false)
            : base(new Dictionary<string, object>(10))
        {
            this.Init(KrConstants.RowID, rowID);
            this.Init(KrConstants.KrPerformersVirtual.PerformerID, performerID);
            this.Init(KrConstants.KrPerformersVirtual.PerformerName, performerName);
            this.Init(KrConstants.StageRowID, stageRowID);

            this.Init(KrConstants.KrPerformersVirtual.SQLApprover, isSql);
            this.Init(KrConstants.Order, 0);
        }

        /// <summary>
        /// Для использования внутри пользовательского кода
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="stageRowID"></param>
        /// <param name="isSql"></param>
        public MultiPerformer(
            Guid id,
            string name,
            Guid stageRowID,
            bool isSql = false)
            : this(Guid.NewGuid(), id, name, stageRowID, isSql)
        {
        }

        /// <summary>
        /// Для использования внутри пользовательского кода
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="stageRowID"></param>
        /// <param name="isSql"></param>
        public MultiPerformer(
            string id,
            string name,
            Guid stageRowID,
            bool isSql = false)
            : this(Guid.Parse(id), name, stageRowID, isSql)
        {
        }

        #endregion

        #region properties

        /// <summary>
        /// Идентификатор строки исполнителя. Используется только для представления в виртуальных секциях
        /// </summary>
        public override Guid RowID => this.Get<Guid>(KrConstants.RowID);

        /// <summary>
        /// Признак того, что исполнитель подставлен через SQL
        /// </summary>
        public override bool IsSql => this.Get<bool>(KrConstants.KrPerformersVirtual.SQLApprover);

        /// <summary>
        /// ID роли исполнителя
        /// </summary>
        public override Guid PerformerID => this.Get<Guid>(KrConstants.KrPerformersVirtual.PerformerID);

        /// <summary>
        /// Имя роли исполнителя
        /// </summary>
        public override string PerformerName => this.Get<string>(KrConstants.KrPerformersVirtual.PerformerName);

        /// <summary>
        /// ID этапа.
        /// </summary>
        public override Guid StageRowID => this.Get<Guid>(KrConstants.StageRowID);

        #endregion
    }
}