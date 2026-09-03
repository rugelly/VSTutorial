using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using VSTutorial.Blocks;

#nullable disable

namespace VSTutorial
{
	public class VSTutorialModSystem : ModSystem
	{

		// Called on server and client
		// Useful for registering block/entity classes on both sides
		public override void Start(ICoreAPI api)
		{
			base.Start(api);
			api.RegisterBlockClass(Mod.Info.ModID + ".blocktub", typeof(BlockTub));
			api.RegisterBlockEntityClass(Mod.Info.ModID + ".blockentitytub", typeof(BlockEntityTub));
			api.RegisterItemClass(Mod.Info.ModID + ".itemtubplatform", typeof(ItemTubPlatform));
		}

		ICoreClientAPI capi;

		public override void StartClientSide(ICoreClientAPI api)
		{
			capi = api;
			api.Event.RegisterGameTickListener(onTick, 100);
		}

		private void onTick(float dt)
		{
			var slot = capi.World.Player.InventoryManager.ActiveHotbarSlot;

			if (slot.Itemstack?.Collectible is ItemTubPlatform)
			{
				var siteList = ItemTubPlatform.siteList;

				var c = ColorUtil.ColorFromRgba(0, 50, 150, 50);
				capi.World.HighlightBlocks(capi.World.Player, 941, siteList, EnumHighlightBlocksMode.AttachedToSelectedBlock, EnumHighlightShape.Cube);
			}
			else
			{
				capi.World.HighlightBlocks(capi.World.Player, 941, ItemRoller.emptyList, EnumHighlightBlocksMode.AttachedToSelectedBlock, EnumHighlightShape.Cube);
				capi.World.HighlightBlocks(capi.World.Player, 942, ItemRoller.emptyList, EnumHighlightBlocksMode.AttachedToSelectedBlock, EnumHighlightShape.Cube);
			}
		}
	}
}
