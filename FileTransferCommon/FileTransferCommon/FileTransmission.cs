using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FileTransferCommon
{
    class FileTransmission
    {
    }
    public class FileReceive
    {
        private static Int32 _default_port = 1230;
        private static int _default_buffer_size = 4096;
        private FileCommon _file_common;
        public FileReceive(string filename)
        {
            _file_common = new FileCommon(filename);
        }
        public async void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _default_port);
            listener.Start();
            Console.WriteLine("File receive listening:");

            while (true)
            {
                try
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Task t = Process(tcpClient);
                    await t;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private async Task Process(TcpClient tcpClient)
        {
            string clientEndPoint =
              tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from "
              + clientEndPoint);
            try
            {
                byte[] dataBuffer = new byte[_default_buffer_size];
                NetworkStream networkStream = tcpClient.GetStream();               
                
                while (true)
                {
                    int receiveSize = await networkStream.ReadAsync(dataBuffer, 0, _default_buffer_size);
                    if (receiveSize >0)
                    {
                        _file_common.Write(dataBuffer, receiveSize);
                    }
                    else
                        break; // Client closed connection
                }
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }
    }
}
