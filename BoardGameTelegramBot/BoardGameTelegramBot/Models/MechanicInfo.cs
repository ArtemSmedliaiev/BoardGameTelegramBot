using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGameTelegramBot.Models
{
    public class MechanicsInfo
    {
        public List<MechanicInfo> mechanics { get; set; }
    }
    public class MechanicInfo
    {
        public string id { get; set; }

        public string name { get; set; }
    }
}
