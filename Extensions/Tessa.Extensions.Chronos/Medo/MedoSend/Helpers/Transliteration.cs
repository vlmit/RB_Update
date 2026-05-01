using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using NLog;
using AngleSharp.Text;

namespace Tessa.Extensions.Chronos.Medo.MedoSend.Helpers
{
    public static class Transliteration
    {

        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static string Translit(string s)
        {
            StringBuilder ret = new StringBuilder();
            string[] rus = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й",
          "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц",
          "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };

            string[] eng = { "A", "B", "V", "G", "D", "E", "E", "ZH", "Z", "I", "Y",
          "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "KH", "TS",
          "CH", "SH", "SHCH", "", "Y", "", "E", "YU", "YA" };

            string[] digits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            for (int j = 0; j < s.Length; j++)
            {
                bool isDigit = false;

                for (int i = 0; i < digits.Length; i++)
                {
                    if (s.Substring(j, 1) == digits[i])
                    {
                        logger.Info($"Transliteration digits[i]: {digits[i]}");
                        ret.Append(eng[i]);
                        isDigit = true;
                        break;
                    }
                }

                if (isDigit)
                {
                    continue;
                }

                bool isRus = true;

                for (int i = 0; i < eng.Length; i++)
                {
                    if (s.Substring(j, 1) == eng[i])
                    {
                        logger.Info($"Transliteration eng[i]: {eng[i]}");
                        ret.Append(eng[i]);
                        isRus = false;
                        break;
                    }
                    else if (s.Substring(j, 1) == eng[i].ToLower())
                    {
                        logger.Info($"Transliteration eng[i].ToLower(): {eng[i].ToLower()}");

                        ret.Append(eng[i].ToLower());
                        isRus = false;
                        break;
                    }
                }

                if (!isRus)
                {
                    continue;
                }

                bool isSymbol = true;

                for (int i = 0; i < rus.Length; i++)
                {
                    if (s.Substring(j, 1) == rus[i])
                    {
                        logger.Info($"Transliteration rus[i]: {rus[i]} eng[i]: {eng[i]}");
                        ret.Append(eng[i]);
                        isSymbol = false;
                        break;
                    }
                    else if (s.Substring(j, 1) == rus[i].ToLower())
                    {
                        logger.Info($"Transliteration rus[i].ToLower(): {rus[i].ToLower()} eng[i].ToLower(): {eng[i].ToLower()}");
                        ret.Append(eng[i].ToLower());
                        isSymbol = false;
                        break;
                    }
                }

                if (isSymbol)
                {
                    ret.Append("_");
                }
                
            }
            
            return ret.ToString();
        }

        
    }
}
