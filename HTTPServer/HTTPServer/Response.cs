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
    /// The Responses status. (e.g. 200 OK)
    /// </summary>
    String status;

    /// <summary>
    /// The Responses mime. (e.g. text/html)
    /// </summary>
    String mime;

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
                Console.WriteLine(file);

                if (File.Exists(file))
                    Console.WriteLine("Specified file exists.");

                bytedata = Encoding.UTF8.GetBytes(File.ReadAllText(file));

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

        writer.WriteLine(String.Format("{0} {1}\r\nServer: {2}\r\nContent-Type: {3}\r\nAccept-Ranges: bytes\r\nContent-Length: {4}\r\n",
            "HTTP / 1.1", "200", "BetoServer", "text/html", bytedata.Length));

        writer.Flush();
        stream.Write(bytedata, 0, bytedata.Length);
        stream.Close();

        Console.WriteLine("Bytes sent to client.");
    }
}