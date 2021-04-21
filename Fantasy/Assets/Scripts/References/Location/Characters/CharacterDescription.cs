using References.Location.Characters.Health;
using UnityEngine;

namespace References.Location.Characters
{
    [CreateAssetMenu(fileName = "CharacterDescription", menuName = "References/CharacterDescription")]
    public class CharacterDescription : ScriptableObject
    {
        public HealthDescription Health;
    }
}