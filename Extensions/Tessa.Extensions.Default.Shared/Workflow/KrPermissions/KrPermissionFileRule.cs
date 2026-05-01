using System;
using System.Collections.Generic;
using Tessa.Platform.Collections;

namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public sealed class KrPermissionFileRule
    {
        #region Constructors

        public KrPermissionFileRule(
            string extensions,
            bool withCategories)
        {
            if (!string.IsNullOrWhiteSpace(extensions))
            {
                this.Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var extension in extensions.Split(' '))
                {
                    if (!string.IsNullOrEmpty(extension))
                    {
                        this.Extensions.Add(extension);
                    }
                }
            }
            else
            {
                this.Extensions = EmptyHolder<string>.Collection;
            }

            if (withCategories)
            {
                this.Categories = new HashSet<Guid>();
            }
            else
            {
                this.Categories = EmptyHolder<Guid>.Collection;
            }
        }

        #endregion

        #region Properties

        public ICollection<string> Extensions { get; }

        public bool CheckOwnFiles { get; set; }

        public ICollection<Guid> Categories { get; }

        public int AccessSetting { get; set; }

        #endregion

    }
}
