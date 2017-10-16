using Mall.Bot.Common.DBHelpers.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Mall.Bot.Common.Helpers
{
    public class BotTextHelper
    {
        private List<BotText> texts;
        private SocialNetworkType type;
        public string Locale;

        public BotTextHelper(string _locale, SocialNetworkType _type, List<BotText> _texts)
        {
            Locale = _locale;
            type = _type;
            texts = _texts;
        }
        /// <summary>
        /// Декодирует в UTF8
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DecodeToUtf8(string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string SmileCodesReplace(string parametr, SocialNetworkType type = SocialNetworkType.Telegram)
        {
            if (type == SocialNetworkType.Facebook) parametr = parametr.Replace("\\\\", "\\");
            var smiles = new List<string>();
            for (int i = 1; i < parametr.Length; i++)
            {
                if (parametr[i] == 'U' && parametr[i-1] == '\\')
                {
                    smiles.Add(parametr.Substring(i + 1, 8));
                    i += 8;
                }
            }

            foreach (var item in smiles)
            {
                int code = int.Parse(item, System.Globalization.NumberStyles.HexNumber);
                string unicodeString = char.ConvertFromUtf32(code);
                parametr = parametr.Replace($"\\U{item}", unicodeString);
            }

            if (type == SocialNetworkType.Facebook || type == SocialNetworkType.Telegram) parametr = parametr.Replace("\\\\", "\\");

            if (type == SocialNetworkType.Telegram) parametr = parametr.Replace("\\r\\n", "\r\n");

            
            
            return parametr;
        }
        public static float LengthOfString(string parametr, BitmapSettings bitmap)
        {
            using (var gr = Graphics.FromImage(bitmap.Bmp))
            {
                SizeF size = gr.MeasureString(parametr, new Font("open sans", 23));
                return size.Width;
            }
        }

        public static string GetVKSmileNumber(int number)
        {
            string res = " ";
            while (number != 0)
            {
                res = res.Insert(0, (number % 10).ToString() + "&#8419;");
                number /= 10;
            }
            return res;
        }

        public static string GetEmojiNumber(int number)
        {
            string[] numerals = {
                "\U00000030\U000020E3",
                "\U00000031\U000020E3",
                "\U00000032\U000020E3",
                "\U00000033\U000020E3",
                "\U00000034\U000020E3",
                "\U00000035\U000020E3",
                "\U00000036\U000020E3",
                "\U00000037\U000020E3",
                "\U00000038\U000020E3",
                "\U00000039\U000020E3" };
            if (number == 0) return numerals[0];
            string res = " ";
            while (number != 0)
            {
                res = res.Insert(0, numerals[number % 10]);
                number /= 10;
            }
            return res;
        }

        /// <summary>
        /// Специфическая перегрузка. В случае, когда otherData содержит множество строк, замена otherDataKey произойет на ту, которая соответствует локали + PartOfotherData
        /// В данном случае порядок соответствия языкам такой otherData[0] - русский otherData[1] - английский
        /// в любой непонятной ситуации подставляется русский
        /// </summary>
        /// <param name="key"></param>
        /// <param name="otherDataKey"></param>
        /// <param name="otherData"></param>
        /// <returns></returns>
        public string GetMessage(string key, string otherDataKey, string PartOfotherData, string [] otherData)
        {
            if (Locale == "ru_RU" || otherData.Length == 1) return GetMessage(key, otherDataKey, PartOfotherData + " " +otherData[0]);
            else return GetMessage(key, otherDataKey, PartOfotherData + " " + otherData[1]);
        }

        public string GetMessage(string key, string otherDataKey, string otherData)
        {
            return GetMessage(key, new string [] { otherDataKey }, new string[]  { otherData });
        }

        public string GetMessage(string key)
        {
            string[] m1 = null;
            string[] m2 = null;
            return GetMessage(key, m1, m2);
        }

        public string GetMessage(string key, string [] otherDataKey = null, string [] otherData = null)
        {
            string message = texts.FirstOrDefault(x => x.Locale == Locale && x.Key == key).Text;
            //красивый вывод команд
            if (type != SocialNetworkType.Telegram)
            {
                if (Locale == "ru_RU")
                {
                    message = message.Replace("%place%", "«место»");
                    message = message.Replace("%tutorial%", "«обучение»");
                    message = message.Replace("%help%", "«помощь»");
                    message = message.Replace("%back%", "«назад»");
                    message = message.Replace("%again%", "«повторить»");
                    message = message.Replace("%getinfo%", "«статус»");
                    message = message.Replace("%question%", "«вопрос»");
                }
                else
                {
                    message = message.Replace("%place%", "«place»");
                    message = message.Replace("%tutorial%", "«tutorial»");
                    message = message.Replace("%help%", "«help»");
                    message = message.Replace("%back%", "«back»");
                    message = message.Replace("%again%", "«again»");
                    message = message.Replace("%getinfo%", "«getinfo»");
                    message = message.Replace("%question%", "«question»");
                }
            }
            else
            {
                message = message.Replace("%place%", "/place");
                message = message.Replace("%tutorial%", "/tutorial");
                message = message.Replace("%help%", "/help");
                message = message.Replace("%back%", "/back");
                message = message.Replace("%again%", "/again");
                message = message.Replace("%getinfo%", "/getinfo");
                message = message.Replace("%question%", "/question");
            }

            if (otherData != null && otherDataKey != null && otherDataKey.Length == otherData.Length)
            {
                for (int i = 0; i < otherDataKey.Length; i++)
                {
                    message = message.Replace(otherDataKey[i], otherData[i]);
                }
            }
            return message;
        }
    }
}
