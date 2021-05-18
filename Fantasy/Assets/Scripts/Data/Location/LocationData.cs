using System.Collections.Generic;
using Data.Location.Characters;
using References.Location;
using References.Location.Characters;

namespace Data.Location
{
    public class LocationData
    {
        public List<CharacterData> Characters;
        
        public LocationData(LocationDescription description)
        {
            Characters = new List<CharacterData>();
        }

        public CharacterData BuildCharacter(CharacterDescription description)
        {
            var character = new CharacterData(description);
            Characters.Add(character);

            return character;
        }
    }
}