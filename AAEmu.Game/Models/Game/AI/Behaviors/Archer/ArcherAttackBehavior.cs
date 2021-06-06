using System;
using System.Linq;

using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.AI.Params.Archer;
using AAEmu.Game.Models.Game.Skills;

namespace AAEmu.Game.Models.Game.AI.Behaviors.Archer
{
    public class ArcherAttackBehavior : BaseCombatBehavior
    {
        private ArcherAiParams _aiParams;
        private bool _meleeMode;
        private int madeAGapCount;
        public override void Enter()
        {
            _aiParams = Ai.Owner.Template.AiParams as ArcherAiParams;
        }

        public override void Tick(TimeSpan delta)
        {
            if (!UpdateTarget() || ShouldReturn)
            {
                Ai.GoToReturn();
                return;
            }
            var trgDistance = Ai.Owner.GetDistanceTo(Ai.Owner.CurrentTarget);

            if (trgDistance > _aiParams.PreferedCombastDist && !_meleeMode || trgDistance > _aiParams.MeleeAttackRange && _meleeMode)
            {
                if (_meleeMode)
                    MoveInRange(Ai.Owner.CurrentTarget, _aiParams.MeleeAttackRange, delta);
                else
                    MoveInRange(Ai.Owner.CurrentTarget, _aiParams.PreferedCombastDist, delta);
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
            if (trgDist < _aiParams.MeleeAttackRange && madeAGapCount < _aiParams.MaxMakeAGapeCount)
            {
                skillId = _aiParams.CombatSkills.MakeAGap
                    .Where(s => !Ai.Owner.Cooldowns.CheckCooldown(s))
                    .Where(s =>
                    {
                        var template = SkillManager.Instance.GetSkillTemplate(s);
                        return template != null && (trgDist >= template.MinRange && trgDist <= template.MaxRange || template.TargetType == SkillTargetType.Self);
                    }).FirstOrDefault();
                if (skillId != 0)
                {
                    _meleeMode = false;
                    return skillId;
                }
            }
            if (trgDist <= _aiParams.MeleeAttackRange)
            {
                skillId = _aiParams.CombatSkills.Melee
                    .Where(s => !Ai.Owner.Cooldowns.CheckCooldown(s))
                    .Where(s =>
                    {
                        var template = SkillManager.Instance.GetSkillTemplate(s);
                        return template != null && (trgDist >= template.MinRange && trgDist <= template.MaxRange || template.TargetType == SkillTargetType.Self);
                    }).FirstOrDefault();
                if (skillId != 0)
                    return skillId;
            }
            else
            {
                skillId = _aiParams.CombatSkills.RangedDef.Concat(_aiParams.CombatSkills.RangedStrong)
                    .Where(s => !Ai.Owner.Cooldowns.CheckCooldown(s))
                    .Where(s =>
                    {
                        var template = SkillManager.Instance.GetSkillTemplate(s);
                        return template != null && (trgDist >= template.MinRange && trgDist <= template.MaxRange || template.TargetType == SkillTargetType.Self);
                    }).FirstOrDefault();
                if (skillId != 0)
                    return skillId;
                else
                    _meleeMode = true;
            }

            return 0;
        }

        public override void Exit()
        {
        }
    }
}
