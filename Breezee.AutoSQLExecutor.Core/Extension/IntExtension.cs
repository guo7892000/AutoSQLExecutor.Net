using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    public static class IntExtension
    {
        /// <summary>
        /// 数字字母数组
        /// 包括0至9、大写A至Z共36个字符
        /// </summary>
        public static string[] NumberLetter = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };


        /// <summary>
        /// 数值转换为大写字母
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToUpperWord(this int num)
        {
            if (num < 0)
            {
                num = 0;
            }
            int times = num / 26;
            int idx = num % 26 + 65;//A为65

            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            byte[] btNumber = new byte[] { (byte)idx };
            string sReturn = asciiEncoding.GetString(btNumber);
            return times > 0 ? sReturn + times.ToString() : sReturn;
        }

        
        /// <summary>
        /// 随机产生指定位数的字符串
        /// </summary>
        /// <param name="num">字符串的位数</param>
        /// <returns></returns>
        public static string RandomNumberString(this int num)
        {
            string code = "";

            Random rd = new Random();
            for (int i = 0; i < num; i++)
            {
                code += NumberLetter[rd.Next(0, NumberLetter.Length)];
            }
            return code;
        }

        /// <summary>
        /// 转换为数字字母流水号
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToNumberLetterSequenceNo(this int num)
        {
            if (num < 0)
            {
                num = 0;
            }


            int times = num / 36;
            int idx = num % 36;

            string code = NumberLetter[idx];

            while(times > 36)
            {

            }
            code = NumberLetter[times] + code;
            return code;
        }

        private static string GetSequenceNo(int num,string sOut)
        {
            string sReturn = sOut;

            return sReturn;
        }

    }
}
