using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using Logic.Gameplay.Skill;
using UnityEngine;

namespace Logic.Gameplay.State
{
    class AIState:IGameState
    {
        private readonly List<GameplayEntity> m_targetableEntities = new List<GameplayEntity>();

        private readonly List<Vector2Int> m_pathBuffer = new List<Vector2Int>();

        private readonly List<Vector2Int> m_bestPathBuffer = new List<Vector2Int>();

        private readonly List<KeyValuePair<ISkill, GameplayEntity>> m_alternateAttacks = new List<KeyValuePair<ISkill, GameplayEntity>>();

        private GameManager m_manager;
        
        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Begin()
        {
            if (m_manager.PlayerEntities.Count == 0)
            {
                //no target left skip turn
                SkipTurn();
                return;
            }
            for (int i = 0; i < m_manager.EnemiesEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.EnemiesEntities[i];
                if (entity.Status.Health > 0)
                {
                    //get closest enemy
                    if (entity.TurnState.CanAttack)
                    {
                        int randomness = Random.Range(0, entity.Skills.Count);

                        m_alternateAttacks.Clear();

                        for (int j = 0; j < entity.Skills.Count; j++)
                        {
                            ISkill skill = entity.Skills[(j + randomness) % entity.Skills.Count];

                            skill.GetValidTargets(m_targetableEntities);

                            //filter targets
                            for (int k = 0; k < m_targetableEntities.Count; k++)
                            {
                                GameplayEntity targetEntity = m_targetableEntities[k];

                                if (skill.Type == ESkillType.Offensive && 
                                    targetEntity.Visual.Type == EntityType.Enemy)
                                {
                                    m_targetableEntities.RemoveAt(k);
                                    k--;
                                }
                                else if (targetEntity.Visual.Type == EntityType.Obstacle)
                                {
                                    m_alternateAttacks.Add(new KeyValuePair<ISkill, GameplayEntity>(skill, targetEntity));
                                    m_targetableEntities.RemoveAt(k);
                                    k--;
                                }
                            }

                            if (m_targetableEntities.Count > 0)
                            {
                                GameplayEntity bestTarget = m_targetableEntities[0];
                                for (int k = 1; k < m_targetableEntities.Count; k++)
                                {
                                    GameplayEntity target = m_targetableEntities[k];
                                    if (target.Status.Health < bestTarget.Status.Health)
                                    {
                                        bestTarget = target;
                                    }
                                }

                                //do attack here
                                skill.Execute(bestTarget);
                                entity.TurnState.CanAttack = false;
                                return;
                            }
                        }

                        if (!entity.TurnState.CanMove)
                        {
                            bool noPathToPlayer = true;
                            for (int j = 0; j < m_manager.PlayerEntities.Count; j++)
                            {
                                if (m_manager.Pathfinder.Navigate(entity.Visual.GridPosition, m_manager.PlayerEntities[j].Visual.GridPosition, m_pathBuffer))
                                {
                                    noPathToPlayer = false;
                                    break;
                                }
                            }

                            if (noPathToPlayer)
                            {
                                //no enemies in target, attack whatever
                                if (m_alternateAttacks.Count > 0)
                                {
                                    KeyValuePair<ISkill, GameplayEntity> alternate = m_alternateAttacks[Random.Range(0, m_alternateAttacks.Count)];

                                    alternate.Key.Execute(alternate.Value);
                                    entity.TurnState.CanAttack = false;
                                    return;

                                }
                            }
                        }
                    }

                    if (entity.TurnState.CanMove)
                    {
                        GameplayEntity bestTarget = m_manager.PlayerEntities[0];
                        m_manager.Pathfinder.Navigate(entity.Visual.GridPosition, bestTarget.Visual.GridPosition, m_bestPathBuffer);

                        for (int j = 1; j < m_manager.PlayerEntities.Count; j++)
                        {
                            GameplayEntity target = m_manager.PlayerEntities[j];
                            m_manager.Pathfinder.Navigate(entity.Visual.GridPosition, target.Visual.GridPosition, m_pathBuffer);

                            if (m_pathBuffer.Count < m_bestPathBuffer.Count)
                            {
                                m_bestPathBuffer.Clear();
                                m_bestPathBuffer.AddRange(m_pathBuffer);
                            }
                        }

                        entity.TurnState.CanMove = false;

                        if (m_bestPathBuffer.Count >= 2)
                        {
                            entity.ValidateMovement(m_bestPathBuffer);
                            m_manager.StateManager.RegisterState<EntityMoveState>().Move(entity, m_bestPathBuffer);
                            return;
                        }
                    }
                }
            }

            SkipTurn();
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

        private void SkipTurn()
        {
            for (int i = 0; i < m_manager.EnemiesEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.EnemiesEntities[i];
                entity.TurnState.CanAttack = entity.TurnState.CanMove = false;
            }

            m_manager.StateManager.RegisterState<StartTurnState>();
        }
    }
}
