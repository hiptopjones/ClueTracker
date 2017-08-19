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

        public List<Accusation> Accusations { get; } = new List<Accusation>();

        public HashSet<Card> Cards { get; } = new HashSet<Card>();

        public override string ToString()
        {
            return Name;
        }
    }
}
