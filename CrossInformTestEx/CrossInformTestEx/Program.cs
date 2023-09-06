using System.Diagnostics;

class CrosInformTestEx
{
    static void Main(string[] args)
    {
        Stopwatch Watcher = Stopwatch.StartNew();
        Watcher.Start();                                                                        //начало отсчета времени

        string[] Separators = { ",", ".", "!", "?", ";", ":", " ", "\'", "\"", "\n" };          //разделительные знаки

        string path = "file.txt";

        //string path = args[0];

        string InnerString;

        using (var reader = new StreamReader(path))
        {
            InnerString = reader.ReadToEnd();                                                   //считывание исодного текста
        }

        var WordPool = InnerString.Split(Separators, StringSplitOptions.RemoveEmptyEntries)     //входная строка разбитая на слова
            .Where(word => word.Length > 2).ToArray();                                          //в словах короче 3х букв нет триплетов

        Dictionary<string, int> TripletsCounter = new Dictionary<string, int>();                //словарь <триплет, количествоВхождений>

        CountdownEvent cde = new CountdownEvent(WordPool.Length);                               //обратный счетчик выполнения

        Worker.init(TripletsCounter, WordPool, cde);                                            //внесение данных в обрабатывающий класс

        for (int i = 0; i < WordPool.Length; i++)                                               //запуск обработки для каждого конкретного слова
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Worker.Work), (object)i);
        }
        cde.Wait();                                                                             //ожидание завершения обработки

        var TripletsTop = TripletsCounter.OrderByDescending(r => r.Value).Take(10);             //выборка топ-10 триплетов по популярности
        foreach (var entry in TripletsTop)                                                      //вывод в консоль
        {
            Console.WriteLine(entry.Key + " " + entry.Value);
        }

        Watcher.Stop();                                                                         //окончание отсчета времени
        Console.WriteLine(Watcher.ElapsedMilliseconds);                                         //вывод миллисекунд


    }
    static class Worker
    {
        public static Dictionary<string, int> ResultDictionary;                             //словарь популярности триплетов
        public static string[] words;                                                       //набор исследуемых слов
        private static object locker = new object();                                        //объект блокировки словаря
        private static CountdownEvent cde;                                                  //обратный счетчик выполения
        public static void init(Dictionary<string, int> dict, string[] str, CountdownEvent cde)     //инициализация данных
        {
            ResultDictionary = dict;
            words = str;
            Worker.cde = cde;
        }
        public static void Work(object? iteration)                                          //подсчет триплетов в слове 
        {
            if (iteration != null)
            {
                var i = (int)iteration;

                for (int j = 0; j < words[i].Length - 2; j++)
                {
                    string triplet = words[i][j].ToString()                                 //выделение триплета
                                    + words[i][j + 1].ToString()
                                    + words[i][j + 2].ToString();
                    lock (locker)
                    {
                        if (!ResultDictionary.TryAdd(triplet, 1))                           //добавляем запись в словарь
                        {
                            ResultDictionary[triplet] += 1;                                 //если запись уже есть, то инкрементируем
                        }
                    }
                }

            }
            cde.Signal();
        }
    }
}
