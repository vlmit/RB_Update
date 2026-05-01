using System.Collections.ObjectModel;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess
{
    public static class KrStrictSecurityHelper
    {
        #region KrStageTemplate

        public static readonly ReadOnlyCollection<string> KrStageTemplateFields =
            new ReadOnlyCollection<string>(
                new[]
                {
                    KrConstants.SqlCondition,
                    KrConstants.SourceCondition,
                    KrConstants.SourceBefore,
                    KrConstants.SourceAfter,
                });

        public static readonly ReadOnlyCollection<string> KrStageTemplateTables =
            new ReadOnlyCollection<string>(
                new[]
                {
                    KrConstants.KrStages.Name,
                    KrConstants.KrStages.Virtual,
                });

        public static readonly ReadOnlyCollection<string> KrStageTemplateTableFields =
            new ReadOnlyCollection<string>(
                new[]
                {
                    KrConstants.RuntimeSqlCondition,
                    KrConstants.RuntimeSourceCondition,
                    KrConstants.RuntimeSourceBefore,
                    KrConstants.RuntimeSourceAfter,
                    KrConstants.KrStages.SqlApproverRole,
                });

        #endregion

        #region KrStageGroup

        public static readonly ReadOnlyCollection<string> KrStageGroupFields =
            new ReadOnlyCollection<string>(
                new[]
                {
                    KrConstants.SqlCondition,
                    KrConstants.SourceCondition,
                    KrConstants.SourceBefore,
                    KrConstants.SourceAfter,
                    KrConstants.RuntimeSqlCondition,
                    KrConstants.RuntimeSourceCondition,
                    KrConstants.RuntimeSourceBefore,
                    KrConstants.RuntimeSourceAfter,
                });

        #endregion

        #region KrProcessButton

        public static readonly ReadOnlyCollection<string> KrSecondaryProcessFields =
            new ReadOnlyCollection<string>(
                new[]
                {
                    KrConstants.KrSecondaryProcesses.ExecutionSourceCondition,
                    KrConstants.KrSecondaryProcesses.ExecutionSqlCondition,
                    KrConstants.KrSecondaryProcesses.VisibilitySourceCondition,
                    KrConstants.KrSecondaryProcesses.VisibilitySqlCondition,
                });
        
        public static readonly ReadOnlyCollection<string> KrSecondaryProcessTables =
            new ReadOnlyCollection<string>(
                new[]
                {
                    KrConstants.KrStages.Name,
                    KrConstants.KrStages.Virtual,
                });

        public static readonly ReadOnlyCollection<string> KrSecondaryProcessTableFields =
            new ReadOnlyCollection<string>(
                new[]
                {
                    KrConstants.RuntimeSqlCondition,
                    KrConstants.RuntimeSourceCondition,
                    KrConstants.RuntimeSourceBefore,
                    KrConstants.RuntimeSourceAfter,
                    KrConstants.KrStages.SqlApproverRole,
                });
        
        #endregion

        #region KrStageCommonMethod

        public const string KrStageCommonMethodSectionName = KrConstants.KrStageCommonMethods.Name;

        public const string KrStageCommonMethodFieldName = KrConstants.KrStageCommonMethods.Source;

        #endregion  
    }
}
