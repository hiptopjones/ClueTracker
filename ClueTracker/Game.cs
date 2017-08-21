﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClueTracker
{
    class Game
    {
        private const string ExitChoice = "X";
        private const int MyPlayerIndex = 0;
        private const int DeckCardCount = 24;
        private const int HiddenCardCount = 3;
        private const string SaveGameFilePath = @"c:\Users\peterj\Desktop\ClueTracker.json";

        private readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Formatting = Formatting.Indented,
        };

        public List<Room> AllRooms { get; set; } = new List<Room>()
        {
            new Room("DiningRoom"),
            new Room("GuestHouse"),
            new Room("Hall"),
            new Room("Kitchen"),
            new Room("LivingRoom"),
            new Room("Observatory"),
            new Room("Patio"),
            new Room("Spa"),
            new Room("Theater"),
        };

        public List<Weapon> AllWeapons { get; set; } = new List<Weapon>()
        {
            new Weapon("Axe"),
            new Weapon("Bat"),
            new Weapon("Candlestick"),
            new Weapon("Dumbbell"),
            new Weapon("Knife"),
            new Weapon("Pistol"),
            new Weapon("Poison"),
            new Weapon("Rope"),
            new Weapon("Trophy"),
        };

        public List<Guest> AllGuests { get; set; } = new List<Guest>()
        {
            new Guest("Green"),
            new Guest("Mustard"),
            new Guest("Peacock"),
            new Guest("Plum"),
            new Guest("Scarlet"),
            new Guest("White"),
        };

        public List<Player> Players { get; set; }

        [JsonIgnore]
        public Player MyPlayer
        {
            get
            {
                return Players?.FirstOrDefault();
            }
        }
        
        [JsonIgnore]
        public List<Card> AllCards
        {
            get
            {
                return Enumerable.Concat(AllGuests, Enumerable.Concat<Card>(AllRooms, AllWeapons)).ToList();
            }
        }

        public void Play()
        {
            EnterSetupMenu();
        }

        private void EnterSetupMenu()
        {
            List<MenuCommand> commands = new List<MenuCommand>
            {
                new MenuCommand("Enter Players", EnterPlayers),
                new MenuCommand("Enter My Cards", EnterCardsForMyPlayer),
                new MenuCommand("Restore Game", RestoreGame),
                new MenuCommand("Start Game", EnterMainMenu),
            };

            MenuCommand exitCommand = new MenuCommand("Exit", null);

            while (true)
            {
                PrintHeader("Setup Menu");

                MenuCommand command = PromptForMenuChoice(commands, exitCommand);
                if (command != null)
                {
                    if (command == exitCommand)
                    {
                        break;
                    }

                    command.CommandAction();
                }
            }
        }

        private void EnterPlayers()
        {
            Players = PromptForPlayerEntry();
        }

        private List<Player> PromptForPlayerEntry()
        {
            PrintHeader("Player Entry");

            Console.WriteLine(" -> The first player is always you.");
            Console.WriteLine(" -> Enter players in order of play.");
            Console.WriteLine(" -> Enter a blank name to stop entering players.");
            Console.WriteLine();

            List<Player> players = new List<Player>();

            while (true)
            {
                Console.Write(" Enter player name: ");
                string playerName = ReadInputLine();

                if (string.IsNullOrEmpty(playerName))
                {
                    break;
                }

                Player player = new Player { Name = playerName };
                players.Add(player);
            }

            return players;
        }

        private void EnterCardsForMyPlayer()
        {
            PrintHeader("Card Entry");
            Console.WriteLine(" -> Enter your cards.");
            Console.WriteLine(" -> Enter a blank name to stop entering cards.");
            Console.WriteLine();

            int cardsPerPlayer = GetNumberOfCardsPerPlayer();

            // Clear any existing cards
            MyPlayer.Cards.Clear();

            while (MyPlayer.Cards.Count < cardsPerPlayer)
            {
                PrintHeader("Card Selection");

                // Only show the cards that haven't been already selected
                List<Card> possibleCards = GetFilteredCards(AllCards, MyPlayer.Cards.ToList());
                Card card = PromptForMenuChoice(possibleCards);
                if (card == null)
                {
                    break;
                }

                AssignCardToPlayer(card, MyPlayer);

                PrintHeader("Assigned Cards");
                PrintCards(MyPlayer.Cards);
            }
        }

        private int GetNumberOfCardsPerPlayer()
        {
            return (DeckCardCount - HiddenCardCount) / Players.Count;
        }

        private void SaveGame()
        {
            string savedGameJson = JsonConvert.SerializeObject(this, SerializerSettings);
            File.WriteAllText(SaveGameFilePath, savedGameJson);
        }

        private void RestoreGame()
        {
            string savedGameJson = File.ReadAllText(SaveGameFilePath);
            Game savedGame = JsonConvert.DeserializeObject<Game>(savedGameJson, SerializerSettings);

            AllGuests = savedGame.AllGuests;
            AllRooms = savedGame.AllRooms;
            AllWeapons = savedGame.AllWeapons;
            Players = savedGame.Players;

            Console.WriteLine("Restored game from disk");
        }

        private void EnterMainMenu()
        {
            if (Players == null || Players.Count < 2)
            {
                Console.WriteLine(" Unable to play with less than 2 players.");
                return;
            }

            if (MyPlayer.Cards == null || MyPlayer.Cards.Count != GetNumberOfCardsPerPlayer())
            {
                Console.WriteLine(" Unable to play due to invalid number of cards assigned to your player.");
                return;
            }

            List<MenuCommand> commands = new List<MenuCommand>
            {
                new MenuCommand("Record Rumor", RecordRumor),
                new MenuCommand("View My Cards", ViewMyCards),
                new MenuCommand("View Player Data", ViewPlayerData),
                new MenuCommand("View All Cards", ViewAllCards),
                new MenuCommand("View All Rumors", ViewAllRumors),
            };

            MenuCommand exitCommand = new MenuCommand("Exit", null);

            while (true)
            {
                PrintHeader("Main Menu");

                MenuCommand command = PromptForMenuChoice(commands, exitCommand);
                if (command != null)
                {
                    if (command == exitCommand)
                    {
                        break;
                    }

                    command.CommandAction();
                }

                // Ensure the game is saved on every action
                SaveGame();
            }
        }

        private void RecordRumor()
        {
            PrintHeader("Record Rumor");

            PrintHeader("Rumor: Player?");

            Player gossipingPlayer = PromptForMenuChoice(Players);
            if (gossipingPlayer != null)
            {
                Rumor rumor = PromptForRumor();
                rumor.Gossiper = gossipingPlayer;
                rumor.Response = PromptForRumorResponse(rumor);

                gossipingPlayer.Rumors.Add(rumor);
            }

            AnalyzeGameState();
        }

        private Rumor PromptForRumor()
        {
            Guest guest = PromptForRumorGuest();
            Room room = PromptForRumorRoom();
            Weapon weapon = PromptForRumorWeapon();

            return new Rumor { Guest = guest, Room = room, Weapon = weapon };
        }

        private Room PromptForRumorRoom()
        {
            PrintHeader("Rumor: Room?");
            return PromptForMenuChoice(AllRooms);
        }

        private Weapon PromptForRumorWeapon()
        {
            PrintHeader("Rumor: Weapon?");
            return PromptForMenuChoice(AllWeapons);
        }

        private Guest PromptForRumorGuest()
        {
            PrintHeader("Rumor: Guest?");
            return PromptForMenuChoice(AllGuests);
        }

        private Response PromptForRumorResponse(Rumor rumor)
        {
            Response response = null;

            Player respondingPlayer = PromptForRumorResponsePlayer(rumor);
            if (respondingPlayer != null)
            {
                response = new Response { Player = respondingPlayer };

                // We can only learn the card if we are the player starting the rumor or the responding player
                if (rumor.Gossiper == MyPlayer || respondingPlayer == MyPlayer)
                {
                    response.Card = PromptForRumorResponseCard(rumor, response);
                    if (response.Card != null)
                    {
                        AssignCardToPlayer(response.Card, response.Player);
                    }
                }
            }

            return response;
        }

        private Player PromptForRumorResponsePlayer(Rumor rumor)
        {
            PrintHeader("Response: Player?");

            List<Player> possiblePlayers = Players.Where(x => x != rumor.Gossiper).ToList();
            return PromptForMenuChoice(possiblePlayers);
        }

        private Card PromptForRumorResponseCard(Rumor rumor, Response response)
        {
            PrintHeader("Response: Card?");

            List<Card> rumorCards = new List<Card>()
            {
                rumor.Guest,
                rumor.Room,
                rumor.Weapon,
            };

            // This code pares down the card list that will be prompted to only include
            // available choices.  Cards that people (other than the responder) are known
            // to own should be excluded from this list.

            // Get the list of all players, not including the responder
            List<Player> allPlayersExceptResponder = Players.Where(x => x != response.Player).ToList();

            // Gets the list of cards known to be owned by these players
            List<Card> playerCards = allPlayersExceptResponder.SelectMany(x => x.Cards).ToList();

            // Remove these player-owned cards from the possible selections
            List<Card> possibleCards = GetFilteredCards(rumorCards, playerCards);

            return PromptForMenuChoice(possibleCards);
        }

        private void AssignCardToPlayer(Card card, Player player)
        {
            if (!player.Cards.Contains(card))
            {
                player.Cards.Add(card);
            }
        }

        private void AnalyzeGameState()
        {
            // This is where the magic should happen
        }

        private List<Card> GetFilteredCards(List<Card> source, List<Card> excludes)
        {
            return source.Where(x => !excludes.Contains(x)).ToList();
        }

        private void ViewMyCards()
        {
            PrintHeader("My Cards");
            PrintCards(MyPlayer.Cards);
        }

        private void ViewAllCards()
        {
            PrintHeader("All Cards");

            Console.WriteLine();
            Console.WriteLine(" Guests");

            foreach (Guest guest in AllGuests)
            {
                Player owner = Players.Where(x => x.Cards.Contains(guest)).SingleOrDefault();
                Console.WriteLine($"  {guest,-20} {owner}");
            }

            Console.WriteLine();
            Console.WriteLine("Rooms");

            foreach (Room room in AllRooms)
            {
                Player owner = Players.Where(x => x.Cards.Contains(room)).SingleOrDefault();
                Console.WriteLine($"  {room,-20} {owner}");
            }

            Console.WriteLine();
            Console.WriteLine("Weapons");

            foreach (Weapon weapon in AllWeapons)
            {
                Player owner = Players.Where(x => x.Cards.Contains(weapon)).SingleOrDefault();
                Console.WriteLine($"  {weapon,-20} {owner}");
            }
        }

        private void ViewAllRumors()
        {
            PrintHeader("All Rumors");

            IEnumerable<Rumor> allRumors = Players.SelectMany(x => x.Rumors);
            PrintRumors(allRumors);
        }

        private void ViewPlayerData()
        {
            PrintHeader("Pick Player");
            Player player = PromptForMenuChoice(Players);
            if (player == null)
            {
                return;
            }

            // Display the knowledge we have of the player
            PrintHeader($"Player Data for {player.Name}");

            // Display the cards this player owns
            PrintHeader("Owned Cards");
            PrintCards(player.Cards);

            // Display the rumors they made
            PrintHeader("Made Rumors");
            IEnumerable<Rumor> madeRumors = player.Rumors;
            PrintRumors(madeRumors);

            // Display the rumors they answered
            PrintHeader("Answered Rumors");
            IEnumerable<Rumor> answeredRumors = Players.SelectMany(x => x.Rumors.Where(a => a.Response?.Player == player));
            PrintRumors(answeredRumors);
        }

        private void PrintCards(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                Console.WriteLine($"  {card.Name}");
            }
        }

        private void PrintRumors(IEnumerable<Rumor> rumors)
        {
            foreach (Rumor rumor in rumors)
            {
                Console.WriteLine($"  {rumor.ToString()}");
            }
        }

        private void PrintHeader(string header)
        {
            Console.WriteLine();
            Console.WriteLine(" ==============================================");
            Console.WriteLine($" {header}");
            Console.WriteLine();
        }

        private T PromptForMenuChoice<T>(List<T> choices, T exitChoice = default(T))
        {
            while (true)
            {
                Console.WriteLine();

                for (int i = 0; i < choices.Count; i++)
                {
                    Console.WriteLine($"  {i}. {choices[i].ToString()}");
                }

                Console.WriteLine($"  {ExitChoice}. Exit");
                Console.WriteLine();
                Console.Write(" Choice? ");
                string choice = ReadInputLine();

                if (choice == ExitChoice)
                {
                    return exitChoice;
                }

                uint choiceIndex;
                if (uint.TryParse(choice, out choiceIndex) && choiceIndex < choices.Count)
                {
                    return choices[(int)choiceIndex];
                }
            }
        }

        private string ReadInputLine()
        {
            return Console.ReadLine().Trim();
        }
    }
}
