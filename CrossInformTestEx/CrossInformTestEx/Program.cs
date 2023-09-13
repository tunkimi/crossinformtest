using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;

class CrosInformTestEx
{
    private static Stopwatch Watcher = Stopwatch.StartNew();
    private static ConcurrentDictionary<string, int> TripletsCounter = new ConcurrentDictionary<string, int>();
    private static string path = "file.txt";
    private static long fileLenth;
    private static int bufferLenth = 2048;
    private static long lettersHasRead = 0;
    private static char[][] headTails;

    private static object locker = new object();
    static void Main(string[] args)
    {
        Watcher.Start();

        using (var reader = new StreamReader(path))
        {
            fileLenth = reader.BaseStream.Length;
            headTails = new char[fileLenth / bufferLenth + 2][];
            for (int i = 0; i < fileLenth / bufferLenth + 2; i++)
                headTails[i] = new char[4];
            Parallel.For(0, fileLenth / bufferLenth + 1, (i) =>
            {
                char[] buffer;
                long tempLen;
                if (fileLenth - lettersHasRead < bufferLenth)
                {
                    short len = (short)(fileLenth - lettersHasRead);
                    buffer = new char[len];
                    lock (locker)
                    {
                        reader.Read(buffer, 0, len);
                        lettersHasRead += bufferLenth;
                        tempLen = lettersHasRead;
                    }
                }
                else
                {
                    buffer = new char[bufferLenth];
                    lock (locker)
                    {
                        reader.Read(buffer, 0, bufferLenth);
                        lettersHasRead += bufferLenth;
                        tempLen = lettersHasRead;
                    }
                }

                if (isBufferEmpty(buffer)) return;
                count(buffer, (int)tempLen / bufferLenth-1);
            });
            
        }
        Parallel.For(0, headTails.Length, (i) =>
        {
            if (isBufferEmpty(headTails[i])) return;
            count(headTails[i], - 1);
        });

        var TripletsTop = TripletsCounter.OrderByDescending(r => r.Value).Take(10);
        foreach (var entry in TripletsTop)
        {
            Console.WriteLine($"{entry.Key} : {entry.Value}");
        }

        Watcher.Stop();
        Console.WriteLine($"time: {Watcher.ElapsedMilliseconds}");
    }
    private static bool isBufferEmpty(char[] buffer) 
        => buffer.Contains('\0') && buffer.Where(c => c != '\0').ToArray().Length == 0;
    private static void count(char[] buffer, int iterNum)
    {
        for (int i = 0;i < buffer.Length - 2; i++)
        {
            var key = buffer.AsSpan(i, 3).ToString();
            if (key.All(Char.IsLetter))
            {
                if (!TripletsCounter.TryAdd(key, 1))
                {
                    TripletsCounter[key]++;
                }
            }
        }
        if (iterNum != -1)
        {
            headTails[iterNum][2] = buffer[0];
            headTails[iterNum][3] = buffer[1];
            headTails[iterNum + 1][0] = buffer[buffer.Length - 2];
            headTails[iterNum + 1][1] = buffer[buffer.Length - 1];
        }
    }
}
