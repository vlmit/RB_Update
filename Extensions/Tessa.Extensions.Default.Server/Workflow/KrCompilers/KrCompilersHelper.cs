using System;
using System.Text.RegularExpressions;
using Tessa.Cards;
using Tessa.Extensions.Default.Server.Workflow.KrObjectModel;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    public static class KrCompilersHelper
    {
        #region Public Methods

        /// <summary>
        /// Возвращает имя автоматически сгенерированного класса.
        /// </summary>
        /// <param name="prefix">Префикс.</param>
        /// <param name="alias">Алиас.</param>
        /// <param name="id">Идентификатор.</param>
        /// <returns>Имя автоматически сгенерированного класса: <paramref name="prefix"/>_<paramref name="alias"/>_<paramref name="id"/>.</returns>
        public static string FormatClassName(
            string prefix,
            string alias,
            Guid id) => $"{prefix}_{alias}_{id:N}";

        /// <summary>
        /// Возвращает значение, показывающее, что указанная строка является правильным именем автоматически сгенерированного класса.
        /// </summary>
        /// <param name="str">Проверяемая строка.</param>
        /// <returns>Значение <see langword="true"/>, если указанная строка является правильным именем автоматически сгенерированного класса, иначе - <see langword="false"/>.</returns>
        /// <seealso cref="FormatClassName(string, string, Guid)"/>
        public static bool CorrectClassName(string str)
        {
            var match = Regex.Match(
                str,
                "^(?:[a-z,A-Z]+)_(?:[a-z,A-Z,0-9]+)_(?:[a-f,0-9]{32})$",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
            return match.Success;
        }

        /// <summary>
        /// Возвращает значение, показывающее, что указанный этап относится к группе с заданным идентификатором.
        /// </summary>
        /// <param name="stageGroupID">Идентификатор группы этапов.</param>
        /// <param name="st">Проверяемый этап.</param>
        /// <returns>Значение <see langword="true"/>, если <see cref="Stage.StageGroupID"/> равно <paramref name="stageGroupID"/> или <paramref name="stageGroupID"/> равно <see cref="Guid.Empty"/>, иначе - <see langword="false"/>.</returns>
        public static bool ReferToGroup(
            Guid stageGroupID,
            Stage st)
        {
            Check.ArgumentNotNull(st, nameof(st));

            return st.StageGroupID == stageGroupID || stageGroupID == Guid.Empty;
        }

        /// <summary>
        /// Очищает коллекцию строк физической секции <see cref="KrConstants.KrStages.Name"/>.
        /// </summary>
        /// <param name="card">Карточка из которой удаляется информация.</param>
        public static void ClearPhysicalSections(Card card)
        {
            Check.ArgumentNotNull(card, nameof(card));

            CardSection sec = default;
            if (card.TryGetSections()?.TryGetValue(KrConstants.KrStages.Name, out sec) == true)
            {
                sec.Rows.Clear();
            }
        }

        #endregion
    }
}
