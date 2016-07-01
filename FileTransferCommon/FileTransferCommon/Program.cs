using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileTransferCommon
{
    public class FileCommon
    {
        private string _file_name;
        private string _file_information_record;
        public FileCommon(string filename)
        {
            _file_name = filename;
            _file_information_record = _file_name + ".record";
        }
        public static bool FileExist(ref string file_path)
        {
            return File.Exists(file_path);
        }
        public bool Exists() { return File.Exists(_file_name); }
        public long GetSize() { return new FileInfo(_file_name).Length; }
    }
    public class CommandResolve
    {
        public CommandResolve() { }
        public static string Resolve(ref string input_line)
        {
            string[] input_array = input_line.Split();
            string command = input_array[0];
            if (command == "retrieve")
            {
                if (input_array.Length == 3)
                    return "store " + input_array[1] + " " + input_array[2];
                else
                    return "error numbers of parameters in retrieve command.";
            }
            else if (command == "store")
            {
                if (input_array.Length == 3 && FileCommon.FileExist(ref input_array[1]))
                {
                    return "upload " + input_array[1] + " " + input_array[2]
                        + " " + new FileCommon(input_array[1]).GetSize();
                }
                else
                    return "error numbers of parameters in store command.";
            }
            else if (command == "upload")
            {

            }
            else if (command == "error")
            {
                return null;
            }
            else
                return "error invalid commandline";
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
