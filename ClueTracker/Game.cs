using Newtonsoft.Json;
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

        public List<Card> ExcludedCards { get; set; } = new List<Card>();

        public List<Card> EnvelopeCards { get; set; } = new List<Card>();

        public List<Player> Players { get; set; } = new List<Player>();

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

                if (players.Any(x => x.Name == playerName))
                {
                    Console.WriteLine($"Player name '{playerName}' already in use.");
                    continue;
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
                IEnumerable<Card> possibleCards = AllCards.Except(MyPlayer.Cards);

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
            ExcludedCards = savedGame.ExcludedCards;
            EnvelopeCards = savedGame.EnvelopeCards;
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
                Console.WriteLine($" Invalid number of cards ({MyPlayer.Cards.Count}) assigned to your player.");
                return;
            }

            List<MenuCommand> commands = new List<MenuCommand>
            {
                new MenuCommand("Record Rumor", RecordRumor),
                new MenuCommand("View My Cards", ViewMyCards),
                new MenuCommand("View Player Data", ViewPlayerData),
                new MenuCommand("View All Cards", ViewAllCards),
                new MenuCommand("View All Rumors", ViewAllRumors),
                new MenuCommand("Exclude Card", ExcludeCard),
            };

            MenuCommand exitCommand = new MenuCommand("Exit", null);

            while (true)
            {
                DisplayEnvelopeCards();

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

            // Ensure the game is analyzed and saved on every action
            AnalyzeGameState();
            SaveGame();
        }

        private void DisplayEnvelopeCards()
        {
            PrintHeader("Envelope Card(s)");

            foreach (Card card in EnvelopeCards)
            {
                Console.WriteLine($"  {card}");
            }
        }

        private void ExcludeCard()
        {
            PrintHeader("Exclude Card");

            IEnumerable<Card> unownedCards = GetUnownedCardsForAllPlayers();
            Card card = PromptForMenuChoice(unownedCards);
            if (card != null)
            {
                ExcludedCards.Add(card);
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
                if (rumor != null)
                {
                    rumor.Gossiper = gossipingPlayer;
                    rumor.Response = PromptForRumorResponse(rumor);

                    if (PromptForRumorConfirmation(rumor))
                    {
                        gossipingPlayer.Rumors.Add(rumor);
                    }
                }
            }
        }

        private bool PromptForRumorConfirmation(Rumor rumor)
        {
            while (true)
            {
                PrintHeader("Confirm Rumor Entry");
                Console.WriteLine($"  {rumor}");
                Console.WriteLine();
                Console.WriteLine("Confirm: (Y or N) ");
                string input = ReadInputLine();
                if (input == "Y")
                {
                    return true;
                }
                else if (input == "N")
                {
                    return false;
                }
            }
        }

        private Rumor PromptForRumor()
        {
            Guest guest = PromptForRumorGuest();
            if (guest == null)
            {
                return null;
            }

            Room room = PromptForRumorRoom();
            if (room == null)
            {
                return null;
            }

            Weapon weapon = PromptForRumorWeapon();
            if (weapon == null)
            {
                return null;
            }

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
            // Filter out the player who started the rumor, and my player if I don't
            // have any of the cards in the rumor
            List<Player> possiblePlayers = Players.Where(x => x != rumor.Gossiper)
                .Where(x => x != MyPlayer || x.Cards.Intersect(rumor.Cards).Any()).ToList();

            PrintHeader("Response: Player?");
            return PromptForMenuChoice(possiblePlayers);
        }

        private Card PromptForRumorResponseCard(Rumor rumor, Response response)
        {
            List<Card> rumorCards = new List<Card>()
            {
                rumor.Guest,
                rumor.Room,
                rumor.Weapon,
            };

            // This code pares down the card list that will be prompted to only include
            // available choices.
            IEnumerable<Card> possibleCards;

            Player respondingPlayer = response.Player;

            if (respondingPlayer == MyPlayer)
            {
                // If I'm responding, then only display cards that I have
                possibleCards = rumorCards.Intersect(MyPlayer.Cards).ToList();
            }
            else
            {
                IEnumerable<Card> ownedAndPossibleCards = GetOwnedAndPossibleCardsForPlayer(respondingPlayer);
                possibleCards = rumorCards.Intersect(ownedAndPossibleCards);
            }

            // If the responder is me, then display which cards have been revealed already.
            if (respondingPlayer == MyPlayer)
            {
                DisplayPreviouslyRevealedCards(possibleCards);
            }

            PrintHeader("Response: Card?");
            return PromptForMenuChoice(possibleCards);
        }

        private void DisplayPreviouslyRevealedCards(IEnumerable<Card> possibleCards)
        {
            PrintHeader("Previously-Revealed Cards");

            foreach (Card card in possibleCards)
            {
                List<Player> revealedPlayers = Players.Where(x => x.Rumors.Any(r => r.Response?.Card == card)).ToList();
                if (revealedPlayers.Any())
                {
                    Console.WriteLine($"  {card,-20} revealed to {string.Join(", ", revealedPlayers)}");
                }
            }
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
            bool anyChanges;

            do
            {
                anyChanges = false;

                // Recalculate the envelope cards
                anyChanges |= UpdateEnvelopeCards();

                // Deduce card reveals between other players
                anyChanges |= UpdateRumorCards();
            }
            while (anyChanges);
        }

        private bool UpdateRumorCards()
        {
            bool anyChanges = false;

            // Go through all of the rumors, and see we can deduce any cards that were
            // revealed to other players
            foreach (Rumor rumor in GetRumorsForAllPlayers())
            {
                // Only look at rumors where there was a response, but we don't know the revealed card
                if (rumor.Response != null && rumor.Response.Card == null)
                {
                    Player respondingPlayer = rumor.Response.Player;

                    // Intersect the list of possible and owned cards for the given player
                    // with the list of cards from the rumor.  If there is only one card in
                    // the intersection, then it is the card that was revealed.

                    IEnumerable<Card> ownedAndPossibleCards = GetOwnedAndPossibleCardsForPlayer(respondingPlayer);
                    List<Card> intersectingCards = rumor.Cards.Intersect(ownedAndPossibleCards).ToList();
                    if (intersectingCards.Count == 1)
                    {
                        rumor.Response.Card = intersectingCards.Single();

                        PrintHeader("Deduced Card");
                        Console.WriteLine($"  {rumor}");

                        anyChanges = true;
                    }
                }
            }

            return anyChanges;
        }

        private bool UpdateEnvelopeCards()
        {
            IEnumerable<Card> allOwnedAndPossibleCards = GetOwnedAndPossibleCardsForAllPlayers();
            IEnumerable<Card> allOwnedPossibleAndExcludedCards = allOwnedAndPossibleCards.Union(ExcludedCards);
            List<Card> updatedEnvelopeCards = AllCards.Except(allOwnedPossibleAndExcludedCards).ToList();

            List<Card> newEnvelopeCards = updatedEnvelopeCards.Except(EnvelopeCards).ToList();
            if (newEnvelopeCards.Any())
            {
                PrintHeader("Deduced Envelope Card(s)");
                foreach (Card card in newEnvelopeCards)
                {
                    Console.WriteLine($"  {card}");
                }

                EnvelopeCards = updatedEnvelopeCards;
                return true;
            }

            return false;
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
                PrintCardWithOwners(guest);
            }

            Console.WriteLine();
            Console.WriteLine("Rooms");

            foreach (Room room in AllRooms)
            {
                PrintCardWithOwners(room);
            }

            Console.WriteLine();
            Console.WriteLine("Weapons");

            foreach (Weapon weapon in AllWeapons)
            {
                PrintCardWithOwners(weapon);
            }
        }

        private void PrintCardWithOwners(Card card)
        {
            const string excludedCardMarker = "<-- [x]";
            const string envelopeCardMarker = "<-- [envelope]";

            string owner = null;
            if (ExcludedCards.Contains(card))
            {
                owner = excludedCardMarker;
            }
            else if (EnvelopeCards.Contains(card))
            {
                owner = envelopeCardMarker;
            }
            else
            {
                Player playerWithCard = Players.Where(x => x.Cards.Contains(card)).SingleOrDefault();
                if (playerWithCard != null)
                {
                    owner = $"+ {playerWithCard}";
                }
            }

            // Only show non-owners when owner is unknown
            string notOwners = null;
            if (string.IsNullOrEmpty(owner))
            {
                IEnumerable<Player> playersNotOwningCard = Players.Where(x => GetImpossibleCardsForPlayer(x).Contains(card));
                notOwners = string.Join(", ", playersNotOwningCard.OrderBy(x => x.Name));
                if (!string.IsNullOrEmpty(notOwners))
                {
                    notOwners = $"- {notOwners}";
                }
            }

            Console.WriteLine($"  {card,-20} {owner,-20} {notOwners}");
        }

        private void ViewAllRumors()
        {
            PrintHeader("All Rumors");

            IEnumerable<Rumor> allRumors = GetRumorsForAllPlayers();
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
            PrintHeader($"Owned Cards (Missing: {GetNumberOfCardsPerPlayer() - player.Cards.Count})");
            PrintCards(player.Cards);

            // Display the cards this player may own (if all of their cards are not known)
            PrintHeader("Possible Cards");
            IEnumerable<Card> possibleCards = GetPossibleCardsForPlayer(player);
            PrintCards(possibleCards);

            // Display the rumors they made
            PrintHeader("Made Rumors");
            IEnumerable<Rumor> madeRumors = player.Rumors;
            PrintRumors(madeRumors);

            // Display the rumors they answered
            PrintHeader("Answered Rumors");
            IEnumerable<Rumor> answeredRumors = GetRumorsForAllPlayers().Where(x => x.Response?.Player == player);
            PrintRumors(answeredRumors);
        }

        private IEnumerable<Card> GetOwnedCardsForAllPlayers()
        {
            return Players.SelectMany(GetOwnedCardsForPlayer);
        }

        private IEnumerable<Card> GetOwnedCardsForPlayer(Player player)
        {
            return player.Cards;
        }

        private IEnumerable<Card> GetUnownedCardsForAllPlayers()
        {
            IEnumerable<Card> ownedCards = GetOwnedCardsForAllPlayers();
            return AllCards.Except(ownedCards);
        }

        private IEnumerable<Card> GetPossibleCardsForPlayer(Player player)
        {
            IEnumerable<Card> possibleCards = Enumerable.Empty<Card>();

            if (player.Cards.Count < GetNumberOfCardsPerPlayer())
            {
                // Start with all of the unowned cards
                possibleCards = GetUnownedCardsForAllPlayers();

                // Remove any cards known to not be owned by this player
                possibleCards = possibleCards.Except(GetImpossibleCardsForPlayer(player));
            }

            return possibleCards;
        }

        private IEnumerable<Card> GetPossibleCardsForAllPlayers()
        {
            return Players.SelectMany(GetPossibleCardsForPlayer).Distinct();
        }

        private IEnumerable<Card> GetOwnedAndPossibleCardsForPlayer(Player player)
        {
            IEnumerable<Card> ownedCards = GetOwnedCardsForPlayer(player);
            IEnumerable<Card> possibleCards = GetPossibleCardsForPlayer(player);
            return ownedCards.Union(possibleCards);
        }

        private IEnumerable<Card> GetOwnedAndPossibleCardsForAllPlayers()
        {
            return Players.SelectMany(GetOwnedAndPossibleCardsForPlayer);
        }

        private IEnumerable<Card> GetImpossibleCardsForPlayer(Player player)
        {
            IEnumerable<Card> impossibleCards = Enumerable.Empty<Card>();

            // Add all cards owned by other players
            impossibleCards = impossibleCards.Union(GetOwnedCardsForAllPlayers().Except(GetOwnedCardsForPlayer(player)));

            // Add any excluded cards
            impossibleCards = impossibleCards.Union(ExcludedCards);

            // Add any cards known to be in the envelope
            impossibleCards = impossibleCards.Union(EnvelopeCards);

            // Add any cards the player was unable to reveal during a rumor
            impossibleCards = impossibleCards.Union(GetCardsNotRevealedForPlayer(player));

            return impossibleCards;
        }

        private IEnumerable<Card> GetCardsNotRevealedForPlayer(Player player)
        {
            IEnumerable<Card> cardsNotRevealed = Enumerable.Empty<Card>();

            // For each rumor, get the sequence of players between the gossiper and the responder
            // This lets us see who didn't reveal a card, enabling us to eliminate it from their cards
            IEnumerable<Rumor> allRumors = GetRumorsForAllPlayers();
            foreach (Rumor rumor in allRumors)
            {
                IEnumerable<Player> nonRevealingPlayers;

                if (rumor.Response == null)
                {
                    // If no player responded to the rumor, then no player has the card
                    nonRevealingPlayers = Players.Where(x => x != rumor.Gossiper).ToList();
                }
                else
                {
                    // Get the players who could not show a card for this rumor
                    nonRevealingPlayers = GetPlayersBetween(rumor.Gossiper, rumor.Response.Player);
                }

                if (nonRevealingPlayers.Contains(player))
                {
                    // This player was unable to prove this rumor false, so add this rumor's cards
                    // to the known unowned list 
                    cardsNotRevealed = cardsNotRevealed.Union(rumor.Cards);
                }
            }

            return cardsNotRevealed;
        }

        private IEnumerable<Rumor> GetRumorsForAllPlayers()
        {
            return Players.SelectMany(x => x.Rumors);
        }

        private IEnumerable<Player> GetPlayersBetween(Player startPlayer, Player endPlayer)
        {
            List<Player> playersBetween = new List<Player>();

            int i = Players.IndexOf(startPlayer);
            while (true)
            {
                Player currentPlayer = Players[++i % Players.Count];
                if (currentPlayer == endPlayer)
                {
                    break;
                }

                playersBetween.Add(currentPlayer);
            }

            return playersBetween;
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

        private T PromptForMenuChoice<T>(IEnumerable<T> choices, T exitChoice = default(T))
        {
            while (true)
            {
                Console.WriteLine();

                List<T> choicesList = choices.ToList();

                for (int i = 0; i < choicesList.Count; i++)
                {
                    Console.WriteLine($"  {i}. {choicesList[i].ToString()}");
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
                if (uint.TryParse(choice, out choiceIndex) && choiceIndex < choicesList.Count)
                {
                    return choicesList[(int)choiceIndex];
                }
            }
        }

        private string ReadInputLine()
        {
            return Console.ReadLine().Trim();
        }
    }
}
