using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace WpfApp1
{
    public class MySettings
    {
        public string client { get; set; }
        public string clientHost { get; set; }
        public string clientPort { get; set; }
        public List<string> clientList { get; set; }

        public void Save(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(MySettings));
                xmls.Serialize(sw, this);
            }
        }
        public MySettings Read(string filename)
        {
            using (StreamReader sw = new StreamReader(filename))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(MySettings));
                return xmls.Deserialize(sw) as MySettings;
            }
        }
    }
}
