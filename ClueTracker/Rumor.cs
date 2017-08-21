using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class Rumor
    {
        public Player Gossiper { get; set; }
        public Room Room { get; set; }
        public Weapon Weapon { get; set; }
        public Guest Guest { get; set; }

        public Response Response { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            // Evenly-spaced table
            builder.Append($"Rumor by {Gossiper} -->");
            builder.Append("\t");
            builder.Append(string.Format("{0,-20}", GetCardRevealString(Guest, Response)));
            builder.Append("\t");
            builder.Append(string.Format("{0,-20}", GetCardRevealString(Room, Response)));
            builder.Append("\t");
            builder.Append(string.Format("{0,-20}", GetCardRevealString(Weapon, Response)));

            builder.Append("\t");
            if (Response != null && Response.Player != null)
            {
                builder.Append($"--> revealed by {Response.Player.Name}");
            }
            else
            {
                builder.Append("--> (None)");
            }

            return builder.ToString();
        }

        private string GetCardRevealString(Card card, Response response)
        {
            if (response != null && response.Card != null && card == response.Card)
            {
                return $"[{card.Name}]";
            }
            else
            {
                return card.Name;
            }
        }
    }
}
