using System;
using System.Collections.Generic;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public interface IKrPermissionSectionSettings
    {
        Guid ID { get; set; }

        bool DisallowRowAdding { get; set; }

        bool DisallowRowDeleting { get; set; }

        bool IsAllowed { get; set; }

        bool IsDisallowed { get; set; }

        bool IsHidden { get; set; }

        bool IsVisible { get; set; }

        bool IsMandatory { get; set; }

        bool IsMasked { get; set; }

        IReadOnlyCollection<Guid> AllowedFields { get; set; }

        IReadOnlyCollection<Guid> DisallowedFields { get; set; }

        IReadOnlyCollection<Guid> HiddenFields { get; set; }

        IReadOnlyCollection<Guid> VisibleFields { get; set; }

        IReadOnlyCollection<Guid> MandatoryFields { get; set; }

        IReadOnlyCollection<Guid> MaskedFields { get; set; }
    }
}
