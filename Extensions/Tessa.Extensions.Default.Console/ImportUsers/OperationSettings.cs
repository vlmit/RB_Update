using System;
using System.Collections.Generic;
using System.Linq;
using Tessa.Localization;
using Tessa.Platform.Runtime;

namespace Tessa.Extensions.Default.Console.ImportUsers
{
    public sealed class OperationSettings
    {
        #region Fields

        private readonly List<string> yes = LocalizationManager.GetAllStrings("UI_Common_Yes");

        private readonly List<string> windowsUser = LocalizationManager.GetAllStrings("Enum_LoginTypes_Windows");

        private readonly List<string> tessaUser = LocalizationManager.GetAllStrings("Enum_LoginTypes_Tessa");

        #endregion

        #region Methods

        public bool GetBool(string value) => this.yes.Contains(value);

        public UserLoginType GetLoginType(string value) =>
            this.tessaUser.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)) != null
                ? UserLoginType.Tessa
                : this.windowsUser.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)) != null
                    ? UserLoginType.Windows
                    : UserLoginType.None;

        #endregion
    }
}