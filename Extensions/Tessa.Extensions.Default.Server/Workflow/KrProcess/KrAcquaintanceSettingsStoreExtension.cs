using System;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Storage;
using Tessa.Platform.Validation;
using Tessa.Roles;

namespace Tessa.Extensions.Default.Server.Workflow.KrProcess
{
    public sealed class KrAcquaintanceSettingsStoreExtension : CardStoreExtension
    {
        #region Fields

        private readonly IRoleRepository roleRepository;

        #endregion

        #region Constructors

        public KrAcquaintanceSettingsStoreExtension(
            IRoleRepository roleRepository)
        {
            this.roleRepository = roleRepository;
        }

        #endregion

        #region Base Overrides

        public override async Task BeforeRequest(ICardStoreExtensionContext context)
        {
            var card = context.Request.Card;

            if (card.Sections.TryGetValue(KrConstants.KrStages.Virtual, out var stageSection))
            {
                foreach (var row in stageSection.Rows)
                {
                    Guid? senderID;
                    if (row.State != CardRowState.Deleted
                        && (senderID = row.TryGet<Guid?>(KrConstants.KrAcquaintanceSettingsVirtual.SenderID)).HasValue)
                    {
                        var role = await roleRepository.GetRoleAsync(senderID.Value, context.CancellationToken);
                        if (role.RoleType != RoleType.Context
                            && role.RoleType != RoleType.Personal)
                        {
                            context.ValidationResult.AddError(this, "$KrProcess_Acquaintance_SenderShoudBePersonalOrContext");
                            return;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
