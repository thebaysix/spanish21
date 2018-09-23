using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;

namespace Spanish21
{
    [Activity(Label = "Spanish21", MainLauncher = true)]
    public class MainActivity : Activity
    {
        // Constants
        private const int numDecks = 1;

        // Declarations
        private ImageView cardImage_d1, cardImage_d2, cardImage_p1, cardImage_p2;
        private LinearLayout dealerLayout, mainLayout, playerLayout;
        private TextView recordText, scoreText;
        private int numWins = 0, numLosses = 0;
        private int cid = -1;
        private Random randObj = new Random();
        private readonly int[,] deckImages = GetDeckImages();
        private List<int> dealerHand, playerHand;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Init
            base.OnCreate(savedInstanceState);

            // Shuffle deck
            var deck = GetShuffledDeck(numDecks);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Initialize UI items:

            // Images
            cardImage_d1 = FindViewById<ImageView>(Resource.Id.cardImage_d1);
            cardImage_d2 = FindViewById<ImageView>(Resource.Id.cardImage_d2);
            cardImage_p1 = FindViewById<ImageView>(Resource.Id.cardImage_p1);
            cardImage_p2 = FindViewById<ImageView>(Resource.Id.cardImage_p2);

            // Layouts
            dealerLayout = FindViewById<LinearLayout>(Resource.Id.dealerLayout);
            mainLayout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
            playerLayout = FindViewById<LinearLayout>(Resource.Id.playerLayout);

            // Text
            recordText = FindViewById<TextView>(Resource.Id.recordText);
            scoreText = FindViewById<TextView>(Resource.Id.scoreText);

            // Action: Hit
            FindViewById<Button>(Resource.Id.hitBtn).Click += (o, e) =>
            {
                // Add a card to the player cards
                var newCardImage = new ImageView(this);
                var newCid = Draw(deck);
                playerHand.Add(newCid);
                newCardImage.SetImageResource(GetDeckImage(newCid));
                var layoutParams = new LinearLayout.LayoutParams(
                    Android.Views.ViewGroup.LayoutParams.MatchParent,
                    Android.Views.ViewGroup.LayoutParams.WrapContent);
                layoutParams.Weight = 1;

                playerLayout.AddView(newCardImage);
            };

            // Action: Stand
            FindViewById<Button>(Resource.Id.standBtn).Click += (o, e) =>
            {
                // Reveal
                cardImage_d1.SetImageResource(GetDeckImage(dealerHand[0]));

                // Add the hand totals
                var dealerTotal = GetHandValue(dealerHand).Item1;
                var playerTotal = GetHandValue(playerHand).Item1;

                var resultText = "";
                if (playerTotal > 21)
                {
                    resultText = "You bust";
                    numLosses++;
                }
                if (dealerTotal > playerTotal)
                {
                    resultText = "You lose";
                    numLosses++;
                }
                else if (playerTotal > dealerTotal)
                {
                    resultText = "You win!";
                    numWins++;
                }
                else
                    resultText = "Tie";

                scoreText.SetText(resultText + " D: " + dealerTotal + ", P: " + playerTotal, TextView.BufferType.Normal);
                recordText.SetText("Wins: " + numWins + ", Losses: " + numLosses, TextView.BufferType.Normal);
            };

            // Action: Deal
            FindViewById<Button>(Resource.Id.dealBtn).Click += (o, e) =>
            {
                var hands = Deal(deck);
                dealerHand = hands.Item1;
                playerHand = hands.Item2;
            };
        }

        private Tuple<List<int>, List<int>> Deal(List<int> deck)
        {
            var dealerCards = new List<int>();
            dealerCards.Add(Draw(deck));
            dealerCards.Add(Draw(deck));
            cardImage_d1.SetImageResource(Resource.Drawable.back); // one card face down
            cardImage_d2.SetImageResource(GetDeckImage(dealerCards[1]));

            var playerCards = new List<int>();
            playerCards.Add(Draw(deck));
            playerCards.Add(Draw(deck));
            cardImage_p1.SetImageResource(GetDeckImage(playerCards[0]));
            cardImage_p2.SetImageResource(GetDeckImage(playerCards[1]));

            return new Tuple<List<int>, List<int>>(dealerCards, playerCards);
        }

        /// <summary>
        /// Get the total value of a given hand
        /// </summary>
        /// <param name="hand">List of cids representing the cards in the player's hand</param>
        /// <returns>A tuple representing the greatest possible hand value (e.g. A + 7 = 18) and
        /// whether or not the hand is soft (e.g. A + 7: isSoft = true)
        /// </returns>
        private Tuple<int, bool> GetHandValue(List<int> hand)
        {
            var value = 0;
            var aceElevenCount = 0; // number of aces counting as 11s
            var aceOneCount = 0; // number of aces counting as 1s

            // Add up the total, tracking whether the hand is soft or hard
            foreach (int cid in hand)
            {
                bool isAce;
                value += GetCardValue(cid, out isAce);

                if (isAce)
                    aceElevenCount++;

                // If this card causes the total to exceed 21, see if you can treat any aces as 1s to save the hand
                if (value > 21 && aceElevenCount > 0)
                {
                    aceElevenCount--;
                    aceOneCount++;
                    value -= 10;
                }
            }

            // A hand is soft if there are any aces counting as 11 (which could later count as 1s)
            var isSoft = aceElevenCount > 0;
            return new Tuple<int, bool>(value, isSoft);
        }

        /// <summary>
        /// Get the value this card has in the game. 
        /// If the rank is 2-10, then value = rank.
        /// If the rank is J-K (11-13), value = 10.
        /// If the rank is A (14), value = 11 and return isAce = true to indicate it could be a 1 as well.
        /// </summary>
        /// <param name="cid">Given cardId</param>
        /// <param name="isAce">Out param indicating if the card is an ace</param>
        /// <returns>Value this card has in the game and a flag iff this card is an ace.</returns>
        private int GetCardValue(int cid, out bool isAce)
        {
            isAce = false;

            // Calculate ranks 2 through 14 (2 = 2, 3 = 3... 10 = 10, J = 11, Q = 12, K = 13, A = 14)
            var rank = ((cid % 52) / 4) + 2;

            if (rank <= 10)
                return rank;
            else if (rank <= 13)
                return 10; // Face card
            else
            {
                isAce = true;
                return 11; // Ace
            }
        }

        private int GetDeckImage(int cid)
        {
            // Card Logic:
            // For n decks there are 52*n cards. Each card gets a cid from 0 to 52*n.
            // From this cid you can deduce info about the card. Note: division truncates.
            // homeDeck = cid / n  (decks are numbered 0...n)
            // row = (cid % 52)/ 4 (rows are numbered 0...12 - each rank has its own row)
            // col = (cid % 52) % 4 = cid % 4 (cols are numbered 0...3 - each suit has its own col)
            return deckImages[(cid % 52) / 4, cid % 4];
        }

        // TODO: Deck should be a class and this should be a function on the class
        private int Draw(List<int> deck)
        {
            var randObj = new Random();

            // If the deck is empty, shuffle
            if (deck.Count == 0)
                deck = GetShuffledDeck(numDecks);

            // Pick the top card from the deck
            cid = deck[0];
            deck.Remove(cid);

            return cid;
        }

        private List<int> GetShuffledDeck(int decks)
        {
            var randObj = new Random();

            // Start with an ordered deck
            var orderedDeck = new List<int>();
            for (int i = 0; i < 52 * decks; i++)
                orderedDeck.Add(i);

            // Now select cards randomly from the ordered deck to build the shuffled deck
            var shuffledDeck = new List<int>();
            while (orderedDeck.Count > 0)
            {
                // Pick a random number from 0 to orderedDeck.Count - 1. Remove that card and put it in the shuffled deck
                var randIndex = randObj.Next(orderedDeck.Count);
                var randCard = orderedDeck[randIndex];
                orderedDeck.Remove(randCard);
                shuffledDeck.Add(randCard);
            }

            return shuffledDeck;
        }

        /// <summary>
        /// Getter for the images representing each card
        /// </summary>
        /// <returns>Returns a 2D array representing images of one deck (13 rows, 4 columns)</returns>
        private static int[,] GetDeckImages()
        {
            int[,] deck = new int[13, 4]
            {
                {  Resource.Drawable._2c, Resource.Drawable._2d, Resource.Drawable._2h, Resource.Drawable._2s },
                {  Resource.Drawable._3c, Resource.Drawable._3d, Resource.Drawable._3h, Resource.Drawable._3s },
                {  Resource.Drawable._4c, Resource.Drawable._4d, Resource.Drawable._4h, Resource.Drawable._4s },
                {  Resource.Drawable._5c, Resource.Drawable._5d, Resource.Drawable._5h, Resource.Drawable._5s },
                {  Resource.Drawable._6c, Resource.Drawable._6d, Resource.Drawable._6h, Resource.Drawable._6s },
                {  Resource.Drawable._7c, Resource.Drawable._7d, Resource.Drawable._7h, Resource.Drawable._7s },
                {  Resource.Drawable._8c, Resource.Drawable._8d, Resource.Drawable._8h, Resource.Drawable._8s },
                {  Resource.Drawable._9c, Resource.Drawable._9d, Resource.Drawable._9h, Resource.Drawable._9s },
                {  Resource.Drawable._Tc, Resource.Drawable._Td, Resource.Drawable._Th, Resource.Drawable._Ts },
                {  Resource.Drawable._Jc, Resource.Drawable._Jd, Resource.Drawable._Jh, Resource.Drawable._Js },
                {  Resource.Drawable._Qc, Resource.Drawable._Qd, Resource.Drawable._Qh, Resource.Drawable._Qs },
                {  Resource.Drawable._Kc, Resource.Drawable._Kd, Resource.Drawable._Kh, Resource.Drawable._Ks },
                {  Resource.Drawable._Ac, Resource.Drawable._Ad, Resource.Drawable._Ah, Resource.Drawable._As },
            };

            return deck;
        }
    }
}

