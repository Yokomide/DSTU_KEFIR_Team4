using UnityEditor;
using UnityEngine;

namespace Ai.Test
{
    public abstract class StateMahine : MonoBehaviour
    {
        protected State State;

        public void SetState(State state)
        {
            State = state;
            StartCoroutine(State.Start());
        }
    }
}