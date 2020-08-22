using System.Collections.Generic;

namespace Logic.Gameplay.State
{
    class StateManager
    {
        private readonly Queue<IGameState> m_gameStates = new Queue<IGameState>();

        private readonly Dictionary<System.Type, Stack<IGameState>> pooledStates = new Dictionary<System.Type, Stack<IGameState>>();

        private IGameState m_activeState;
        
        private GameManager m_manager;

        public void Initialize(GameManager manager)
        {
            this.m_manager = manager;
        }

        public void Reset()
        {
            m_gameStates.Clear();
            m_activeState = null;
        }

        public T RegisterState<T>() where T:IGameState
        {
            System.Type type = typeof(T);
            Stack<IGameState> pool;
            T state;
            //try to get it from pool first
            if (pooledStates.TryGetValue(type, out pool))
            {
                if (pool.Count > 0)
                {
                    state = (T)pool.Pop();
                    return state;
                }
            }
            else
            {
                pooledStates[type] = new Stack<IGameState>();
            }
            //create new
            state = System.Activator.CreateInstance<T>();
            state.Initialize(m_manager);
            m_gameStates.Enqueue(state);
            return state;
        }

        public void Tick()
        {
            if (m_activeState == null)
            {
                if (m_gameStates.Count > 0)
                {
                    m_activeState = m_gameStates.Dequeue();
                    m_activeState.Begin();
                }
            }
            else
            {
                m_activeState.Tick();

                if (m_activeState.ShouldEnd)
                {
                    m_activeState.End();
                    m_activeState = null;
                }
            }
        }
    }
}
