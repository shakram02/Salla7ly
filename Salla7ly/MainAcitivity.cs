using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Interop;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Salla7ly
{
    // TODO add gov drop-down list -> not needed as it'll make the search process more complex TODO authorization

    [Activity(MainLauncher = true,
               Icon = "@drawable/ic_launcher", Label = "@string/app_name",
               Theme = "@style/AppTheme")]
    public class MainAcitivity : Activity
    {
        private const int AddTechnicianRequest = 1;

        // The request code
        private const string ApplicationUrl = @"https://salla7ly.azurewebsites.net";

        // Define a authenticated user.
        private MobileServiceUser _user;

        private bool _authenticated;

        private TechnicianDatabase _techDb;

        private MobileServiceClient _client;

        private ListView _technicianListView;


        private ProgressDialog _dialog;


        private Spinner _fieldSpinner;

        //Adapter to map the items list to the view
        private TechnicianAdapter _adapter;
        private ConnectivityManager _connectivityManager;
        private ArrayAdapter _fieldAdapter;
        private bool _syncContextInitialized;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MainActivity);

            CurrentPlatform.Init();
            _connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            InitializeGui();
            InitializeAppLogic();

            _dialog = new ProgressDialog(this);
            _dialog.SetMessage("Fetching data from Microsoft Azure...");

#if DEBUG
            var uri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            MediaPlayer player = MediaPlayer.Create(this, uri);
            player.Looping = false;
            player.Start();
#endif
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.activity_main, menu);
            return true;
        }

        private bool IsConnected()
        {

            NetworkInfo activeConnection = _connectivityManager.ActiveNetworkInfo;
            return (activeConnection != null) && activeConnection.IsConnected && activeConnection.IsAvailable;
        }

        //Select an option from the menu
        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            //if (item.ItemId == Resource.Id.menu_refresh)
            //{
            //    item.SetEnabled(false);
            //    Task.Factory.StartNew(async () =>
            //    {
            //        ShowProgressDialog("Fetching all data from Microsoft Azure");

            //        await OnRefreshItemsSelected();
            //        item.SetEnabled(true);

            //        HideProgressDialog();
            //    });
            //}
            //else
            if (item.ItemId == Resource.Id.menu_about)
            {
                UiHelper.CreateAndShowDialog(this,
                    $"Created by Ahmed Hamdy Mahmoud,{System.Environment.NewLine}" +
                    $"MSP - 2015/2016{System.Environment.NewLine}" +
                $"Email: ahmedhamdyau@gmail.com, address: Block No. 133 - Smouha, Alexandria, Egypt{System.Environment.NewLine}Thanks!", "Info");
            }
            else if (item.ItemId == Resource.Id.menu_add_technician)
            {
                Task authorize = new Task(async () =>
                {
                    //Add a new technician directly if authenticated.
                    if (_authenticated)
                    {
                        AddTechnician();
                        return;
                    }

                    _authenticated = await Authenticate();
                    if (_authenticated)
                    {
                        AddTechnician();
                    }
                    else
                    {
                        RunOnUiThread(() => UiHelper.MakeToast(this, "Please login to add a technician"));
                    }
                });
                authorize.Start();
            }

            return true;
        }

        [Export]
        public async void LoginUser(View view)
        {
            // Load data only after authentication succeeds.
            _authenticated = await Authenticate();
            if (_authenticated)
            {
                //Hide the button after authentication succeeds.
                //FindViewById<Button>(Resource.Id.buttonLoginUser).Visibility = ViewStates.Gone;

                // Load the data.
                await OnRefreshItemsSelected();
            }
        }

        [Export]
        public async void FindTechnicainByField(View view)
        {
            _dialog.Show();
            string fieldName = _fieldSpinner.SelectedItem.ToString();


            if (!_syncContextInitialized)
            {
                _syncContextInitialized = true;
                await Task.Factory.StartNew(async () => await InitAzureSync());
            }

            var result = await _techDb.Find(fieldName, IsConnected());

            if (result.Count == 0)
            {
                UiHelper.MakeToast(this, "Couldn't find technicians");
                return;
            }

            //AddItemsToListView(result);   // Add tracks to listView, display the track and luster name
            DisplayItemsInListView(result);
            _dialog.Dismiss();
        }

        private async Task InitAzureSync()
        {

            bool isOnline = IsConnected();
            await _techDb.InitSync();


            if (!isOnline)
            {
                RunOnUiThread(() => UiHelper.MakeToast(this, "Can't connect to the Internet, Offline sync will be used"));
            }
        }

        [Export]
        public void TestMethod(View view)
        {
            // Menu button

            // Hide soft keyboard if it's open
            InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            var currentFocus = this.CurrentFocus;
            if (currentFocus != null)
            {
                inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }

            OpenOptionsMenu();
        }

        public void AddTechnician()
        {
            if (_client == null)
            {
                UiHelper.MakeToast(this, "Client doesn't exist");
                return;
            }

            // Open the fill form window
            Intent addTechnicianIntent = new Intent(this, typeof(AddTechnicianActivity));
            StartActivityForResult(addTechnicianIntent, AddTechnicianRequest);
        }

        protected override async void OnActivityResult(int requestCode, [Android.Runtime.GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode != AddTechnicianRequest || resultCode != Result.Ok ||
                !data.HasExtra(Constants.AddedTechnician)) return;

            //var technician = StaticHelper.AddTechnicianResult;
            var technician = JsonConvert.DeserializeObject<Technician>(data.GetStringExtra(Constants.AddedTechnician));

            ShowProgressDialog("Adding technician...");
            try
            {
                await _techDb.AddTechnician(technician);
                await _techDb.CommitChanges();
                DisplayItemsInListView(await _techDb.GetAllTechnicians());

                //_adapter.Add(technician);
                //StaticHelper.AddTechnicianResult = null;
            }
            catch (Exception e)
            {
                UiHelper.MakeToast(this, $"Error:{e.Message}");
            }
            finally
            {
                HideProgressDialog();
            }
        }

        void ShowProgressDialog(string text)
        {
            _dialog.SetMessage(text);
            RunOnUiThread(() => _dialog.Show());
        }

        void HideProgressDialog()
        {
            RunOnUiThread(() => _dialog.Dismiss());
        }

        private async Task<bool> Authenticate()
        {
            bool success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                _user = await _client.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook);
                //StaticHelper.CreateAndShowDialog(string.Format("you are now logged in"), "Logged in!");
                success = true;
            }
            catch (Exception)
            {
                // User cancelled auth.
            }
            return success;
        }

        private void InitializeAppLogic()
        {
            // Create the Mobile Service Client instance, using the provided Mobile Service URL

            _client = new MobileServiceClient(ApplicationUrl);
            _techDb = new TechnicianDatabase(_client);
        }

        private void InitializeGui()
        {
            // Create an adapter to bind the items with the view
            _adapter = new TechnicianAdapter(this, Resource.Layout.Row_List_Technician);
            _technicianListView = FindViewById<ListView>(Resource.Id.listViewToDo);

            _fieldSpinner = FindViewById<Spinner>(Resource.Id.fieldSearchSpinner);
            _fieldAdapter = ArrayAdapter.CreateFromResource(
                  this, Resource.Array.fields_array, Android.Resource.Layout.SimpleSpinnerItem);

            _fieldAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            RunOnUiThread(() =>
            {
                _technicianListView.ItemClick += TechnicianClick;
                _technicianListView.Adapter = _adapter;
                _fieldSpinner.Adapter = _fieldAdapter;
            });
        }

        private void TechnicianClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var technician = _adapter[e.Position];
            Intent displayTechnicaianDetails = new Intent(this, typeof(DisplayTechnicianActivity));
            displayTechnicaianDetails.PutExtra(Constants.SelectedTechnician, JsonConvert.SerializeObject(technician));

            this.StartActivity(displayTechnicaianDetails);
        }

        // Called when the refresh menu option is selected
        private async Task OnRefreshItemsSelected()
        {
            try
            {
                await _techDb.CommitChanges(true);
                var list = await _techDb.GetAllTechnicians();

                UiHelper.MakeToast(this, "Done!");
                DisplayItemsInListView(list);  // refresh view using local database
            }
            catch (Exception exception)
            {
                UiHelper.MakeToast(this, "Error:" + exception.Message);
            }
        }

        //Refresh the list with the items in the local database

        private void DisplayItemsInListView(IEnumerable<Technician> items)
        {
            _adapter.Clear();
            foreach (Technician item in items)
                _adapter.Add(item);
        }
    }
}