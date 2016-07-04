using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;

namespace FileTransferCommon
{
    public class FileCommon
    {
        private string _file_name;
        private string _file_information_record;
        private FileStream _file_stream;
        private FileStream _file_information_stream;
        private StreamWriter _file_information_writer;
        private string _sha256_str;
        private long _total_size;
        private long _offset;

        public FileCommon(string filename)
        {
            _file_name = filename;
            _file_information_record = _file_name + ".record";
        }

        public static long GetSize(string file_path)
        {
            if (File.Exists(file_path))
                return new FileInfo(file_path).Length;
            else
                return 0;
        }
        public static string ComputeHash(string file_path)
        {
            if (!File.Exists(file_path))
            {
                return null;
            }
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] hash_value;
            using (FileStream fileStream = new FileStream(file_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                hash_value = mySHA256.ComputeHash(fileStream);
            }
            return BitConverter.ToString(hash_value).Replace("-", string.Empty);
        }
        public long PrepareToWrite(long file_size, string file_hash)
        {
            long current_size = FileCommon.GetSize(_file_name);
            if (current_size >= file_size) //seems need not to transmit
            {
                return -1;
            }
            if (current_size == 0) //create new file and new information record file
            {
                _file_stream = new FileStream(_file_name, FileMode.OpenOrCreate, FileAccess.Write);
                _file_information_stream = new FileStream(_file_information_record, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                _file_information_writer = new StreamWriter(_file_information_stream);
                _file_information_writer.WriteLine(String.Format("{0} {1} {2}", file_hash, file_size, 0));
            }
            else if (current_size>0 && current_size< file_size) //compare current file information to lastest record information
            {
                _file_information_stream = new FileStream(_file_information_record, FileMode.Open, FileAccess.ReadWrite);
                _file_information_writer = new StreamWriter(_file_information_stream);
                string[] info_list;
                StreamReader info_reader = new StreamReader(_file_information_stream);
                info_list = info_reader.ReadLine().Split();

                if (info_list.Length!=3 || info_list[0]!= file_hash || Convert.ToInt64(info_list[1])!= file_size)
                {
                    return -1;
                }
                else
                {
                    _file_stream = new FileStream(_file_name, FileMode.Append, FileAccess.Write);
                    _file_information_stream.Seek(0, SeekOrigin.Begin);
                }                                   
            }
            _sha256_str = file_hash;
            _total_size = file_size;
            _offset = current_size;
            return current_size;        
        }
        public void Write(ref byte[] buffer, int size)
        {
            _file_stream.Write(buffer, 0, size);
            _file_information_writer.WriteLine(String.Format("{0} {1} {2}", _sha256_str, _total_size, _offset+size));
        }
        public async Task WriteAsync(byte[] buffer, int size)
        {
            await _file_stream.WriteAsync(buffer, 0, size);
            _offset += size;
            await _file_information_writer.WriteLineAsync(String.Format("{0} {1} {2}", _sha256_str, _total_size, _offset));
        }
        public void Close()
        {
            _file_stream.Close();
            _file_information_writer.Close();
            _file_information_stream.Close();
        }
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
        public long PrepareToWrite(long file_size, string file_hash)
        {
            return _file_common.PrepareToWrite(file_size, file_hash);
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
                            Console.WriteLine("file receive size:"+receiveSize);
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
