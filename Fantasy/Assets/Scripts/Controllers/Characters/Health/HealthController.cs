using Data.Location.Characters.Health;
using UnityEngine;

namespace Controllers.Characters.Health
{
    public class HealthController : MonoBehaviour
    {
        private HealthData _data;
        public void Set(HealthData data)
        {
            _data = data;
            _data.Changed += OnHealthChanged;
        }

        private void OnHealthChanged(float oldValue, float newValue)
        {
            Debug.Log($"Health changed from {oldValue} to {newValue}");
        }

        private void OnDestroy()
        {
            _data.Changed -= OnHealthChanged;
        }
    }
}