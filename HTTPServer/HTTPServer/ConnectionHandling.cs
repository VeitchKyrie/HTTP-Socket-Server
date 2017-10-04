using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// Class that creates a new Thread that handles a connection and creates a new DataHandling instance for each new incoming data.
/// </summary>
public class ConnectionHandling
{
    /// <summary>
    /// The connected Socket
    /// </summary>
    public Socket handler;

    /// <summary>
    /// The connection ID
    /// </summary>
    public uint ID;

    /// <summary>
    /// The total amount of finished Threats created by the DataHandling instances.
    /// </summary>
    public int totalClosedThreats = 0;

    /// <summary>
    /// The total amount Threats created by the DataHandling instances.
    /// </summary>
    public uint totalThreads = 0;

    /// <summary>
    /// True if the client hasn't sent any valid incoming data for a determined period of time 
    /// and all Threads on all the DataHandling instances on this instance have finished.
    /// Prevents the creation of new DataHandling instances on this connection. 
    /// </summary>
    bool closing = false;

    /// <summary>
    /// True when the handleClient Thread confirms that it's not going to create any more DataHandle instances.
    /// </summary>
    public bool Closed {get; private set; }

    /// <summary>
    /// The client's cookie;
    /// </summary>
    public string Cookie = "";

    Thread handleClient;

    int unhandleddata = 0;

    /// <summary>
    /// The ConnectionHandling constructor.
    /// </summary>
    /// <param name="handler">The connected Socket</param>
    /// <param name="clientID">The ID of this instance</param>
    public ConnectionHandling(Socket handler, uint clientID)
    {
        this.handler = handler;
        ID = clientID;

        handleClient = new Thread(HandleClient);
        handleClient.IsBackground = false;
        handleClient.Start();

        Thread timeOut = new Thread(ConectionTimeoutCheck);
        timeOut.Start();
    }

    /// <summary>
    /// Manages all incoming data from the connection.
    /// When new data arrives a new DataHandling instance is created.
    /// </summary>
    void HandleClient()
    {
        Closed = false;

        try
        {
            while (!closing)
            {
                byte[] bytes = new byte[1024];

                int a = handler.Receive(bytes);
                string data = Encoding.UTF8.GetString(bytes, 0, a);
                int handled = -1;

                if (data != "")
                {
                    handled = 0;

                    if (!closing)
                    {
                        handled = 1;
                        totalThreads++;
                        DataHandling dataHandle = new DataHandling(data, this, closing);
                    }
                }

                if (handled == 0)
                    unhandleddata++;
            }
        }
        catch (Exception e)
        {
            if (handler != null)
                Console.WriteLine(e.Message);
        }

        Closed = true;
    }

    /// <summary>
    /// Thread that looks constantly if the connection with the client should be closed because it has't sent anything for a specific period of time.
    /// If so, it makes sure that no new data comes in from that connection and waits until all open threads are finished to close the Socket.
    /// </summary>
    /// <param name="state"></param>
    private void ConectionTimeoutCheck(object state)
    {
        do
        {
            Thread.Sleep(250);
        }
        while (totalThreads != totalClosedThreats);

        closing = true;

        int timer = 0;

        while (!Closed)
        {
            Thread.Sleep(5);

            // Since the handleClient Thread can be stuck at 
            // "int a = handler.Receive(bytes);"
            // this timer makes this method continue after some time if totalThreads is equal to totalClosedThreats even though closed is still false.
            if (totalThreads == totalClosedThreats)
            {
                timer += 5;
                if (timer > 100)
                {
                    Closed = true;
                    Console.WriteLine("ABORTED, " + unhandleddata + " data incomes have NOT been handled. Total threads: " + totalThreads + ", Finished Threads: " + totalClosedThreats);
                    handleClient.Abort();
                    break;
                }
            }
        }

        if(HttpServer.DebugLevel <= 2)
            Console.WriteLine("\nConnection timeout, closing Connection.\nConnection ID: " + ID + ", Total processed Threads: " + totalThreads);

        // Closes connection
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        handler = null;
    }
}

