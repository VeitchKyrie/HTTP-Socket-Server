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

    public int totalClosedThreats = 0;
    public uint totalThreads = 0;

    bool closing = false;
    bool closed = false;


    public ConnectionHandling(Socket handler, uint clientID)
    {
        this.handler = handler;
        ID = clientID;

        Thread handleClient = new Thread(HandleClient);
        handleClient.IsBackground = false;
        handleClient.Start();

        Thread timeOut = new Thread(ConectionTimeoutCheck);
        timeOut.Start();
    }

    void HandleClient()
    {
        try
        {
            while (!closing)
            {
                byte[] bytes = new byte[1024];

                int a = handler.Receive(bytes);
                string data = Encoding.UTF8.GetString(bytes, 0, a);

                if (data != "")
                {
                    totalThreads++;
                    DataHandling dataHandle = new DataHandling(data, this, closing);
                }
            }
        }
        catch (Exception e)
        {
            if (handler != null)
                Console.WriteLine(e.Message);
        }

        closed = true;
    }

    private void ConectionTimeoutCheck(object state)
    {
        do
        {
            Thread.Sleep(250);
        }
        while (totalThreads != totalClosedThreats);

        closing = true;

        int timer = 0;

        while (!closed)
        {
            Thread.Sleep(5);

            if (totalThreads == totalClosedThreats)
            {
                timer += 5;
                if (timer > 100)
                    break;
            }
        }

        Console.WriteLine("\nConnection timeout, closing Connection.\nConnection ID: " + ID + ", Total processed Threads: " + totalThreads);

        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        handler = null;
    }
}

