using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BuyingListMaker
{
    class MyArrayAdapter1 : BaseAdapter
    {
        private readonly List<string> _items;
        private readonly Activity _context;

        public MyArrayAdapter1(Activity context, List<string> items) : base()
        {
            _context = context;
            _items = new List<string>(items);
        }

        public override long GetItemId(int position)
        {
            return Convert.ToInt64(position);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View view = convertView; // re-use an existing view, if one is available
                if (view == null) // otherwise create a new one
                {
                    view = _context.LayoutInflater.Inflate(Resource.Layout.MyListLayout2, parent, false);
                }
                var nameView = view.FindViewById<TextView>(Resource.Id.ListName);
                nameView.Text = _items[position];
                return view;
            }
            catch (System.Exception ex)
            {
                var msg = ex.ToString();
                throw;
            }
        }
        public override int Count
        {
            get { return _items.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return _items[position]; ;
        }

        public void AddItem(string text)
        {
            _items.Add(text);
        }

        public void RemoveItem(int position)
        {
            _items.RemoveAt(position);
        }

        public void SetItemName(int position, string newListName)
        {
            _items[position] = newListName;
        }

        public List<string> GetAllItems()
        {
            return _items;
        }
    }
}