using System;
using System.Collections.Generic;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    public class KrPermissionsManagerCheckResult
    {
        #region Fields

        private Dictionary<string, object> info;

        #endregion

        #region Constructors

        public KrPermissionsManagerCheckResult(
            bool result,
            Dictionary<string, object> info = null)
        {
            this.Result = result;
            this.info = info;
        }

        #endregion

        #region Properties

        public bool Result { get; }

        public Dictionary<string, object> Info => this.info ??= new Dictionary<string, object>(StringComparer.Ordinal);

        #endregion

        #region Convert

        public static implicit operator KrPermissionsManagerCheckResult(bool b)
        {
            return new KrPermissionsManagerCheckResult(b);
        }

        public static implicit operator bool(KrPermissionsManagerCheckResult result)
        {
            return result.Result;
        }

        #endregion
    }
}
