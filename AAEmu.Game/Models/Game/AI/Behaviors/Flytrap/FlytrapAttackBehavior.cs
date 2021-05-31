using System;
using System.Linq;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.AI.Params.Flytrap;
using AAEmu.Game.Models.Game.Skills;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.AI.Behaviors.Flytrap
{
    public class FlytrapAttackBehavior : BaseCombatBehavior
    {
        FlytrapAiParams _aiParams;

        public override void Enter()
        {
            _aiParams = Ai.Owner.Template.AiParams as FlytrapAiParams;
        }

        public override void Tick(TimeSpan delta)
        {
            if (_aiParams == null)
                return;

            if (!UpdateTarget() || ShouldReturn)
            {
                Ai.GoToReturn();
                return;
            }

            if (!CanUseSkill)
                return;

            var trgDistance = Ai.Owner.GetDistanceTo(Ai.Owner.CurrentTarget);

            if (trgDistance > _aiParams.AttackEndDistance)
            {
                Ai.Owner.ClearAggroOfUnit((Unit)Ai.Owner.CurrentTarget);
                return;
            }

            uint skillId = PickSkill(trgDistance);

            if (skillId != 0)
            {
                var skillTemplate = SkillManager.Instance.GetSkillTemplate(skillId);
                if (skillTemplate != null)
                    UseSkill(new Skill(skillTemplate), Ai.Owner.CurrentTarget, 0);
            }
        }

        public uint PickSkill(float trgDist)
        {
            uint skillId;
            if (trgDist <= _aiParams.MeleeAttackRange)
            {
                skillId = _aiParams.CombatSkills.Melee
                    .Where(s => !Ai.Owner.Cooldowns.CheckCooldown(s))
                    .Where(s =>
                    {
                        var template = SkillManager.Instance.GetSkillTemplate(s);
                        return (template != null && (trgDist >= template.MinRange && trgDist <= template.MaxRange || template.TargetType == SkillTargetType.Self));
                    }).FirstOrDefault();
                if (skillId != 0)
                    return skillId;
            }
            else
            {
                skillId = _aiParams.CombatSkills.Ranged
                    .Where(s => !Ai.Owner.Cooldowns.CheckCooldown(s))
                    .Where(s =>
                    {
                        var template = SkillManager.Instance.GetSkillTemplate(s);
                        return (template != null && (trgDist >= template.MinRange && trgDist <= template.MaxRange || template.TargetType == SkillTargetType.Self));
                    }).FirstOrDefault();
                if (skillId != 0)
                    return skillId;
            }

            return 0;
        }

        public override void Exit()
        {
        }
    }
}
