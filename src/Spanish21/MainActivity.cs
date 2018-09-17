using Android.App;
using Android.Widget;
using Android.OS;
using System;

namespace Spanish21
{
    [Activity(Label = "Spanish21", MainLauncher = true)]
    public class MainActivity : Activity
    {
        ImageView cardImage;
        int cardId = -1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Init
            base.OnCreate(savedInstanceState);
            Random randObj = new Random();
            int[] deck = InializeDeck();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            cardImage = FindViewById<ImageView>(Resource.Id.cardImage);

            FindViewById<Button>(Resource.Id.newCardBtn).Click += (o, e) =>
            {
                cardId = randObj.Next(52);
                cardImage.SetImageResource(deck[cardId]);
            };
        }

        private int[] InializeDeck()
        {
            int[] deck = new int[]
            {
                Resource.Drawable._2c,
                Resource.Drawable._2d,
                Resource.Drawable._2h,
                Resource.Drawable._2s,
                Resource.Drawable._3c,
                Resource.Drawable._3d,
                Resource.Drawable._3h,
                Resource.Drawable._3s,
                Resource.Drawable._4c,
                Resource.Drawable._4d,
                Resource.Drawable._4h,
                Resource.Drawable._4s,
                Resource.Drawable._5c,
                Resource.Drawable._5d,
                Resource.Drawable._5h,
                Resource.Drawable._5s,
                Resource.Drawable._6c,
                Resource.Drawable._6d,
                Resource.Drawable._6h,
                Resource.Drawable._6s,
                Resource.Drawable._7c,
                Resource.Drawable._7d,
                Resource.Drawable._7h,
                Resource.Drawable._7s,
                Resource.Drawable._8c,
                Resource.Drawable._8d,
                Resource.Drawable._8h,
                Resource.Drawable._8s,
                Resource.Drawable._9c,
                Resource.Drawable._9d,
                Resource.Drawable._9h,
                Resource.Drawable._9s,
                Resource.Drawable._Tc,
                Resource.Drawable._Td,
                Resource.Drawable._Th,
                Resource.Drawable._Ts,
                Resource.Drawable._Jc,
                Resource.Drawable._Jd,
                Resource.Drawable._Jh,
                Resource.Drawable._Js,
                Resource.Drawable._Qc,
                Resource.Drawable._Qd,
                Resource.Drawable._Qh,
                Resource.Drawable._Qs,
                Resource.Drawable._Kc,
                Resource.Drawable._Kd,
                Resource.Drawable._Kh,
                Resource.Drawable._Ks,
                Resource.Drawable._Ac,
                Resource.Drawable._Ad,
                Resource.Drawable._Ah,
                Resource.Drawable._As,
            };

            return deck;
        }
    }
}

