using System.Collections;
using UnityEngine;

namespace Ai.Test
{
    public enum MovingState { MobWalk, MobRun, MobDead, MobHits, MobDamage, GamePause }

    public class Test : StateMahine
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform  playerPosition;
        [SerializeField] private Transform  mobPosition;

        private Test _state;
        private Test _previousState;

        private State _currentState;

        public void SetState(State state)
        {
            _currentState = state;
            StartCoroutine(State.Start());
        }
        public void OnPauseButton()
        {
            StartCoroutine(State.Pause());
        }
        private void PauseGame()
        {
            throw new System.NotImplementedException();
        }
        public void OnResumeButton()
        {
            StartCoroutine(State.Resume());
        }
        private void ResumeGame()
        {
            throw new System.NotImplementedException();
        }
    }
}
