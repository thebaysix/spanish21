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
        const int numDecks = 1;

        // Initializations
        ImageView cardImage;
        TextView textView;
        int cid = -1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Init
            base.OnCreate(savedInstanceState);
            var randObj = new Random();
            var deckImages = GetDeckImages();

            // Shuffle deck
            var deck = GetShuffledDeck(numDecks);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            cardImage = FindViewById<ImageView>(Resource.Id.cardImage);
            textView = FindViewById<TextView>(Resource.Id.textView);
            FindViewById<Button>(Resource.Id.newCardBtn).Click += (o, e) =>
            {
                // Pick the top card from the deck
                var randIndex = randObj.Next(deck.Count);
                cid = deck[randIndex];
                deck.Remove(cid);

                // Card Logic:
                // For n decks there are 52*n cards. Each card gets a cid from 0 to 52*n.
                // From this cid you can deduce info about the card. Note: division truncates.
                // homeDeck = cid / n  (decks are numbered 0...n)
                // rank = (cid % 52)/ 4 (ranks are numbered 0...12)
                // suit = (cid % 52) % 4 = cid % 4 (suits are numbered 0...3)
                var rank = (cid % 52) / 4;
                var suit = cid % 4;

                // Display the image
                cardImage.SetImageResource(deckImages[rank, suit]);

                // Display deck info
                textView.SetText("deckCount: " + deck.Count, TextView.BufferType.Normal);
            };
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
        private int[,] GetDeckImages()
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

