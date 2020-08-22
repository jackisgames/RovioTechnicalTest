namespace Logic.Gameplay.State
{
    class GameOverState:IGameState
    {
        private GameManager m_manager;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Begin()
        {
            bool playerWin = m_manager.PlayerEntities.Count > 0;
            m_manager.Ui.ShowBanner(playerWin ? "GameOver, You win!" : "GameOver, You lose!");

            m_manager.Ui.GameoverScreen.gameObject.SetActive(true);
            m_manager.Ui.NextLevelButton.gameObject.SetActive(playerWin);

        }

        public void Tick()
        {

        }

        public void End()
        {
            
        }

        public bool ShouldEnd { get; }
    }
}
