using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using Logic.Gameplay.State;
using UnityEngine;

namespace Logic.Gameplay.Skill
{
    class Reinforcement:ISkill
    {
        private readonly Vector2Int[] m_validDirections = new Vector2Int[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down};

        private readonly EntityType[] m_randomEntityTypes = new EntityType[] {EntityType.Player, EntityType.Enemy, EntityType.Obstacle};

        private GameManager m_manager;

        private GameplayEntity m_owner;

        public void Initialize(GameplayEntity owner, GameManager manager)
        {
            m_manager = manager;
            m_owner = owner;
        }

        public void Execute(GameplayEntity target)
        {
            m_manager.StateManager.RegisterState<ReinforcementState>().Setup(m_owner,this);
        }

        public void GetValidTargets(List<GameplayEntity> results)
        {
            results.Clear();

            for (int i = 0; i < m_validDirections.Length; i++)
            {
                Vector2Int position = m_owner.Visual.GridPosition + m_validDirections[i];
                if (!m_manager.Pathfinder.NavGraph.OutsideBounds(position) &&
                    !m_manager.Pathfinder.NavGraph.IsBlocked(position))
                {
                    results.Add(m_owner);
                    return;
                }
            }
        }

        public void GetValidPositions(List<Vector2Int> results)
        {
            results.Clear();

            for (int i = 0; i < m_validDirections.Length; i++)
            {
                Vector2Int position = m_owner.Visual.GridPosition + m_validDirections[i];

                if (!m_manager.Pathfinder.NavGraph.OutsideBounds(position) &&
                    !m_manager.Pathfinder.NavGraph.IsBlocked(position))
                {
                    results.Add(position);
                    return;
                }
            }
        }

        public EntityType SpawnType
        {
            get
            {
                if (Random.value < .75f)
                {
                    return m_randomEntityTypes[Random.Range(0, m_randomEntityTypes.Length)];
                }
                return m_owner.Visual.Type;
            }
        }

        public string Name
        {
            get { return "Reinforcement"; }
        }

        public string Description
        {
            get { return "Summon allies from magic portal (sometimes failed)"; }
        }

        public ESkillType Type
        {
            get { return ESkillType.Defensive; }
        }
    }

    class ReinforcementState:IGameState
    {
        private readonly List<Vector2Int> m_spawnPositions = new List<Vector2Int>();

        private GameManager m_manager;

        private GameplayEntity m_owner;

        private Reinforcement m_skill;

        private float m_timer;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Setup(GameplayEntity owner,Reinforcement skill)
        {
            m_owner = owner;
            m_skill = skill;
        }

        public void Begin()
        {
            m_skill.GetValidPositions(m_spawnPositions);
            
            if (m_spawnPositions.Count > 0)
            {
                m_timer = 3f;
                Vector2Int spawnPosition = m_spawnPositions[Random.Range(0, m_spawnPositions.Count)];

                EntityType spawnType = m_skill.SpawnType;
                EntityComponent visual = m_manager.Service.InstantiateEntity(spawnPosition.x, spawnPosition.y, spawnType);

                GameplayEntity gameplayEntity = m_manager.RegisterEntity(visual);

                gameplayEntity.Move(spawnPosition, 0);

                m_manager.Service.PlayQuakeAnimation(spawnPosition.x, spawnPosition.y, 2);
                m_manager.Pathfinder.NavGraph.SetGridType(spawnPosition, visual.Type);

                if (m_owner.Visual.Type == spawnType)
                {
                    m_manager.Ui.ShowAndHideBanner("Reinforcement has arrived", 0, 2);
                }
                else
                {
                    m_manager.Ui.ShowAndHideBanner("Magic malfunction", 0, 2);
                }
            }
            else
            {
                m_timer = .1f;
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
