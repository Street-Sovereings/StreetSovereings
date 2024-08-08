using System;
using System.IO;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello, Enter path for the mod:");
        string path = Console.ReadLine();
        string fileExtension = Path.GetExtension(path);
        fileExtension = fileExtension.TrimStart('.');

        if (File.Exists(path))
        {
            if (fileExtension == "ssmod")
            {
                string fileData = File.ReadAllText(path);
                Console.WriteLine(fileData);
                Console.WriteLine("\n Valid? Y/N");
                string valid = Console.ReadLine().Trim().ToUpper();

                if (valid == "Y")
                {
                    Console.WriteLine("Checking");
                }
                else if (valid == "N")
                {
                    Console.WriteLine("Exit code: 0x5 (ERROR_ACCESS_DENIED)");
                }
                else
                {
                    Console.WriteLine("Exit code: 0xD (ERROR_INVALID_DATA)");
                }
            }
            else
            {
                Console.WriteLine("Exit code: 0xB (ERROR_BAD_FORMAT)");
            }
            
        }
        else
        {
            Console.WriteLine("Exit code: 0x2 (ERROR_FILE_NOT_FOUND)");
        }
    }
}

// Exit codes here: https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-