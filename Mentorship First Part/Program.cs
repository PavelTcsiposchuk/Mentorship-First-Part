using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mentorship_First_Part
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 100;

            Run(count);

            count = 5000;

            Run(count);

            count = 5000;

            Run(count);

            count = 50000;

            Run(count);

            count = 500000;

            Run(count);

            Console.ReadLine();

        }

        private static void Run(int count)
        {
            List<string> set = GenerateSetStrings(count);
            Console.WriteLine(string.Format("Start proccess {0} strings", count));

            //PLINQ
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            CalculateFirst(set);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            PrintTime(ts);

            //Tasks
            stopWatch = new Stopwatch();
            stopWatch.Start();
            CalculateSync(set);
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            PrintTime(ts);

            //sync
            stopWatch = new Stopwatch();
            stopWatch.Start();
            CalculateSecond(set);
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            PrintTime(ts);

            //Threads
            stopWatch = new Stopwatch();
            stopWatch.Start();
            CalculateThird(set);
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            PrintTime(ts);
        }

        private static void PrintTime(TimeSpan ts)
        {
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
        }

        public static void CalculateFirst(List<string> set)
        {
            ConcurrentDictionary<char, double> result = new ConcurrentDictionary<char, double>();
            set.AsParallel().ForAll(x =>
            {
                ProccessString(x, result);
            });
        }

        public static void CalculateSecond(List<string> set)
        {
            ConcurrentDictionary<char, double> result = new ConcurrentDictionary<char, double>();
            List<Task> tasks = new List<Task>();
            set.ForEach(x =>
            {
                Task task = Task.Factory.StartNew(() =>
                {
                    ProccessString(x, result);
                });
                tasks.Add(task);
            });
            Task.WaitAll(tasks.ToArray());
        }

        private static void ProccessString(string x, ConcurrentDictionary<char, double> result)
        {
            Dictionary<char, double> keyValues = new Dictionary<char, double>();
            var chars = x.ToCharArray();
            foreach (var letter in chars)
            {

                if (keyValues.ContainsKey(letter))
                {
                    keyValues[letter]++;
                }
                else
                {
                    keyValues.Add(letter, 1);
                }
            }

            foreach (var kvp in keyValues)
            {
                if (!result.TryAdd(kvp.Key, kvp.Value))
                {
                    result.AddOrUpdate(kvp.Key, kvp.Value, (key, oldValue) => oldValue + kvp.Value);
                }

            }
        }

        public static void CalculateThird(List<string> set)
        {
            ConcurrentDictionary<char, double> result = new ConcurrentDictionary<char, double>();
            List<Thread> threads = new List<Thread>();
            for(int i = 0; i < 10; i++)
            {
                Thread thread = new Thread(ProcessInThread);
                thread.Start(set.Skip(set.Count/10 *i).Take(set.Count / 10));
                threads.Add(thread);
            }
            while(!threads.TrueForAll(thread => thread.ThreadState == System.Threading.ThreadState.Stopped))
            {
                Thread.Sleep(100);
            }
            result = ResultsOfThreads.result;
            ResultsOfThreads.result = new ConcurrentDictionary<char, double>();
        }

        public static void ProcessInThread(object obj)
        {
            IEnumerable<string> list = obj as IEnumerable<string>;
            foreach(var str in list)
            {
                ProccessString(str, ResultsOfThreads.result);
            }
           
        }


        public static void CalculateSync(List<string> set)
        {
            Dictionary<char, double> result = new Dictionary<char, double>();
            set.ForEach(x =>
            {
                var chars = x.ToCharArray();
                foreach (var letter in chars)
                {
                    if (result.ContainsKey(letter))
                    {
                        result[letter]++;
                    }
                    else
                    {
                        result.Add(letter, 1);
                    }
                }
            });
        }

        private static List<string> GenerateSetStrings(int count)
        {
            List<string> set = new List<string>();
            for (int i = 0; i < count; i++)
            {
                set.Add(StringGeneration());
            }
            return set;
        }

        public static string StringGeneration()
        {
            int numberLetters = 1000;

            string capsLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string smallLeters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower();
            string specialCharacters = "1234567890";
            char[] symbols = (capsLetters + smallLeters + specialCharacters).ToCharArray();

            Random rand = new Random();

            StringBuilder word = new StringBuilder();
            for (int j = 1; j <= numberLetters; j++)
            {
                int letterNum = rand.Next(0, symbols.Length - 1);
                word.Append(symbols[letterNum]);
            }

            return word.ToString();
        }

        public class ResultsOfThreads
        {
            public static ConcurrentDictionary<char, double> result = new ConcurrentDictionary<char, double>();
        }
    }
}
