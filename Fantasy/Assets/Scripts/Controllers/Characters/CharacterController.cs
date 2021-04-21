using System.Collections;
using Controllers.Characters.Health;
using Data.Location.Characters;
using References.Location.Characters;
using UnityEngine;

namespace Controllers.Characters
{
    public class CharacterController : MonoBehaviour
    {
        public GameObject PlayerOnScene;
        
        public HealthController HealthController;
        
        [SerializeField]
        private CharacterDescription characterDescription;

        private CharacterData _data;
        private void Start()
        {
            var player = PlayerOnScene.GetComponent<PlayerDataController>();
            _data = player.Data.Location.BuildCharacter(characterDescription);

            HealthController.Set(_data.Health);
            
            StartCoroutine(ExampleCoroutine());
        }

        private IEnumerator ExampleCoroutine()
        {
            while (true)
            {
                _data.Health.Damage(1);
                yield return new WaitForSeconds(1);
            }
        }
    }
}