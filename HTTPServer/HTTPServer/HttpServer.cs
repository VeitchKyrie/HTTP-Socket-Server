using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

public class HttpServer
{
    /// <summary>
    /// The server name.
    /// </summary>
    public const string SERVER_NAME = "BetoServer";

    /// <summary>
    /// The used HTTP version.
    /// </summary>
    public const string VERSION = "HTTP/1.1";

    /// <summary>
    /// The Servers debug level. The higher the debug level the higher the filtering level of debug information.
    /// </summary>
    public const int DebugLevel = 1;

    /// <summary>
    /// The error message's directory.
    /// </summary>
    public const string MSG_D = "/root/msg";

    /// <summary>
    /// The root directory where all files can be found. (except for the error message files) 
    /// </summary>
    public const string WEB_D = "/root/web";

    /// <summary>
    /// The total amount of connections the server has dealt with.
    /// </summary>
    uint totalConnections = 0;

    /// <summary>
    /// The IP Adress used to create the localEndPoint.
    /// </summary>
    IPAddress ipAddress;

    /// <summary>
    /// The IPEndPoint the Socket will be binded to.
    /// </summary>
    IPEndPoint localEndPoint;

    /// <summary>
    /// The servers port number.
    /// </summary>
    public int port { get; private set; }

    /// <summary>
    /// If the server is running.
    /// </summary>
    bool running = false;

    /// <summary>
    /// The server's main socket, creates a new socket for each new connection.
    /// </summary>
    Socket listener;

    /// <summary>
    /// The HttpServer constructor.
    /// </summary>
    /// <param name="port">The port the server should connect through with its clients.</param>
    public HttpServer(int port)
    {
        this.port = port;
    }

    /// <summary>
    /// Initializes the server and starts listening for incoming connections.
    /// </summary>
    public void Start()
    {
        if (!running)
        {
            Initialize();
            Listen();
        }
    }

    /// <summary>
    /// Initializes the server creating a new Socket, binding it to the created localEndPoint and placing it in a listening state.
    /// </summary>
    void Initialize()
    {
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        ipAddress = ipHostInfo.AddressList[0];
        localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.
        listener.Bind(localEndPoint);
        listener.Listen(500);

        Console.WriteLine("Server IP Adress: " + ipAddress);
        Console.WriteLine("Server Port: " + port + "\n");
    }

    /// <summary>
    /// Creates a new ConnectionHandling instance when a new client establishes a connection.
    /// </summary>
    void Listen()
    {
        while (true)
        {
            if(DebugLevel <= 0)
            { 
                Console.WriteLine("\nHandled Connections: " + totalConnections + " Waiting for a connection...");
            }

            Socket handler = listener.Accept();

            ConnectionHandling client = new ConnectionHandling(handler, totalConnections);

            totalConnections++;
        }
    }
}
