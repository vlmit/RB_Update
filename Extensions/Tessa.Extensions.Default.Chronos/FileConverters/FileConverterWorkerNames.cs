namespace Tessa.Extensions.Default.Chronos.FileConverters
{
    /// <summary>
    /// Имена стандартных конвертеров <see cref="Tessa.FileConverters.IFileConverterWorker"/>,
    /// которые используются в других конвертерах.
    /// </summary>
    public static class FileConverterWorkerNames
    {
        #region Constants

        /// <summary>
        /// Конвертер в TIFF из PDF, который используется в <see cref="PdfFileConverterWorker"/> или его наследниках.
        /// </summary>
        public const string TiffToPdf = "TiffToPdf";

        #endregion
    }
}
