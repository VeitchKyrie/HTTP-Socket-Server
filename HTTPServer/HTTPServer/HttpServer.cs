using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

public class HttpServer
{
    public const string SERVER_NAME = "BetoServer";
    public const string VERSION = "HTTP / 1.1";

    public const string MSG_D = "/root/msg";
    public const string WEB_D = "/root/web";

    public IPAddress ipAddress { get; private set; }

    public int port { get; private set; }

    Socket handler;

    Thread read;

    bool running = false;
    bool responseSent = false;

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
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        ipAddress = ipHostInfo.AddressList[0];

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.
        listener.Bind(localEndPoint);
        listener.Listen(10);

        Console.WriteLine("Server IP Adress: " + ipAddress);
        Console.WriteLine("Server Port: " + port);

        Console.WriteLine("\n________________________________________\n\n");

        try
        {
            read = new Thread(ReadThread);
            read.IsBackground = false;
            read.Start();

            while (true)
            {
                Console.WriteLine("Waiting for a connection... \n");

                responseSent = false;
                handler = listener.Accept();

                while (true)
                {
                    if (responseSent)
                        break;
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

            // Incoming data from the client.
            string data = Encoding.UTF8.GetString(bytes, 0, a);

            Request request = new Request(data);
            Response response = new Response(request);
            response.Post(handler);
            responseSent = true;

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            handler = null;
        }
    }
}
