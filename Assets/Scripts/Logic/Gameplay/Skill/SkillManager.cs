using System.Collections.Generic;
using Logic.Gameplay.Entity;
using UnityEngine;

namespace Logic.Gameplay.Skill
{
    class SkillManager
    {
        private GameManager m_manager;

        public void Initialize(GameManager manager)
        {
            this.m_manager = manager;
        }

        public T GetSkill<T>(GameplayEntity owner) where T : ISkill
        {
            T skill = System.Activator.CreateInstance<T>();
            skill.Initialize(owner, m_manager);
            return skill;
        }
    }
}
