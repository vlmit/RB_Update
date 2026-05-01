using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;
using Tessa.Platform;
using Tessa.Platform.Runtime;
using Tessa.Platform.Validation;

namespace Tessa.Test.Default.Shared
{
    /// <summary>
    /// Объект, обеспечивающий вывод сообщений.
    /// Зарегистрирован для использования в тестах.
    /// </summary>
    /// <remarks>
    /// Особенности:
    /// <list type="bullet">
    ///     <item><description>Метод <see cref="ShowNotEmptyAsync(ValidationResult, CancellationToken)"/>, при наличии ошибок, выводит их как результаты выполнения теста. Если ошибок нет, то результаты выводятся через <see cref="LoggerMessageProvider.ShowNotEmptyAsync(ValidationResult, CancellationToken)"/>.</description></item>
    ///     <item><description>Метод <see cref="ShowExceptionAsync(Exception, string, CancellationToken)"/> заданное исключение выводит как результат выполнения теста.</description></item>
    ///     <item><description>Метод <see cref="ConfirmAsync(string, string, CancellationToken)"/> всегда возвращает значение <see langword="true"/>.</description></item>
    /// </list>
    /// </remarks>
    public sealed class TestMessageProvider :
        IMessageProvider
    {
        #region Fields

        private readonly IMessageProvider loggerMessageProvider;

        #endregion

        #region Constructors

        public TestMessageProvider(LoggerMessageProvider loggerMessageProvider)
        {
            Check.ArgumentNotNull(loggerMessageProvider, nameof(loggerMessageProvider));

            this.loggerMessageProvider = loggerMessageProvider;
        }

        #endregion

        #region IMessageProvider Members

        /// <inheritdoc/>
        public async Task ShowNotEmptyAsync(
            ValidationResult result,
            CancellationToken cancellationToken = default)
        {
            Check.ArgumentNotNull(result, nameof(result));

            if (result.Items.Count == 0)
            {
                return;
            }

            if (result.HasErrors)
            {
                var str = result.ToString(ValidationLevel.Detailed);
                TestHelper.SetAssertionResult(FailureSite.Test, str);
            }
            else
            {
                await this.loggerMessageProvider.ShowNotEmptyAsync(result, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public Task ShowExceptionAsync(
            Exception ex,
            string caption = null,
            CancellationToken cancellationToken = default)
        {
            TestHelper.SetAssertionResult(FailureSite.Test, ex);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> ConfirmAsync(
            string message,
            string caption = null,
            CancellationToken cancellationToken = default) =>
            TaskBoxes.True;

        #endregion
    }
}
