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
using Android.Text;
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
        private TextView _totalTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.Items);
            
            _listName = Intent.GetStringExtra("ListName") ?? "Data not available";
            this.Title =  "Category: " + _listName;
        }

        protected override void OnPause()
        {
            base.OnPause();
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString(_listName, string.Join(",", _adapter.GetAllItems()));
            editor.PutString(_listName + "MarkingList", string.Join(",", _adapter.GetAllMarksMapping()));
            editor.PutString(_listName + "PriceList", string.Join(",", _adapter.GetAllPriceItems()));
            //editor.PutString(_listName + "total", Convert.ToString(_adapter.GetPriceTotal()));
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
            var items = storedItemListData ?? new List<string>();

            var markingListString = prefs.GetString(_listName + "MarkingList", null);
            var storedMarkingListData = !string.IsNullOrEmpty(markingListString) ? markingListString.Split(new[] { "," }, StringSplitOptions.None).Select(s => Convert.ToInt32(s)).ToList() : null;
            var markingElements = storedMarkingListData ?? new List<int>();

            var priceListString = prefs.GetString(_listName + "PriceList", null);
            var priceListData = !string.IsNullOrEmpty(priceListString) ? priceListString.Split(new[] { "," }, StringSplitOptions.None).Select(s => Convert.ToInt32(s)).ToList() : null;
            var priceList = priceListData ?? new List<int>();
            
            _listView = FindViewById<ListView>(Resource.Id.ItemListView);
            _button = FindViewById<Button>(Resource.Id.ItemButton);
            _button.Click += ButtonOnClick;
            _listView.Adapter = _adapter = new MyArrayAdapter(this, items, markingElements, priceList);
            _listView.ItemClick += ListViewOnItemClick;
            _listView.ItemLongClick += ListViewOnItemLongClick;
            _totalTextView = FindViewById<TextView>(Resource.Id.TotalValue);
            _totalTextView.Text = Convert.ToString(_adapter.GetPriceTotal());
            FindViewById<TextView>(Resource.Id.PriceEditText).InputType = Android.Text.InputTypes.ClassNumber;
        }

        private void ListViewOnItemLongClick(object sender, AdapterView.ItemLongClickEventArgs itemLongClickEventArgs)
        {
            try
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                var optionBox = builder.Create();
                optionBox.SetTitle("Choose Operation");
                optionBox.SetIcon(Resource.Drawable.Icon);
                optionBox.SetMessage("Select one option");
                var view = optionBox.LayoutInflater.Inflate(Resource.Layout.ItemOptions, null);
                optionBox.SetView(view);
                optionBox.SetCancelable(false);
                view.FindViewById<Button>(Resource.Id.NameEditButton).Click += new EventHandler((s, e) => EditNameOptionClicked(s, e, itemLongClickEventArgs.Position, optionBox));
                view.FindViewById<Button>(Resource.Id.PriceEditButton).Click += new EventHandler((s, e) => EditPriceOptionClicked(s, e, itemLongClickEventArgs.Position, optionBox));
                view.FindViewById<Button>(Resource.Id.ItemDeleteButton).Click += new EventHandler((s, e) => DeleteOptionClicked(s, e, itemLongClickEventArgs.Position, optionBox));
                optionBox.SetButton("Cancle", (o, args) => { });
                optionBox.Show();
            }
            catch (Exception ex)
            {
                Log.Error("Exception:", ex.Message +  "|StackTrace:"+ex.StackTrace);
            }
        }

        private void EditNameOptionClicked(object s, EventArgs e, int position, AlertDialog optionBox)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            var editBox = builder.Create();
            editBox.SetTitle("Update Name");
            editBox.SetIcon(Resource.Drawable.Icon);
            editBox.SetMessage("Enter New Name: ");
            var view = editBox.LayoutInflater.Inflate(Resource.Layout.LimitedInputBoxString, null);
            var inputTextField = view.FindViewById<EditText>(Resource.Id.LimitedEditTextString);
            editBox.SetView(view);
            editBox.SetCancelable(false);

            editBox.SetButton("Done", (o, args) =>
            {
            var oldListName = (string)_listView.GetItemAtPosition(position);
                if (!string.IsNullOrEmpty(oldListName))
                {
                    var newListName = inputTextField.Text;
                    if (!string.IsNullOrEmpty(newListName))
                    {
                        _adapter.SetItemName(position, newListName);
                        RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                    }
                }
            });
            editBox.SetButton2("Cancle", (o, args) => { });
            editBox.Show();
            optionBox.Cancel();
        }

        private void EditPriceOptionClicked(object s, EventArgs e, int position, AlertDialog optionBox)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            var editBox = builder.Create();
            editBox.SetTitle("Update Price");
            editBox.SetIcon(Resource.Drawable.Icon);
            editBox.SetMessage("Enter New Price: ");
            var view = editBox.LayoutInflater.Inflate(Resource.Layout.LimitedInputBoxNumber, null);
            var inputTextField = view.FindViewById<EditText>(Resource.Id.LimitedEditTextNumber);
            inputTextField.InputType = Android.Text.InputTypes.ClassNumber;
            editBox.SetView(view);
            editBox.SetCancelable(false);

            editBox.SetButton("Done", (o, args) =>
            {
                var item = _listView.GetItemAtPosition(position);
                if (item != null)
                {
                    _adapter.SetPrice(position, !string.IsNullOrEmpty(inputTextField.Text) ? Convert.ToInt32(inputTextField.Text) : 0);
                    _totalTextView.Text = Convert.ToString(_adapter.GetPriceTotal());
                    RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                }
            });
            editBox.SetButton2("Cancle", (o, args) => { });
            editBox.Show();
            optionBox.Cancel();
        }

        private void DeleteOptionClicked(object s, EventArgs e, int position, AlertDialog optionBox)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            var deleteBox = builder.Create();
            deleteBox.SetTitle("Delete");
            deleteBox.SetIcon(Resource.Drawable.Icon);
            deleteBox.SetMessage("Do you want to delete this item?");
            deleteBox.SetCancelable(false);
            deleteBox.SetButton("Yes", (p, argsp) =>
            {
                var item = _listView.GetItemAtPosition(position);
                if (item != null)
                {
                    _adapter.RemovePrice(position);
                    _adapter.RemoveMarkMap(position);
                    _adapter.RemoveItem(position);
                    RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                }
            });
            deleteBox.SetButton2("No", (p, argsp) => { });
            deleteBox.Show();
            optionBox.Cancel();
        }

        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var inputTextField = FindViewById<TextView>(Resource.Id.ItemEditText);
                var priceTextFeild = FindViewById<TextView>(Resource.Id.PriceEditText); 
                var text = inputTextField.Text;
                var price = !string.IsNullOrEmpty(priceTextFeild.Text) ? Convert.ToInt32(priceTextFeild.Text) : 0;
                if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                {
                    _adapter.AddPrice(price);
                    _adapter.AddMarkMap(-1);
                    _adapter.AddItem(text);
                    _totalTextView.Text = Convert.ToString(_adapter.GetPriceTotal());
                    RunOnUiThread(() =>
                    {
                        _adapter.NotifyDataSetChanged();
                    });
                    inputTextField.Text = string.Empty;
                    priceTextFeild.Text = string.Empty;
                    Toast.MakeText(this, "List Added:" + text, ToastLength.Short).Show();
                }

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
                var view = _adapter.GetView(itemClickEventArgs.Position, itemClickEventArgs.View, null);
                var textViewName = (TextView)view.FindViewById<TextView>(Resource.Id.ItemName);
                var textViewPrice = (TextView)view.FindViewById<TextView>(Resource.Id.ItemPrice);
                Typeface font = Typeface.Default;
                _adapter.SetMarkMap(itemClickEventArgs.Position, -1 * (itemClickEventArgs.Position + 1));
                if (textViewName.Typeface == Typeface.Default)
                {
                    _adapter.SetMarkMap(itemClickEventArgs.Position, 1 * (itemClickEventArgs.Position + 1));
                    font = STRIKE_THROUGH;
                }
                textViewName.Typeface = font;
                RunOnUiThread(() =>
                {
                    _adapter.NotifyDataSetChanged();
                });
                Toast toast = Toast.MakeText(this, "Item:" + item.ToString() + "| Price:"+ textViewPrice.Text, ToastLength.Short);
                toast.Show();
            }
        }
    }
}