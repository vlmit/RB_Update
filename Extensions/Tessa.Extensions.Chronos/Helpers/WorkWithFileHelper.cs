using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Identity.Client;
using Tessa.Cards;
using Tessa.Platform.Data;
using Tessa.Platform.Validation;
using FileData = System.Tuple<System.Guid, System.Guid, string, System.Guid>;

namespace Tessa.Extensions.Chronos.Helpers
{
    public static class WorkWithFileHelper
    {
        /// <summary>
        /// метод для сохранения файлов
        /// </summary>
        /// <param name="contentResult">ICardFileContentResult</param>
        /// <param name="directory">глобальная папка</param>
        /// <param name="name">имя файла</param>
        /// <param name="versionID">id последней версии файла</param>
        public delegate Task SaveFiles(ICardFileContentResult contentResult, string directory, string name,
            string versionID);

        /// <summary>
        /// получаем информацию о файлах
        /// </summary>
        /// <param name="dbScope">IDbScope</param>
        /// <param name="query">запрос</param>
        /// <param name="param">DataParameter</param>
        /// <returns></returns>
        public static async Task<IList<FileData>> GetFilesInfoAsync(IDbScope dbScope, string query, DataParameter[] param = null)
        {
            var result = new List<FileData>();

            await using (dbScope.Create())
            {
                var db = dbScope.Db;

                await db.SetCommand(query, param).LogCommand().ExecuteNonQueryAsync();
                using var reader = await db.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new FileData
                        (reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2), reader.GetGuid(3)));
                }
            }

            return result;
        }

        /// <summary>
        /// сохраняем файлы во временную папку
        /// </summary>
        /// <param name="permissionsProvider">ICardServerPermissionsProvider</param>
        /// <param name="cardStreamRepository">ICardStreamServerRepository</param>
        /// <param name="validationResult">IValidationResultBuilder</param>
        /// <param name="filesToParse">информация о файлах</param>
        /// <param name="globalPath">глобальный путь к папке</param>
        /// <param name="saveFiles">метод для сохранеия файлов</param>
        public static async Task SaveFilesToTempFolderAsync(
            ICardServerPermissionsProvider permissionsProvider,
            ICardStreamServerRepository cardStreamRepository,
            IValidationResultBuilder validationResult,
            IList<FileData> filesToParse,
            string globalPath, SaveFiles saveFiles, NLog.Logger logger)
        {
            foreach (var fileInfo in filesToParse)
            {
                var contentRequest = new CardGetFileContentRequest
                {
                    CardID = fileInfo.Item1,
                    FileID = fileInfo.Item2,
                    FileName = fileInfo.Item3,
                    VersionRowID = fileInfo.Item4
                };

                permissionsProvider.SetFullPermissions(contentRequest);
                contentRequest.SetForbidStoringHistory(true);

                var contentResult = await cardStreamRepository.GetFileContentAsync(contentRequest);
                var contentResponse = contentResult.Response;

                if (!contentResponse.ValidationResult.IsSuccessful())
                {
                    validationResult.Add(contentResponse.ValidationResult.Build());
                    logger.Info("SaveFilesToTempFolderAsync" + contentResponse.ValidationResult.Build());
                }
                else if (!contentResult.HasContent)
                {
                    validationResult.AddError($"CardFileContentResult does not have value " +
                        $"cardID: {fileInfo.Item1}, fileID: {fileInfo.Item2}, " +
                        $"filename: {fileInfo.Item3}, versionRowID: {fileInfo.Item4}");
                    logger.Info($"SaveFilesToTempFolderAsync CardFileContentResult does not have value " +
                        $"cardID: {fileInfo.Item1}, fileID: {fileInfo.Item2}, " +
                        $"filename: {fileInfo.Item3}, versionRowID: {fileInfo.Item4}");
                }
                else
                {
                    //SaveNewFile(contentResult, globalPath, fileInfo.Item3, flag ? fileInfo.Item4.ToString() : null);
                    await saveFiles(contentResult, globalPath, fileInfo.Item3, fileInfo.Item4.ToString());
                    logger.Info($"SaveFilesToTempFolderAsync globalPath: {globalPath} fileInfo.Item3: {fileInfo.Item3} fileInfo.Item4.ToString(): {fileInfo.Item4.ToString()}");
                }
            }
        }
    }
}