using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace Salla7ly
{
    [Activity(Label = "DisplayTechnicianActivity")]
    internal class DisplayTechnicianActivity : Activity
    {
        private ClipboardManager _clipBoardManager;
        private TextView _nameTextView;

        private TextView _cityTextView;
        private TextView _govTextView;
        private TextView _fieldTextView;
        private TextView _phoneNumberTextView;
        private Technician _tech;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.DisplayTechnician);
            GetGuiElements();
            if (!Intent.Extras.ContainsKey(Constants.SelectedTechnician)) return;
            // Display technician
            string serializedTechnician = this.Intent.GetStringExtra(Constants.SelectedTechnician);
            _tech = JsonConvert.DeserializeObject<Technician>(serializedTechnician);

            _nameTextView.Text = _tech.Name;
            _phoneNumberTextView.Text = _tech.PhoneNumber;
            _cityTextView.Text = _tech.City;
            _govTextView.Text = _tech.Governorate;
            _fieldTextView.Text = _tech.Field;

            _clipBoardManager = (ClipboardManager)this.GetSystemService(ClipboardService);
        }

        private void GetGuiElements()
        {
            _nameTextView = FindViewById<TextView>(Resource.Id.techNameDisplayTxt);
            _cityTextView = FindViewById<TextView>(Resource.Id.techCityDisplayTxt);
            _govTextView = FindViewById<TextView>(Resource.Id.techGovDisplayTxt);
            _fieldTextView = FindViewById<TextView>(Resource.Id.techFieldDisplayTxt);
            _phoneNumberTextView = FindViewById<TextView>(Resource.Id.techPhoneNumberDisplayTxt);

        }

        public void CopyTechPhone()
        {
            _clipBoardManager.PrimaryClip = ClipData.NewPlainText($"{_tech.Name}'s phone number",
                _tech.PhoneNumber);
            UiHelper.MakeToast(this, "Phone number copied to clipboard");

            SetResult(Result.Canceled);
        }
    }
}