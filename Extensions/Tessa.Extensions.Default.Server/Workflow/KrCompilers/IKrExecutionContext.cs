using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Cards;
using Tessa.Cards.Extensions;
using Tessa.Extensions.Default.Server.Workflow.KrObjectModel;
using Tessa.Extensions.Default.Server.Workflow.KrProcess;
using Tessa.Extensions.Default.Shared.Workflow.KrProcess;
using Tessa.Platform.Validation;

namespace Tessa.Extensions.Default.Server.Workflow.KrCompilers
{
    /// <summary>
    /// Контекст выполнения методов шаблонов этапов Before, After, Condition.
    /// </summary>
    public interface IKrExecutionContext :
        IExtensionContext
    {
        /// <summary>
        /// Возвращает признак, показывающий, необходимость выполнения всех единиц выполнения.
        /// </summary>
        bool ExecuteAll { get; }

        /// <summary>
        /// Возвращает список идентификаторов единиц выполнения, которые необходимо выполнить.
        /// </summary>
        HashSet<Guid> ExecutionUnitIDs { get; }

        /// <summary>
        /// Возвращает стратегию загрузки основной карточки.
        /// </summary>
        IMainCardAccessStrategy MainCardAccessStrategy { get; }

        /// <summary>
        /// Возвращает идентификатор карточки.
        /// </summary>
        Guid? CardID { get; }

        /// <summary>
        /// Возвращает тип карточки.
        /// </summary>
        CardType CardType { get; }

        /// <summary>
        /// Возвращает идентификатор типа документа.
        /// </summary>
        Guid? DocTypeID { get; }

        /// <summary>
        /// Возвращает идентификатор типа карточки или документа.
        /// </summary>
        /// <remarks>Возвращает идентификатор типа документа, если он задан, иначе идентификатор типа карточки.</remarks>
        Guid? TypeID { get; }

        /// <summary>
        /// Возвращает включённые компоненты типового решения для текущей карточки.
        /// </summary>
        KrComponents? KrComponents { get; }

        /// <summary>
        /// Возвращает объектную модель процесса.
        /// </summary>
        WorkflowProcess WorkflowProcess { get; }

        /// <summary>
        /// Возвращает контекст расширения карточки содержащейся в контексте выполнения.
        /// </summary>
        ICardExtensionContext CardContext { get; }

        /// <summary>
        /// Возвращает информацию по вторичному процессу для которого выполняется пересчёт.
        /// </summary>
        IKrSecondaryProcess SecondaryProcess { get; }

        /// <summary>
        /// Возвращает идентификатор группы единиц выполнения <see cref="ExecutionUnitIDs"/>.
        /// Используется для передачи идентификатора группы при расчете шаблонов по одной группе.
        /// </summary>
        Guid? GroupID { get; }

        /// <summary>
        /// Создаёт новый контекст выполнения на основе существующего с учётом новых элементов исполнения.
        /// </summary>
        /// <param name="executionUnits">Список новых элементов исполнения или <c>null</c>, если нужно выполнить все доступные.</param>
        /// <returns>Контекст выполнения, созданный на основе существующего с учётом новых элементов исполнения.</returns>
        IKrExecutionContext Copy(
            IEnumerable<Guid> executionUnits = null);

        /// <summary>
        /// Создаёт новый контекст выполнения на основе существующего с учетом новых элементов исполнения и идентификатора группы.
        /// </summary>
        /// <param name="groupID">Идентификатор группы единиц выполнения.</param>
        /// <param name="executionUnits">Список новых элементов исполнения или <c>null</c>, если нужно выполнить все доступные.</param>
        /// <returns>Контекст выполнения, созданный на основе существующего с учётом новых элементов исполнения и идентификатора группы.</returns>
        IKrExecutionContext Copy(
            Guid? groupID,
            IEnumerable<Guid> executionUnits = null);

        /// <summary>
        /// Возвращает результат компиляции <see cref="IKrCompilationResult"/>.
        /// </summary>
        /// <param name="validationResult"><inheritdoc cref="IValidationResultBuilder" path="/summary"/></param>
        /// <param name="cancellationToken">Объект, посредством которого можно отменить асинхронную задачу.</param>
        /// <returns>Объект <see cref="IKrCompilationResult"/> или значение <see langword="null"/>, если произошла ошибка.</returns>
        ValueTask<IKrCompilationResult?> GetCompilationResultAsync(
            IValidationResultBuilder validationResult,
            CancellationToken cancellationToken = default);
    }
}
