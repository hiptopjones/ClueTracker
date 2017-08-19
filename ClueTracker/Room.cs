using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class Room : Card
    {
        public static Room DiningRoom { get; } = new Room("DiningRoom");
        public static Room GuestHouse { get; } = new Room("GuestHouse");
        public static Room Hall { get; } = new Room("Hall");
        public static Room Kitchen { get; } = new Room("Kitchen");
        public static Room LivingRoom { get; } = new Room("LivingRoom");
        public static Room Observatory { get; } = new Room("Observatory");
        public static Room Patio { get; } = new Room("Patio");
        public static Room Pool { get; } = new Room("Pool");
        public static Room Spa { get; } = new Room("Spa");
        public static Room Theater { get; } = new Room("Theater");

        public static List<Room> AllRooms { get; } = new List<Room>
        {
            DiningRoom,
            GuestHouse,
            Hall,
            Kitchen,
            LivingRoom,
            Observatory,
            Patio,
            Pool,
            Spa,
            Theater,
        };

        private Room(string name) : base(name)
        {
        }
    }
}
