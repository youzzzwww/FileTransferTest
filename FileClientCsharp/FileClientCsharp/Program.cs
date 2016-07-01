using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

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
                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                StreamReader reader = new StreamReader(networkStream);
                writer.AutoFlush = true;
                string line;
                while ((line = Console.ReadLine()) != null)
                {                  
                    await writer.WriteLineAsync(line);
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
            Task A = Task.Run(() => SendRequest("127.0.0.1", 1234));
            A.Wait();
        }
    }
}
