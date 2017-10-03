using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class DataHandling
{
    public ConnectionHandling connection;
    string data;
    public uint threadID;
    public bool closeAfterResponse;

    public DataHandling(string data, ConnectionHandling connection, bool closeAfterResponse)
    {
        this.closeAfterResponse = closeAfterResponse;
        this.data = data;
        this.connection = connection;

        Thread thread = new Thread(HandleData);
        thread.IsBackground = false;
        thread.Start();

        threadID = connection.totalThreads;
    }

    void HandleData()
    {
        Request request = new Request(data, this);
        Response response = new Response(request);
        response.Post(connection.handler);

        connection.totalClosedThreats++;
    }
}
