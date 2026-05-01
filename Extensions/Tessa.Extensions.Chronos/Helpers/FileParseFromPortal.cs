using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessa.Extensions.Chronos.Helpers
{
    public class FileParseFromPortal
    {
        private string[] SPKeys =
                {
                    "UID",
                    "DocTitle",
                    "RegDate",
                    "RegNumber",
                    "WebId",
                    "Annotation",
                    "Comments",
                    "DeliveryTypeId",
                    "DocGroup",
                    "FileDir"
            };

        /// <summary>
        /// Обработка всех файлов в заданной папке
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> GetSPCardsFromDir(string path)
        {
            try
            {
                //получение файлов *.txt из заданной папки
                string[] dirFiles = Directory.GetFiles(path, "*.txt");

                //если файлов нет
                if (dirFiles.Length == 0)
                {
                    throw new Exception("Отсутствуют файлы в папке.");
                }

                //иначе инициализация списка словарей
                List<Dictionary<string, string>> SPCards = new List<Dictionary<string, string>>();

                //проход по всем файлам из папки
                foreach (string f in dirFiles)
                {
                    SPCards.Add(GetSPCardFromFile(f));
                }

                //если ни один словарь не добавился в список
                if (SPCards.Count == 0)
                {
                    throw new Exception("Карточки не распознаны.");
                }

                return SPCards;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        /// <summary>
        /// Считывание карточки из одного файла и создание словаря
        /// </summary>
        /// <param name="fileSource"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetSPCardFromFile(string fileSource)
        {
            try
            {
                Dictionary<string, string> dic;

                using (StreamReader sr = new StreamReader(fileSource))
                {
                    string s;

                    dic = new Dictionary<string, string>();

                    string lastKey = "";

                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] sSplit = s.Split(":", 2);

                        string sKey = sSplit[0].TrimEnd(' ');

                        if (SPKeys.Contains(sKey) && !string.IsNullOrEmpty(sKey))
                        {
                            dic.Add(sKey, sSplit[1].TrimStart(' '));

                            lastKey = sKey;
                        }
                        else if (!string.IsNullOrEmpty(sKey))
                        {
                            dic[lastKey] += " " + s.TrimStart(' ');
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
