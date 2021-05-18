using Data;
using References.Location;
using UnityEngine;

namespace Controllers
{
    public class PlayerDataController : MonoBehaviour
    {
        public PlayerData Data { get; private set; }

        [SerializeField]
        private LocationDescription locationDescription;

        private void Awake()
        {
            Data = new PlayerData(locationDescription);
        }
    }
}