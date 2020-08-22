using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Entity;
using UnityEngine;

namespace Logic.Gameplay.State
{
    class EntityMoveState:IGameState
    {
        private GameManager m_manager;

        private GameplayEntity m_entity;

        private List<Vector2Int> m_path;

        private float m_travelTime;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;
        }

        public void Move(GameplayEntity entity, List<Vector2Int> path)
        {
            m_entity = entity;
            m_path = path;
        }

        public void Begin()
        {
            if (m_path.Count >= 2)
            {
                Vector2Int start = m_entity.Visual.GridPosition;
                Vector2Int end = m_path[m_path.Count - 1];

                m_manager.Pathfinder.NavGraph.SetGridType(start,EntityType.None);
                m_manager.Pathfinder.NavGraph.SetGridType(end,m_entity.Visual.Type);

                m_travelTime = m_entity.Move(m_path);
                
            }
            else
            {
                m_travelTime = 0;
            }
        }

        public void Tick()
        {
            m_travelTime -= Time.deltaTime;
        }

        public void End()
        {
            m_manager.Service.HideAllBreadCrumbs();
            m_manager.StateManager.RegisterState<StartTurnState>();
        }

        public bool ShouldEnd
        {
            get { return m_travelTime <= 0; }
        }
    }
}
