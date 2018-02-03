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
        private int _position = -1;
        private TextView _totalTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.Items);
            
            _listName = Intent.GetStringExtra("ListName") ?? "Data not available";
            _position = Intent.GetIntExtra("Position", -1);
            this.Title =  "MyList: " + _listName;
        }

        protected override void OnPause()
        {
            base.OnPause();
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();

            var saveName = _position != -1 ? _listName + _position : _listName;
            editor.PutString(saveName, string.Join(",", _adapter.GetAllItems()));
            editor.PutString(saveName + "MarkingList", string.Join(",", _adapter.GetAllMarksMapping()));
            editor.PutString(saveName + "PriceList", string.Join(",", _adapter.GetAllPriceItems()));
            //editor.PutString(saveName + "total", Convert.ToString(_adapter.GetPriceTotal()));
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

            var saveName = _position != -1 ? _listName + _position : _listName;

            var storedItemString = prefs.GetString(saveName, null);
            var storedItemListData = !string.IsNullOrEmpty(storedItemString) ? storedItemString.Split(new[] { "," }, StringSplitOptions.None).ToList() : null;
            var items = storedItemListData ?? new List<string>();

            var markingListString = prefs.GetString(saveName + "MarkingList", null);
            var storedMarkingListData = !string.IsNullOrEmpty(markingListString) ? markingListString.Split(new[] { "," }, StringSplitOptions.None).Select(s => Convert.ToInt32(s)).ToList() : null;
            var markingElements = storedMarkingListData ?? new List<int>();

            var priceListString = prefs.GetString(saveName + "PriceList", null);
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
            if(_adapter.GetListCount() <= 0)
                        FindViewById<View>(Resource.Id.TotalLayout).Visibility = ViewStates.Invisible;
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
                //optionBox.SetCancelable(false);
                view.FindViewById<Button>(Resource.Id.EditItemButton).Click += new EventHandler((s, e) => EditOptionClicked(s, e, itemLongClickEventArgs.Position, optionBox));
                view.FindViewById<Button>(Resource.Id.ItemDeleteButton).Click += new EventHandler((s, e) => DeleteOptionClicked(s, e, itemLongClickEventArgs.Position, optionBox));
                optionBox.SetButton("Cancle", (o, args) => { });
                optionBox.Show();
            }
            catch (Exception ex)
            {
                Log.Error("Exception:", ex.Message +  "|StackTrace:"+ex.StackTrace);
            }
        }

        private void EditOptionClicked(object s, EventArgs e, int position, AlertDialog optionBox)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            var editBox = builder.Create();
            editBox.SetTitle("Update Item");
            editBox.SetIcon(Resource.Drawable.Icon);
            editBox.SetMessage("Enter New Values: ");
            var view = editBox.LayoutInflater.Inflate(Resource.Layout.ItemEditLayout, null);
            var inputTextFieldName = view.FindViewById<EditText>(Resource.Id.EditedNameBox);
            var inputTextFieldPrice = view.FindViewById<EditText>(Resource.Id.EditedPriceBox);
            inputTextFieldPrice.InputType = Android.Text.InputTypes.ClassNumber;
            

            editBox.SetView(view);
            //editBox.SetCancelable(false);

            var oldListName = (string)_listView.GetItemAtPosition(position);
            var oldPrice = Convert.ToString(_adapter.GetPrice(position));

            inputTextFieldName.Text = oldListName;
            inputTextFieldPrice.Text = oldPrice;

            editBox.SetButton("Done", (o, args) =>
            {
                
                if (!string.IsNullOrEmpty(oldListName))
                {
                    var newListName = inputTextFieldName.Text;
                    if (!string.IsNullOrEmpty(newListName))
                    {
                        _adapter.SetItemName(position, newListName);
                    }
                    _adapter.SetPrice(position, !string.IsNullOrEmpty(inputTextFieldPrice.Text) ? Convert.ToInt32(inputTextFieldPrice.Text) : 0);
                    RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                    _totalTextView.Text = Convert.ToString(_adapter.GetPriceTotal());
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
            //deleteBox.SetCancelable(false);
            deleteBox.SetButton("Yes", (p, argsp) =>
            {
                var item = _listView.GetItemAtPosition(position);
                if (item != null)
                {
                    _adapter.RemovePrice(position);
                    _adapter.RemoveMarkMap(position);
                    _adapter.RemoveItem(position);
                    
                    if (_adapter.GetListCount() <= 0)
                        FindViewById<View>(Resource.Id.TotalLayout).Visibility = ViewStates.Invisible;
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
                    FindViewById<View>(Resource.Id.TotalLayout).Visibility = ViewStates.Visible;
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