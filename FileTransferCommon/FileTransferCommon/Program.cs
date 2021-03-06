﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FileTransferCommon
{
    public class CommandResolve
    {
        public CommandResolve() { }
        public static async Task ProcessInput(Stream networkStream, string input_line)
        {
            if (!String.IsNullOrEmpty(input_line))
            {
                StreamWriter writer = new StreamWriter(networkStream);              
                writer.AutoFlush = true;
                await writer.WriteLineAsync(await CommandResolve.Resolve(input_line));
                await CommandResolve.ProcessStream(networkStream);               
            }
        }
        public static async Task ProcessStream(Stream networkStream)
        {
            try
            {
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);                   
                writer.AutoFlush = true;
                while (true)
                {
                    string request = await reader.ReadLineAsync();
                    if (request != null)
                    {
                        Console.WriteLine("Received echo: " + request);
                        string echo_str = await CommandResolve.Resolve(request);
                        if (!String.IsNullOrEmpty(echo_str))
                        {
                            await writer.WriteLineAsync(echo_str);
                        }
                        else
                            break; // unvalid command or command complete
                    }
                    else
                        break; // Client closed connection
                }                                   
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static async Task<string> Resolve(string input_line)
        {
            string[] input_array = input_line.Split();
            string command = input_array[0];
            if (command == "retrieve")
            {
                if (input_array.Length == 3)
                    return "store " + input_array[1] + " " + input_array[2];
                else
                    return "error number of parameters in retrieve command.";
            }
            else if (command == "store")
            {
                if (input_array.Length == 3 && File.Exists(input_array[1]))
                {
                    return "upload " + input_array[1] + " " + input_array[2]
                        + " " + FileCommon.GetSize(input_array[1]) + " " + FileCommon.ComputeHash(input_array[1]);
                }
                else
                    return "error number of parameters in store command.";
            }
            else if (command == "upload")
            {
                if (input_array.Length == 5 && Convert.ToInt64(input_array[3])>0)
                {
                    FileReceive fileReceive = new FileReceive(input_array[2]);
                    long seek_position = fileReceive.PrepareToWrite(Convert.ToInt64(input_array[3]), input_array[4]);
                    if (seek_position >= 0)
                    {
                        IPEndPoint localPoint = fileReceive.InitAndGetLocalEndPoint();
                        fileReceive.Run();
                        return "move " + input_array[1] + " " + input_array[2] + " "
                            + seek_position + " " + localPoint.Address + " " + localPoint.Port;
                    }
                    else
                    {
                        return "error file already exists";
                    }
                }
                else
                    return "error file size";
            }
            else if (command == "move")
            {
                FileSend.SendFile(input_array[1], Convert.ToInt64(input_array[3]), 
                    input_array[4], Convert.ToInt32(input_array[5]));
                return null;
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
