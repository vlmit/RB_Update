using System;
using System.Collections.Generic;
using Tessa.Platform;
using Tessa.Platform.Storage;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public readonly struct KrPermissionVisibilitySettings
    {
        #region Constructors

        public KrPermissionVisibilitySettings(
            string alias,
            int controlType,
            bool isHidden)
        {
            this.Alias = alias;
            this.ControlType = controlType;
            this.IsHidden = isHidden;
        }

        #endregion

        #region Properties

        public string Alias { get; }

        public int ControlType { get; }

        public bool IsHidden { get; }

        #endregion

        #region Public Methods

        public Dictionary<string, object> ToStorage()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [nameof(Alias)] = Alias,
                [nameof(ControlType)] = Int32Boxes.Box(ControlType),
                [nameof(IsHidden)] = BooleanBoxes.Box(IsHidden),
            };
        }

        public static KrPermissionVisibilitySettings FromStorage(Dictionary<string, object> storage)
        {
            return new KrPermissionVisibilitySettings(
                storage.Get<string>(nameof(Alias)),
                storage.Get<int>(nameof(ControlType)),
                storage.Get<bool>(nameof(IsHidden)));
        }

        #endregion
    }
}
