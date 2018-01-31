using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace BuyingListMaker
{
    [Activity(Label = "Listify")]
    public class ItemsActivity : Activity
    {
        ListView _listView;
        private Button _button;
        private MyArrayAdapter _adapter;
        public static Typeface STRIKE_THROUGH;
        private string _listName = null;
        private bool _init;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.Items);
            
            _listName = Intent.GetStringExtra("ListName") ?? "Data not available";
            this.Title = _listName + " Items";
        }

        protected override void OnPause()
        {
            base.OnPause();
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString(_listName, string.Join(",", _adapter.GetAllItems()));
            editor.PutString(_listName + "MarkingList", string.Join(",", _adapter.GetAllMarksMapping()));
            editor.Apply();    // applies changes synchronously on older APIs
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!_init)
            {
                InitActivity();
            }
        }

        private void InitActivity()
        {
            _init = true;
            STRIKE_THROUGH = Typeface.CreateFromAsset(Assets, "bptypewritestrikethrough.ttf");
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var storedItemString = prefs.GetString(_listName, null);
            var storedItemListData = !string.IsNullOrEmpty(storedItemString) ? storedItemString.Split(new[] { "," }, StringSplitOptions.None).ToList() : null;
            var markingListString = prefs.GetString(_listName + "MarkingList", null);
            var storedMarkingListData = !string.IsNullOrEmpty(markingListString) ? markingListString.Split(new[] { "," }, StringSplitOptions.None).Select(s => Convert.ToInt32(s)).ToList() : null;
            var items = storedItemListData ?? new List<string>();
            var markingElements = storedMarkingListData ?? new List<int>();

            _listView = FindViewById<ListView>(Resource.Id.ItemListView);
            _button = FindViewById<Button>(Resource.Id.ItemButton);
            _button.Click += ButtonOnClick;
            _listView.Adapter = _adapter = new MyArrayAdapter(this, items, markingElements);
            _listView.ItemClick += ListViewOnItemClick;
            _listView.ItemLongClick += ListViewOnItemLongClick;

        }

        private void ListViewOnItemLongClick(object sender, AdapterView.ItemLongClickEventArgs itemLongClickEventArgs)
        {
            try
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                var alterDialog = builder.Create();
                alterDialog.SetTitle("Delete");
                alterDialog.SetIcon(Resource.Drawable.Icon);
                alterDialog.SetMessage("Do you want to delete this item?");
                alterDialog.SetCancelable(false);
                alterDialog.SetButton("Yes", (o, args) =>
                {
                    var item = _listView.GetItemAtPosition(itemLongClickEventArgs.Position);
                    if (item != null)
                    {
                        _adapter.RemoveItem(itemLongClickEventArgs.Position, item);
                        RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                    }
                });
                alterDialog.SetButton2("No", (o, args) => { });
                alterDialog.Show();
            }
            catch (Exception ex)
            {
                Log.Error("Exception:", ex.Message +  "|StackTrace:"+ex.StackTrace);
            }
        }

        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var inputTextField = FindViewById<TextView>(Resource.Id.ItemEditText);
                var text = inputTextField.Text;
                if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                {
                    _adapter.AddMarkMap(-1);
                    _adapter.AddItem(text);
                    RunOnUiThread(() =>
                    {
                        _adapter.NotifyDataSetChanged();
                    });
                    inputTextField.Text = string.Empty;
                    Toast.MakeText(this, "List Added:" + text, ToastLength.Short).Show();
                }

                #region Commented useful code
                //AlertDialog.Builder builder = new AlertDialog.Builder(this);
                //var alterDialog = builder.Create();
                //alterDialog.SetTitle("AddItem");
                //alterDialog.SetIcon(Resource.Drawable.Icon);
                //alterDialog.SetMessage("Enter Item: ");
                //var inputTextField = new EditText(this);
                //inputTextField.SetTextSize(ComplexUnitType.Dip, 17);
                //inputTextField.SetText("", TextView.BufferType.Editable);
                //alterDialog.SetView(inputTextField);
                //alterDialog.SetCancelable(false);

                //alterDialog.SetButton("AddItem", (o, args) =>
                //{
                //    if (!_items.Contains(inputTextField.Text))
                //    {
                //        _items.AddItem(inputTextField.Text);
                //        _adapter.AddItem(inputTextField.Text);
                //        RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                //    }
                //    else
                //    {
                //        Toast.MakeText(this, "Item already exist!", ToastLength.Short).Show();
                //    }
                //});
                //alterDialog.SetButton2("Cancle", (o, args) => { });
                //alterDialog.Show(); 
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error("Exception:", ex.Message + "|StackTrace:" + ex.StackTrace);
            }
        }

        private void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            var item = _listView.GetItemAtPosition(itemClickEventArgs.Position);
            if (item != null)
            {
                var textView = (TextView)_adapter.GetView(itemClickEventArgs.Position, itemClickEventArgs.View, null);
                Typeface font = Typeface.Default;
                _adapter.SetMarkMap(itemClickEventArgs.Position, -1 * (itemClickEventArgs.Position + 1));
                if (textView.Typeface == Typeface.Default)
                {
                    _adapter.SetMarkMap(itemClickEventArgs.Position, 1 * (itemClickEventArgs.Position + 1));
                    font = STRIKE_THROUGH;
                }
                textView.Typeface = font;
                RunOnUiThread(() =>
                {
                    _adapter.NotifyDataSetChanged();
                });
                Toast toast = Toast.MakeText(this, item.ToString(), ToastLength.Short);
                toast.Show();
            }
        }
    }
}