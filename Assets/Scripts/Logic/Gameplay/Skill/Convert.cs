using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using Logic.Gameplay.State;
using UnityEngine;

namespace Logic.Gameplay.Skill
{
    class Convert:ISkill
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
            m_manager.StateManager.RegisterState<ConvertState>().Setup(m_owner, target);
        }

        public void GetValidTargets(List<GameplayEntity> results)
        {
            results.Clear();

            for (int i = 0; i < m_manager.GameplayEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.GameplayEntities[i];
                if (m_owner.Visual.Type != entity.Visual.Type && entity.Visual.Type != EntityType.Obstacle)
                {
                    if (Vector2Int.Distance(m_owner.Visual.GridPosition, entity.Visual.GridPosition) <= 3)
                    {
                        results.Add(entity);
                    }
                }
            }
        }

        public string Name
        {
            get { return "Yololo"; }
        }

        public string Description
        {
            get { return "Convince enemy to switch side. If fail you will be converted instead"; }
        }

        public ESkillType Type
        {
            get { return ESkillType.Offensive; }
        }
    }

    class ConvertState : IGameState
    {
        private GameManager m_manager;
        private GameplayEntity m_attacker;
        private GameplayEntity m_target;

        private bool m_end;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Setup(GameplayEntity attacker, GameplayEntity target)
        {
            m_attacker = attacker;
            m_target = target;
        }
        public void Begin()
        {
            m_end = false;
            bool isSuccess = Random.value > (.35f + m_target.Status.Health * .25f);

            GameplayEntity targetConvert;
            EntityType convertTo;

            if (isSuccess)
            {
                targetConvert = m_target;
                convertTo = m_attacker.Visual.Type;
            }
            else
            {
                targetConvert = m_attacker;
                convertTo = m_target.Visual.Type;
            }

            
            m_manager.Pathfinder.NavGraph.SetGridType(targetConvert.Visual.GridPosition, convertTo);

            m_attacker.Move(m_attacker.Visual.GridPosition, 0);
            m_manager.StartCoroutine(ConvertActions(targetConvert, convertTo, .35f));
            m_manager.Ui.ShowAndHideBanner(isSuccess ? "Yololo yeah" : "Yololonoooo!", .3f);
        }


        private IEnumerator ConvertActions(GameplayEntity targetConvert,EntityType convertTo,float delay)
        {
            yield return new WaitForSeconds(delay);

            EntityComponent visual = m_manager.Service.InstantiateEntity(targetConvert.Visual.GridPosition.x, targetConvert.Visual.GridPosition.y, convertTo);
            GameplayEntity gameplayEntity = m_manager.RegisterEntity(visual);
            gameplayEntity.Status.SetHealth(targetConvert.Status.Health);
            gameplayEntity.Visual.PlayHealthBarAnimation(gameplayEntity.Status.HealthPercentage);

            targetConvert.Status.SetHealth(0);
            targetConvert.Visual.gameObject.SetActive(false);

            yield return new WaitForSeconds(2);
            m_end = true;
        }

        public void Tick()
        {
            
        }

        public void End()
        {
            m_manager.StateManager.RegisterState<StartTurnState>();
        }

        public bool ShouldEnd { get { return m_end; } }
    }
}
