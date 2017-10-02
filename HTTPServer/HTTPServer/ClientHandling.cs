using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class ClientHandling
{
    string data;
    Socket handler;
    uint ID;

    public ClientHandling(string data, Socket handler, uint ID)
    {
        this.data = data;
        this.handler = handler;
        this.ID = ID;

        Thread thread = new Thread(HandleClient);
        thread.IsBackground = false;
        thread.Start();
    }

    void HandleClient()
    {
        Request request = new Request(data);
        Response response = new Response(request);
        response.Post(handler, ID);

        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        handler = null;
    }
}
