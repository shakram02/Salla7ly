using System;
using Android.App;
using Android.Content;
using Microsoft.WindowsAzure.MobileServices;

namespace Salla7ly
{
    public static class StaticHelper
    {
        public static MobileServiceClient Client { get; set; }

     

        public static void CreateAndShowDialog(Context content, string message, string title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(content);

            builder.SetMessage(message);
            builder.SetTitle(title).SetPositiveButton("Ok", handler: null);
            builder.Create().Show();
        }
#if DEBUG
        public static bool CreateOptionsDialog(Context content, string message, string title)
        {
            bool result= false;
            AlertDialog.Builder builder = new AlertDialog.Builder(content);

            builder.SetMessage(message);
            builder.SetTitle(title)
                .SetNegativeButton("No", handler: (sender, args) => result = false)
                .SetPositiveButton("Yes", handler: (sender, args) => result = true);
            builder.Create().Show();

            return result;
        }
#endif

        public static Technician AddTechnicianResult { get; set; }
    }
}