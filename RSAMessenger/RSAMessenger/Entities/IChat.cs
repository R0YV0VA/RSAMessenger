using System;
using System.Collections.Generic;
using System.Text;

namespace RSAMessenger.Entities
{
    public class IChat
    {
        public string message { get; set; }
        public string[] users { get; set; }
        public string senderName { get; set; }
    }
}
