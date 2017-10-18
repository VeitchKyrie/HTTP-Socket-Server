using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


/// <summary>
/// Class that uses the gotten data from the Request class to generate and send a response to the client.
/// </summary>
public class Response
{
    /// <summary>
    /// The Request that lead to this Response.
    /// </summary>
    Request request;

    /// <summary>
    /// The bytes to be transmitted to the client.
    /// </summary>
    public byte[] bytedata { get; private set; }

    /// <summary>
    /// The Response status. (e.g. 200 OK)
    /// </summary>
    public string status { get; private set; }

    /// <summary>
    /// The Response mime. (e.g. text/html)
    /// </summary>
    public string mime { get; private set; }

    /// <summary>
    /// Response constructor.
    /// </summary>
    public Response(Request request)
    {
        this.request = request;
        ProcessRequest();
    }

    /// <summary>
    /// Generates the bytes to be sent to the client.
    /// </summary>
    void ProcessRequest()
    {
        try
        {
            mime = request.Mimes[0];

            switch (request.Type)
            {
                case RequestType.GET:
                    string file = Environment.CurrentDirectory + HttpServer.WEB_D + request.Url;

                    Console.WriteLine("Requested File: " + file);

                    if (File.Exists(file))
                    {
                        Console.WriteLine("Specified file exists.");
                        bytedata = File.ReadAllBytes(file);
                        status = "200";
                    }
                    else
                    {
                        Console.WriteLine("Specified file doesn't exist, sending Error 404. Requested File: " + request.Url);

                        file = Environment.CurrentDirectory + HttpServer.MSG_D + "/404.html";
                        bytedata = File.ReadAllBytes(file);
                        status = "404";
                    }

                    Console.WriteLine("Bytes gotten from file.");
                    break;
            }
        }
        catch (Exception e)
        {
            SendInternErrorPage(e);
        }
    }
    /// <summary>
    /// Sends the error code 500 to the client with the "500.html" file
    /// </summary>
    /// <param name="e"></param>
    void SendInternErrorPage(Exception e)
    {
        if(e != null)
            Console.WriteLine("ERROR, sending code 500.\n" + e.Message);
        else
            Console.WriteLine("ERROR, sending code 500.\n");

        string file = Environment.CurrentDirectory + HttpServer.MSG_D + "/500.html";
        bytedata = File.ReadAllBytes(file);
        status = "500";
    }

    /// <summary>
    /// Sends the bytedata to the client.
    /// </summary>
    /// <param name="handler">The Socket in which the data will be sent through.</param>
    public void Post(Socket handler)
    {
        try
        {
            if (request.Type != RequestType.UNDEFINED)
            {
                NetworkStream stream = new NetworkStream(handler);
                StreamWriter writer = new StreamWriter(stream);

                string Header = String.Format("{0} {1}\r\nServer: {2}\r\nContent-Type: {3}\r\nAccept-Ranges: bytes\r\nContent-Length: {4}\r\n",
                    HttpServer.VERSION, status, HttpServer.SERVER_NAME, mime, bytedata.Length);

                writer.WriteLine(Header);

                writer.Flush();
                if(bytedata != null)
                    stream.Write(bytedata, 0, bytedata.Length);
                stream.Close();

                Console.WriteLine("\nConnection ID: " + request.dataHandler.connection.ID + ", Thread ID:" + request.dataHandler.threadID + "\nRESPONSE:\n" + Header + "\nBytes sent to client.");
                Console.WriteLine("________________________________________________________________");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("________________________________________________________________");
            Console.WriteLine("________________________________________________________________");
            Console.WriteLine("________________________________________________________________\n\n");

            Console.WriteLine("-FATAL ERROR-");
            Console.WriteLine("Connection ID: " + request.dataHandler.connection.ID);
            Console.WriteLine("Thread ID: " + request.dataHandler.threadID);
            Console.WriteLine("Requested file: " + request.Url);
            Console.WriteLine("Message: " + e.Message);

            Console.WriteLine("\n________________________________________________________________");
            Console.WriteLine("________________________________________________________________");
            Console.WriteLine("________________________________________________________________\n");
        }
    }
}