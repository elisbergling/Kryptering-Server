using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace Kryptering_Server
{
    class Program
    {
        static TcpListener? tcpListener;

        // =======================================================================
        // Main(), lyssnar efter trafik. Loopar till dess att ctrl-C trycks. I
        // varje varv i loopen väntar servern på en ny anslutning.
        // =======================================================================
        public static void Main()
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            XmlElement messages = xmlDoc.CreateElement("messages");
            xmlDoc.AppendChild(messages);

            xmlDoc.Save("messeges.xml");

            Console.CancelKeyPress += CancelKeyPress;
            // Skapa ett TcpListener-objekt, börja lyssna och vänta på anslutning
            IPAddress myIp = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(myIp, 8001);
            tcpListener.Start();
            Console.WriteLine("Väntar på anslutning...");
            // Någon försöker ansluta. Acceptera anslutningen
            Socket socket = tcpListener.AcceptSocket();
            Console.WriteLine("Anslutning accepterad från " +
                              socket.RemoteEndPoint);
            while (true)
            {
                try
                {
                    // Tag emot meddelandet
                    byte[] bMessage = new byte[256];
                    int messageSize = socket.Receive(bMessage);
                    Console.WriteLine("Meddelandet mottogs...");
                    // Spara meddelandet i XML
                    string message = "";
                    for (int i = 0; i < messageSize; i++)
                        message += Convert.ToChar(bMessage[i]);
                    if (message == "0")
                    {
                        HämtaOchSkickaMeddalnden(xmlDoc, socket);
                    }
                    else if (message == "1")
                    {
                        // Sluta lyssna efter trafik:
                        tcpListener?.Stop();
                        socket.Close();
                        Console.WriteLine("Servern stängdes av!");
                        break;
                    }
                    else
                    {
                        SparaMeddelanden(message, xmlDoc, messages);
                    }
                }
                catch (Exception e)
                {
                    tcpListener?.Stop();
                    socket.Close();
                    Console.WriteLine("Servern kopplades bort");
                    Console.WriteLine("Error: " + e.Message);
                    break;
                }
            }

            Console.WriteLine("Servern säger hejdå");
        }

        static void HämtaOchSkickaMeddalnden(XmlDocument xmlDoc, Socket socket)
        {
            try
            {
                //Hämtar från XML fil
                XmlNodeList messages = xmlDoc.SelectNodes("messages/message") ?? throw new NoMessagesException();

                if (messages.Count == 0) throw new NoMessagesException();

                string ms = "";

                //Alla meddelanden lägs tillsammans på en sträng
                foreach (XmlNode message in messages)
                {
                    string text = message.SelectSingleNode("text")!.InnerText;
                    string sender = message.SelectSingleNode("sender")!.InnerText;
                    string messageNumber = message.SelectSingleNode("messageNumber")!.InnerText;

                    ms += new Message(text, sender, messageNumber) + "@@@";
                }

                //Skickar meddelanderna till Klienten
                byte[] bSend = Encoding.ASCII.GetBytes(ms);
                socket.Send(bSend);
                Console.WriteLine("Svar skickat");
            }
            catch (NoMessagesException e)
            {
                byte[]
                    bSend = Encoding.ASCII
                        .GetBytes("2"); //2 används för att meddela Klienten att det inte finns några meddelanden
                socket.Send(bSend);
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void SparaMeddelanden(string message, XmlDocument xmlDoc, XmlElement messages)
        {
            //Sparar meddelandet
            Message m = new Message(message);
            XmlElement messageXml = xmlDoc.CreateElement("message");
            messages.AppendChild(messageXml);

            XmlElement sender = xmlDoc.CreateElement("sender");
            sender.InnerText = m.Sender;
            messageXml.AppendChild(sender);

            XmlElement messageNumber = xmlDoc.CreateElement("messageNumber");
            messageNumber.InnerText = m.MessageNumber;
            messageXml.AppendChild(messageNumber);

            XmlElement text = xmlDoc.CreateElement("text");
            text.InnerText = m.Text;
            messageXml.AppendChild(text);

            xmlDoc.Save("messeges.xml");
        }

        // =======================================================================
        // CancelKeyPress(), anropas då användaren trycker på Ctrl-C.
        // =======================================================================
        static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Sluta lyssna efter trafik:
            tcpListener?.Stop();
            Console.WriteLine("Servern stängdes av!");
        }
    }
}