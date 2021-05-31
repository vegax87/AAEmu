using System;
using AAEmu.Game.Models.Game.AI.Framework;

namespace AAEmu.Game.Models.Game.AI.Behaviors.Common
{
    public class RunCommandSetBehavior : Behavior
    {
        public override void Enter()
        {
        }

        public override void Tick(TimeSpan delta)
        {
            // TODO: Proper code
            Ai.GoToIdle();
        }

        public override void Exit()
        {
        }
    }
}
