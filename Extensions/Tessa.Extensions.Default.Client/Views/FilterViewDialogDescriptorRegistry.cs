#nullable enable

using System;
using System.Collections.Concurrent;
using Tessa.Platform;

namespace Tessa.Extensions.Default.Client.Views
{
    /// <inheritdoc cref="IFilterViewDialogDescriptorRegistry"/>
    public sealed class FilterViewDialogDescriptorRegistry :
        IFilterViewDialogDescriptorRegistry
    {
        #region Fields

        private readonly ConcurrentDictionary<Guid, FilterViewDialogDescriptor> descriptors = new();

        #endregion

        #region IFilterViewDescriptorRegistry Members

        /// <inheritdoc/>
        public void Register(
            Guid compositionID,
            FilterViewDialogDescriptor descriptor)
        {
            Check.ArgumentNotNull(descriptor, nameof(descriptor));

            this.descriptors.AddOrUpdate(
                compositionID,
                descriptor,
                (_, _) => descriptor);
        }

        /// <inheritdoc/>
        public FilterViewDialogDescriptor? TryGet(
            Guid compositionID)
        {
            this.descriptors.TryGetValue(compositionID, out var descriptor);
            return descriptor;
        }

        #endregion
    }
}
