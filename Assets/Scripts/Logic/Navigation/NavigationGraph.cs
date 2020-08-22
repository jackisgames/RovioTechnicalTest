using Assets.Scripts.Presentation.Entities;
using Assets.Scripts.Presentation.Levels;
using UnityEngine;

namespace Logic.Navigation
{
    /// <summary>
    /// This class store the graph data for navigation purpose
    /// </summary>
    public class NavigationGraph
    {
        public readonly Vector2Int LevelSize;
        public readonly EntityType[,] EntityMap;
        public NavigationGraph(LevelData levelData)
        {
            LevelSize = new Vector2Int(levelData.Width, levelData.Height);

            EntityMap = new EntityType[LevelSize.x, LevelSize.y];

            for (int i = 0; i < levelData.Entities.Count; i++)
            {
                EntityComponent entity = levelData.Entities[i];
                if (entity.Type != EntityType.None)
                {
                    EntityMap[entity.GridPosition.x, entity.GridPosition.y] = entity.Type;
                }
            }
        }

        public bool IsBlocked(Vector2Int position)
        {
            return EntityMap[position.x, position.y] != EntityType.None;
        }

        public void SetGridType(Vector2Int position,EntityType type)
        {
            if (!OutsideBounds(position))
            {
                EntityMap[position.x, position.y] = type;
            }
        }

        public bool OutsideBounds(Vector2Int position)
        {
            return position.x < 0 || position.y < 0 || position.x >= LevelSize.x || position.y >= LevelSize.y;
        }
    }
}
