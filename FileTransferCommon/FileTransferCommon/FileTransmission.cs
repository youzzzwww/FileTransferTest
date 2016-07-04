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
                        int bytes_read = 0;
                        while ((bytes_read = await file_send.ReadAsync(send_buffer, 0, _default_buffer_size)) > 0)
                        {                           
                            await networkStream.WriteAsync(send_buffer, 0, bytes_read);
                            //Console.WriteLine("file send size:" + bytes_read);
                        }
                    }
                }
                client.Close();
                Console.WriteLine("file send complete");
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
        private static IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
        public IPEndPoint InitAndGetLocalEndPoint()
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
            _listener.Start();
            Console.WriteLine("File receive listening:");

            int local_port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            return new IPEndPoint(FileReceive.LocalIPAddress(), local_port);
        }
        public async Task Run()
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
                            await _file_common.WriteAsync(dataBuffer, receiveSize);
                            //Console.WriteLine("file receive size:"+receiveSize);
                        }
                        else
                            break; // Client closed connection
                    }
                }
                Console.WriteLine("receive complete and close data transmission");
                _file_common.Close();
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
