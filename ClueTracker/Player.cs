using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class Player
    {
        public string Name { get; set; }

        public List<Rumor> Rumors { get; set; } = new List<Rumor>();

        public HashSet<Card> Cards { get; set; } = new HashSet<Card>();

        public override string ToString()
        {
            return Name;
        }
    }
}
