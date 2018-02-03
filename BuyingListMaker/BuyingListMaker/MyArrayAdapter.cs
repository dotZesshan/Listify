using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace BuyingListMaker
{
    public class MyArrayAdapter : BaseAdapter
    {
        private readonly List<string> _items;
        private readonly Activity _context;
        private readonly List<int> _markingList;
        private List<int> _priceList;

        public MyArrayAdapter(Activity context, List<string> items, List<int> markingList, List<int> priceList) : base()
        {
            _context = context;
            _markingList = new List<int>( markingList);
            _items = new List<string>(items);
            _priceList = new List<int>(priceList);
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
                    view = _context.LayoutInflater.Inflate(Resource.Layout.MyListLayout1, parent, false);
                }
                
                var nameView = view.FindViewById<TextView>(Resource.Id.ItemName);
                var priceView = view.FindViewById<TextView>(Resource.Id.ItemPrice);
                if (_markingList != null && _markingList.Count>position && _markingList[position] > 0)
                {
                    nameView.Typeface = ItemsActivity.STRIKE_THROUGH;
                }
                else
                {
                    nameView.Typeface = Typeface.Default;
                }
                nameView.Text = _items[position];
                if (_priceList != null ) priceView.Text = _priceList[position] > 0 ? Convert.ToString(_priceList[position]) : "0";
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

        public int GetListCount()
        {
            return _items.Count;
        }

        public void AddMarkMap(int mark)
        {
            _markingList?.Add(mark * (_markingList.Count+1));
        }

        public void SetMarkMap(int position, int mark)
        {
            if(_markingList != null)
                _markingList[position] = mark;
        }

        public void RemoveMarkMap(int position)
        {
            _markingList?.RemoveAt(position);
        }

        public List<int> GetAllMarksMapping()
        {
            return _markingList;
        }

        public void AddPrice(int value)
        {
            _priceList?.Add(value);
        }

        public void SetPrice(int position, int value)
        {
            if (_priceList != null)
                _priceList[position] = value;
        }

        public int GetPrice(int position)
        {
            if(_priceList != null)
                return _priceList.Count > position ? _priceList[position] : 0;
            return 0;
        }

        public void RemovePrice(int position)
        {
            _priceList?.RemoveAt(position);
        }

        public List<int> GetAllPriceItems()
        {
            return _priceList;
        }

        public int GetPriceTotal()
        {
            if (_priceList != null)
                return Convert.ToInt32(_priceList.Sum(item => item));
            return 0;
        }

        public void ChangeTotalPriceList(List<int> priceList)
        {
            _priceList = new List<int>(priceList);
        }
    }
}