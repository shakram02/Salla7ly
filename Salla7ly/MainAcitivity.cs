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
using System.Linq;
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

        // Define a authenticated user.
        private MobileServiceUser _user;
        private bool _authenticated;
        private TechnicianDatabase _techDb;
        private ListView _technicianListView;
        private ProgressDialog _progressDialog;
        private Spinner _fieldSpinner;

        //Adapter to map the items list to the view
        private TechnicianAdapter _adapter;

        private Button _searchButton;
        private ArrayAdapter _fieldAdapter;
        private bool _syncContextInitialized;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);

            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MainActivity);

            CurrentPlatform.Init();

            InitializeGui();
            _techDb = new TechnicianDatabase();

            _searchButton = FindViewById<Button>(Resource.Id.searchButton);
#if DEBUG
            var uri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            MediaPlayer player = MediaPlayer.Create(this, uri);
            player.Looping = false;
            player.Start();
#endif

            _progressDialog = new ProgressDialog(this);

            if (!IsConnected())
            {
                UiHelper.MakeToast(this, "Can't connect to the Internet, Offline sync will be used");
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.activity_main, menu);
            return true;
        }

        private bool IsConnected()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            return (activeConnection != null) && activeConnection.IsConnected;
        }

        //Select an option from the menu
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_about)
            {
                UiHelper.CreateAndShowDialog(this,
                    $"Created by Ahmed Hamdy Mahmoud,{System.Environment.NewLine}" +
                    $"MSP - 2015/2016{System.Environment.NewLine}" +
                    "Email: ahmedhamdy.mahmoud@studentpartner.com," +
                    $" Address: Smouha, Alexandria, Egypt{System.Environment.NewLine}Thanks!", "Info");
            }
            else if (item.ItemId == Resource.Id.menu_add_technician)
            {
                Task.Run(async () =>
                {
                    if (!_authenticated)
                    {
                        _authenticated = await Authenticate();
                    }
                    if (_authenticated)
                    {
                        AddTechnician();
                    }
                    else
                    {
                        RunOnUiThread(() => UiHelper.MakeToast(this, "Please login to add a technician"));
                    }
                });
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
                // Load the data.
                await OnRefreshItemsSelected();
            }
        }

        [Export]
        public async void FindTechnicainByField(View view)
        {
            string fieldName = _fieldSpinner.SelectedItem.ToString();

            _searchButton.Enabled = false;
            ShowProgressDialog(IsConnected() ? "Fetching from Microsoft Azure..." : "Searching local store...");

            if (!_syncContextInitialized)
            {
                _syncContextInitialized = true;
                await Task.Run(async () => await _techDb.StartDb());
            }

            var result = await _techDb.Find(fieldName);
            _searchButton.Enabled = true;
            HideProgressDialog();

            if (result.Count == 0)
            {
                UiHelper.MakeToast(this, "Couldn't find technicians");
                return;
            }

            DisplayItemsInListView(result);
        }

        [Export]
        public void OpenPopupMenu(View view)
        {
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
            if (_techDb.Client == null)
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

        private void ShowProgressDialog(string text)
        {
            _progressDialog.SetMessage(text);
            RunOnUiThread(() => _progressDialog.Show());
        }

        private void HideProgressDialog()
        {
            RunOnUiThread(() => _progressDialog.Dismiss());
        }

        private async Task<bool> Authenticate()
        {
            bool success = false;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                _user = await _techDb.Client.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook);
                success = true;
            }
            catch (Exception)
            {
                // User cancelled auth.
            }
            return success;
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