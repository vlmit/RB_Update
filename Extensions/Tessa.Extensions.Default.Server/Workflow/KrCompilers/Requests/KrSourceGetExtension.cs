using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers.Requests
{
    /// <summary>
    /// Расширение выполняет заполнение виртуальных секций результата
    /// компиляции для карточек содержащих шаблоны маршрутов.
    /// </summary>
    public sealed class KrSourceGetExtension : CardGetExtension
    {
        #region Fields

        private readonly IKrCompilationResultStorage compilationResultStorage;

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="KrSourceGetExtension"/>.
        /// </summary>
        /// <param name="compilationResultStorage">Хранилище результатов компиляции.</param>
        public KrSourceGetExtension(
            IKrCompilationResultStorage compilationResultStorage)
        {
            this.compilationResultStorage = compilationResultStorage ?? throw new System.ArgumentNullException(nameof(compilationResultStorage));
        }

        #endregion

        #region Private Methods

        private async Task FillCompilerOutputAsync(Card card, CancellationToken cancellationToken = default)
        {
            StringDictionaryStorage<CardSection> sections;
            if ((sections = card.TryGetSections()) is null)
            {
                return;
            }

            if (!sections.TryGetValue(KrConstants.KrStageBuildOutputVirtual.Name, out var stageBuildOutputSection))
            {
                return;
            }

            var fields = stageBuildOutputSection.RawFields;
            var output = await this.compilationResultStorage.GetCompilationOutputAsync(card.ID, cancellationToken);

            fields[KrConstants.KrStageBuildOutputVirtual.LocalBuildOutput] = output.Local;
            fields[KrConstants.KrStageBuildOutputVirtual.GlobalBuildOutput] = output.Global;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override Task AfterRequest(ICardGetExtensionContext context)
        {
            Card card;
            if ((card = context.Response.TryGetCard()) is null)
            {
                return Task.CompletedTask;
            }

            return this.FillCompilerOutputAsync(card, context.CancellationToken);
        }

        #endregion
    }
}
