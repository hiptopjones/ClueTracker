using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class Guest : Card
    {
        public static Guest Green { get; } = new Guest("Green");
        public static Guest Mustard { get; } = new Guest("Mustard");
        public static Guest Peacock { get; } = new Guest("Peacock");
        public static Guest Plum { get; } = new Guest("Plum");
        public static Guest Scarlet { get; } = new Guest("Scarlet");
        public static Guest White { get; } = new Guest("White");

        public static List<Guest> AllGuests { get; } = new List<Guest>
        {
            Green,
            Mustard,
            Peacock,
            Plum,
            Scarlet,
            White,
        };

        private Guest(string name) : base(name)
        {
        }
    }
}
