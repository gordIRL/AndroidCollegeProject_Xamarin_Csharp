﻿using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CurrencyAlertApp.DataAccess;


namespace CurrencyAlertApp
{
    // Adapter to connect the data set (photo album) to the RecyclerView: 
    public class UserAlerts_RecycleAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;

        // Underlying data set (a List<newsObject>):
        public List<UserAlert> mUserAlertList;

        // Load the adapter with the data set (List<newsObject>) at construction time:
        public UserAlerts_RecycleAdapter(List<UserAlert> userAlertList)
        {
            mUserAlertList = userAlertList;
        }


        // Create a new views / photo CardView (invoked by the layout manager): 
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Setup your layout here
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.UserAlert_CardView, parent, false);

            // Create a ViewHolder to find and hold these view references, and 
            // register OnClick with the view holder:
            //NewsObject_ViewHolder vh = new NewsObject_ViewHolder(itemView, OnClick); (original)
            var vh = new UserAlert_RecycleAdapter_NEW_ViewHolder(itemView, OnClick);       // , OnLongClick
            return vh;
        }


        // Fill in the contents of a view / the photo card (invoked by the layout manager):
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            // Replace the contents of the view with that element          
            UserAlert_RecycleAdapter_NEW_ViewHolder vh = viewHolder as UserAlert_RecycleAdapter_NEW_ViewHolder;

            // Set the ImageView and TextView in this ViewHolder's CardView 
            // from this position in the List<newsObject> (photo album)

            //  Assign content (get currency icon 1st)
            string countryChar = mUserAlertList[position].CountryChar.ToString().ToUpper();
            int imageID = GetImageForCurrency(countryChar);
            vh.Icon.SetImageResource(imageID);

            //  Assign content - continued
            vh.Caption1.Text = mUserAlertList[position].CountryChar + ": " + mUserAlertList[position].MarketImpact;
            vh.Caption2.Text = mUserAlertList[position].DateAndTime.ToString("dd/MM/yyyy") + ":  "
                    + mUserAlertList[position].DateAndTime.ToString("HH:mmtt") + "\n"
                    + mUserAlertList[position].Title + ":" + 
                    "\n\n" + mUserAlertList[position].DescriptionOfPersonalEvent;
        }



        // Return the number of items in list
        public override int ItemCount
        {
            get { return mUserAlertList.Count; }
        }




        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);    // ItemClick?.Invoke(this, position);  (simplified delegate)
        }



        public int GetImageForCurrency(string currentCurrency)
        {
            int imageID = 0;

            switch (currentCurrency)
            {
                case "USD":
                    imageID = Resource.Mipmap.united_states;
                    break;
                case "GBP":
                    imageID = Resource.Mipmap.united_kingdom;
                    break;
                case "CAD":
                    imageID = Resource.Mipmap.canada;
                    break;
                case "JPY":
                    imageID = Resource.Mipmap.japan;
                    break;
                case "CNY":
                    imageID = Resource.Mipmap.china;
                    break;
                case "NZD":
                    imageID = Resource.Mipmap.new_zealand;
                    break;
                case "AUD":
                    imageID = Resource.Mipmap.australia;
                    break;
                case "CHF":
                    imageID = Resource.Mipmap.switzerland;
                    break;
                case "EUR":
                    imageID = Resource.Mipmap.european_union;
                    break;
                default:
                    imageID = Resource.Mipmap.globe;
                    break;
            }
            return imageID;
        }
    }


    public class UserAlert_RecycleAdapter_NEW_ViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public ImageView Icon { get; private set; }
        public TextView Caption1 { get; private set; }
        public TextView Caption2 { get; private set; }

        // Get references to the views defined in the CardView layout.        
        public UserAlert_RecycleAdapter_NEW_ViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            // Locate and cache view references:

            Icon = itemView.FindViewById<ImageView>(Resource.Id.img_1_UserAlert_CardView);
            //Icon = itemView.FindViewById<ImageView>(Resource.Id.img_1_News_CardView);

            Caption1 = itemView.FindViewById<TextView>(Resource.Id.txt_1_UserAlert_CardView);
            //Caption1 = itemView.FindViewById<TextView>(Resource.Id.txt_1_News_CardView);

            Caption2 = itemView.FindViewById<TextView>(Resource.Id.txt_2_UserAlert_CardView);
            //Caption2 = itemView.FindViewById<TextView>(Resource.Id.txt_2_News_CardView);

            // Detect user clicks on the item view and report which item was clicked 
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }
}