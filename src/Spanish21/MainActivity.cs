﻿using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Spanish21
{
    [Activity(Label = "Spanish21", MainLauncher = true)]
    public class MainActivity : Activity
    {
        // Constants
        private const int numDecks = 1;
        private const int numSuits = 4;
        private const int numRanks = 13;
        private const int cardsPerDeck = numSuits * numRanks;

        // Declarations
        private List<ImageView> dealerCardImages, playerCardImages;
        private LinearLayout dealerLayout, mainLayout, playerLayout;
        private TextView recordText, scoreText;
        private int numWins = 0, numLosses = 0;
        private int cid = -1;
        private Random randObj = new Random();
        private readonly int[,] deckImages = GetDeckImages();
        private List<int> dealerHand, playerHand;

        private enum GameResult { Win = 0, Loss = 1, Tie = 2 }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Init OnCreate
            base.OnCreate(savedInstanceState);

            // Initialize game logic items:

            // Shuffle deck
            var deck = GetShuffledDeck(numDecks);

            // Hands
            dealerHand = new List<int>();
            playerHand = new List<int>();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Initialize UI items:

            // Images
            dealerCardImages = new List<ImageView>();
            playerCardImages = new List<ImageView>();

            // Layouts
            dealerLayout = FindViewById<LinearLayout>(Resource.Id.dealerLayout);
            mainLayout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
            playerLayout = FindViewById<LinearLayout>(Resource.Id.playerLayout);

            // Text
            recordText = FindViewById<TextView>(Resource.Id.recordText);
            recordText.SetText("Wins: 0, Losses: 0", TextView.BufferType.Normal);
            scoreText = FindViewById<TextView>(Resource.Id.scoreText);

            // Action: Hit
            FindViewById<Button>(Resource.Id.hitBtn).Click += (o, e) =>
            {
                // Add a card to the player cards
                var newCardResult = DrawNewImageAndCard(deck);
                playerCardImages.Add(newCardResult.Item1);
                playerLayout.AddView(newCardResult.Item1);
                playerHand.Add(newCardResult.Item2);

                // See if the player busted
                var playerTotal = GetHandValue(playerHand).Item1;
                if (playerTotal > 21)
                {
                    var resultText = "You bust";
                    var dealerTotal = GetHandValue(dealerHand).Item1;
                    UpdateResultInfo(resultText, GameResult.Loss, dealerTotal, playerTotal);
                }
            };

            // Action: Stand
            FindViewById<Button>(Resource.Id.standBtn).Click += (o, e) =>
            {
                // Reveal the dealer's secret card
                dealerCardImages[0].SetImageResource(GetDeckImage(dealerHand[0]));

                // Follow dealer protocol (stand on 17+, hit on soft 17)
                var dealerHandValue = GetHandValue(dealerHand);
                var dealerTotal = dealerHandValue.Item1;
                var isSoft = dealerHandValue.Item2;
                while (dealerTotal < 17 || (dealerTotal == 17 && isSoft))
                {
                    // Pause for suspense
                    Thread.Sleep(1000);

                    // Add a card to the dealer cards
                    var newCardResult = DrawNewImageAndCard(deck);
                    dealerCardImages.Add(newCardResult.Item1);
                    dealerLayout.AddView(newCardResult.Item1);
                    dealerHand.Add(newCardResult.Item2);

                    // Update dealer hand info
                    dealerHandValue = GetHandValue(dealerHand);
                    dealerTotal = dealerHandValue.Item1;
                    isSoft = dealerHandValue.Item2;
                }

                // Compare the hand totals
                var playerTotal = GetHandValue(playerHand).Item1;

                var resultText = "";
                GameResult gameResult;
                if (dealerTotal > 21)
                {
                    resultText = "Dealer busts, you win!";
                    gameResult = GameResult.Win;
                }
                else if (dealerTotal > playerTotal)
                {
                    resultText = "You lose";
                    gameResult = GameResult.Loss;
                }
                else if (playerTotal > dealerTotal)
                {
                    resultText = "You win!";
                    gameResult = GameResult.Win;
                }
                else
                {
                    resultText = "Tie";
                    gameResult = GameResult.Tie;
                }

                UpdateResultInfo(resultText, gameResult, dealerTotal, playerTotal);
            };

            // Action: Deal
            FindViewById<Button>(Resource.Id.dealBtn).Click += (o, e) =>
            {
                Deal(deck);
            };
        }

        private void UpdateResultInfo(string resultTextInfo, GameResult gameResult, int dealerTotal, int playerTotal)
        {
            switch (gameResult)
            {
                case GameResult.Win:
                    numWins++;
                    break;
                case GameResult.Loss:
                    numLosses++;
                    break;
                default:
                    break;
            }

            scoreText.SetText(resultTextInfo + " D: " + dealerTotal + ", P: " + playerTotal, TextView.BufferType.Normal);
            recordText.SetText("Wins: " + numWins + ", Losses: " + numLosses, TextView.BufferType.Normal);
        }

        private void Deal(List<int> deck)
        {
            // Cleanup any existing cardImages
            // TODO: figure out how to remove old images properly
            foreach (var dealerCardImage in dealerCardImages)
                dealerCardImage.Visibility = Android.Views.ViewStates.Gone;
            foreach (var playerCardImage in playerCardImages)
                playerCardImage.Visibility = Android.Views.ViewStates.Gone;

            var newDealerCardResult1 = DrawNewImageAndCard(deck, false);
            var newDealerCardResult2 = DrawNewImageAndCard(deck);
            dealerCardImages.Clear();
            dealerCardImages.Add(newDealerCardResult1.Item1);
            dealerCardImages.Add(newDealerCardResult2.Item1);
            dealerLayout.AddView(newDealerCardResult1.Item1);
            dealerLayout.AddView(newDealerCardResult2.Item1);

            dealerHand.Clear();
            dealerHand.Add(newDealerCardResult1.Item2);
            dealerHand.Add(newDealerCardResult2.Item2);

            var newPlayerCardResult1 = DrawNewImageAndCard(deck);
            var newPlayerCardResult2 = DrawNewImageAndCard(deck);
            playerCardImages.Clear();
            playerCardImages.Add(newPlayerCardResult1.Item1);
            playerCardImages.Add(newPlayerCardResult2.Item1);
            playerLayout.AddView(newPlayerCardResult1.Item1);
            playerLayout.AddView(newPlayerCardResult2.Item1);

            playerHand.Clear();
            playerHand.Add(newPlayerCardResult1.Item2);
            playerHand.Add(newPlayerCardResult2.Item2);

            scoreText.SetText("", TextView.BufferType.Normal);
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
            var rank = ((cid % cardsPerDeck) / 4) + 2;

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
            // For n decks there are cardsPerDeck*n cards. Each card gets a cid from 0 to cardsPerDeck*n.
            // From this cid you can deduce info about the card. Note: division truncates.
            // homeDeck = cid / n  (decks are numbered 0...n)
            // row = (cid % cardsPerDeck)/ 4 (rows are numbered 0...12 - each rank has its own row)
            // col = (cid % cardsPerDeck) % 4 = cid % 4 (cols are numbered 0...3 - each suit has its own col)
            return deckImages[(cid % cardsPerDeck) / 4, cid % 4];
        }

        /// <summary>
        /// Wrapper around Draw function that also provides UI logic
        /// </summary>
        /// <param name="deck"></param>
        /// <returns>Pair of the image requested and the cid that image represents</returns>
        private Tuple<ImageView, int> DrawNewImageAndCard(List<int> deck, bool cardFaceUp = true)
        {
            var newCardImage = new ImageView(this);
            var newCid = Draw(deck);
            newCardImage.SetImageResource(cardFaceUp ? GetDeckImage(newCid) : Resource.Drawable.back);
            var layoutParams = new LinearLayout.LayoutParams(
                Android.Views.ViewGroup.LayoutParams.MatchParent,
                Android.Views.ViewGroup.LayoutParams.WrapContent);
            layoutParams.Weight = 1;

            // TODO: Figure out how to display new cards properly
            layoutParams.Width = 80;
            layoutParams.Height = 200;
            newCardImage.LayoutParameters = layoutParams;

            return new Tuple<ImageView, int>(newCardImage, newCid);
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
            for (int i = 0; i < cardsPerDeck * decks; i++)
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
        /// <returns>Returns a 2D array representing images of one deck</returns>
        private static int[,] GetDeckImages()
        {
            int[,] deck = new int[numRanks, numSuits]
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

