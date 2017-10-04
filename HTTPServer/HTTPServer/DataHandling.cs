using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// Class that creates a new Thread that handles incoming data. Doesn't have an endless loop, finishes after the first incoming data has been dealt with.
/// </summary>
public class DataHandling
{
    /// <summary>
    /// The ConnectionHandling instance that created this instance.
    /// </summary>
    public ConnectionHandling connection;

    /// <summary>
    /// The incoming data.
    /// </summary>
    string data;

    /// <summary>
    /// The ID of this DataHandling instance in its ConnectionHandling instance.
    /// (= If it's the first instance of its ConnectionHandling instance its 1, if second 2, etc)
    /// </summary>
    public uint threadID;

    /// <summary>
    /// If the server should tell the client to close the connection after the following response or not.
    /// </summary>
    public bool closeAfterResponse;

    /// <summary>
    /// Constructor of the DataHandling class. Saves the parameters and starts a new Thread.
    /// </summary>
    /// <param name="data">The incoming data.</param>
    /// <param name="connection">The ConnectionHandling instance that is creating this instance.</param>
    /// <param name="closeAfterResponse">If the server should tell the client to close the connection after the following response or not.</param>
    public DataHandling(string data, ConnectionHandling connection, bool closeAfterResponse)
    {
        this.closeAfterResponse = closeAfterResponse;
        this.data = data;
        this.connection = connection;

        Thread handleData = new Thread(HandleData);
        handleData.IsBackground = false;
        handleData.Start();

        threadID = connection.totalThreads;
    }

    /// <summary>
    /// Creates a new Request instance to analyze the incoming data.
    /// The Response instance uses the information gotten by the Request instance to generate an answer.
    /// Then the answer is sent to the client by the Response's Post method.
    /// </summary>
    void HandleData()
    {
        Request request = new Request(data, this);
        Response response = new Response(request);
        response.Post(connection.handler);

        connection.totalClosedThreats++;
    }
}
