using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Extensions.Default.Shared.Workplaces;
using Tessa.UI;
using Tessa.UI.Cards;
using Tessa.UI.Views.Extensions;
using Tessa.Views;

namespace Tessa.Extensions.Default.Client.Workplaces
{
    /// <summary>
    ///     Конфигуратор для расширения <see cref="AutomaticNodeRefreshExtension" />
    /// </summary>
    public class AutomaticNodeRefreshExtensionConfigurator : ExtensionSettingsConfiguratorBase
    {
        #region Private Fields

        private readonly CreateDialogFormFuncAsync createDialogFormFunc;
        private const string DescriptionLocalization = "$AutomaticNodeRefreshExtension_Description";
        private const string NameLocalization = null;

        #endregion

        #region Constructor

        public AutomaticNodeRefreshExtensionConfigurator(
            CreateDialogFormFuncAsync createDialogFormFunc)
            : base(ViewExtensionConfiguratorType.Form, NameLocalization, DescriptionLocalization)
        {
            this.createDialogFormFunc = createDialogFormFunc;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async override ValueTask<(IFormViewModel, Action)> GetConfiguratorFormAsync(
            IExtensionConfigurationContext context,
            Action markedAsDirtyAction,
            CancellationToken cancellationToken = default)
        {
            var settingsDict = context.GetSettings();
            var settings = settingsDict == null ? new AutomaticNodeRefreshSettings() : this.Load(settingsDict);

            var (form, cardmodel) = await this.createDialogFormFunc(
                "ViewExtensions",
                "AutomaticNodeRefreshExtension",
                cancellationToken: cancellationToken);

            if (form is null)
            {
                TessaDialog.ShowError("$CardTypes_MetadataEditor_ViewExtensionDialog_NotFound");
                return (null, null);
            }

            var section = cardmodel.Card.Sections["AutomaticNodeRefreshExtension"];
            section.Fields["RefreshInterval"] = settings.RefreshInterval;
            section.Fields["WithContentDataRefreshing"] = settings.WithContentDataRefreshing;

            section.FieldChanged += (o, e) =>
            {
                markedAsDirtyAction();
                switch (e.FieldName)
                {
                    case "RefreshInterval":
                        settings.RefreshInterval = (int)e.FieldValue;
                        break;
                    case "WithContentDataRefreshing":
                        settings.WithContentDataRefreshing = (bool)e.FieldValue;
                        break;
                }
            };

            void saveChangesAction()
            {
                var settingsDict = ExtensionSettingsSerializationHelper.SerializeDictionary(settings);
                context.SaveSettings(settingsDict);
            }
            return (form, saveChangesAction);
        }

        /// <inheritdoc />
        public override void Initialize(IExtensionConfigurationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var model = new AutomaticNodeRefreshSettings();
            SaveSettings(context, model);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The save settings.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        private static void SaveSettings(IExtensionConfigurationContext context, IAutomaticNodeRefreshSettings model)
        {
            var settings = ExtensionSettingsSerializationHelper.SerializeDictionary(model);
            context.SaveSettings(settings);
        }

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// The <see cref="IAutomaticNodeRefreshSettings"/>.
        /// </returns>
        private IAutomaticNodeRefreshSettings Load(Dictionary<string, object> settings)
        {
            if (settings.Count == 0)
            {
                return new AutomaticNodeRefreshSettings();
            }
            return (IAutomaticNodeRefreshSettings) ExtensionSettingsSerializationHelper.DeserializeDictionary<AutomaticNodeRefreshSettings>(settings);
        }

        #endregion
    }
}