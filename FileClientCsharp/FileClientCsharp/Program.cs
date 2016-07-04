using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

using FileTransferCommon;

namespace FileClientCsharp
{
    class Program
    {
        private static async Task SendRequest(string server,
            int port)
        {
            try
            {
                TcpClient client = new TcpClient();
                await client.ConnectAsync(IPAddress.Parse(server), port); // Connect

                using (NetworkStream networkStream = client.GetStream())
                { 
                    string line;
                    while ((line = Console.ReadLine()) != null)
                    {
                        await CommandResolve.ProcessInput(networkStream, line);
                    }
                }
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void Main(string[] args)
        {
            Task A = SendRequest("127.0.0.1", 1234);
            A.Wait();
        }
    }
}
