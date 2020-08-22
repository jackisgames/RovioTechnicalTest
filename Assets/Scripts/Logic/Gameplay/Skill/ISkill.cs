using System.Collections.Generic;
using Logic.Gameplay.Entity;

namespace Logic.Gameplay.Skill
{
    interface ISkill
    {
        void Initialize(GameplayEntity owner, GameManager manager);

        void Execute(GameplayEntity target);

        void GetValidTargets(List<GameplayEntity> results);

        string Name { get; }

        string Description { get; }

        ESkillType Type { get; }
    }
}
