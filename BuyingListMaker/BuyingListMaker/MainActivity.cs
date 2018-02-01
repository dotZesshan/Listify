using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Android.OS;
using Android.Preferences;
using System.Threading;
using Android.Util;

namespace BuyingListMaker
{
    [Activity(Label = "Listify", MainLauncher = true, Icon = "@drawable/Listify")]
    public class MainActivity : Activity
    {
        ListView _listView;
        private Button _button;
        private MyArrayAdapter _adapter;
        private bool _init;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            //InitActivity();
        }

        protected override void OnPause()
        {
            base.OnPause();
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString("Lists", string.Join(",", _adapter.GetAllItems()));
            editor.Apply();    // applies changes synchronously on older APIs
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!_init)
            {
                InitActivity();
            }
            else
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                var priceList = _adapter.GetAllItems().Select(s => Convert.ToInt32(prefs.GetString(s + "total", "0"))).ToList();
                if (priceList != null)
                {
                    _adapter.ChangeTotalPriceList(priceList);
                }
                RunOnUiThread(() =>
                {
                    _adapter.NotifyDataSetChanged();
                });
            }
        }

        private void InitActivity()
        {
            _init = true;
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var storedItemString = prefs.GetString("Lists", null);
            var storedItemListData = !string.IsNullOrEmpty(storedItemString) ? storedItemString.Split(new[] { "," }, StringSplitOptions.None).ToList() : null;
            var items = storedItemListData ?? new List<string>();
            var priceList = items.Select(s => Convert.ToInt32(prefs.GetString(s + "total", "0"))).ToList();
            _listView = FindViewById<ListView>(Resource.Id.ListsView);
            _button = FindViewById<Button>(Resource.Id.ListButton);
            _button.Click += ButtonOnClick;
            _listView.Adapter = _adapter = new MyArrayAdapter(this, items, priceList);
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
                        _adapter.RemoveItem(itemLongClickEventArgs.Position);
                        _adapter.RemovePrice(itemLongClickEventArgs.Position);
                        RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                    }
                });
                alterDialog.SetButton2("No", (o, args) => { });
                alterDialog.Show();
            }
            catch (Exception ex)
            {
                Log.Error("Exception:", ex.Message + "|StackTrace:" + ex.StackTrace);
            }
        }

        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var inputTextField = FindViewById<TextView>(Resource.Id.ListEditText);
                var text = inputTextField.Text;
                if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                {
                    _adapter.AddItem(text);
                    _adapter.AddPrice(0);
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
                Log.Info("ListViewOnItemClick", item.ToString());
                var intent = new Intent(this, typeof(ItemsActivity));
                intent.PutExtra("ListName", item.ToString());
                StartActivity(intent);
            }
        }
    }
}

