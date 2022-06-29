using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGameTelegramBot.Models
{
    public class CategoriesInfo
    {
        public List<CategorieInfo> categories { get; set; }
    }
    public class CategorieInfo
    {
        public string id { get; set; }

        public string name { get; set; }
    }
}
