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
    /// The Thread that handles the Connection.
    /// </summary>
    Thread handleConnection;

    /// <summary>
    /// The ConnectionHandling constructor.
    /// </summary>
    /// <param name="handler">The connected Socket</param>
    /// <param name="clientID">The ID of this instance</param>
    public ConnectionHandling(Socket handler, uint clientID)
    {
        this.handler = handler;
        ID = clientID;

        handleConnection = new Thread(HandleConnection);
        handleConnection.IsBackground = false;
        handleConnection.Start();
    }

    /// <summary>
    /// Manages all incoming data from the connection.
    /// When new data arrives a new DataHandling instance is created.
    /// </summary>
    void HandleConnection()
    {
        try
        {
            while (true)
            {
                byte[] bytes = new byte[1024];

                int a = handler.Receive(bytes);
                string data = Encoding.UTF8.GetString(bytes, 0, a);

                if (data != "")
                {
                    DataHandling dataHandle = new DataHandling(data, this);
                }
            }
        }
        catch (Exception e)
        {
            if (handler != null)
                Console.WriteLine(e.Message);
        }
    }
}

