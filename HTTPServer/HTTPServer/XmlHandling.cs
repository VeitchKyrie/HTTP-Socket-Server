using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

/// <summary>
/// Class that makes the server's REST Api work. 
/// GET, POST, PUT and DELETE Requests are managed modifying the database accordingly.
/// </summary>
public static class XmlHandler
{
    const string tab = "&nbsp&nbsp&nbsp&nbsp";

    /// <summary>
    /// Used to generate a response for a "GET" request.
    /// </summary>
    public static XmlContent HandleXmlGet(Request request)
    {
        XmlContent result;

        try
        {
            string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode node = doc.DocumentElement.SelectSingleNode(@"/users");

            string data = node.InnerXml;

            data = data.Replace("<user>", "-User-<br/>");
            data = data.Replace("</user>", "<br/>");

            // Format XML data to an easyly-readable form and react to the specified filter.

            if (!request.Url.Contains("/name") && !request.Url.Contains("/birthdate"))
            {
                data = data.Replace("<username>", "User name: \"");
                data = data.Replace(@"</username>", "\" <br/>");
            }
            else
                HideInfo("username", ref data);

            if (!request.Url.Contains("/username") && !request.Url.Contains("/birthdate"))
            {
                data = data.Replace("<name>", "Name: \"");
                data = data.Replace(@"</name>", "\" <br/>");
            }
            else
                HideInfo("name", ref data);

            if (!request.Url.Contains("/username") && !request.Url.Contains("/name"))
            {
                data = data.Replace("<birthdate>", "Birthdate: \"");
                data = data.Replace(@"</birthdate>", "\"<br/>");
            }
            else
                HideInfo("birthdate", ref data);

            // Return data converted into bytes and the status.

            result = new XmlContent();
            result.ByteData = Encoding.UTF8.GetBytes(data);
            result.Status = "200";

            return result;
        }
        catch
        {
            result = new XmlContent();
            result.ByteData = Encoding.UTF8.GetBytes("Error: The database or the specified attribute doesn't exist yet, please specify existing attributes or create new users.");
            result.Status = "200";
            return result;
        }
    }

    /// <summary>
    /// Used to generate a response for a "POST" request.
    /// Addes a new user to the database or creates it if it doesn't exist.
    /// </summary>
    public static XmlContent HandleXmlPost(string url, string content)
    {
        XmlContent result = new XmlContent();

        string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";
        string[] words = content.Split('*');

        string username = words[1];
        string name = words[3];
        string birthdate = words[5];

        XDocument doc;
        if (!File.Exists(path))
        {
            doc = new XDocument(
                new XElement("users",
                    new XElement("user",
                        new XElement("username", username),
                        new XElement("name", name),
                        new XElement("birthdate", birthdate))));
        }
        else
        {
            doc = XDocument.Load(path);

            doc.Root.Add(
                    new XElement("user",
                        new XElement("username", username),
                        new XElement("name", name),
                        new XElement("birthdate", birthdate)));
        }
        doc.Save(path);

        result.Status = "200";
        result.ByteData = Encoding.UTF8.GetBytes("User has been created succesfully.");
        return result;
    }

    /// <summary>
    /// Used to generate a response for a "DELETE" request.
    /// Deletes a user from the database if it exists.
    /// </summary>
    public static XmlContent HandleXmlRemove(Request request)
    {
        XmlContent result = new XmlContent();

        try
        {
            string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            string[] contentwords = request.Content.Split('*');
            string user = contentwords[1];

            XmlNode node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + user + "\"]");

            node.RemoveAll();
            node.ParentNode.RemoveChild(node);
            doc.Save(path);

            result.ByteData = Encoding.UTF8.GetBytes("User has been removed succesfully.");
            result.Status = "200";
        }
        catch
        {
            result.ByteData = Encoding.UTF8.GetBytes("Error on removing the specified user. Are you sure it exists?");
            result.Status = "404";
        }
        return result;
    }

    /// <summary>
    /// Used to generate a response for a "PUT" request.
    /// Updates a user in the database if it exists.
    /// </summary>
    public static XmlContent HandleXmlPut(Request request)
    {
        XmlContent result = new XmlContent();
        string[] contentwords = request.Content.Split('*');
        string user = contentwords[1];

        string[] newData = new string[contentwords.Length - 2];

        for (int x = 2; x < contentwords.Length; x++)
            newData[x - 2] = contentwords[x];

        try
        {
            string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + user + "\"]");
            XmlNodeList nodes = node.ChildNodes;

            int index = 1;
            for (int x = 0; x < nodes.Count; x++)
            {
                nodes.Item(x).InnerText = newData[index];
                Console.WriteLine(nodes.Item(x).Name + ": old:" + newData[index] + ", new: " + nodes.Item(x).InnerText);
                index += 2;
            }

            doc.Save(path);
            
            result.ByteData = Encoding.UTF8.GetBytes("User has been updated succesfully.");
            result.Status = "200";
        }
        catch
        {
            result.ByteData = Encoding.UTF8.GetBytes("Error on updating the specified user. Are you sure it exists?");
            result.Status = "404";
        }
        return result;
    }

    /// <summary>
    /// Hides a Xml node completely from a string.
    /// </summary>
    /// <param name="info">The name of the node to be hidden</param>
    /// <param name="data">The string where the node will be hidden</param>
    static void HideInfo(string info, ref string data)
    {
        data = data.Replace("<" + info + ">", "§");
        data = data.Replace(@"</" + info + ">", "§");

        string[] substrings = data.Split('§');

        data = "";

        for(int x = 0; x < substrings.Length; x += 2)
        {
            data += substrings[x];
        }
    }
}

/// <summary>
/// Class that is returned in all methods in the XmlHandler class. Specifies the http status and the bytes to be sent to the client as a response.
/// </summary>
public class XmlContent
{
    public string Status;
    public byte[] ByteData;
}