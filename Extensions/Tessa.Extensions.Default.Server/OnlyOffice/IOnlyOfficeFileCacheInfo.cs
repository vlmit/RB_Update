using System;

namespace Tessa.Extensions.Default.Server.OnlyOffice
{
    /// <summary>
    /// Содержит информацию о файле, добавленном в кэш <see cref="IOnlyOfficeFileCache"/>.
    /// Информация используется для редактирования документов в редакторах OnlyOffice.
    /// </summary>
    public interface IOnlyOfficeFileCacheInfo
    {
        /// <summary>
        /// Идентификатор записи.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// Идентификатор исходной версии файла.
        /// </summary>
        Guid SourceFileVersionID { get; }

        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        Guid CreatedByID { get; }

        /// <summary>
        /// Имя исходного файла.
        /// </summary>
        string? SourceFileName { get; }

        /// <summary>
        /// URL к отредактированному файлу на сервере документов.
        /// </summary>
        string? ModifiedFileUrl { get; }

        /// <summary>
        /// Время последнего изменения URL к модицированному файлу <see cref="ModifiedFileUrl"/>.
        /// </summary>
        DateTime? LastModifiedFileUrlTime { get; }

        /// <summary>
        /// Время последнего обращения или создания, изменения этой информации.
        /// </summary>
        DateTime LastAccessTime { get; }

        /// <summary>
        /// Признак того, что после закрытия редактора файл был изменён,
        /// или <c>null</c>, если редактор не был закрыт.
        /// </summary>
        bool? HasChangesAfterClose { get; }

        /// <summary>
        /// Признак того, что сервер документов уведомил нас о том, что редактор был открыт с этим файлом.
        /// </summary>
        bool EditorWasOpen { get; }
    }
}