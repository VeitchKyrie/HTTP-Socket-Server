using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Response
{
    /// <summary>
    /// The Request that lead to this Response.
    /// </summary>
    Request request;

    /// <summary>
    /// The bytes to be transmitted to the client.
    /// </summary>
    Byte[] bytedata;

    /// <summary>
    /// The Response status. (e.g. 200 OK)
    /// </summary>
    string status;

    /// <summary>
    /// The Response mime. (e.g. text/html)
    /// </summary>
    string mime;

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
        switch (request.Type)
        {
            case RequestType.GET:

                string file = Environment.CurrentDirectory + HttpServer.WEB_D + request.Url;
                Console.WriteLine("Requested File: " + file);

                mime = request.mimes[0];

                if (File.Exists(file))
                {
                    Console.WriteLine("Specified file exists.");
                    bytedata = File.ReadAllBytes(file);
                    status = "200";
                }
                else
                {
                    Console.WriteLine("Specified file doesn't exist, sending Error 404.");

                    file = Environment.CurrentDirectory + HttpServer.MSG_D + "/404.html";
                    bytedata = File.ReadAllBytes(file);
                    status = "404";
                }

                Console.WriteLine("Bytes gotten from file.");
                break;
        }
    }
    
    /// <summary>
    /// Sends the bytedata to the client.
    /// </summary>
    /// <param name="handler">The Socket in which the data will be sent through.</param>
    public void Post(Socket handler)
    {
        if (request.Type == RequestType.UNDEFINED)
            return;

        NetworkStream stream = new NetworkStream(handler);
        StreamWriter writer = new StreamWriter(stream);

        string Header = String.Format("{0} {1}\r\nServer: {2}\r\nContent-Type: {3}\r\nAccept-Ranges: bytes\r\nContent-Length: {4}\r\nSet-Cookie: HasVisited = 1\r\n",
            HttpServer.VERSION, status, HttpServer.SERVER_NAME, mime, bytedata.Length);

        writer.WriteLine(Header);

        writer.Flush();
        stream.Write(bytedata, 0, bytedata.Length);
        stream.Close();

        Console.WriteLine("\nRESPONSE:\n" + Header + "\nBytes sent to client.");
        Console.WriteLine("\n________________________________________\n\n");
    }
}