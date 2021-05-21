using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        var usage_info = "Usage examples: du-cs.exe c:\\temp\ndu-cs.exe -t 4 -m 2 c:\\temp";
        if (args != null && args.Length > 0)
        {
            var value_expected = false;
            var prefx = "";
#nullable enable annotations
            int? threads = null, method = null;
            string? parameter = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-m" || args[i] == "--method" || args[i] == "-t" || args[i] == "--threads")
                {
                    if (!value_expected)
                    {
                        value_expected = true;
                        prefx = args[i];
                    }
                    else PrintError(usage_info);
                }
                else
                {
                    if (value_expected) // here check prefix
                    {
                        value_expected = false;
                        if (prefx == "-t" || prefx == "--threads") { if (int.TryParse(args[i], out int t)) threads = t; else PrintError(usage_info); }
                        else if (prefx == "-m" || prefx == "--method") { if (int.TryParse(args[i], out int m)) method = m; else PrintError(usage_info); }
                        else PrintError(usage_info); // unexpected value  
                        prefx = "";
                    }
                    else //unexpected arguments
                    {
                        if (!string.IsNullOrEmpty(parameter)) PrintError(usage_info);
                        else parameter = args[i];
                    }
                }
            }

            var startFolder = parameter ?? Environment.CurrentDirectory;
            var degreeParallel = threads ?? Environment.ProcessorCount;
            if (degreeParallel < 1) degreeParallel = Environment.ProcessorCount;
            long dirSize;
            ThreadPool.SetMinThreads(degreeParallel, 3 * degreeParallel);
            if (method != null && method == 2) dirSize = CalcDirSize(new DirectoryInfo(startFolder), true, degreeParallel);
            else dirSize = GetDirectorySize(new DirectoryInfo(startFolder), true, degreeParallel);
            Console.WriteLine("{0}M", (dirSize / 1048576d).ToString("F2"));
        }
        else Console.WriteLine(usage_info);
    }

    static void PrintError(string info)
    {
        Console.WriteLine("Error parsing commandline.\n{0}", info);
        Environment.Exit(1);
    }

    static long GetDirectorySize(DirectoryInfo directoryInfo, bool recursive = true, int degreeParallel = 16)
    {
        // ============================================================================================	
        //  Recursive-Parallel.ForEach -> https://stackoverflow.com/questions/468119/whats-the-best-way-to-calculate-the-size-of-a-directory-in-net
        //  Directory size example -> https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-simple-parallel-for-loop
        // ============================================================================================	
        var startDirectorySize = default(long);
        if (directoryInfo == null || !directoryInfo.Exists)
            return startDirectorySize; //Return 0 while Directory does not exist.
                                       //Add size of files in the Current Directory to main size.
        foreach (var fileInfo in directoryInfo.GetFiles())
            Interlocked.Add(ref startDirectorySize, fileInfo.Length);
        if (recursive) //Loop on Sub Direcotries in the Current Directory and Calculate it's files size.
            Parallel.ForEach(directoryInfo.GetDirectories(), new ParallelOptions { MaxDegreeOfParallelism = degreeParallel }, (subDirectory) => Interlocked.Add(ref startDirectorySize, GetDirectorySize(subDirectory, recursive)));
        return startDirectorySize; //Return full Size of this Directory.
    }

    static long CalcDirSize(DirectoryInfo di, bool recurse = true, int degreeParallel = 16)
    {
        // ================================================================================================
        // Recursive-Tasks.Parallel.For -> https://stackoverflow.com/questions/2979432/directory-file-size-calculation-how-to-make-it-faster
        // ================================================================================================
        long size = 0;
        FileInfo[] fiEntries = di.GetFiles();
        foreach (var fiEntry in fiEntries)
        {
            Interlocked.Add(ref size, fiEntry.Length);
        }

        if (recurse)
        {
            DirectoryInfo[] diEntries = di.GetDirectories("*", SearchOption.TopDirectoryOnly);
            System.Threading.Tasks.Parallel.For<long>(0, diEntries.Length, new ParallelOptions { MaxDegreeOfParallelism = degreeParallel }, () => 0, (i, loop, subtotal) =>
               {
                   if ((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                       return 0;
                   subtotal += CalcDirSize(diEntries[i], true, degreeParallel);
                   return subtotal;
               }, (x) => Interlocked.Add(ref size, x));
        }

        return size;
    }
}
