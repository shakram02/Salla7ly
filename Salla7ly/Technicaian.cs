using Newtonsoft.Json;

namespace Salla7ly
{

    public class Technician : DataEntity
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        [JsonProperty(PropertyName = "addedBy")]
        public string AddedBy { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "governorate")]
        public string Governorate { get; set; }


        // TODO change LusterName to LusterID in portal
    }
}