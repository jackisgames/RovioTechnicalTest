using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Logic.Gameplay.Level
{
    class LevelManager
    {
        private readonly StringBuilder m_stringBuilder = new StringBuilder();

        private readonly char[] m_mapTiles = new char[] {'1', '2', '3', '4'};

        private readonly HashSet<Vector2Int> m_playerPlacementFlags = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> m_enemyPlacementFlags = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> m_obstaclePlacementFlags = new HashSet<Vector2Int>();

        public string GetLevelData(int level)
        {
            TextAsset resourceLevel = Resources.Load<TextAsset>($"Levels/Level{level}");
            if (resourceLevel != null)
            {
                return resourceLevel.text;
            }
            return GenerateProceduralLevel();
        }

        public string GenerateProceduralLevel()
        {
            m_stringBuilder.Clear();
            Vector2Int levelSize = new Vector2Int(Random.Range(5,9), Random.Range(5,15));
            m_stringBuilder.AppendLine(levelSize.x.ToString());
            m_stringBuilder.AppendLine(levelSize.y.ToString());

            //grounds
            m_stringBuilder.AppendLine();
            for (int y = 0; y < levelSize.y; y++)
            {
                for (int x = 0; x < levelSize.x; x++)
                {
                    m_stringBuilder.Append(m_mapTiles[Random.Range(0, m_mapTiles.Length)]);
                }

                m_stringBuilder.AppendLine();
            }

            //entities
            int numPlayer = Random.Range(1, 5);
            PlaceRandom(numPlayer, levelSize, m_playerPlacementFlags);

            int numEnemy = numPlayer + Random.Range(0, 3);
            PlaceRandom(numEnemy, levelSize, m_enemyPlacementFlags);

            int numFences = Random.Range(0,Mathf.FloorToInt((levelSize.x* levelSize.y)*.2f));
            PlaceRandom(numFences, levelSize, m_obstaclePlacementFlags);

            m_stringBuilder.AppendLine();
            for (int y = 0; y < levelSize.y; y++)
            {
                for (int x = 0; x < levelSize.x; x++)
                {
                    Vector2Int location = new Vector2Int(x, y);
                    if (m_playerPlacementFlags.Contains(location))
                    {
                        m_stringBuilder.Append('p');
                    }
                    else if (m_enemyPlacementFlags.Contains(location))
                    {
                        m_stringBuilder.Append('e');
                    }
                    else if (m_obstaclePlacementFlags.Contains(location))
                    {
                        m_stringBuilder.Append('#');
                    }
                    else
                    {
                        m_stringBuilder.Append('.');
                    }
                }

                m_stringBuilder.AppendLine();
            }

            return m_stringBuilder.ToString();
        }

        private void PlaceRandom(int count, Vector2Int levelSize, HashSet<Vector2Int> flags)
        {
            flags.Clear();

            while (count>0)
            {
                if(flags.Add(new Vector2Int(Random.Range(0,levelSize.x), Random.Range(0, levelSize.y))))
                {
                    count--;
                }
            }
        }
    }
}
