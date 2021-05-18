using System.Collections.Generic;
using References.Location.Characters;
using UnityEngine;

namespace References.Location
{
    [CreateAssetMenu(fileName = "LocationDescription", menuName = "References/LocationDescription")]
    public class LocationDescription : ScriptableObject
    {
        public List<CharacterDescription> Characters;
    }
}