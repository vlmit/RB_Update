using System;
using System.Collections.Generic;
using Tessa.Extensions.Default.Shared.Workflow.KrPermissions;
using Tessa.Platform.Collections;

namespace Tessa.Extensions.Default.Server.Workflow.KrPermissions
{
    public sealed class KrPermissionsDescriptor
    {
        #region Fields

        private readonly HashSet<KrPermissionFlagDescriptor> originalRequiredPermissions;

        #endregion

        #region Constructors

        public KrPermissionsDescriptor(params KrPermissionFlagDescriptor[] requiredPermissions)
        {
            this.StillRequired = CalculatePermissions(requiredPermissions);
            this.originalRequiredPermissions = new HashSet<KrPermissionFlagDescriptor>(this.StillRequired);
        }

        #endregion

        #region Properties

        public HashSet<KrPermissionFlagDescriptor> Permissions { get; } = new HashSet<KrPermissionFlagDescriptor>();

        public HashSet<KrPermissionFlagDescriptor> StillRequired { get; }

        public HashSet<Guid, IKrPermissionSectionSettingsBuilder> ExtendedCardSettings { get; } = new HashSet<Guid, IKrPermissionSectionSettingsBuilder>(x => x.SectionID);

        public Dictionary<Guid, HashSet<Guid, IKrPermissionSectionSettingsBuilder>> ExtendedTasksSettings { get; }
            = new Dictionary<Guid, HashSet<Guid, IKrPermissionSectionSettingsBuilder>>();

        public List<KrPermissionMandatoryRule> ExtendedMandatorySettings { get; } = new List<KrPermissionMandatoryRule>();

        public KrPermissionVisibilitySettingsBuilder VisibilitySettings { get; } = new KrPermissionVisibilitySettingsBuilder();

        public List<KrPermissionFileRule> FileRules { get; } = new List<KrPermissionFileRule>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Добавляет или удаляет разрешение из набора, в зависимости от параметра allow
        /// </summary>
        /// <param name="flag">Флаг, который нужно добавить к набору или изключить из набора</param>
        /// <param name="allow">
        /// true - добавить к набору, false - исключить из набора. 
        /// Если параметр <paramref name="force"/> равен false, то настройка доступа добавится только, если она была запрошена
        /// </param>
        /// <param name="force">Определяет, должен ли данный флаг установиться принудительно, даже если он не был запрошен</param>
        /// <returns>Текущий объект для цепочки вызовов.</returns>
        public KrPermissionsDescriptor Set(KrPermissionFlagDescriptor flag, bool allow, bool force = false)
        {
            if (allow)
            {
                AddFlag(flag, force);
            }
            else
            {
                RemoveFlag(flag);
            }

            return this;
        }

        /// <summary>
        /// Проверяет что набор прав содержит указанное разрешение
        /// </summary>
        /// <param name="flag">Проверяемый набор прав</param>
        /// <returns>true - если набор содержит переданное разрешение, иначе - false</returns>
        public bool Has(KrPermissionFlagDescriptor flag)
        {
            return flag.IsVirtual ? Has(flag.IncludedPermissions) : Permissions.Contains(flag);
        }

        /// <summary>
        /// Проверяет что набор прав содержит указанно разрешения.
        /// </summary>
        /// <param name="flags">Проверяемый набор прав</param>
        /// <returns>true - если набор содержит переданные разрешения, иначе - false</returns>
        public bool Has(IEnumerable<KrPermissionFlagDescriptor> flags)
        {
            foreach(var flag in flags)
            {
                if (!Permissions.Contains(flag))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Метод проверяет, были ли добавлены все запрашиваемые разрешения
        /// </summary>
        /// <returns>Возвращает true, если все запрашиваемые разрешения уже были добавлены, иначе false</returns>
        public bool AllChecked()
        {
            return StillRequired.Count == 0;
        }

        #endregion

        #region Private Methods

        private void RemoveFlag(KrPermissionFlagDescriptor flag)
        {
            if (originalRequiredPermissions.Contains(flag))
            {
                StillRequired.Add(flag);
            }

            if (flag.IsVirtual || this.Permissions.Remove(flag))
            {
                if (flag.IncludedPermissions.Count > 0)
                {
                    foreach (var includeFlag in flag.IncludedPermissions)
                    {
                        RemoveFlag(includeFlag);
                    }
                }
            }
        }

        private void AddFlag(KrPermissionFlagDescriptor flag, bool force)
        {
            if (StillRequired.Remove(flag)
                || force
                || flag.IsVirtual)
            {
                if ((flag.IsVirtual || this.Permissions.Add(flag))
                    && flag.IncludedPermissions.Count > 0)
                {
                    foreach (var includeFlag in flag.IncludedPermissions)
                    {
                        AddFlag(includeFlag, force);
                    }
                }
            }
        }

        /// <summary>
        /// Метод для разворота цепочки прав доступа с включением в список подчиненных прав доступа и исключением виртуальных
        /// </summary>
        /// <param name="requiredPermissions">Запрашиваемый набор прав доступа</param>
        /// <returns>Возвращает набор прав доступа с учетом подчиеннных прав доступа и с исключением виртуальных</returns>
        private static HashSet<KrPermissionFlagDescriptor> CalculatePermissions(KrPermissionFlagDescriptor[] requiredPermissions)
        {
            HashSet<KrPermissionFlagDescriptor> result = new HashSet<KrPermissionFlagDescriptor>();
            HashSet<KrPermissionFlagDescriptor> virtuals = null;
            void FillFromPermission(KrPermissionFlagDescriptor krPermissionFlag)
            {
                if (krPermissionFlag.IsVirtual)
                {
                    if (virtuals == null)
                    {
                        virtuals = new HashSet<KrPermissionFlagDescriptor>();
                    }
                    if (!virtuals.Add(krPermissionFlag))
                    {
                        // Уже был обработан, значит не добавляем вложенные элементы снова
                        return;
                    }
                }
                else
                {
                    if (!result.Add(krPermissionFlag))
                    {
                        // Уже был добавлен, значит не добавляем вложенные элементы снова
                        return;
                    }
                }

                if (krPermissionFlag.IncludedPermissions.Count > 0)
                {
                    foreach(var permission in krPermissionFlag.IncludedPermissions)
                    {
                        FillFromPermission(permission);
                    }
                }
            }

            for (int i = 0; i < requiredPermissions.Length; i++)
            {
                FillFromPermission(requiredPermissions[i]);
            }

            return result;
        }


        #endregion
    }
}
