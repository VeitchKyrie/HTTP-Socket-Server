using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class ConnectionHandling
{
    public Socket handler;

    public uint ID;

    public int openThreads = 0;
    public uint totalThreads = 0;

    public ConnectionHandling(Socket handler, uint clientID)
    {
        this.handler = handler;
        ID = clientID;

        Thread handleClient = new Thread(HandleClient);
        handleClient.IsBackground = false;
        handleClient.Start();

        Thread timeOut = new Thread(ConectionTimeoutCallback);
        timeOut.Start();
    }

    void HandleClient()
    {
        Console.WriteLine("\n\n\n\n\n CONNECTION STARTED" + ID);

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

    private void ConectionTimeoutCallback(object state)
    {
        Thread.Sleep(500);

        while (openThreads != 0)
            Thread.Sleep(200);        

        Console.WriteLine("Connection timeout, closing Connection.\nConnection ID: " + ID + ", Total processed Threads: " + totalThreads + "\n");

        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        handler = null;
    }
}

