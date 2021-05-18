using Data.Location;
using References.Location;

namespace Data
{
    public class PlayerData
    {
        public LocationData Location;
        
        public PlayerData(LocationDescription locationDescription)
        {
            Location = new LocationData(locationDescription);
        }
    }
}