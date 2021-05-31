using System;
using System.Collections.Generic;
using System.Linq;

using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.AI.Params;
using AAEmu.Game.Models.Game.AI.Params.Almighty;
using AAEmu.Game.Models.Game.Skills;

namespace AAEmu.Game.Models.Game.AI.Behaviors.Common
{
    public class AlmightyAttackBehavior : BaseCombatBehavior
    {
        private AlmightyNpcAiParams _aiParams;
        private Queue<AiSkill> _skillQueue;
        private DateTime _combatStartTime;

        public override void Enter()
        {
            _combatStartTime = DateTime.UtcNow;
            Ai.Owner.InterruptSkills();
            _aiParams = Ai.Owner.Template.AiParams as AlmightyNpcAiParams;
            _skillQueue = new Queue<AiSkill>();
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

            if (CanStrafe && !IsUsingSkill)
                MoveInRange(Ai.Owner.CurrentTarget, 2f, delta);

            if (!CanUseSkill)
                return;

            _strafeDuringDelay = false;
            #region Pick a skill

            var targetDist = Ai.Owner.GetDistanceTo(Ai.Owner.CurrentTarget);

            if (_skillQueue.Count == 0)
            {
                if (!RefreshSkillQueue(targetDist))
                    return;
            }

            var selectedSkill = _skillQueue.Dequeue();
            if (selectedSkill == null)
                return;
            var skillTemplate = SkillManager.Instance.GetSkillTemplate(selectedSkill.SkillId);
            if (skillTemplate != null)
            {
                if (targetDist >= skillTemplate.MinRange && targetDist <= skillTemplate.MaxRange)
                {
                    Ai.Owner.StopMovement();
                    UseSkill(new Skill(skillTemplate), Ai.Owner.CurrentTarget, selectedSkill.Delay);
                    _strafeDuringDelay = selectedSkill.Strafe;
                }
            }
            // If skill list is empty, get Base skill
            #endregion
        }

        public override void Exit()
        {
        }

        private bool RefreshSkillQueue(float trgDist)
        {
            var availableSkills = RequestAvailableSkillList(trgDist);

            if (availableSkills.Count > 0)
            {
                var selectedSkillList = availableSkills.RandomElementByWeight(s => s.Dice);

                foreach (var skill in selectedSkillList.Skills)
                {
                    _skillQueue.Enqueue(skill);
                }

                return _skillQueue.Count > 0;
            }
            else
            {
                if (Ai.Owner.Template.BaseSkillId != 0)
                {
                    _skillQueue.Enqueue(new AiSkill
                    {
                        SkillId = (uint)Ai.Owner.Template.BaseSkillId,
                        Strafe = Ai.Owner.Template.BaseSkillStrafe,
                        Delay = Ai.Owner.Template.BaseSkillDelay
                    });
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private List<AiSkillList> RequestAvailableSkillList(float trgDist)
        {
            int healthRatio = (int)(((float)Ai.Owner.Hp / Ai.Owner.MaxHp) * 100);

            var baseList = _aiParams.AiSkillLists.AsEnumerable();
            var timeElapsed = (DateTime.UtcNow - _combatStartTime).TotalSeconds;

            baseList = baseList.Where(s => s.HealthRangeMin <= healthRatio && healthRatio <= s.HealthRangeMax);
            baseList = baseList.Where(s => s.Skills.All(skill => !Ai.Owner.Cooldowns.CheckCooldown(skill.SkillId)));
            baseList = baseList.Where(s =>
            {
                return s.Skills.All(skill =>
                {
                    var template = SkillManager.Instance.GetSkillTemplate(skill.SkillId);
                    return (template != null && (trgDist >= template.MinRange && trgDist <= template.MaxRange || template.TargetType == SkillTargetType.Self));
                });
            });

            baseList = baseList.Where(s => ((s.TimeRangeStart >= 0 && s.TimeRangeEnd > 0) || (s.TimeRangeStart > 0 && s.TimeRangeEnd >= 0)) // (0, x) or (x, 0), conditions for skils to be eligible for timerange
                                            && ((s.TimeRangeStart <= timeElapsed && timeElapsed <= s.TimeRangeEnd) || // (timeStart <= x >= timeEnd) => (x, x), (0, x)
                                                (s.TimeRangeStart <= timeElapsed && s.TimeRangeEnd == 0)));  // timeStart <= x && timeEnd == 0 => (x, 0)
            var test2 = baseList.ToList();
            return baseList.ToList();
        }
    }
}
