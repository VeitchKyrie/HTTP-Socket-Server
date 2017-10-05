using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    byte[] bytedata;

    /// <summary>
    /// The Response status. (e.g. 200 OK)
    /// </summary>
    string status;

    /// <summary>
    /// The Response mime. (e.g. text/html)
    /// </summary>
    string mime;

    /// <summary>
    /// The Servers connection answer (either "close" or "keep-alive"). 
    /// Tells the client if it should close the connection after receiving the response or not.
    /// </summary>
    string connection;

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
            switch (request.Type)
            {
                case RequestType.GET:

                    string file = Environment.CurrentDirectory + HttpServer.WEB_D + request.Url;
                    if (HttpServer.DebugLevel <= 1)
                        Console.WriteLine("Requested File: " + file);

                    mime = request.Mimes[0];

                    if (request.dataHandler.closeAfterResponse)
                        connection = "close";
                    else
                        connection = "keep-alive";

                    if (File.Exists(file))
                    {
                        if (HttpServer.DebugLevel <= 1)
                            Console.WriteLine("Specified file exists.");
                        bytedata = File.ReadAllBytes(file);
                        status = "200";
                    }
                    else
                    {
                        if (request.Url.Contains("cgi-bin"))
                        {
                            Console.WriteLine("Running CGI Script.");
                            GetCGIResult();
                            return;
                        }

                        Console.WriteLine("Specified file doesn't exist, sending Error 404. Requested File: " + request.Url);

                        file = Environment.CurrentDirectory + HttpServer.MSG_D + "/404.html";
                        bytedata = File.ReadAllBytes(file);
                        status = "404";
                    }

                    if (HttpServer.DebugLevel <= 1)
                        Console.WriteLine("Bytes gotten from file.");
                    break;
            }
        }
        catch(Exception e)
        {
            SendInternErrorPage(e);
        }
    }

    /// <summary>
    /// Starts the CGI.exe Process and attaches the GetGeneratedFileData Method to the Process's Exited EventHandler.
    /// The CGI.exe Process creates a new .html file with custom input.
    /// </summary>
    void GetCGIResult()
    {
        string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\cgi-bin\Build\";
        string argument = request.Url;

        argument = argument.Replace("/", "");
        argument = argument.Replace(@"\", "");

        if (argument.StartsWith("cgi-bin"))
            argument = argument.Substring(7);

        Process createNewFile = new Process();
        createNewFile.StartInfo.FileName = path + "CGI.exe";
        createNewFile.StartInfo.Arguments = argument;
        createNewFile.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        createNewFile.EnableRaisingEvents = true;
        createNewFile.Exited += new EventHandler(GetGeneratedFileData);
        createNewFile.Start();

        while(bytedata == null)
            Thread.Sleep(10);
    }

    /// <summary>
    /// Is called when the CGI.exe process is terminated. Gets the bytes of the created .html file by the CGI.exe process and deletes it afterwards.
    /// </summary>
    void GetGeneratedFileData(object sender, EventArgs ev)
    {
        try
        {
            if(HttpServer.DebugLevel <= 0)
                Console.WriteLine("CGI Process terminated");

            string file = Environment.CurrentDirectory + HttpServer.WEB_D + request.Url;

            if (!file.EndsWith(".html"))
                file += ".html";

            status = "200";
            bytedata = File.ReadAllBytes(file);

            File.Delete(file);
        }
        catch(Exception e)
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
        Console.WriteLine("ERROR, sending code 500.\n" + e.Message);

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

                string Header = String.Format("{0} {1}\r\nServer: {2}\r\nConnection: {3}\r\nContent-Type: {4}\r\nAccept-Ranges: bytes\r\nContent-Length: {5}\r\n",
                    HttpServer.VERSION, status, HttpServer.SERVER_NAME, connection, mime, bytedata.Length);

                if (request.Cookie == "")
                {
                    if (request.dataHandler.connection.Cookie == "")
                    {
                        request.dataHandler.connection.Cookie = GenerateUniqueClientCookie();
                    }

                    Header += "Set - Cookie: " + request.dataHandler.connection.Cookie + "\r\n";
                }

                writer.WriteLine(Header);

                writer.Flush();
                stream.Write(bytedata, 0, bytedata.Length);
                stream.Close();

                if (HttpServer.DebugLevel <= 1)
                {
                    Console.WriteLine("\nConnection ID: " + request.dataHandler.connection.ID + ", Thread ID:" + request.dataHandler.threadID + "\nRESPONSE:\n" + Header + "\nBytes sent to client.");
                    Console.WriteLine("________________________________________________________________");
                }

                request.dataHandler.connection.totalClosedThreats++;
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
            Console.WriteLine("Total threads on this Connection: " + request.dataHandler.connection.totalThreads);
            Console.WriteLine("Total closed threads on this Connection: " + request.dataHandler.connection.totalClosedThreats);
            Console.WriteLine("Requested file: " + request.Url);
            Console.WriteLine("Message: " + e.Message);

            Console.WriteLine("\n________________________________________________________________");
            Console.WriteLine("________________________________________________________________");
            Console.WriteLine("________________________________________________________________\n");
        }
    }

    public string GenerateUniqueClientCookie()
    {
        Random random = new Random();
        return ((float)random.NextDouble()).ToString();
    }
}