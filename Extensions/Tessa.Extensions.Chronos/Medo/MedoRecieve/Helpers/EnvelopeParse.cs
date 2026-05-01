using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessa.Extensions.Chronos.Medo.MedoRecieve.Helpers
{
    public class EnvelopeParse
    {
        private string[] EnvelopeKeys =
                {
                    "[ПИСЬМО КП ПС СЗИ]",
                    "[АДРЕСАТЫ]",
                    "[ФАЙЛЫ]",
                    "[ТЕКСТ]"
            };

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public List<string> GetFilesFromEnvelope(string fileSource)
        {
            try
            {
                logger.Info($"GetFilesFromEnvelope source: {fileSource}");

                List<string> result;

                using (StreamReader sr = new StreamReader(fileSource, encoding: Encoding.GetEncoding(1251)))
                {
                    string s;

                    bool isFiles = false;

                    result = new List<string>();

                    while ((s = sr.ReadLine()) != null)
                    {
                        //logger.Info($"GetFilesFromEnvelope s: {s}");

                        if (string.IsNullOrWhiteSpace(s))
                        {
                            continue;
                        }

                        if (s.Trim() == "[ФАЙЛЫ]")
                        {
                            //logger.Info($"GetFilesFromEnvelope s: {s} if1");
                            isFiles = true;
                            continue;
                        }

                        if (isFiles && EnvelopeKeys.Contains(s.Trim())) 
                        {
                            //logger.Info($"GetFilesFromEnvelope s: {s} if2");
                            isFiles = false;
                            continue;
                        }

                        var fSplit = s.Split('=');

                        if (isFiles && fSplit.Length == 2)
                        {
                            //logger.Info($"GetFilesFromEnvelope s: {s} if3");
                            result.Add(fSplit[1]);
                        }
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

                        if (EnvelopeKeys.Contains(sKey) && !string.IsNullOrEmpty(sKey) && sKey != "Executors")
                        {
                            dic.Add(sKey, sSplit[1].TrimStart(' '));

                            lastKey = sKey;
                        }
                        else if (EnvelopeKeys.Contains(sKey) && !string.IsNullOrEmpty(sKey) && sKey == "Executors")
                        {
                            if (!string.IsNullOrEmpty(sSplit[1]) && !String.IsNullOrWhiteSpace(sSplit[1]))
                            {
                                string[] executorSplit = sSplit[1].Split("|", 2);

                                dic.Add(sKey, executorSplit[1].Trim(' '));

                                lastKey = sKey;

                            }
                            else
                            {
                                dic.Add(sKey, "исполнитель не указан");
                            }
                            
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
