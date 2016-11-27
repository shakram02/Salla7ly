using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Salla7ly
{
    class UiHelper
    {
        public static void MakeToast(Context context, string text, ToastLength duration = ToastLength.Short)
        {
            Toast.MakeText(context, text, duration).Show();
        }

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
            bool result = false;
            AlertDialog.Builder builder = new AlertDialog.Builder(content);

            builder.SetMessage(message);
            builder.SetTitle(title)
                .SetNegativeButton("No", handler: (sender, args) => result = false)
                .SetPositiveButton("Yes", handler: (sender, args) => result = true);
            builder.Create().Show();

            return result;
        }
#endif
    }
}