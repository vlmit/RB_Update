using System;
using System.Linq;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Platform;
using Tessa.Platform.Collections;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workflow.Wf
{
    public sealed class WfCardMetadataExtension :
        CardTypeMetadataExtension
    {
        #region Constructors

        public WfCardMetadataExtension(ICardMetadata clientCardMetadata)
            : base(clientCardMetadata)
        {
        }

        public WfCardMetadataExtension()
            : base()
        {
        }

        #endregion

        #region Base Overrides

        public override async Task ModifyTypes(ICardMetadataExtensionContext context)
        {
            CardType resolutionType = await this.TryGetCardTypeAsync(context, DefaultTaskTypes.WfResolutionTypeID).ConfigureAwait(false);
            if (resolutionType is null)
            {
                return;
            }
            // сделать необходимые части объекта глобальными.
            resolutionType = MakeGlobal(resolutionType, context);

            if (!resolutionType.IsSealed)
            {
                // тип получен с сервера, скорее всего для предпросмотра в редакторе типов карточек
                CopyMainFormToOtherForms(resolutionType);
            }

            foreach (Guid taskTypeID in WfHelper.MetadataResolutionTaskTypeIDList)
            {
                CardType taskType = await this.TryGetCardTypeAsync(context, taskTypeID, useServerMetadataOnClient: false).ConfigureAwait(false);
                if (taskType is null)
                {
                    continue;
                }

                CopyResolutionTaskType(resolutionType, taskType);

                // для проекта резолюций не должно быть отзыва
                SealableObjectList<CardTypeCompletionOption> options = taskType.CompletionOptions;
                if (taskTypeID == DefaultTaskTypes.WfResolutionProjectTypeID)
                {
                    int revokeOptionIndex = options.IndexOf(x => x.TypeID == DefaultCompletionOptions.Revoke);
                    if (revokeOptionIndex >= 0)
                    {
                        options.RemoveAt(revokeOptionIndex);
                    }
                    
                    // для проекта резолюции вариант "Завершить" должен быть спрятан в "ещё" и располагаться должен над вариантом "Отмена"
                    int completeOptionIndex = options.IndexOf(x => x.TypeID == DefaultCompletionOptions.Complete);
                    if (completeOptionIndex >= 0)
                    {
                        CardTypeCompletionOption completeOption = options[completeOptionIndex];
                        // т.к. здесь модифицируется вариант завершения, мы снимем с него копию и он станет "локальным".
                        completeOption = completeOption.DeepClone();
                        completeOption.Flags = completeOption.Flags.SetFlag(CardTypeCompletionOptionFlags.Additional, true);
                        // изымаем старый вариант, т.к. мы уже работаем с локальной копией.
                        options.RemoveAt(completeOptionIndex);
                        int cancelOptionIndex = options.IndexOf(x => x.TypeID == DefaultCompletionOptions.Cancel);
                        // вставляем в новое место. Это же работает если здесь всего один вариант завершения, поскольку мы меняем глобальный вариант на локальный.
                        if (cancelOptionIndex >= 0)
                        {
                            options.Insert(cancelOptionIndex, completeOption);
                        }
                        else
                        {
                            options.Add(completeOption);
                        }
                    }
                }

                // для варианта завершения "Отмена" надо установить форму, которая не выбирается через редактор.
                // варианты завершения глобальные и форма у всех вариантов "Отмена" тоже глобальная.
                // допустимо устанавливать её много раз, т.к. устанавливается одно и тоже значение.
                CardTypeCompletionOption cancelOption = options.FirstOrDefault(x => x.TypeID == DefaultCompletionOptions.Cancel);
                if (cancelOption is not null)
                {
                    cancelOption.FormName = WfHelper.RevokeOrCancelFormName;
                }
            }
        }

        #endregion
        
        #region Private Methods

        private static CardType MakeGlobal(CardType cardType, ICardMetadataExtensionContext context)
        {
            var type = cardType;
            if (cardType.IsSealed)
            {
                type = cardType.DeepClone();
            }
            using var ctx = new CardGlobalReferencesContext(context, type);
            var mainForm = type;
            // регистрация глобальных объектов.
            // все блоки первой формы.
            mainForm.Blocks.MakeGlobal(ctx, mainForm);
            // все формы, кроме первой.
            type.Forms.MakeGlobal(ctx);
            // все варианты завершения.
            type.CompletionOptions.MakeGlobal(ctx);
            // все валидаторы.
            type.Validators.MakeGlobal(ctx);
            // все расширения типа.
            type.Extensions.MakeGlobal(ctx);
            // возвращаем тип для работы.
            return type;
        }
        
        private static void CopyMainFormToOtherForms(CardType sourceType)
        {
            // блоки глобальные.
            foreach (CardTypeNamedForm namedForm in sourceType.Forms)
            {
                sourceType.Blocks.InsertWithoutCopy(namedForm.Blocks);
                StorageHelper.Merge(sourceType.FormSettings, namedForm.FormSettings);
            }
        }

        private static void CopyResolutionTaskType(CardType sourceType, CardType targetType)
        {
            sourceType.Blocks.InsertWithoutCopy(targetType.Blocks);
            sourceType.SchemeItems.InsertWithoutCopy(targetType.SchemeItems);
            sourceType.Forms.InsertWithoutCopy(targetType.Forms);
            sourceType.CompletionOptions.InsertWithoutCopy(targetType.CompletionOptions);
            sourceType.Validators.InsertWithoutCopy(targetType.Validators);
            sourceType.Extensions.InsertWithoutCopy(targetType.Extensions);

            StorageHelper.Merge(sourceType.FormSettings, targetType.FormSettings);
        }

        #endregion
    }
}
