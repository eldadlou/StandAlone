using System.Collections.Generic;
using MyGame.Core.Skills;

namespace MyGame.Core.Units
{
    public interface ISkillUser
    {
        void UseSkill(int skillIndex);
        List<Skill> Skills { get; }
    }
}
