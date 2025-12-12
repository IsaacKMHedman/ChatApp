using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Model
{
    //Bara vårt messageprotokoll
    public class ChatMessage
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }

    }
}
