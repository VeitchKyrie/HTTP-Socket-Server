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
    public static XmlContent HandleXmlGet(string urlinput)
    {
        string url = urlinput;

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
            else if (!url.Substring(1).Contains(@"\") && !url.Contains("delete"))
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
            else if (!url.Substring(1).Contains(@"\") && url.Contains("/delete") || url.Contains(@"\delete"))
            {
                return HandleXmlRemove(urlinput);
            }

            data += footer;
            XmlContent result = new XmlContent();
            result.ByteData = Encoding.UTF8.GetBytes(data);
            result.Status = "200";

            return result;
        }
        catch
        {
            XmlContent result = new XmlContent();
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
    /// Deletes a user from the database if i exists.
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
