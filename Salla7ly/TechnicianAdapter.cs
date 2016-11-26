using Android.App;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace Salla7ly
{
    public class TechnicianAdapter : BaseAdapter<Technician>
    {
        public TechnicianAdapter(Activity activity, int layoutResourceId)
        {
            this._activity = activity;
            this._layoutResourceId = layoutResourceId;
        }

        public override int Count
        {
            get
            {
                return _items.Count;
            }
        }

        public override Technician this[int position]
        {
            get
            {
                return _items[position];
            }
        }

        //Returns the view for a specific item on the list
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var row = convertView;
            var currentItem = this[position];
            TextView listItem;
            TextView fieldTextView;
            TextView phoneNumberTextView;
            if (row == null)
            {
                var inflater = _activity.LayoutInflater;
                row = inflater.Inflate(_layoutResourceId, parent, false);

                listItem = row.FindViewById<TextView>(Resource.Id.checkToDoItem);
                fieldTextView = row.FindViewById<TextView>(Resource.Id.fieldTextView);
                phoneNumberTextView = row.FindViewById<TextView>(Resource.Id.phoneNumberTextView);
            }
            else
            {
                listItem = row.FindViewById<TextView>(Resource.Id.checkToDoItem);
                fieldTextView = row.FindViewById<TextView>(Resource.Id.fieldTextView);
                phoneNumberTextView = row.FindViewById<TextView>(Resource.Id.phoneNumberTextView);
            }

            //checkBox.Text = currentItem.Text + " Artist:" + currentItem.Artist;
            listItem.Text = $"{position} - {currentItem.Name}";
            listItem.Enabled = true;
            listItem.Tag = new TechnicianWrapper(currentItem);

            fieldTextView.Text = currentItem.Field;
            phoneNumberTextView.Text = currentItem.PhoneNumber;

            return row;
        }

        public void Add(Technician item)
        {
            _items.Add(item);
            NotifyDataSetChanged();
        }

        public void Clear()
        {
            _items.Clear();
            NotifyDataSetChanged();
        }

        public void Remove(Technician item)
        {
            _items.Remove(item);
            NotifyDataSetChanged();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        private readonly Activity _activity;
        private readonly int _layoutResourceId;
        private readonly List<Technician> _items = new List<Technician>();
    }
}