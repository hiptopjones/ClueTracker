using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    public class Card
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

        public override bool Equals(object obj)
        {
            Card thisCard = this;
            Card thatCard = obj as Card;

            return (thatCard != null && 
                thisCard.GetType() == thatCard.GetType() &&
                thisCard.Name == thatCard.Name);
        }
    }
}
