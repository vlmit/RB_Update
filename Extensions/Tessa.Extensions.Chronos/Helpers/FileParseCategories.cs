using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Files;

namespace Tessa.Extensions.Chronos.Helpers
{
    public class FileParseCategories
    {
  
        /// <summary>
        /// Считывание категорий из файла, получение GUID и номер категории
        /// </summary>
        /// <param name="fileSource"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetCategoriesFromFile(string fileSource)
        {
            try
            {
                Dictionary<string, string> dic;

                using (StreamReader sr = new StreamReader(fileSource))
                {
                    string s;

                    dic = new Dictionary<string, string>();

                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSplit = s.Split("|", 2);

                        string cGUID = sSplit[0].Trim(' ');

                        string cCategory = sSplit[1].Trim(' ');

                        if (cCategory != "NULL")
                        {
                            dic.Add(cGUID, cCategory);
                        }
                    }
                }
                return dic;
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка распознавания карточки." + e.Message);
                return null;
            }
        }

        public List<OGCategory> GetCategories(string fileSource)
        {
            try
            {
                List<OGCategory> categories;
                using (StreamReader sr = new StreamReader(fileSource))
                {
                    string s;

                    categories = new List<OGCategory>();

                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSplit = s.Split("|", 2);

                        string cGUID = sSplit[0].Trim(' ');

                        string cCategory = sSplit[1].Trim(' ');

                        if (cCategory != "NULL")
                        {
                            categories.Add(new OGCategory { UID = cGUID, category = cCategory });
                        }
                    }
                }
                return categories;

            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка распознавания карточки." + e.Message);
                return null;
            }
        }

        public void PrintCards(List<Dictionary<string, string>> cards)
        {
            foreach (var c in cards)
            {
                foreach (var s in c)
                {
                    Console.WriteLine(s.Key + " : " + s.Value);
                }
                Console.WriteLine();
            }
        }
    }
}
