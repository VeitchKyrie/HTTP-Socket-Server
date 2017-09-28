using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

public class HttpServer
{
    public const string MSG_D = "/root/msg";
    public const string WEB_D = "/root/web";

    Socket handler;
    bool ResponseSent = false;

    IPHostEntry ipHostInfo;
    IPAddress ipAddress;
    int port;
    IPEndPoint localEndPoint;
    Socket listener;

    // Incoming data from the client.
    public static string data = null;

    Thread read;

    bool running = false;

    public HttpServer(int port)
    {
        this.port = port;
    }

    public void Start()
    {
        if(!running)
            StartListening();
    }

    void StartListening()
    {
        ipHostInfo = Dns.Resolve(Dns.GetHostName());
        ipAddress = ipHostInfo.AddressList[0];

        localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.
        listener.Bind(localEndPoint);
        listener.Listen(10);

        Console.WriteLine("Server IP Adress: " + ipAddress);
        Console.WriteLine("Server Port: " + port);

        Console.WriteLine("\n________________________________________\n\n");

        ContinueListening();
    }

    void ContinueListening()
    {
        try
        {
            data = null;

            read = new Thread(ReadThread);
            read.IsBackground = false;
            read.Start();

            while (true)
            {
                Console.WriteLine("Waiting for a connection... \n");

                ResponseSent = false;
                handler = listener.Accept();

                while (true)
                {
                    if (ResponseSent)
                    {
                        Thread.Sleep(500);


                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    void ReadThread()
    {
        while (true)
        {
            if (handler == null || !handler.Connected)
                continue;

            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            int a = handler.Receive(bytes);
            data = Encoding.UTF8.GetString(bytes, 0, a);

            Request request = new Request(data);
            Response response = new Response(request);
            response.Post(handler);
            ResponseSent = true;

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            handler = null;
        }
    }
}
