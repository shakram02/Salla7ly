using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using System;

namespace Salla7ly
{
    [Activity(Label = "AddTechnicianActivity")]
    public class AddTechnicianActivity : Activity
    {
        private Technician _addedTechnician;

        private EditText _nameEditText;

        private EditText _phoneNumberEditText;

        private EditText _cityEditText;

        private EditText _govEditText;

        private Spinner _fieldSpinner;
        private Button _addButton;

        [Export]
        public void AddTechnician(View view)
        {
            this.SetResult(Result.Canceled);
            if (!VerifyTechnicianData()) return;

            StaticHelper.AddTechnicianResult = _addedTechnician;
            SetResult(Result.Ok);
            Finish();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.AddTech);

            //CurrentPlatform.Init();

            GetGuiElements();

            // New Technician
            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.fields_array, Android.Resource.Layout.SimpleSpinnerItem);

            _addButton.Click += (s, e) => AddTechnician(null);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _fieldSpinner.Adapter = adapter;
        }

        private void GetGuiElements()
        {
            // fields_array
            _fieldSpinner = FindViewById<Spinner>(Resource.Id.fieldSpinner);
            // Technician text boxes
            _nameEditText = FindViewById<EditText>(Resource.Id.nameEditText);
            _phoneNumberEditText = FindViewById<EditText>(Resource.Id.phoneNumberEditText);
            _cityEditText = FindViewById<EditText>(Resource.Id.cityEditText);
            _govEditText = FindViewById<EditText>(Resource.Id.govEditText);
            _addButton = FindViewById<Button>(Resource.Id.buttonAddTech);
            _addButton = FindViewById<Button>(Resource.Id.buttonAddTech);
        }

        private bool VerifyTechnicianData()
        {
            string name = _nameEditText.Text;
            string city = _cityEditText.Text;
            string gov = _govEditText.Text;
            string field = _fieldSpinner.SelectedItem.ToString();

            string phoneNumber = _phoneNumberEditText.Text;

            if (String.IsNullOrEmpty(name)
                || String.IsNullOrEmpty(city)
                || String.IsNullOrEmpty(field)
                || String.IsNullOrEmpty(gov)
                || String.IsNullOrEmpty(phoneNumber))
            {
                UiHelper.MakeToast(this, "All fields are required");
                return false;
            }

            if (!ValidatePhoneNumber(phoneNumber))
            {
                return false;
            }

            _addedTechnician = new Technician()
            {
                Name = name,
                City = city,
                Field = field,
                Governorate = gov,
                PhoneNumber = phoneNumber
            };

            _nameEditText.Text = "";
            _cityEditText.Text = "";
            _govEditText.Text = "";

            return true;
        }

        private bool ValidatePhoneNumber(string phoneNumber)
        {
            int dummy;
            if (!phoneNumber.StartsWith("01"))
            {
                UiHelper.MakeToast(this, "Phone number format: 01XXXXXXXXX");
                return false;
            }
            if (phoneNumber.Length != 11)
            {
                UiHelper.MakeToast(this, "Phone number must be 11 characters");
                return false;
            }
            if (!int.TryParse(phoneNumber, out dummy))
            {
                UiHelper.MakeToast(this, "Phone number can't have non numeric characters");
                return false;
            }

            return true;
        }
    }
}