using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Utilities
{
    public static class Helpers
    {
        [Deterministic]
        public static string DeserializeServiceBusMessage(Message message)
        {
            string deserializedBody;
            try
            {
                // TODO: this custom serialization nonsense is supposed to be fixed with an upcoming SB nuget package version
                XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(new MemoryStream(message.Body), null,
                    XmlDictionaryReaderQuotas.Max);
                var doc = new XmlDocument();
                doc.Load(reader);
                deserializedBody =
                    HttpUtility.JavaScriptStringEncode(doc.InnerText.Replace("\r\n", ""), false).Replace("\"", "'")
                        .Replace("\\", "");

            }
            catch (XmlException)
            {
                string converted = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

                deserializedBody =
                    HttpUtility.JavaScriptStringEncode(converted.Replace("\r\n", ""), false).Replace("\"", "'")
                        .Replace("\\", "");
            }

            return deserializedBody;
        }
    }
}