using System;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Caching;
using Tessa.Cards.Extensions;
using Tessa.Platform.Collections;
using Tessa.Platform.Data;
using Tessa.Platform.Storage;
using Tessa.Scheme;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    /// <summary>
    /// Расширение метаданных, модифицирующее тип карточки правила доступа.
    /// </summary>
    public sealed class KrPermissionsExtensionMetadataExtension :
        CardTypeMetadataExtension
    {
        #region Constructors

        public KrPermissionsExtensionMetadataExtension(IDbScope dbScope)
        {
            //конструктор для сервера
            this.dbScope = dbScope;
        }

        public KrPermissionsExtensionMetadataExtension(ICardMetadata clientCardMetadata, ICardCache cache) : base(clientCardMetadata)
        {
            //конструктор для клиента
            this.cache = cache;
        }

        #endregion

        #region Fields

        private readonly IDbScope dbScope;

        private readonly ICardCache cache;

        #endregion

        #region Base Overrides

        public override async Task ModifyTypes(ICardMetadataExtensionContext context)
        {
            CardType permissionsType = await this.TryGetCardTypeAsync(context, DefaultCardTypes.KrPermissionsTypeID, false).ConfigureAwait(false);
            if (permissionsType is null)
            {
                return;
            }
            await this.AddFlagsAsync(permissionsType, context);
            await this.AddExtensionsAsync(permissionsType, context);
        }

        #endregion
        
        #region Private Methods

        private async Task AddFlagsAsync(CardType permissionsType, ICardMetadataExtensionContext context)
        {
            CardTypeBlock flagsBlock = permissionsType.Blocks.FirstOrDefault(x => x.Name == "Flags");
            SchemeTable permissionsSection = await context.SchemeService.GetTableAsync("KrPermissions", context.CancellationToken).ConfigureAwait(false);

            if (flagsBlock == null
                || permissionsType.SchemeItems.FirstOrDefault(x => x.SectionID == permissionsSection.ID) is not
                    CardTypeSchemeItem permissionsSchemeItem)
            {
                return;
            }
            
            // Для каждого флага добавляем его контрол в блок и добавляем флаг в секцию, если он еще не был добавлен
            foreach(var flag in KrPermissionFlagDescriptors.Full.IncludedPermissions.OrderBy(x => x.Order))
            {
                if (flag.IsVirtual
                    || permissionsSection.Columns.GetColumn(flag.SqlName) is not SchemeColumn column)
                {
                    continue;
                }

                var columnID = column.ID;
                if (!permissionsSchemeItem.ColumnIDList.Contains(columnID))
                {
                    permissionsSchemeItem.ColumnIDList.Add(columnID);
                }

                flagsBlock.Controls.Add(
                    new CardTypeEntryControl()
                    {
                        Caption = flag.ControlCaption,
                        Name = flag.Name,
                        Order = flag.Order,
                        PhysicalColumnIDList = new SealableList<Guid>() { columnID },
                        SectionID = permissionsSection.ID,
                        ToolTip = flag.ControlTooltip,
                        Type = CardControlTypes.Boolean,
                    });
            }
        }

        private async Task AddExtensionsAsync(CardType permissionsType, ICardMetadataExtensionContext context)
        {
            var extensionType = await this.TryGetExtensionTypeAsync(context);
            if (extensionType is null)
            {
                return;
            }

            StorageHelper.Merge(extensionType.FormSettings, permissionsType.FormSettings);
            // снимаем копию с типа.
            var copy = extensionType.DeepClone();
            
            // регистрация глобальных объектов.
            using var ctx = new CardGlobalReferencesContext(context, extensionType);
            // т.к. формы при помещении в другой объект меняют Order, то делаем глобальными их блоки.
            foreach (var form in extensionType.Forms)
            {
                form.Blocks.MakeGlobal(ctx, form);
            }
            // меняем блоки в формах копии на глобальные.
            copy.Forms.ReplaceBlocks(extensionType.Forms);
            // у главной формы копируются блоки, которые меняют Order, глобальными могут быть только контролы.
            foreach (var block in extensionType.Blocks)
            {
                block.Controls.MakeGlobal(ctx, extensionType, block);
            }
            // меняем контролы в блоках копии на глобальные.
            copy.Blocks.ReplaceControls(extensionType.Blocks);
            // все валидаторы.
            extensionType.Validators.MakeGlobal(ctx);
            // все расширения типа.
            extensionType.Extensions.MakeGlobal(ctx);
            
            copy.SchemeItems.InsertWithoutCopy(permissionsType.SchemeItems);
            copy.Forms.InsertWithoutCopy(permissionsType.Forms);
            extensionType.Validators.InsertWithoutCopy(permissionsType.Validators);
            extensionType.Extensions.InsertWithoutCopy(permissionsType.Extensions);

            InsertBlocks(copy, permissionsType);
        }

        private async Task<CardType> TryGetExtensionTypeAsync(ICardMetadataExtensionContext context)
        {
            Guid? extensionTypeID;
            if (this.ClientMode)
            {
                if (context.CardTypes.All(x => x.ID != DefaultCardTypes.KrPermissionsTypeID))
                {
                    // на клиенте просматривают тип карточки с расширениями, но не KrPermissions
                    return null;
                }

                var krSettings = await this.cache.Cards.GetAsync("KrSettings", context.CancellationToken).ConfigureAwait(false);
                extensionTypeID = krSettings.IsSuccess
                    ? krSettings.GetValue().Sections["KrSettings"].Fields.Get<Guid?>("PermissionsExtensionTypeID")
                    : null;
            }
            else
            {
                await using (this.dbScope.Create())
                {
                    var db = this.dbScope.Db;
                    var builder = this.dbScope.BuilderFactory
                        .Select().Top(1).C("PermissionsExtensionTypeID")
                        .From("KrSettings").NoLock()
                        .Limit(1);

                    extensionTypeID = await db
                        .SetCommand(builder.Build())
                        .LogCommand()
                        .ExecuteAsync<Guid?>(context.CancellationToken).ConfigureAwait(false);
                }
            }

            // нечего расширять
            if (!extensionTypeID.HasValue)
            {
                return null;
            }

            return await this.TryGetCardTypeAsync(context, extensionTypeID.Value).ConfigureAwait(false);
        }
        
        private static void InsertBlocks(CardType source, CardType target)
        {
            // поиск места вставки.
            int insertIndex = target.Blocks.IndexOf(x => x.Name == "ExtensionMarker");
            insertIndex = insertIndex < 0 ? 0 : insertIndex + 1;
            // вставка.
            source.Blocks.InsertWithoutCopy(target.Blocks, insertIndex);
        }
        
        #endregion
    }
}
