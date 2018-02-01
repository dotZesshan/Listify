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
using Android.Text;

namespace BuyingListMaker
{
    [Activity(Label = "Listify", MainLauncher = true, Icon = "@drawable/Listify")]
    public class MainActivity : Activity
    {
        ListView _listView;
        private Button _button;
        private MyArrayAdapter1 _adapter;
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
            //else
            //{
            //    ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            //    var priceList = _adapter.GetAllItems().Select(s => Convert.ToInt32(prefs.GetString(s + "total", "0"))).ToList();
            //    if (priceList != null)
            //    {
            //        _adapter.ChangeTotalPriceList(priceList);
            //    }
            //    RunOnUiThread(() =>
            //    {
            //        _adapter.NotifyDataSetChanged();
            //    });
            //}
        }

        private void InitActivity()
        {
            _init = true;
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var storedItemString = prefs.GetString("Lists", null);
            var storedItemListData = !string.IsNullOrEmpty(storedItemString) ? storedItemString.Split(new[] { "," }, StringSplitOptions.None).ToList() : null;
            var items = storedItemListData ?? new List<string>();
            //var priceList = items.Select(s => Convert.ToInt32(prefs.GetString(s + "total", "0"))).ToList();
            _listView = FindViewById<ListView>(Resource.Id.ListsView);
            _button = FindViewById<Button>(Resource.Id.ListButton);
            _button.Click += ButtonOnClick;
            _listView.Adapter = _adapter = new MyArrayAdapter1(this, items);
            _listView.ItemClick += ListViewOnItemClick;
            _listView.ItemLongClick += ListViewOnItemLongClick;
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
                var view = optionBox.LayoutInflater.Inflate(Resource.Layout.ListOptions, null);
                optionBox.SetView(view);
                optionBox.SetCancelable(false);

                view.FindViewById<Button>(Resource.Id.EditListNameButton).Click += new EventHandler((s, e) => EditListNameOptionClicked(s, e, itemLongClickEventArgs.Position, optionBox));
                view.FindViewById<Button>(Resource.Id.ListDeleteButton).Click += new EventHandler((s, e) => DeleteOptionClicked(s, e, itemLongClickEventArgs.Position, optionBox));
                optionBox.SetButton("Cancle", (o, args) => { });
                optionBox.Show();
            }
            catch (Exception ex)
            {
                Log.Error("Exception:", ex.Message + "|StackTrace:" + ex.StackTrace);
            }
        }

        private void EditListNameOptionClicked(object s, EventArgs e, int position, AlertDialog optionBox)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            var editBox = builder.Create();
            editBox.SetTitle("Update Category Name");
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
                        ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                        var storedItemString = prefs.GetString(oldListName, null);
                        var markingListString = prefs.GetString(oldListName + "MarkingList", null);
                        var priceListString = prefs.GetString(oldListName + "PriceList", null);

                        ISharedPreferencesEditor editor = prefs.Edit();
                        editor.PutString(newListName, storedItemString);
                        editor.PutString(newListName + "MarkingList", markingListString);
                        editor.PutString(newListName + "PriceList", priceListString);
                        //editor.PutString(_listName + "total", Convert.ToString(_adapter.GetPriceTotal()));
                        editor.Apply();    // applies changes synchronously on older APIs
                    }
                    
                    RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                }
            });
            editBox.SetButton2("Cancle", (o, args) => { });
            editBox.Show();
            optionBox.Cancel();
        }

        private void DeleteOptionClicked(object s, EventArgs e, int position, AlertDialog optionBox)
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
                    var item = _listView.GetItemAtPosition(position);
                    if (item != null)
                    {
                        _adapter.RemoveItem(position);
                        RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
                    }
                });
                alterDialog.SetButton2("No", (o, args) => { });
                alterDialog.Show();
                optionBox.Cancel();
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
                    RunOnUiThread(() =>
                    {
                        _adapter.NotifyDataSetChanged();
                    });
                    inputTextField.Text = string.Empty;
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
                Log.Info("ListViewOnItemClick", item.ToString());
                var intent = new Intent(this, typeof(ItemsActivity));
                intent.PutExtra("ListName", item.ToString());
                StartActivity(intent);
            }
        }
    }
}

