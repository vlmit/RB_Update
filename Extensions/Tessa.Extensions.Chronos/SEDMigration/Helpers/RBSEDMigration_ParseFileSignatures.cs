using DocumentFormat.OpenXml.Wordprocessing;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Extensions.Chronos.Helpers;
using Tessa.Files;
using Tessa.Platform;

namespace Tessa.Extensions.Chronos.SEDMigration.Helpers
{
    public static class RBSEDMigration_ParseFileSignatures
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static List<SEDMigrationCardsToSign> GetCardsToSign(string path)
        {
            try
            {
                //получение файлов *.txt из заданной папки
                string[] dirs = Directory.GetDirectories(path);

                //если файлов нет
                if (dirs.Length == 0)
                {
                    throw new Exception($"Отсутствуют файлы в папке {path}");
                }

                //иначе инициализация списка
                List<SEDMigrationCardsToSign> result = new List<SEDMigrationCardsToSign>();

                foreach (string dir in dirs)
                {
                    SEDMigrationCardsToSign entry = new SEDMigrationCardsToSign();

                    DirectoryInfo directoryInfo= new DirectoryInfo(dir);

                    entry.SPUID = directoryInfo.Name;

                    entry.files = GetFileToSign(dir);

                    result.Add(entry);
                }


                ////проход по всем файлам из папки
                //foreach (string f in dirFiles)
                //{
                //    SEDMigrationFileToSign entry = new SEDMigrationFileToSign();

                //    var fileName = Path.GetFileNameWithoutExtension(f);
                //    logger.Info($"Path.GetFileNameWithoutExtension(f): {fileName}");

                //    entry.name = fileName;
                //    entry.signs = GetSignsFromFile(f);

                //    fileSignatures.Add(entry);
                //}

                ////если ни один словарь не добавился в список
                //if (fileSignatures.Count == 0)
                //{
                //    throw new Exception("Карточки не распознаны.");
                //}

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка распознавания папки {path} " + e.Message);
                return null;
            }
        }


        public static List<SEDMigrationFileToSign> GetFileToSign(string path)
        {
            try
            {
                //получение файлов *.txt из заданной папки
                string[] dirFiles = Directory.GetFiles(path, "*.txt");

                //если файлов нет
                if (dirFiles.Length == 0)
                {
                    throw new Exception($"Отсутствуют файлы в папке {path}");
                }

                //иначе инициализация списка
                List<SEDMigrationFileToSign> fileSignatures = new List<SEDMigrationFileToSign>();

                //проход по всем файлам из папки
                foreach (string f in dirFiles)
                {
                    SEDMigrationFileToSign entry = new SEDMigrationFileToSign();

                    var fileName = Path.GetFileNameWithoutExtension(f);
                    //logger.Info($"Path.GetFileNameWithoutExtension(f): {fileName}");

                    entry.name = fileName;
                    entry.signs = GetSignsFromFile(f);

                    fileSignatures.Add(entry);
                }

                ////если ни один словарь не добавился в список
                //if (fileSignatures.Count == 0)
                //{
                //    throw new Exception("Карточки не распознаны.");
                //}

                return fileSignatures;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка распознавания папки {path} " + e.Message);
                return null;
            }
        }

        public static List<SEDMigrationFileSign> GetSignsFromFile(string fileSource)
        {
            try
            {
                List<SEDMigrationFileSign> result;
                using (StreamReader sr = new StreamReader(fileSource))
                {
                    string s;

                    result = new List<SEDMigrationFileSign>();

                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSplit = s.Split("||");

                        if (sSplit.Length < 4)
                        {
                            continue;
                        }
                        
                        SEDMigrationFileSign entry = new SEDMigrationFileSign();

                        DateTime signedDate;

                        if (!DateTime.TryParseExact(sSplit[0], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out signedDate))
                        {
                            signedDate = DateTime.Now;
                        }

                        //logger.Info($"{signedDate}");

                        entry.signed = signedDate.AddHours(8);

                        entry.authorExtID = sSplit[1].Split(";")[0];

                        switch(sSplit[2])
                        {
                            case "ApproveYes":
                                entry.signType = FileSignType.ApproveYes;
                                break;
                            case "ApproveNo":
                                entry.signType = FileSignType.ApproveNo;
                                break;
                            case "EndorseYesComments":
                                entry.signType = FileSignType.EndorseYesComments;
                                break;
                            case "EndorseYes":
                                entry.signType = FileSignType.EndorseYes;
                                break;
                            case "EndorseNo":
                                entry.signType = FileSignType.EndorseNo;
                                break;
                            case "CertificateYes":
                                entry.signType = FileSignType.CertificateYes;
                                break;
                            case "CertificateNo":
                                entry.signType = FileSignType.CertificateNo;
                                break;
                            default:
                                break;
                        }

                        entry.content = sSplit[3];

                        result.Add(entry);
                    }
                }
                return result;

            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка распознавания файла {fileSource} " + e.Message);
                return null;
            }
        }
    }
}
