using System;

namespace Kryptering_Server
{
    public class Message
    {
        string text;
        string sender;
        string messageNumber;

        public string Text
        {
            get => text;
        }

        public string Sender
        {
            get => sender;
        }

        public string MessageNumber
        {
            get => messageNumber;
        }

        public Message(string message)
        {
            string[] pattern = {"???"};
            string[] splitedString = message.Split(pattern, StringSplitOptions.None);

            text = splitedString[0];
            sender = splitedString[1];
            messageNumber = splitedString[2];
        }

        public Message(string text, string sender, string messageNumber)
        {
            this.text = text;
            this.sender = sender;
            this.messageNumber = messageNumber;
        }

        public override string ToString()
        {
            return text + "???" + sender + "???" +
                   messageNumber;
        }
    }
}