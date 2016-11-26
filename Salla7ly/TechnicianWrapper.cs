namespace Salla7ly
{
    public class TechnicianWrapper : Java.Lang.Object
    {
        public TechnicianWrapper(Technician item)
        {
            Technicaian = item;
        }

        public Technician Technicaian { get; private set; }
    }
}