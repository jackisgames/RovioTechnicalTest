using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using Logic.Gameplay.State;
using UnityEngine;

namespace Logic.Gameplay.Skill
{
    class PushAttack:ISkill
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
            m_manager.StateManager.RegisterState<PushAttackState>().Setup(m_owner, target, 3, 1);
        }

        public void GetValidTargets(List<GameplayEntity> results)
        {
            results.Clear();

            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];
                if (m_owner.Visual.GridPosition != entity.Visual.GridPosition)
                {
                    if (Vector2Int.Distance(m_owner.Visual.GridPosition, entity.Visual.GridPosition) <= 1)
                    {
                        results.Add(entity);
                    }
                }
            }
        }

        public string Name
        {
            get { return "Push"; }
        }

        public string Description
        {
            get { return "Push target back"; }
        }

        public ESkillType Type
        {
            get { return ESkillType.Offensive; }
        }
    }

    class PushAttackState:IGameState
    {
        private GameManager m_manager;

        private GameplayEntity m_attacker;

        private GameplayEntity m_target;

        private readonly Dictionary<Vector2Int, GameplayEntity> m_gameplayEntitiesRemap = new Dictionary<Vector2Int, GameplayEntity>();

        private readonly List<GameplayEntity> m_movedEntities = new List<GameplayEntity>();

        private int m_pushDistance;

        private int m_hitDamage;

        private float m_timer;

        private bool m_hitAnother;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Setup(GameplayEntity attacker, GameplayEntity target, int pushDistance,int hitDamage)
        {
            m_attacker = attacker;
            m_target = target;
            m_pushDistance = pushDistance;
            m_hitDamage = hitDamage;
        }

        public void Begin()
        {
            m_gameplayEntitiesRemap.Clear();
            m_movedEntities.Clear();

            Vector2Int delta = m_target.Visual.GridPosition - m_attacker.Visual.GridPosition;
            delta.x = Mathf.Clamp(delta.x, -1, 1);
            delta.y = Mathf.Clamp(delta.y, -1, 1);
            
            m_gameplayEntitiesRemap.Clear();

            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];
                m_gameplayEntitiesRemap[entity.Visual.GridPosition] = entity;
            }


            m_movedEntities.Add(m_target);

            for (int i = 0; i < m_pushDistance; i++)
            {
                Vector2Int targetCheck = m_target.Visual.GridPosition + delta * (1 + i);
                if (m_manager.Pathfinder.NavGraph.OutsideBounds(targetCheck))
                {
                    m_pushDistance = i;
                    break;
                }

                GameplayEntity entity;
                if (m_gameplayEntitiesRemap.TryGetValue(targetCheck, out entity))
                {
                    m_movedEntities.Add(entity);
                }
            }

            if (m_pushDistance > 0)
            {
                if (m_movedEntities.Count > 1)
                {
                    for (int i = m_movedEntities.Count - 1; i >= 0; i--)
                    {
                        GameplayEntity entity = m_movedEntities[i];


                        if (i == m_movedEntities.Count - 1)
                        {
                            //last one, move if there's empty spot
                            Vector2Int targetPush = entity.Visual.GridPosition + delta;
                            if (!m_manager.Pathfinder.NavGraph.OutsideBounds(targetPush) && 
                                !m_manager.Pathfinder.NavGraph.IsBlocked(targetPush))
                            {
                                PushBack(entity, targetPush, m_hitDamage, i * .3f + .3f);
                            }
                            else
                            {
                                PushBack(entity, entity.Visual.GridPosition, m_hitDamage, i * .3f + .3f);
                            }
                        }
                        else
                        {
                            Vector2Int targetPush = m_movedEntities[i + 1].Visual.GridPosition;

                            targetPush -= delta;
                            while (targetPush != entity.Visual.GridPosition)
                            {
                                if (!m_manager.Pathfinder.NavGraph.IsBlocked(targetPush))
                                {
                                    break;
                                }
                                targetPush -= delta;
                            }

                            PushBack(entity, targetPush, m_hitDamage, i * .3f + .3f);
                        }
                    }

                    m_timer = 1.5f + m_movedEntities.Count * .3f;
                }
                else
                {
                    m_timer = 1;
                    Vector2Int pushPosition = m_target.Visual.GridPosition + delta * m_pushDistance;
                    PushBack(m_target, pushPosition, m_hitDamage, .3f);
                }

                m_attacker.Visual.Leap(m_attacker.Visual.GridPosition, .3f);
                m_attacker.Visual.Leap(m_target.Visual.GridPosition);

            }
        }

        private void PushBack(GameplayEntity entity, Vector2Int location, int damage, float delay)
        {
            entity.Status.Damage(damage);

            entity.Visual.Leap(location, delay);
            entity.Visual.PlayTakeDamageAnimation(delay);
            entity.PlayHealthBarAnimation(delay);

            m_manager.Pathfinder.NavGraph.SetGridType(entity.Visual.GridPosition, EntityType.None);

            if (entity.Status.Health<=0)
            {
                entity.Visual.PlayDeathAnimation(delay + .3f);
            }
            else
            {
                m_manager.Pathfinder.NavGraph.SetGridType(location, entity.Visual.Type);
            }
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
            get { return m_timer < 0; }
        }
    }
}
