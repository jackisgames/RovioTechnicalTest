using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Logic.Gameplay.Skill;
using UnityEngine;

namespace Logic.Gameplay.Entity
{
    class GameplayEntity
    {
        public readonly TurnState TurnState = new TurnState();

        public readonly EntityStatus Status = new EntityStatus();

        public readonly EntityComponent Visual;

        public List<ISkill> Skills = new List<ISkill>();

        internal GameplayEntity(EntityComponent visual)
        {
            Visual = visual;
        }

        public void ValidateMovement(List<Vector2Int> moves)
        {
            int overflow = moves.Count - Status.MaxTraverseCount;

            if (overflow>0)
            {
                moves.RemoveRange(Status.MaxTraverseCount, overflow);
            }
        }

        public void PlayHealthBarAnimation(float delay)
        {
            Visual.PlayHealthBarAnimation(Status.HealthPercentage, delay);
        }

        public float Move(List<Vector2Int> moves)
        {
            for (int i = 1; i < moves.Count; i++)
            {
                Visual.Leap(moves[i],.3f*i);
            }

            return moves.Count * .3f;
        }

        public void Move(Vector2Int target,float delay)
        {
            Visual.Leap(target, delay);
        }

        //reset flags & visuals before start of the turn
        public void InitializeTurn()
        {
            Visual.ShowSelectionAttackTarget(false);
            Visual.ShowSelection(false);

            if (Status.Health <= 0)
            {
                return;
            }
            TurnState.CanAttack = true;
            TurnState.CanMove = true;

        }

        public bool HaveAction
        {
            get { return Status.Health > 0 && (TurnState.CanMove || TurnState.CanAttack); }
        }
    }

    class EntityStatus
    {
        private int m_health = 2;
        private int m_healthMax = 2;

        public int Health
        {
            get { return m_health; }
        }

        public int HealthMax
        {
            get { return m_healthMax; }
        }

        public void Heal(int amount)
        {
            m_health = Mathf.Min(m_health + amount, m_healthMax);
        }

        public void SetHealth(int amount)
        {
            m_health = Mathf.Clamp(amount, 0, m_healthMax);
        }

        public void Damage(int amount)
        {
            m_health = Mathf.Max(m_health - amount, 0);
        }

        public float HealthPercentage
        {
            get { return (float)m_health / m_healthMax; }
        }

        public int MaxTraverseCount;
    }

    class TurnState
    {
        public bool CanMove;
        public bool CanAttack;
    }
}