using System;
using References.Location.Characters.Health;

namespace Data.Location.Characters.Health
{
    public class HealthData
    {
        public float Amount { get; private set; }

        public event Action<float, float> Changed;
        
        public HealthData(HealthDescription description)
        {
            Amount = description.Maximum;
        }

        public void Damage(float amount)
        {
            var oldAmount = Amount;
            Amount -= amount;

            if (Changed != null)
                Changed(oldAmount, Amount);
        }
    }
}