using UnityEngine.Events;

namespace Logic.Gameplay.State
{
    interface IGameState
    {
        void Initialize(GameManager manager);
        void Begin();
        void Tick();
        void End();
        
        bool ShouldEnd { get; }
    }
}
