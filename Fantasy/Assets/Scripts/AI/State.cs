using System.Collections;
using UnityEngine;

namespace Ai.Test
{
    public abstract class State
    {
        protected readonly Test _system;
        public State(Test system)
        {
            _system = system;
        }
        public virtual IEnumerator Start()
        {
            yield break;
        }

        public virtual IEnumerator Pause()
        {
            yield break;
        }
        public virtual IEnumerator Resume()
        {
            yield break;
        }
    }
}