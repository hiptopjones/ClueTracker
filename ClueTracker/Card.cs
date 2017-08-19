using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class Card
    {
        public string Name { get; }

        public Card(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
