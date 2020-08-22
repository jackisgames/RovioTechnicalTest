using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using UnityEngine;

namespace Logic.Gameplay.State
{
    class StartTurnState : IGameState
    {
        private const float BannerDuration = 2;

        public bool ShouldEnd
        {
            get { return m_timer <= 0; }
        }

        private GameManager m_manager;

        private float m_timer;

        //determine which side go first, prioritize player first
        public void Begin()
        {
            m_timer = 0;

            m_manager.Ui.SkipTurnButton.gameObject.SetActive(false);

            //evaluate and remove dead entity
            m_manager.EnemiesEntities.Clear();
            m_manager.PlayerEntities.Clear();

            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];
                if (entity.Status.Health <= 0)
                {
                    m_manager.GameplayEntities.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (entity.Visual.Type == EntityType.Player)
                    {
                        m_manager.PlayerEntities.Add(entity);
                    }
                    else if (entity.Visual.Type == EntityType.Enemy)
                    {
                        m_manager.EnemiesEntities.Add(entity);
                    }
                }
            }

            if (HaveMoveLeft(m_manager.PlayerEntities))
            {
                m_manager.StateManager.RegisterState<PlayerInputState>();
                if (m_manager.RoundInfo.PlayerRound != m_manager.RoundInfo.CurrentRound)
                {
                    m_manager.RoundInfo.PlayerRound = m_manager.RoundInfo.CurrentRound;
                    m_manager.Ui.ShowAndHideBanner("Player's Turn", 0, BannerDuration);
                    m_timer = BannerDuration;
                }
            }
            else if (HaveMoveLeft(m_manager.EnemiesEntities))
            {
                m_manager.StateManager.RegisterState<AIState>();
                if (m_manager.RoundInfo.EnemyRound != m_manager.RoundInfo.CurrentRound)
                {
                    m_manager.RoundInfo.EnemyRound = m_manager.RoundInfo.CurrentRound;
                    m_manager.Ui.ShowAndHideBanner("Enemy Turn", 0, BannerDuration);
                    m_timer = BannerDuration;
                }
            }
            else
            {
                m_manager.StateManager.RegisterState<StartRoundState>();
            }
        }

        public void End()
        {

        }

        public void Initialize(GameManager manager)
        {
            this.m_manager = manager;
        }

        public void Tick()
        {
            m_timer -= Time.deltaTime;
        }

        private bool HaveMoveLeft(List<GameplayEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                GameplayEntity entity = entities[i];

                if (entity.HaveAction)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
