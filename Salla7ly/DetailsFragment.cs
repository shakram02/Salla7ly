using Android.App;
using Android.OS;
using Android.Views;

namespace Salla7ly
{
    public class DetailsFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
             return inflater.Inflate(Resource.Layout.DetailsFragment, container,true);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}