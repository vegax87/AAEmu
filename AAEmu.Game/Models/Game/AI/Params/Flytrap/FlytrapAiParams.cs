using NLua;

namespace AAEmu.Game.Models.Game.AI.Params.Flytrap
{
    public class FlytrapAiParams : AiParams
    {
        public float AlertDuration { get; set; } = 3.0f;
        public float AlertSafeTargetRememberTime { get; set; } = 5.0f;
        public float AttackEndDistance { get; set; } // This is found in the entity?
        public float MeleeAttackRange { get; set; }
        public FlytrapCombatSkill CombatSkills { get; set; }

        public FlytrapAiParams(string aiPramsString)
        {
            Parse(aiPramsString);
        }

        private void Parse(string data)
        {
            using (var aiParams = new AiLua())
            {
                aiParams.DoString($"data = {{\n{data}\n}}");

                if (aiParams.GetObjectFromPath("data.alertDuration") != null)
                    AlertDuration = (float)aiParams.GetNumber("data.alertDuration");
                if (aiParams.GetObjectFromPath("data.alertSafeTargetRememberTime") != null)
                    AlertSafeTargetRememberTime = (float)aiParams.GetNumber("data.alertSafeTargetRememberTime");
                if (aiParams.GetObjectFromPath("data.attackEndDistance") != null)
                    AttackEndDistance = (float)aiParams.GetNumber("data.attackEndDistance");
                if (aiParams.GetObjectFromPath("data.meleeAttackRange") != null)
                    MeleeAttackRange = (float)aiParams.GetNumber("data.meleeAttackRange");
                CombatSkills = new FlytrapCombatSkill();
                if (aiParams.GetTable("data.combatSkills") is LuaTable table)
                {
                    CombatSkills.ParseLua(table);
                }
            }
        }
    }
}
