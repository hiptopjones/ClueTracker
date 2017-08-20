using System;
using System.Collections.Generic;
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

        public List<Player> Players { get; set; }

        public Player MyPlayer { get; set; }

        public List<Card> AllCards { get; set; }

        public Game()
        {
            List<Card> cards = new List<Card>();

            cards.AddRange(Guest.AllGuests);
            cards.AddRange(Room.AllRooms);
            cards.AddRange(Weapon.AllWeapons);

            AllCards = cards;
        }
        public void Play()
        {
            Players = PromptForPlayerEntry();
            if (Players.Count < 2)
            {
                Console.WriteLine(" Unable to play with less than 2 players.");
                return;
            }

            MyPlayer = Players[MyPlayerIndex];

            PromptForMyPlayerCardEntry();

            MainMenu();
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

        private void PromptForMyPlayerCardEntry()
        {
            PrintHeader("Card Entry");
            Console.WriteLine(" -> Enter your cards.");
            Console.WriteLine(" -> Enter a blank name to stop entering cards.");
            Console.WriteLine();

            int cardsPerPlayer = (DeckCardCount - HiddenCardCount) / Players.Count;

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

                PrintHeader("Selected Cards");
                PrintCards(MyPlayer.Cards);
            }
        }

        private void MainMenu()
        {
            List<MenuCommand> commands = new List<MenuCommand>
            {
                new MenuCommand("My Cards", ViewMyCards),
                new MenuCommand("View Player", ViewPlayerData),
                new MenuCommand("Record Accusation", RecordAccusation),
                new MenuCommand("Card Sheet", ViewCardSheet),

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
            }
        }

        private void RecordAccusation()
        {
            PrintHeader("Record Accusation");

            PrintHeader("Accusation: Player?");

            Player accusingPlayer = PromptForMenuChoice(Players);
            if (accusingPlayer != null)
            {
                Accusation accusation = PromptForAccusation();
                accusation.Accuser = accusingPlayer;
                accusation.Response = PromptForAccusationResponse(accusation);

                accusingPlayer.Accusations.Add(accusation);
            }
        }

        private Accusation PromptForAccusation()
        {
            Guest guest = PromptForAccusationGuest();
            Room room = PromptForAccusationRoom();
            Weapon weapon = PromptForAccusationWeapon();

            return new Accusation { Guest = guest, Room = room, Weapon = weapon };
        }

        private Room PromptForAccusationRoom()
        {
            PrintHeader("Accusation: Room?");
            return PromptForMenuChoice(Room.AllRooms);
        }

        private Weapon PromptForAccusationWeapon()
        {
            PrintHeader("Accusation: Weapon?");
            return PromptForMenuChoice(Weapon.AllWeapons);
        }

        private Guest PromptForAccusationGuest()
        {
            PrintHeader("Accusation: Guest?");
            return PromptForMenuChoice(Guest.AllGuests);
        }

        private Response PromptForAccusationResponse(Accusation accusation)
        {
            Response response = null;

            Player respondingPlayer = PromptForAccusationResponsePlayer();
            if (respondingPlayer != null)
            {
                response = new Response { Player = respondingPlayer };

                // We can only learn the card if we are the accusing player
                if (accusation.Accuser == MyPlayer)
                {
                    response.Card = PromptForAccusationResponseCard(accusation, response);
                    if (response.Card != null)
                    {
                        AssignCardToPlayer(response.Card, response.Player);
                    }
                }
            }

            return response;
        }

        private Player PromptForAccusationResponsePlayer()
        {
            PrintHeader("Response: Player?");

            List<Player> possiblePlayers = Players.Where(x => x != MyPlayer).ToList();
            return PromptForMenuChoice(possiblePlayers);
        }

        private Card PromptForAccusationResponseCard(Accusation accusation, Response response)
        {
            PrintHeader("Response: Card?");

            List<Card> accusationCards = new List<Card>()
            {
                accusation.Guest,
                accusation.Room,
                accusation.Weapon,
            };

            // This code pares down the card list that will be prompted to only include
            // available choices.  Cards that people (other than the responder) are known
            // to own should be excluded from this list.

            // Get the list of all players, not including the responder
            List<Player> allPlayersExceptResponder = Players.Where(x => x != response.Player).ToList();

            // Gets the list of cards known to be owned by these players
            List<Card> playerCards = allPlayersExceptResponder.SelectMany(x => x.Cards).ToList();

            // Remove these player-owned cards from the possible selections
            List<Card> possibleCards = GetFilteredCards(accusationCards, playerCards);

            return PromptForMenuChoice(possibleCards);
        }

        private void AssignCardToPlayer(Card card, Player player)
        {
            if (!player.Cards.Contains(card))
            {
                player.Cards.Add(card);
            }
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

        private void ViewCardSheet()
        {
            PrintHeader("Card Sheet");

            Console.WriteLine();
            Console.WriteLine(" Guests");

            foreach (Guest guest in Guest.AllGuests)
            {
                Player owner = Players.Where(x => x.Cards.Contains(guest)).SingleOrDefault();
                Console.WriteLine($"  {guest,-20} {owner}");
            }

            Console.WriteLine();
            Console.WriteLine("Rooms");

            foreach (Room room in Room.AllRooms)
            {
                Player owner = Players.Where(x => x.Cards.Contains(room)).SingleOrDefault();
                Console.WriteLine($"  {room,-20} {owner}");
            }

            Console.WriteLine();
            Console.WriteLine("Weapons");

            foreach (Weapon weapon in Weapon.AllWeapons)
            {
                Player owner = Players.Where(x => x.Cards.Contains(weapon)).SingleOrDefault();
                Console.WriteLine($"  {weapon,-20} {owner}");
            }
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

            // Display the accusations they made
            PrintHeader("Made Accusations");
            IEnumerable<Accusation> madeAccusations = player.Accusations;
            PrintAccusations(madeAccusations);

            // Display the accusations they answered
            PrintHeader("Answered Accusations");
            IEnumerable<Accusation> answeredAccusations = Players.SelectMany(x => x.Accusations.Where(a => a.Response?.Player == player));
            PrintAccusations(answeredAccusations);
        }

        private void PrintCards(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                Console.WriteLine($"  {card.Name}");
            }
        }

        private void PrintAccusations(IEnumerable<Accusation> accusations)
        {
            foreach (Accusation accusation in accusations)
            {
                Console.WriteLine($"  {accusation.ToString()}");
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
