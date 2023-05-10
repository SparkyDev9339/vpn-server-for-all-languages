using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        // Создание сокета
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));

        // Генерация ключа шифрования
        byte[] key = new byte[32];
        using (Aes aes = Aes.Create())
        {
            aes.GenerateKey();
            key = aes.Key;
        }
        var cipher = new AesCryptoServiceProvider();

        // Ожидание подключения клиента
        listener.Listen(1);
        Socket client = listener.Accept();

        // Получение данных от клиента
        byte[] buffer = new byte[1024];
        int received = client.Receive(buffer);

        byte[] data = new byte[received];
        Array.Copy(buffer, data, received);

        // Шифрование данных
        cipher.Key = key;
        cipher.GenerateIV();
        byte[] encryptedData = cipher.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);

        // Отправка зашифрованных данных через прокси-сервер
        Socket proxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        proxy.Connect("proxy_address", 8080);
        proxy.Send(cipher.IV);
        proxy.Send(key);
        proxy.Send(encryptedData);

        // Получение ответа от прокси-сервера
        byte[] iv = new byte[16];
        proxy.Receive(iv);
        byte[] encryptedResponse = new byte[1024];
        int responseLength = proxy.Receive(encryptedResponse);

        // Расшифровка ответа
        cipher.IV = iv;
        byte[] decryptedResponse = cipher.CreateDecryptor().TransformFinalBlock(encryptedResponse, 0, responseLength);

        // Отправка расшифрованного ответа клиенту
        client.Send(decryptedResponse);

        // Закрытие соединений
        client.Shutdown(SocketShutdown.Both);
        client.Close();
        proxy.Close();
        listener.Close();
    }
}
