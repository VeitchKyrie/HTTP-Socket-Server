using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public static class XmlHandler
{
    const string header = "<!DOCTYPE html><html><body> ";
    const string footer = "</body></html>";
    const string tab = "&nbsp&nbsp&nbsp&nbsp";

    /// <summary>
    /// Used to generate a response for a "GET" request.
    /// </summary>
    public static XmlContent HandleXmlGet(Request request)
    {
        XmlContent result;
        string url = request.Url;

        try
        {
            string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";
            string data = header;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            url = url.Replace("/api/users", "");
            url = url.Replace("/api/user", "");
            XmlNode node;

            // Is the url "/api/user"?
            if (url == "")
            {
                node = doc.DocumentElement.SelectSingleNode(@"/users");

                string datatoadd = node.InnerXml;
                datatoadd = datatoadd.Replace("<user>", "User: <br/>");
                datatoadd = datatoadd.Replace(@"</user>", "<br/>");
                datatoadd = datatoadd.Replace("<name>", tab + "Name: \"");
                datatoadd = datatoadd.Replace(@"</name>", "\" <br/>");
                datatoadd = datatoadd.Replace("<username>", tab + "User name: \"");
                datatoadd = datatoadd.Replace(@"</username>", "\" <br/>");
                datatoadd = datatoadd.Replace("<birthdate>", tab + "Birthdate: \"");
                datatoadd = datatoadd.Replace(@"</birthdate>", "\"<br/>");
                data += datatoadd;
            }

            // Is the url "/api/user/[username]"?
            else if (!url.Substring(1).Contains(@"\") && !url.Contains("delete") && !url.Contains("update"))
            {
                string[] urlwords = url.Split('?');
                string user = urlwords[0].Substring(1);
                string[] tokens = new string[0];

                node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + user + "\"]");
                string datatoadd = node.InnerXml;

                if (urlwords.Length > 1)
                {
                    tokens = urlwords[1].Split('&');
                    Console.WriteLine(urlwords[1]);
                }

                // Data is converted from xml style to a more readable form. If filter is on inforamtion is hidden accordingly.

                if (urlwords.Length < 2 || CheckIfStringIsIncluded("username", tokens))
                {
                    datatoadd = datatoadd.Replace("<username>", "User name: \"");
                    datatoadd = datatoadd.Replace(@"</username>", "\" <br/>");
                }
                else
                    HideInfo("username", ref datatoadd);

                if (urlwords.Length < 2 || CheckIfStringIsIncluded("name", tokens))
                {
                    datatoadd = datatoadd.Replace("<name>", "Name: \"");
                    datatoadd = datatoadd.Replace(@"</name>", "\" <br/>");
                }
                else
                    HideInfo("name", ref datatoadd);

                if (urlwords.Length < 2 || CheckIfStringIsIncluded("birthdate", tokens))
                {
                    datatoadd = datatoadd.Replace("<birthdate>", "Birthdate: \"");
                    datatoadd = datatoadd.Replace(@"</birthdate>", "\"<br/>");
                }
                else
                     HideInfo("birthdate", ref datatoadd);

                data += datatoadd;
            }

            // Is the url "/api/user/[username]/update"?
            else if (!url.Substring(1).Contains(@"\") && (url.Contains("/update") || url.Contains(@"\update")))
            {
                string[] refererwords = request.Url.Split('/');
                string user = refererwords[refererwords.Length - 2];

                node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + user + "\"]");
                if (node != null)
                {
                    result = new XmlContent();
                    result.ByteData = File.ReadAllBytes(Environment.CurrentDirectory + HttpServer.WEB_D + @"/Aufgabe8/updateuser.html");
                    result.Status = "200";
                }
                else
                {
                    result = new XmlContent();
                    result.ByteData = Encoding.UTF8.GetBytes("The specified user doesn't exist, therefore it cannot be updated.");
                    result.Status = "404";
                }
                return result;
            }

            // Is the url "/api/user/[username]/delete"?
            else if (!url.Substring(1).Contains(@"\") && (url.Contains("/delete") || url.Contains(@"\delete")))
            {
                string[] refererwords = request.Url.Split('/');
                string user = refererwords[refererwords.Length - 2];

                node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + user + "\"]");
                if (node != null)
                {
                    result = new XmlContent();
                    result.ByteData = File.ReadAllBytes(Environment.CurrentDirectory + HttpServer.WEB_D + @"/Aufgabe8/deleteuser.html");
                    result.Status = "200";
                }
                else
                {
                    result = new XmlContent();
                    result.ByteData = Encoding.UTF8.GetBytes("The specified user doesn't exist, therefore it cannot be deleted.");
                    result.Status = "404";
                }
                return result;
            }

            data += footer;
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
    public static bool HandleXmlPost(string url, string content)
    {
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
        return true;
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
            string[] refererwords = request.Referer.Split('/');

            string user = refererwords[refererwords.Length - 2];
            Console.WriteLine(user);
            XmlNode node;

            node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + user + "\"]");
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
        string[] refererwords = request.Referer.Split('/');

        string user = refererwords[refererwords.Length - 2];

        try
        {
            string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode node;

            node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + user + "\"]");
            XmlNodeList nodes = node.ChildNodes;

            Console.WriteLine(node);

            int index = 1;
            for (int x = 0; x < nodes.Count; x++)
            {
                nodes.Item(x).InnerText = contentwords[index];
                Console.WriteLine(nodes.Item(x).Name + ": old:" + contentwords[index] + ", new: " + nodes.Item(x).InnerText);
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

    static bool CheckIfStringIsIncluded(string token, string[] tokens)
    {
        foreach(string element in tokens)
        {
            if (element == token)
                return true;
        }
        return false;
    }

    static void HideInfo(string info, ref string data)
    {
        data = data.Replace("<" + info + ">", "§");
        data = data.Replace(@"</" + info + ">", "§");

        string[] substrings = data.Split('§');
        data = substrings[0] + substrings[2];
    }
}

/// <summary>
/// Class that is returned in some methods in the XmlHandler class. Specifies the http status and the bytes to be sent to the client as a response.
/// </summary>
public class XmlContent
{
    public string Status;
    public byte[] ByteData;
}