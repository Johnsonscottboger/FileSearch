using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace FileSerach.Core
{
    public static class SearchHelper
    {
        public static string[] Search(string param, IEnumerable<string> items)
        {
            if (string.IsNullOrWhiteSpace(param) || items == null || items.Count() == 0)
                return new string[0];

            string[] words = param
                                .Split(new char[] { ' ', '\u3000' }, StringSplitOptions.RemoveEmptyEntries)
                                .OrderBy(s => s.Length)
                                .ToArray();

            var q = from sentence in items.AsParallel()
                    let MLL = Mul_LnCS_Length(sentence, words)
                    where MLL >= 0
                    orderby (MLL + 0.5) / sentence.Length, sentence
                    select sentence;

            return q.ToArray();
        }

        //static int[,] C = new int[5000, 100];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="words">多个关键字。长度必须大于0，必须按照字符串长度升序排列。</param>
        /// <returns></returns>
        static int Mul_LnCS_Length(string sentence, string[] words)
        {
            int sLength = sentence.Length;
            int result = sLength;
            bool[] flags = new bool[sLength];
            int[,] C = new int[sLength + 1, words[words.Length - 1].Length + 1];
            //GetArrayFromPool(out flags, out C, sLength);
            foreach (string word in words)
            {
                int wLength = word.Length;
                int first = 0, last = 0;
                int i = 0, j = 0, LCS_L;
                //foreach 速度会有所提升，还可以加剪枝
                for (i = 0; i < sLength; i++)
                {
                    for (j = 0; j < wLength; j++)
                    {
                        if (sentence[i] == word[j])
                        {
                            C[i + 1, j + 1] = C[i, j] + 1;
                            if (first < C[i, j])
                            {
                                last = i;
                                first = C[i, j];
                            }
                        }
                        else
                            C[i + 1, j + 1] = Math.Max(C[i, j + 1], C[i + 1, j]);
                    }
                }
                LCS_L = C[i, j];
                if (LCS_L <= wLength >> 1)
                    return -1;

                while (i > 0 && j > 0)
                {
                    if (C[i - 1, j - 1] + 1 == C[i, j])
                    {
                        i--;
                        j--;
                        if (!flags[i])
                        {
                            flags[i] = true;
                            result--;
                        }
                        first = i;
                    }
                    else if (C[i - 1, j] == C[i, j])
                        i--;
                    else// if (C[i, j - 1] == C[i, j])
                        j--;
                }

                if (LCS_L <= (last - first + 1) >> 1)
                    return -1;
            }

            return result;
        }

        const int sizex = 5000;
        const int sizey = 100;
        static Dictionary<int, bool[]> _flagsPool = new Dictionary<int, bool[]>();
        static Dictionary<int, int[,]> _cPool = new Dictionary<int, int[,]>();
        static HashSet<int> _flags = new HashSet<int>();
        static bool[] _emptyFlags = new bool[sizex];
        static object _syncRoot = new object();
        static void GetArrayFromPool(out bool[] flags, out int[,] C, int length)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            lock (_syncRoot)
            {
                if (_flags.Contains(id))
                {
                    flags = _flagsPool[id];
                    C = _cPool[id];
                    Array.Copy(_emptyFlags, flags, length);
                }
                else
                {
                    _flagsPool[id] = flags = new bool[sizex];
                    _cPool[id] = C = new int[sizex, sizey];
                    _flags.Add(id);
                }
            }
        }
    }
}
