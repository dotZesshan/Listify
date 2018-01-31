using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace BuyingListMaker
{
    public class MyArrayAdapter : BaseAdapter<string>
    {
        private readonly List<string> _items;
        private readonly Activity _context;
        private readonly List<int> _markingList;

        public MyArrayAdapter(Activity context, List<string> items) : base()
        {
            _context = context;
            _markingList = null;
            _items = new List<string>(items);
        }

        public MyArrayAdapter(Activity context, List<string> items, List<int> markingList) : base()
        {
            _context = context;
            _markingList = markingList;
            _items = new List<string>(items);
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View view = convertView; // re-use an existing view, if one is available
                if (view == null) // otherwise create a new one
                {
                    view = _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
                }

                if (_markingList != null && _markingList.Count>position && _markingList[position] > 0)
                {
                    view.FindViewById<TextView>(Android.Resource.Id.Text1).Typeface = ItemsActivity.STRIKE_THROUGH;
                }
                else
                {
                    view.FindViewById<TextView>(Android.Resource.Id.Text1).Typeface = Typeface.Default;
                }
                view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = _items[position];
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

        public override string this[int position]
        {
            get { return _items[position]; }
        }

        public void AddItem(string text)
        {
            _items.Add(text);
        }

        public void SetItem(int position, string text)
        {
            _items[position] = text;
        }

        public void RemoveItem(int position, Object item)
        {
            _items.RemoveAt(position);
        }

        public List<string> GetAllItems()
        {
            return _items;
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
    }
}