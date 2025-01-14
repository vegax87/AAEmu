using System.Linq;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.Id;
using AAEmu.Game.Models.Game.Items;
using AAEmu.Game.Models.Game.Items.Actions;
using AAEmu.Game.Models.Game.Items.Containers;
using AAEmu.Game.Utils;
using AAEmu.Tests.Utils;
using AAEmu.Tests.Utils.Mocks;
using Xunit;

namespace AAEmu.Tests
{
    public class InventoryTests
    {
        [Fact]
        public void InventoryAddsItem()
        {
            // ItemIdManager.Instance.Initialize();
            
            var container = new ItemContainer(new CharacterMock().Id, SlotType.Inventory, false, false);
            var item = InventoryTestUtils.MockItem(1, 1);
           
            Assert.True(container.AddOrMoveExistingItem(ItemTaskType.Gm, item, 1));

            var i = container.Items.SingleOrDefault(it => it.TemplateId == 1);
           
            Assert.NotNull(i);
        }
    }
}
