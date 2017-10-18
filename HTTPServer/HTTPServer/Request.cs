using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class that analyzes the incoming data.
/// </summary>
public class Request
{
    /// <summary>
    /// The Request Type.
    /// </summary>
    public RequestType Type;

    /// <summary>
    /// The requested Url.
    /// </summary>
    public string Url;

    /// <summary>
    /// The Request mimes. (e.g. text/html, image/*)
    /// </summary>
    public string[] Mimes;

    /// <summary>
    /// The DataHandling instance that created this instace.
    /// </summary>
    public DataHandling dataHandler;

    /// <summary>
    /// The request's content. Can be empty.
    /// </summary>
    public string Content;

    /// <summary>
    /// Request Constructor.
    /// </summary>
    /// <param name="data">The incomed data.</param>
    public Request (string data, DataHandling dataHandling)
    {
        this.dataHandler = dataHandling;
        ProcessWords(GetWords(data));
    }

    /// <summary>
    /// Divides a single string into several substrings. Separators are '\n' and ' '.
    /// </summary>
    /// <param name="data">The single string to be split.</param>
    /// <returns>data split by '\n' and ' '</returns>
    string[] GetWords(string data)
    {
        string[] lines = data.Split('\n');
        string[] words = new string[0];

        foreach (string line in lines)
        {
            string[] wordsInLine = line.Split(' ');
            string[] joinedArrays = new string[words.Length + wordsInLine.Length];

            for (int x = 0; x < words.Length; x++)
                joinedArrays[x] = words[x];

            for (int x = words.Length; x < joinedArrays.Length; x++)
                joinedArrays[x] = wordsInLine[x - words.Length];

            words = joinedArrays;
        }

        Console.WriteLine("\nConnection ID: " + dataHandler.connection.ID + ", Thread ID: " + dataHandler.threadID + "\nREQUEST: ");
        Console.WriteLine(data);
        Console.WriteLine("\n________________________________________________________________");

        string[] split = data.Split('\n');
        Content = split[split.Length - 1];
        return words;
    }

    /// <summary>
    /// Gets the Request Type, URL, mimes and the host from a string array.
    /// </summary>
    /// <param name="words">The words to be processed. Input to this method is the output of the GenerateWords method.</param>
    void ProcessWords(string[] words)
    {
        switch (words[0])
        {
            case "GET":
                Type = RequestType.GET;
                break;

            case "POST":
                Type = RequestType.POST;
                break;

            case "PUT":
                Type = RequestType.PUT;
                break;

            case "DELETE":
                Type = RequestType.DELETE;
                break;

            default:
                Type = RequestType.UNDEFINED;
                return;
        }

        Url = words[1];

        if (Url == "/")
            Url = "/index.html";

        int mimesIndex = GetSpecificIndex("Accept:", words);
        Mimes = words[mimesIndex + 1].Split(',');
    }

    /// <summary>
    /// Searches for a specific string in a string array and returns its index.
    /// </summary>
    /// <param name="info">The string to be found.</param>
    /// <param name="words">The string array in which to look for.</param>
    /// <returns></returns>
    int GetSpecificIndex(string info, string[] words)
    {
        for (int x = 0; x < words.Length; x++)
        {
            if (words[x] == info)
                return x;
        }
        return -1;
    }
}

/// <summary>
/// The Request type enum of a Request. It's normally GET, but whatever.
/// </summary>
public enum RequestType
{
    GET,
    POST,
    PUT,
    DELETE,
    //LINK,
    //UNLINK,
    //TRACE,
    //OPTIONS,
    UNDEFINED
}
