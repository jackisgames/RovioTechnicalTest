using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using Logic.Gameplay.State;
using UnityEngine;

namespace Logic.Gameplay.Skill
{
    class Heal:ISkill
    {
        private GameManager m_manager;

        private GameplayEntity m_owner;

        public void Initialize(GameplayEntity owner, GameManager manager)
        {
            m_manager = manager;
            m_owner = owner;
        }

        public void Execute(GameplayEntity target)
        {
            m_manager.StateManager.RegisterState<HealState>().Setup(target);
        }

        public void GetValidTargets(List<GameplayEntity> results)
        {
            results.Clear();
            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];
                if (entity.Visual.Type == m_owner.Visual.Type &&
                    entity.Status.Health < entity.Status.HealthMax)
                {
                    if (Vector2Int.Distance(m_owner.Visual.GridPosition, entity.Visual.GridPosition) <= 2)
                    {
                        results.Add(entity);
                    }
                }
            }
        }

        public string Name
        {
            get { return "Heal"; }
        }

        public string Description
        {
            get { return "Replenish 1 health"; }
        }

        public ESkillType Type
        {
            get { return ESkillType.Defensive; }
        }
    }

    class HealState:IGameState
    {
        private float m_timer;

        private GameplayEntity m_target;

        private GameManager m_manager;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Setup(GameplayEntity target)
        {
            m_target = target;
        }

        public void Begin()
        {
            m_timer = 1;
            m_target.Status.Heal(1);
            m_target.Visual.PlayHealthBarAnimation(m_target.Status.HealthPercentage, .3f);
            m_target.Move(m_target.Visual.GridPosition, 0);
        }

        public void Tick()
        {
            m_timer -= Time.deltaTime;
        }

        public void End()
        {
            m_manager.StateManager.RegisterState<StartTurnState>();
        }

        public bool ShouldEnd
        {
            get { return m_timer <= 0; }
        }
    }
}
