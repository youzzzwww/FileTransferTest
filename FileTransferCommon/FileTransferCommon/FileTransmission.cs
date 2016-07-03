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
    public class FileSend
    {
        private static int _default_buffer_size = 4096;
        public async static Task SendFile(string file_path, long offset, string ip, int port)
        {
            try
            {
                TcpClient client = new TcpClient();
                await client.ConnectAsync(IPAddress.Parse(ip), port); // Connect
                using (NetworkStream networkStream = client.GetStream())
                {
                    byte[] send_buffer = new byte[_default_buffer_size];
                    using (FileStream file_send = new FileStream(file_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        file_send.Seek(offset, SeekOrigin.Begin);
                        int bytes_read = file_send.Read(send_buffer, 0, _default_buffer_size);
                        while (bytes_read > 0)
                        {
                            await networkStream.WriteAsync(send_buffer, 0, bytes_read);
                        }
                    }
                }
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    public class FileReceive
    {
        private static int _default_buffer_size = 4096;
        private FileCommon _file_common;
        private TcpListener _listener;
        public FileReceive(string filename)
        {
            _file_common = new FileCommon(filename);

        }
        public long PrepareToWrite(long file_size)
        {
            return _file_common.PrepareToWrite(file_size);
        }
        public IPEndPoint InitAndGetLocalEndPoint()
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
            _listener.Start();
            Console.WriteLine("File receive listening:");
            return (IPEndPoint)_listener.LocalEndpoint;
        }
        public async void Run()
        {        
            try
            {
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                Task t = Process(tcpClient);
                await t;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

                using (NetworkStream networkStream = tcpClient.GetStream())
                {
                    while (true)
                    {
                        int receiveSize = await networkStream.ReadAsync(dataBuffer, 0, _default_buffer_size);
                        if (receiveSize > 0)
                        {
                            _file_common.Write(ref dataBuffer, receiveSize);
                        }
                        else
                            break; // Client closed connection
                    }
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
