using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessa.Files;

namespace Tessa.Extensions.Chronos.Helpers
{
    public class FileParseCitizens
    {
  
        /// <summary>
        /// Считывание категорий из файла, получение GUID и номер категории
        /// </summary>
        /// <param name="fileSource"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetCitizensFromFile(string fileSource)
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

                        string cCitizen = sSplit[1].Trim(' ');

                        if (cCitizen != "NULL")
                        {
                            dic.Add(cGUID, cCitizen);
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

        public List<OGCitizen> GetCategories(string fileSource)
        {
            try
            {
                List<OGCitizen> categories;
                using (StreamReader sr = new StreamReader(fileSource, Encoding.UTF8))
                {
                    string s;

                    categories = new List<OGCitizen>();

                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSplit = s.Split("|", 2);

                        string cGUID = sSplit[0].Trim(' ');

                        string cCitizen = sSplit[1].Trim(' ');

                        if (cCitizen != "NULL")
                        {
                            categories.Add(new OGCitizen { UID = cGUID, citizen = cCitizen });
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
