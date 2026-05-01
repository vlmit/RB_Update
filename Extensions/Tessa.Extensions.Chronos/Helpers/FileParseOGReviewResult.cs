using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Files;

namespace Tessa.Extensions.Chronos.Helpers
{
    public class OGReviewResult
    {
        public string UID { get; set; }
        public string reviewResult { get; set; }
    }
    
    public class FileParseOGReviewResult
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

        /// <summary>
        /// Считывание категорий из файла, получение GUID и номер категории
        /// </summary>
        /// <param name="fileSource"></param>
        /// <returns></returns>
        public List<OGReviewResult> GetData(string fileSource)
        {
            try
            {
                List<OGReviewResult> result;
                using (StreamReader sr = new StreamReader(fileSource))
                {
                    string s;

                    result = new List<OGReviewResult>();

                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSplit = s.Split("|", 2);

                        if(sSplit.Length != 2)
                        {
                            continue;
                        }

                        OGReviewResult entry = new OGReviewResult()
                        {
                            UID = sSplit[0],
                            reviewResult = sSplit[1]
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
