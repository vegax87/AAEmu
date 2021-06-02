namespace AAEmu.Game.Models.Game.Char
{
    public enum ActionSlotType : byte
    {
        None = 0,
        Item1 = 1,
        Skill = 2,
        Unk3 = 3,
        Item4 = 4,
        Unk5 = 5,
        Unk6 = 6
    }

    public class ActionSlot
    {
        public ActionSlotType Type { get; set; }
        public ulong ActionId { get; set; }
    }
}
