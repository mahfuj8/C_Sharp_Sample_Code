using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace C_SharpExample
{
    public class XMLFileWrite
    {



        /* 
     -------------Method Output output---------------
       <users>
       <user age = "42" > John Doe</user>
       <user age = "39" > Jane Doe</user>
       </users>

       */

        public void DataToXmlFileWrite()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("users");
            xmlDoc.AppendChild(rootNode);

            XmlNode userNode = xmlDoc.CreateElement("user");
            XmlAttribute attribute = xmlDoc.CreateAttribute("age");
            attribute.Value = "42";
            userNode.Attributes.Append(attribute);
            userNode.InnerText = "John Doe";

            //add root node
            rootNode.AppendChild(userNode);

            userNode = xmlDoc.CreateElement("user");
            attribute = xmlDoc.CreateAttribute("age");
            attribute.Value = "39";
            userNode.Attributes.Append(attribute);
            userNode.InnerText = "Jane Doe";

            //add root node
            rootNode.AppendChild(userNode);

            xmlDoc.Save(@"G:\xamarin code\test-doc.xml");

        }
    }
}
