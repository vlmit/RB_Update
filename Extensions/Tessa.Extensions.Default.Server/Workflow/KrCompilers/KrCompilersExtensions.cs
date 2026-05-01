using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tessa.Cards;
using Tessa.Extensions.Default.Server.Workflow.KrObjectModel;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Localization;
using Tessa.Platform;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    public static class KrCompilersExtensions
    {
        #region card sections

        public static bool TryGetKrStageTemplatesSection(this Card card, out CardSection section)
        {
            return card.Sections.TryGetValue(KrConstants.KrStageTemplates.Name, out section);
        }

        public static bool TryGetKrStageCommonMethodsSection(this Card card, out CardSection section)
        {
            return card.Sections.TryGetValue(KrConstants.KrStageCommonMethods.Name, out section);
        }

        public static CardSection GetKrStageTemplateSection(this Card card)
        {
            return card.Sections.Get<CardSection>(KrConstants.KrStageTemplates.Name);
        }

        #endregion

        #region IKrCompilationResult Extensions

        public static ValidationResult ToMissingAssemblyResult(this IKrCompilationResult compilationResult)
        {
            Check.ArgumentNotNull(compilationResult, nameof(compilationResult));

            var validationResult = new ValidationResultBuilder();

            ValidationSequence
                .Begin(validationResult)
                .SetObjectName(compilationResult)
                .ErrorDetails("$KrProcess_AssemblyMissed", compilationResult.Result.RawOutput)
                .End();

            return validationResult.Build();
        }

        #endregion

        #region object model to string

        private static string Indent(int spaces) =>
            new string(Enumerable.Range(0, spaces).Select(_ => ' ').ToArray());

        public static string ToStringSummary(this Stage stage, int spaces = 0)
        {
            var sb = new StringBuilder();
            sb.Append(Indent(spaces));
            sb.Append(LocalizationManager.Localize(stage.StageTypeCaption));
            sb.Append(" ");
            sb.Append(stage.Name);
            sb.Append("(");
            if (stage.BasedOnTemplate)
            {
                sb.Append(LocalizationManager.GetString("CardTypes_TypesNames_KrStageTemplate"));
                sb.Append(" \"");
                sb.Append(stage.TemplateName);
                sb.Append("\", ");
            }

            sb.Append(LocalizationManager.GetString("CardTypes_TypesNames_KrStageGroup"));
            sb.Append(" \"");
            sb.Append(stage.StageGroupName);
            sb.Append("\")");
            return sb.ToString();
        }

        public static string ToStringDetailed(this Stage stage, int spaces = 0)
        {
            var stringBuilder = new StringBuilder();
            var indent = Indent(spaces);
            var properties = typeof(Stage)
                .GetProperties()
                .Where(p => p.Name != "Performers"
                    && p.Name != "Info"
                    && p.Name != "Settings")
                .OrderBy(p => p.Name);
            foreach (var property in properties)
            {
                AppendProperty(property, stage, stringBuilder, indent);
            }
            
            stringBuilder
                .Append(indent)
                .AppendLine("]");

            return stringBuilder.ToString();
        }

        public static string ToStringSummary(this Performer performer, int spaces = 0)
        {
            return $"{Indent(spaces)}{performer.PerformerID} {LocalizationManager.Localize(performer.PerformerName)}";
        }

        public static string ToStringDetailed(this Performer performer, int spaces = 0)
        {
            var stringBuilder = new StringBuilder();
            var indent = Indent(spaces);

            var properties = typeof(Performer)
                .GetProperties()
                .OrderBy(p => p.Name);
            foreach (var property in properties)
            {
                AppendProperty(property, performer, stringBuilder, indent);
            }
            
            return stringBuilder.ToString();
        }

        private static void AppendProperty(
            PropertyInfo property,
            object obj,
            StringBuilder sb,
            string indent)
        {
            string strValue;
            var value = property.GetValue(obj, null);
            if (value is Dictionary<string, object> valueDict)
            {
                strValue = StorageHelper.Print(valueDict);
            }
            else
            {
                strValue = property.GetValue(obj, null)?.ToString() ?? "null";
                strValue = LocalizationManager.Localize(strValue);
            }
            sb
                .Append(indent)
                .Append(property.Name)
                .Append(" = ")
                .AppendLine(strValue);
        }

        #endregion
    }
}