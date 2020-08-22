using System.Collections.Generic;
using UnityEngine;

namespace Logic.Navigation
{
    public class Pathfinder
    {
        public NavigationGraph NavGraph;

        private readonly Vector2Int[] m_validDirections=new Vector2Int[]{Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

        private readonly HashSet<int> m_closedNodes = new HashSet<int>();

        private readonly Dictionary<int, Node> m_openNodeLookup = new Dictionary<int,Node>();

        private readonly Queue<Node> m_openNodes = new Queue<Node>();

        public void Load(NavigationGraph graph)
        {
            NavGraph = graph;
        }

        public bool Navigate(Vector2Int origin, Vector2Int destination,List<Vector2Int> result)
        {
            result.Clear();

            if (NavGraph.OutsideBounds(origin) || NavGraph.OutsideBounds(destination))
            {
                return false;
            }
            m_openNodes.Clear();
            m_closedNodes.Clear();
            m_openNodeLookup.Clear();

            bool destinationReached = false;
            int traversalCount = 0;
            float bestDistance = Distance(origin, destination);
            Node bestResult = new Node(origin)
            {
                Distance = bestDistance
            };
            m_openNodes.Enqueue(bestResult);
            m_openNodeLookup[ToHash(origin)] = bestResult;


            while (m_openNodes.Count>0)
            {
                
                Node pointer = m_openNodes.Dequeue();

                if (pointer.Position == destination)
                {
                    destinationReached = true;
                    bestResult = pointer;
                    break;
                }
                else
                {
                    m_openNodeLookup.Remove(ToHash(pointer.Position));

                    m_closedNodes.Add(ToHash(pointer.Position));
                    traversalCount++;

                    for (int i = 0; i < m_validDirections.Length; i++)
                    {
                        Vector2Int neighbourPosition = pointer.Position + m_validDirections[i];
                        if (!NavGraph.OutsideBounds(neighbourPosition) &&
                            !NavGraph.IsBlocked(neighbourPosition) &&
                            m_closedNodes.Add(ToHash(neighbourPosition)))
                        {
                            Node neighbourNode;
                            float distance = Distance(neighbourPosition, destination);
                            int hash = ToHash(neighbourPosition);
                            if (m_openNodeLookup.TryGetValue(hash, out neighbourNode))
                            {
                                if (traversalCount + distance < neighbourNode.Distance + neighbourNode.TraversalCount)
                                {
                                    neighbourNode.Distance = distance;
                                    neighbourNode.TraversalCount = traversalCount;
                                    neighbourNode.Previous = pointer;
                                }
                            }
                            else
                            {
                                neighbourNode = new Node(neighbourPosition);
                                neighbourNode.Distance = distance;
                                neighbourNode.TraversalCount = traversalCount;
                                neighbourNode.Previous = pointer;

                                m_openNodes.Enqueue(neighbourNode);
                                m_openNodeLookup[hash] = neighbourNode;
                            }

                            if (distance < bestDistance)
                            {
                                bestResult = neighbourNode;
                                bestDistance = distance;
                            }
                        }
                    }
                }
            }

            while (bestResult.Previous!=null)
            {
                result.Add(bestResult.Position);
                bestResult = bestResult.Previous;
            }
            result.Add(bestResult.Position);
            result.Reverse();

            return destinationReached;
        }

        private int ToHash(Vector2Int v)
        {
            return v.x * NavGraph.LevelSize.y + v.y;
        }

        private float Distance(Vector2Int origin, Vector2Int destination)
        {
            return Vector2Int.Distance(origin, destination);
        }
    }

    class Node
    {
        public readonly Vector2Int Position;
        public Node Previous;
        public int TraversalCount;
        public float Distance;
        internal Node(Vector2Int position)
        {
            Position = position;
        }
    }
}
