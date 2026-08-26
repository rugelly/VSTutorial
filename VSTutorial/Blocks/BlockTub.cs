using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
#nullable disable
namespace VSTutorial.Blocks
{
	public class BlockTub : BlockLiquidContainerTopOpened
	{
		public override int GetContainerSlotId(BlockPos pos) => 4; // are these both the max item slots???
		public override int GetContainerSlotId(ItemStack containerStack) => 4;

		public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
		{
			return base.GetPlacedBlockInfo(world, pos, forPlayer);
			// TODO: looks like sealing timer goes here. maybe items currently inside could also?
			// woudl be a nice indicator of when done etc
		}

		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			BlockEntityTub blockEntityTub = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityTub;
			if (blockEntityTub != null && blockEntityTub.Sealed)
				return true;

			bool flag = base.OnBlockInteractStart(world, byPlayer, blockSel);
			if (!flag && !byPlayer.WorldData.EntityControls.ShiftKey && blockSel.Position != null)
			{
				if (blockEntityTub != null)
					blockEntityTub.OnPlayerRightClick(byPlayer);
				return true;
			}
			return flag;
		}
	}
}
