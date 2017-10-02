using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGI
{
    public class CGIScript
    {
        public CGIScript(string[] input)
        {
            string path = Environment.CurrentDirectory + @"\root\web\cgi-bin";

            string inputWithSpace = input[0].Replace(@"%20", " ");
            string content = File.ReadAllText(path + @"\Resources\Prefab.html").Replace("#", inputWithSpace);

            int indexOfSeparator = content.IndexOf('#');

            FileStream stream = File.Create(path + @"\" + input[0] + ".html");
            byte[] bytebuffer = Encoding.UTF8.GetBytes(content);

            stream.Write(bytebuffer, 0, bytebuffer.Length);
            stream.Flush();
            stream.Close();
        }
    }
}
