using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using Logic.Gameplay.State;
using UnityEngine;

namespace Logic.Gameplay.Skill
{
    class BasicAttack:ISkill
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
            m_manager.StateManager.RegisterState<BasicAttackState>().Setup(m_owner,target, 1);
        }

        public void GetValidTargets(List<GameplayEntity> results)
        {
            results.Clear();

            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];
                if (m_owner.Visual.Type != entity.Visual.Type)
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
            get { return "Attack"; }
        }

        public string Description
        {
            get { return "Basic Attack"; }
        }

        public ESkillType Type
        {
            get { return ESkillType.Offensive; }
        }
    }

    class BasicAttackState : IGameState
    {
        private GameManager m_manager;
        private GameplayEntity m_attacker;
        private GameplayEntity m_target;
        private int m_damage;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Setup(GameplayEntity attacker, GameplayEntity target, int damage)
        {
            m_attacker = attacker;
            m_target = target;
            m_damage = damage;

        }

        public void Begin()
        {
            if (m_target.Status.Health > 0)
            {
                m_target.Status.Damage(m_damage);
                ShouldEnd = false;
            }
            else
            {
                ShouldEnd = true;
            }

            m_attacker.Visual.StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            yield return new WaitForSeconds(.2f);

            m_attacker.Move(m_attacker.Visual.GridPosition, .3f);
            m_attacker.Move(m_target.Visual.GridPosition, 0);


            yield return new WaitForSeconds(.3f);

            m_target.PlayHealthBarAnimation(0);
            m_target.Visual.PlayTakeDamageAnimation();
            m_target.Visual.InstantiateSlash(m_target.Visual.GridPosition.x, m_target.Visual.GridPosition.y);

            yield return new WaitForSeconds(.1f);

            if (m_target.Status.Health <= 0)
            {
                m_manager.Pathfinder.NavGraph.SetGridType(m_target.Visual.GridPosition, EntityType.None);
                m_target.Visual.PlayDeathAnimation();
            }
            yield return new WaitForSeconds(1.5f);
            ShouldEnd = true;
        }

        public void Tick()
        {

        }

        public void End()
        {
            m_manager.StateManager.RegisterState<StartTurnState>();
        }

        public bool ShouldEnd { get; private set; }
    }
}
