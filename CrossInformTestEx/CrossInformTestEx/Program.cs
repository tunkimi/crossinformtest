using System.Collections.Concurrent;
using System.Diagnostics;

class CrosInformTestEx
{
    static void Main(string[] args)
    {
        Stopwatch Watcher = Stopwatch.StartNew();
        Watcher.Start();                                                                            //начало отсчета времени

        char[] Separators = { ',', '.', '!', '?', ';', ':', ' ', '\'', '\"', '\n', '-' };           //разделительные знаки

        string path = "file.txt";

        //string path = args[0];

        string InnerString;


        using (var reader = new StreamReader(path))
        {
            InnerString = reader.ReadToEnd();                                                       //считывание исодного текста
        }

        var WordPool = InnerString.Split(Separators)                                                //входная строка разбитая на слова
            .AsParallel().Where(word => word.Length > 2).ToArray();                                 //длиннее 3-х букв

        ConcurrentDictionary<string, int> TripletsCounter = new ConcurrentDictionary<string, int>();//словарь <триплет, количествоВхождений>

        Worker.init(TripletsCounter, WordPool);                                                     //внесение данных в обрабатывающий класс


        #region 1 вариант
        Parallel.ForEach<string>(WordPool, Worker.Work);
        #endregion


        #region 2 вариант
        //Task[] tasks = new Task[WordPool.Length];
        //for (int i = 0; i < tasks.Length; i++)
        //{
        //    int j = i;
        //    tasks[i] = Task.Factory.StartNew(() => Worker.Work(WordPool[j]));
        //}
        //Task.WaitAll(tasks);
        #endregion



        var TripletsTop = TripletsCounter.OrderByDescending(r => r.Value).Take(10);                 //выборка топ-10 триплетов по популярности
        foreach (var entry in TripletsTop)                                                          //вывод в консоль
        {
            Console.WriteLine(entry.Key + " " + entry.Value);
        }

        Watcher.Stop();                                                                             //окончание отсчета времени
        Console.WriteLine(Watcher.ElapsedMilliseconds);                                             //вывод миллисекунд

    }
    static class Worker
    {
        public static ConcurrentDictionary<string, int> ResultDictionary;                           //словарь популярности триплетов
        public static string[] words;                                                               //набор исследуемых слов
        private static object locker = new object();                                                //объект блокировки словаря
        public static void init(ConcurrentDictionary<string, int> dict, string[] str)               //инициализация данных
        {
            ResultDictionary = dict;
            words = str;
        }
        public static void Work(string word)                                                        //подсчет триплетов в слове 
        {
            for (int i = 0; i < word.Length - 2; i++)
            {
                var triplet = word.Substring(i, 3);                                                 //выделение триплета

                ResultDictionary.AddOrUpdate(triplet, 1, (itemKey, itemValue) => itemValue + 1);    //обновление словаря

            }
        }
    }
}
