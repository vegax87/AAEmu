using NLua;

namespace AAEmu.Game.Models.Game.AI.Params.Archer
{
    public class ArcherAiParams : AiParams
    {
        public float AlertDuration { get; set; } = 3.0f;
        public float AlertSafeTargetRememberTime { get; set; } = 5.0f;
        public int MeleeAttackRange { get; set; } // This is found in the entity?
        public ArcherCombatSkill CombatSkills { get; set; }
        public int PreferedCombastDist { get; set; }
        public int MaxMakeAGapeCount { get; set; } = 3;

        public ArcherAiParams(string aiPramsString)
        {
            Parse(aiPramsString);
        }

        private void Parse(string data)
        {
            using (var aiParams = new AiLua())
            {
                aiParams.DoString($"data = {{\n{data}\n}}");

                if (aiParams.GetObjectFromPath("data.alertDuration") != null)
                    AlertDuration = aiParams.GetInteger("data.alertDuration");
                if (aiParams.GetObjectFromPath("data.alertSafeTargetRememberTime") != null)
                    AlertSafeTargetRememberTime = aiParams.GetInteger("data.alertSafeTargetRememberTime");
                if (aiParams.GetObjectFromPath("data.preferedCombatDist") != null)
                    PreferedCombastDist = aiParams.GetInteger("data.preferedCombatDist");
                if (aiParams.GetObjectFromPath("data.meleeAttackRange") != null)
                    MeleeAttackRange = aiParams.GetInteger("data.meleeAttackRange");
                CombatSkills = new ArcherCombatSkill();
                if (aiParams.GetTable("data.combatSkills") is LuaTable table)
                {
                    CombatSkills.ParseLua(table);
                }
                if (aiParams.GetObjectFromPath("data.maxMakeAGapCount") != null)
                    MaxMakeAGapeCount = aiParams.GetInteger("data.maxMakeAGapCount");
            }
        }
    }
}
