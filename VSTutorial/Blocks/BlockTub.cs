using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
#nullable disable
namespace VSTutorial.Blocks
{
	public class BlockTub : Block
	{
		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			if (blockSel != null && !world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
			{
				return false;
			}

			BlockEntityTub betub = null;
			if (blockSel.Position != null)
			{
				betub = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityTub;
			}
			//if (betub != null && betub.Sealed)
			//{
			//	return true;
			//}

			var hslot = byPlayer.InventoryManager.ActiveHotbarSlot;
			if (!hslot.Empty && hslot.Itemstack.Collectible.HasBehavior<CollectibleBehaviorQuenchable>()) return false;

			bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);

			if (!handled && !byPlayer.WorldData.EntityControls.ShiftKey && blockSel.Position != null)
			{
				if (betub != null)
				{
					betub.OnPlayerRightClick(byPlayer);
				}

				return true;
			}

			return handled;
		}
	}
}
