using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Platform;

namespace Tessa.Test.Default.Shared
{
    /// <inheritdoc cref="ITestNameResolver"/>
    public sealed class TestNameResolver :
        ITestNameResolver
    {
        #region Constants

        private const int SaltLength = 100;

        #endregion

        #region Fields

        private Random random;

        #endregion

        #region Private Methods

        private byte[] GetRandomBytes(int count = SaltLength)
        {
            var randomBytes = new byte[count];
            this.random.NextBytes(randomBytes);
            return randomBytes;
        }

        #endregion

        #region ITestNameResolver Members

        /// <inheritdoc />
        public ValueTask<string> GetFixtureNameAsync(
            Type type,
            CancellationToken cancellationToken = default)
        {
            if (this.random is null)
            {
                var seed = TestSettings.FixtureSeed;

                this.random = seed > 0
                    ? new Random((int) seed)
                    : new Random();
            }

            return ValueTask.FromResult(
                (type?.AssemblyQualifiedName + Encoding.UTF8.GetString(this.GetRandomBytes()))
                .GetConstantHashCode()
                .ToString("x4"));
        }

        /// <inheritdoc />
        public ValueTask<DateTime> GetFixtureDateTimeAsync(
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(TestSettings.FixtureDate ?? DateTime.UtcNow);

        #endregion
    }
}
