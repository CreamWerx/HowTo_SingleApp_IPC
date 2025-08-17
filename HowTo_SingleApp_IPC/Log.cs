using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowTo_SingleApp_IPC;
public static class Log
{
    public static void WriteToTextFile(string message)
    {
        string filePath = "log.txt";
        try
        {
            using (var writer = new System.IO.StreamWriter(filePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }

    public static void ClearLog()
    {
        string filePath = "log.txt";
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.WriteAllText(filePath, string.Empty);
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Failed to clear log file: {ex.Message}");
        }
    }
}
