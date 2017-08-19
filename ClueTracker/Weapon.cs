using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class Weapon : Card
    {
        public static Weapon Axe { get; } = new Weapon("Axe");
        public static Weapon Bat { get; } = new Weapon("Bat");
        public static Weapon Candlestick { get; } = new Weapon("Candlestick");
        public static Weapon Dumbbell { get; } = new Weapon("Dumbbell");
        public static Weapon Knife { get; } = new Weapon("Knife");
        public static Weapon Pistol { get; } = new Weapon("Pistol");
        public static Weapon Poison { get; } = new Weapon("Poison");
        public static Weapon Rope { get; } = new Weapon("Rope");
        public static Weapon Trophy { get; } = new Weapon("Trophy");

        public static List<Weapon> AllWeapons { get; } = new List<Weapon>
        {
            Axe,
            Bat,
            Candlestick,
            Dumbbell,
            Knife,
            Pistol,
            Poison,
            Rope,
            Trophy,
        };

        private Weapon(string name) : base(name)
        {
        }
    }
}
