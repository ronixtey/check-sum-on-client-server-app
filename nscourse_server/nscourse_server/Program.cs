using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace SocketTcpServer
{
    class Program
    {
        static int port = 8005; // порт для приема входящих запросов
        static string address = "127.0.0.1"; // адрес сервера

        static int BUFFER_SIZE = 256; // размер буффера для данных
        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[BUFFER_SIZE]; // буфер для получаемых данных
         
                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    // ивзлекаем и показывем сообщение
                    string[] split_data = builder.ToString().Split('|');
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + split_data[0]);

                    // извлекаем сумму клиента
                    byte[] checksum_client = Encoding.Unicode.GetBytes(split_data[1]);
                    // вычисляем сумму от сообщения сами
                    byte[] checksum_server = CRC32.Calculate(Encoding.Unicode.GetBytes(split_data[0]));

                    Console.Write("Вычисленная сумма: ");
                    foreach (byte a in checksum_server)
                        Console.Write(a + " ");
                    Console.WriteLine();

                    // сравниваем
                    string message = checksum_server.SequenceEqual(checksum_client) ? "Успешно" : "Ошибочно";
                    handler.Send(Encoding.Unicode.GetBytes(message));
                    
                    // закрываем сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}