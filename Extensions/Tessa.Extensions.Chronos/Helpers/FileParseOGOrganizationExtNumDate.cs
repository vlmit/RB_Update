using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Files;

namespace Tessa.Extensions.Chronos.Helpers
{
    public class FileParseOGOrganizationExtNumDate
    {

        ///// <summary>
        ///// Считывание категорий из файла, получение GUID и номер категории
        ///// </summary>
        ///// <param name="fileSource"></param>
        ///// <returns></returns>
        //public Dictionary<string, string> GetCategoriesFromFile(string fileSource)
        //{
        //    try
        //    {
        //        Dictionary<string, string> dic;

        //        using (StreamReader sr = new StreamReader(fileSource))
        //        {
        //            string s;

        //            dic = new Dictionary<string, string>();

        //            while ((s = sr.ReadLine()) != null)
        //            {
        //                string[] sSplit = s.Split("|", 2);

        //                string cGUID = sSplit[0].Trim(' ');

        //                string cCategory = sSplit[1].Trim(' ');

        //                if (cCategory != "NULL")
        //                {
        //                    dic.Add(cGUID, cCategory);
        //                }
        //            }
        //        }
        //        return dic;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Ошибка распознавания карточки." + e.Message);
        //        return null;
        //    }
        //}

        private static Logger _logger;

        /// <summary>
        /// Считывание категорий из файла, получение GUID и номер категории
        /// </summary>
        /// <param name="fileSource"></param>
        /// <returns></returns>
        public List<OGOrganizationExtNumDate> GetData(string fileSource, Logger logger)
        {
            try
            {
                _logger = logger;

                List<OGOrganizationExtNumDate> result;
                using (StreamReader sr = new StreamReader(fileSource))
                {
                    string s;

                    result = new List<OGOrganizationExtNumDate>();

                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSplit = s.Split("|", 4);

                        if(sSplit.Length != 4)
                        {
                            logger.Info("Строка не может быть обработана: " + s);
                            continue;
                        }

                        OGOrganizationExtNumDate entry = new OGOrganizationExtNumDate()
                        {
                            UID = sSplit[0],
                            organization = sSplit[1],
                            extNum = sSplit[2],
                            extDate = sSplit[3]
                        };

                        result.Add(entry);
                    }
                }
                return result;

            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка распознавания карточки." + e.Message);
                return null;
            }
        }

        //public void PrintCards(List<Dictionary<string, string>> cards)
        //{
        //    foreach (var c in cards)
        //    {
        //        foreach (var s in c)
        //        {
        //            Console.WriteLine(s.Key + " : " + s.Value);
        //        }
        //        Console.WriteLine();
        //    }
        //}
    }
}
