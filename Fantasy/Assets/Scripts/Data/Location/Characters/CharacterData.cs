using Data.Location.Characters.Health;
using References.Location.Characters;

namespace Data.Location.Characters
{
    public class CharacterData
    {
        public HealthData Health { get; }
        
        public CharacterData(CharacterDescription description)
        {
            Health = new HealthData(description.Health);
        }
    }
}