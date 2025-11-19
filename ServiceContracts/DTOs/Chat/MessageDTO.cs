using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.Chat
{
    internal class MessageDTO
    {

        public string Sender { get; set; }

        public string Receiver { get; set; }
        
        
        public string Content { get; set; }

    }
}
