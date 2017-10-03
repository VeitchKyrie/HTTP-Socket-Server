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

    public DataHandling(string data, ConnectionHandling client)
    {
        this.data = data;
        this.connection = client;

        Thread thread = new Thread(HandleData);
        thread.IsBackground = false;
        thread.Start();

        client.openThreads++;
        client.totalThreads++;
        threadID = client.totalThreads;
    }

    void HandleData()
    {
        Request request = new Request(data, this);
        Response response = new Response(request);
        response.Post(connection.handler);

        connection.openThreads--;
    }
}
