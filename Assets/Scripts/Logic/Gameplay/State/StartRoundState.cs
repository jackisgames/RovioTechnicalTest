using Logic.Gameplay.Entity;

namespace Logic.Gameplay.State
{
    class StartRoundState:IGameState
    {
        private GameManager m_manager;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Begin()
        {
            m_manager.Ui.SkipTurnButton.gameObject.SetActive(false);

            //update gameplay entities before starting round
            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];
                entity.InitializeTurn();
            }

            if (m_manager.PlayerEntities.Count == 0 || m_manager.EnemiesEntities.Count == 0)
            {
                m_manager.StateManager.RegisterState<GameOverState>();
            }
            else
            {
                m_manager.RoundInfo.CurrentRound++;
                m_manager.StateManager.RegisterState<StartTurnState>();
            }
        }

        public void Tick()
        {

        }

        public void End()
        {

        }

        public bool ShouldEnd
        {
            get { return true; }
        }
    }
}
