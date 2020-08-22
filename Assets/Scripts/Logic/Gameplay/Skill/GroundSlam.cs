using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using Logic.Gameplay.State;
using UnityEngine;

namespace Logic.Gameplay.Skill
{
    class GroundSlam:ISkill
    {
        private GameplayEntity m_owner;

        private GameManager m_manager;

        public void Initialize(GameplayEntity owner, GameManager manager)
        {
            m_owner = owner;

            m_manager = manager;
        }

        public void Execute(GameplayEntity target)
        {
            m_manager.StateManager.RegisterState<GroundSlamState>().Setup(this);
        }

        public GameplayEntity Owner
        {
            get { return m_owner; }
        }

        public Vector2Int Center
        {
            get { return m_owner.Visual.GridPosition; }
        }

        public int Radius
        {
            get { return 2; }
        }

        public void GetValidTargets(List<GameplayEntity> results)
        {
            results.Clear();

            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];

                if (entity.Visual.GridPosition != Center &&
                    Vector2.Distance(Center, entity.Visual.GridPosition) <= Radius)
                {
                    results.Add(entity);
                }
            }
        }

        public string Name
        {
            get { return "Ground Slam"; }
        }

        public string Description { get { return "Damage everything around you"; } }

        public ESkillType Type
        {
            get { return ESkillType.Offensive; }
        }
    }

    class GroundSlamState:IGameState
    {
        private readonly List<GameplayEntity> m_targets = new List<GameplayEntity>();
        private GameManager m_manager;
        private GroundSlam m_skill;
        private float m_timer;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Setup(GroundSlam skill)
        {
            m_skill = skill;
        }

        public void Begin()
        {
            m_timer = 2.5f;
            m_skill.GetValidTargets(m_targets);

            m_skill.Owner.Move(m_skill.Center, 0);

            m_manager.Service.PlayQuakeAnimation(m_skill.Center.x, m_skill.Center.y, m_skill.Radius,.2f);

            for (int i = 0; i < m_targets.Count; i++)
            {
                GameplayEntity target = m_targets[i];
                float delay = .2f + Vector2Int.Distance(m_skill.Center, target.Visual.GridPosition) * .1f;

                target.Status.Damage(1);

                target.PlayHealthBarAnimation(delay);
                target.Visual.PlayTakeDamageAnimation(delay);

                if (target.Status.Health <= 0)
                {
                    m_manager.Pathfinder.NavGraph.SetGridType(target.Visual.GridPosition, EntityType.None);
                    target.Visual.PlayDeathAnimation(delay + .15f);
                }
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
            get { return m_timer <= 0; }
        }
    }
}
