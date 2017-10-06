using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public class RestApi
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

            User[] users = new User[0];
            url = url.Replace("/api/users", "");
            url = url.Replace("/api/user", "");
            XmlNode node;

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
            else if (!url.Substring(1).Contains(@"\") && !url.Contains("delete") && !url.Contains("update"))
            {
                node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + url.Substring(1) + "\"]");

                string datatoadd = node.InnerXml;
                datatoadd = datatoadd.Replace("<name>", "Name: \"");
                datatoadd = datatoadd.Replace(@"</name>", "\" <br/>");
                datatoadd = datatoadd.Replace("<username>", "User name: \"");
                datatoadd = datatoadd.Replace(@"</username>", "\" <br/>");
                datatoadd = datatoadd.Replace("<birthdate>", "Birthdate: \"");
                datatoadd = datatoadd.Replace(@"</birthdate>", "\"<br/>");
                data += datatoadd;
            }
            else if (!url.Substring(1).Contains(@"\") && (url.Contains("/delete") || url.Contains(@"\delete")))
            {
                return HandleXmlRemove(request.Url);
            }
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
        string[] words = content.Split('*');
        string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";

        User newUser = new User();
        newUser.username = words[1];
        newUser.name = words[3];
        newUser.birthdate = words[5];

        XDocument doc;
        if (!File.Exists(path))
        {
            doc = new XDocument(
                new XElement("users",
                    new XElement("user",
                        new XElement("username", newUser.username),
                        new XElement("name", newUser.name),
                        new XElement("birthdate", newUser.birthdate))));
            doc.Save(path);
        }
        else
        {
            doc = XDocument.Load(path);

            doc.Root.Add(
                    new XElement("user",
                        new XElement("username", newUser.username),
                        new XElement("name", newUser.name),
                        new XElement("birthdate", newUser.birthdate)));

            doc.Save(path);
        }
        return true;
    }

    /// <summary>
    /// Used to generate a response for a "DELETE" request.
    /// Deletes a user from the database if it exists.
    /// </summary>
    public static XmlContent HandleXmlRemove(string url)
    {
        XmlContent result = new XmlContent();

        try
        {
            string path = Environment.CurrentDirectory + HttpServer.WEB_D + @"\Aufgabe8\database.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            url = url.Replace("/api/users", "");
            url = url.Replace("/api/user", "");
            url = url.Replace("/delete", "");
            XmlNode node;

            if (!url.Substring(1).Contains(@"\"))
            {
                node = doc.DocumentElement.SelectSingleNode("//user[username=\"" + url.Substring(1) + "\"]");
                node.RemoveAll();
                node.ParentNode.RemoveChild(node);
                doc.Save(path);
            }

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
        Console.WriteLine(user);

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
}

public class XmlContent
{
    public string Status;
    public byte[] ByteData;
}

public class User
{
    public string name;
    public string username;
    public string birthdate;
}
