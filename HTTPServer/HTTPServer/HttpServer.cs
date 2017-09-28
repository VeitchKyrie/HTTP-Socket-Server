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

    // Data buffer for incoming data.
    byte[] bytes;

    Socket handler;
    bool threadsInStandby = false;

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

        ContinueListening();
    }

    void ContinueListening()
    {
        try
        {
            read = new Thread(ReadThread);
            read.IsBackground = false;

            data = null;

            read.Start();

            while (true)
            {
                threadsInStandby = false;
                Console.WriteLine("Waiting for a connection... \n");
                handler = listener.Accept();

                while (!threadsInStandby)
                    Thread.Sleep(1);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                handler = null;
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

            bytes = new byte[1024];

            int a = handler.Receive(bytes);
            data = Encoding.UTF8.GetString(bytes, 0, a);

            Request request = new Request(data);
            Response response = new Response(request);
            response.Post(handler);

            while (threadsInStandby || handler == null)
                Thread.Sleep(1);
        }
    }
}
