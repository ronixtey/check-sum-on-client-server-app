using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SocketTcpClient
{
    class Program
    {
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера
        
        static int BUFFER_SIZE = 256; // размер буффера для данных


        static void Main(string[] args)
        {
            try
            {
                SendMessageFromSocket(port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }

        static void SendMessageFromSocket(int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // подключаемся к удаленному хосту
            socket.Connect(ipPoint);

            Console.Write("Введите сообщение: ");
            string message = Console.ReadLine();
            byte[] checksum_client = CRC32.Calculate(Encoding.Unicode.GetBytes(message));
            StringBuilder builder = new StringBuilder();

            // Случайные помехи с вероятностью 1 к 10
            Random r = new Random(DateTime.Now.Millisecond);
            int value = r.Next(10);
            if (value == 0) message = "Error";

            builder.AppendFormat("{0}|{1}", message, Encoding.Unicode.GetString(checksum_client));
            byte[] data = Encoding.Unicode.GetBytes(builder.ToString());

            Console.Write("Сумма: ");
            foreach (byte a in checksum_client)
                Console.Write(a + " ");
            Console.WriteLine();

            // Отправляем
            socket.Send(data);

            // получаем ответ
            data = new byte[BUFFER_SIZE]; // буфер для ответа
            int bytes = 0; // количество полученных байт

            builder.Clear();
            do
            {
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);

            message = builder.ToString();
            Console.Write("Ответ сервера: ");
            Console.ForegroundColor = message == "Успешно" ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(message + "\n\n");
            Console.ResetColor();

            // Используем рекурсию для неоднократной отправки
            SendMessageFromSocket(port);

            // закрываем сокет
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}