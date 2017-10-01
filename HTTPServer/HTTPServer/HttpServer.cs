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

    uint handledRequests = 0;

    public IPAddress ipAddress { get; private set; }
    IPEndPoint localEndPoint;

    public int port { get; private set; }

    Socket handler;

    bool running = false;
    bool responseSent = false;

    public HttpServer(int port)
    {
        this.port = port;
    }

    public void Start()
    {
        if (!running)
        {
            Initialize();
            StartListening();
        }
    }

    void Initialize()
    {
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        ipAddress = ipHostInfo.AddressList[0];
        localEndPoint = new IPEndPoint(ipAddress, port);
    }

    void StartListening()
    {
        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.
        listener.Bind(localEndPoint);
        listener.Listen(10);

        Console.WriteLine("Server IP Adress: " + ipAddress);
        Console.WriteLine("Server Port: " + port);
        Console.WriteLine("\n________________________________________________________________\n\n");

        while (true)
        {
            Console.WriteLine("Handled Requests: " + handledRequests + " Waiting for a connection... \n");

            handler = listener.Accept();

            byte[] bytes = new byte[1024];

            int a = handler.Receive(bytes);
            string data = Encoding.UTF8.GetString(bytes, 0, a);

            ClientHandling clientHandle = new ClientHandling(data, handler);
            handledRequests++;
        }
    }
}
