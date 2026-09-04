using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using VSTutorial.Blocks;

#nullable disable

namespace VSTutorial
{
	internal class ModSystemTubConstructionSitePreview : ModSystem
	{
		private ICoreClientAPI capi;
		public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

		public override void StartClientSide(ICoreClientAPI api)
		{
			capi = api;
			api.Event.RegisterGameTickListener(onTick, 100);
		}

		private void onTick(float dt)
		{
			
			var slot = capi.World.Player.InventoryManager.ActiveHotbarSlot;
			ItemStack itemStack = slot.Itemstack;
			ItemTubPlatform platform = ((itemStack != null) ? itemStack.Collectible : null) as ItemTubPlatform; // stolen straight from shipwright

			if (platform != null)
			{
				platform.UpdateListsFromStack(slot.Itemstack);
				int orient = ItemTubPlatform.GetOrient(capi.World.Player);
				var siteList = ItemTubPlatform.siteListByFacing[orient];

				capi.World.HighlightBlocks(capi.World.Player, 1194, siteList, EnumHighlightBlocksMode.AttachedToSelectedBlock, EnumHighlightShape.Cube);
			}
			else
			{
				capi.World.HighlightBlocks(capi.World.Player, 1194, ItemTubPlatform.emptyList, EnumHighlightBlocksMode.AttachedToSelectedBlock, EnumHighlightShape.Cube);
			}
		}
	}
}
