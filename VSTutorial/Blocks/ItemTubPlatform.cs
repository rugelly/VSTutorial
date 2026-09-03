using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

#nullable disable

namespace VSTutorial.Blocks
{
	// copied from ship construction with water edge removed
	public class ModSystemTubConstructionSitePreview : ModSystem
	{
		ICoreClientAPI capi;

		public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

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
				//int orient = ItemTubPlatform.GetOrient(capi.World.Player);
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

	public class ItemTubPlatform : Item
	{
		public static List<BlockPos> emptyList = new List<BlockPos>();

		//public static List<List<BlockPos>> siteListByFacing = new List<List<BlockPos>>();

		public static List<BlockPos> siteList = new List<BlockPos>() { new BlockPos(0, 0, 0), new BlockPos(1, 1, 1) };

		public SkillItem[] skillItems;

		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);

			//siteList.Add(siteList);

			//for (int i = 1; i < 4; i++)
			//{
			//	siteListByFacing.Add(rotateList(siteListN, i));
			//}

			//skillItems = new SkillItem[]
			//{
			//	new SkillItem() { Code = new AssetLocation("east"), Name = Lang.Get("facing-east") },
			//	new SkillItem() { Code = new AssetLocation("north"), Name = Lang.Get("facing-north") },
			//	new SkillItem() { Code = new AssetLocation("west"), Name = Lang.Get("facing-west") },
			//	new SkillItem() { Code = new AssetLocation("south"), Name = Lang.Get("facing-south") },
			//};

			//if (api is ICoreClientAPI capi)
			//{
			//	skillItems[0].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/pointeast.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
			//	skillItems[1].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/pointnorth.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
			//	skillItems[2].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/pointwest.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
			//	skillItems[3].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/pointsouth.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
			//}
		}

		//public override void OnUnloaded(ICoreAPI api)
		//{
		//	if (skillItems != null)
		//	{
		//		foreach (var sk in skillItems) sk.Dispose();
		//	}
		//}

		//private static List<BlockPos> rotateList(List<BlockPos> startlist, int i)
		//{
		//	Matrixf matrixf = new Matrixf();
		//	matrixf.RotateY(i * GameMath.PIHALF);
		//	switch (i)
		//	{
		//		case 1:
		//			matrixf.Translate(-1f, 0f, 0f);
		//			break;
		//		case 2:
		//			matrixf.Translate(-1f, 0f, -1f);
		//			break;
		//		case 3:
		//			matrixf.Translate(0f, 0f, -1f);
		//			break;
		//	}

		//	List<BlockPos> list = new List<BlockPos>();
		//	var vec1 = matrixf.TransformVector(new Vec4f(startlist[0].X, startlist[0].Y, startlist[0].Z, 1));
		//	var vec2 = matrixf.TransformVector(new Vec4f(startlist[1].X, startlist[1].Y, startlist[1].Z, 1));

		//	var minpos = new BlockPos((int)Math.Round(Math.Min(vec1.X, vec2.X)), (int)Math.Round(Math.Min(vec1.Y, vec2.Y)), (int)Math.Round(Math.Min(vec1.Z, vec2.Z)));
		//	var maxpos = new BlockPos((int)Math.Round(Math.Max(vec1.X, vec2.X)), (int)Math.Round(Math.Max(vec1.Y, vec2.Y)), (int)Math.Round(Math.Max(vec1.Z, vec2.Z)));

		//	list.Add(minpos);
		//	list.Add(maxpos);

		//	return list;
		//}

		//public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
		//{
		//	return GetOrient(byPlayer);
		//}

		//public static int GetOrient(IPlayer byPlayer)
		//{
		//	siteListN = new List<BlockPos>() { new BlockPos(0, 0, 0), new BlockPos(1, 1, 1) };
		//	siteListByFacing.Clear();
		//	siteListByFacing.Add(siteListN);
		//	for (int i = 1; i < 4; i++)
		//	{
		//		siteListByFacing.Add(rotateList(siteListN, i));
		//	}

		//	return ObjectCacheUtil.GetOrCreate(byPlayer.Entity.Api, "tubPlatformOrient-" + byPlayer.PlayerUID, () => 0);
		//}

		//public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
		//{
		//	return skillItems;
		//}

		//public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
		//{
		//	api.ObjectCache["tubPlatformOrient-" + byPlayer.PlayerUID] = toolMode;
		//}

		public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
		{
			if (blockSel == null) return;
			var player = (byEntity as EntityPlayer)?.Player;
			int orient = (int)GameMath.RoundTo(player.Entity.BodyYaw, 1); // TODO: would like this to face towards player eventually

			if (slot.StackSize < 4)
			{
				(api as ICoreClientAPI)?.TriggerIngameError(this, "need4", Lang.Get("_NUMBER OF ITEMS NEEDED TO PLACE THIS_"));
				return;
			}
			if (!suitableLocation(player, blockSel, orient))
			{
				(api as ICoreClientAPI)?.TriggerIngameError(this, "unsuitableLocation", Lang.Get("_INSTRUCTIONS FOR SPACE NEEDED TO PLACE THIS_"));
				return;
			}

			slot.TakeOut(5);
			slot.MarkDirty();

			string material = "oak";
			

			EntityProperties type = byEntity.World.GetEntityType(new AssetLocation("boatconstruction-sailed-" + material));
			var entity = byEntity.World.ClassRegistry.CreateEntity(type);
			entity.Pos.SetPos(blockSel.Position.ToVec3d().AddCopy(0.5, 1, 0.5));
			entity.Pos.Yaw = -GameMath.PIHALF + orient * GameMath.PIHALF;

			byEntity.World.SpawnEntity(entity);

			api.World.PlaySoundAt(new AssetLocation("sounds/block/planks"), byEntity, player);

			handling = EnumHandHandling.PreventDefault;
		}

		private bool suitableLocation(IPlayer forPlayer, BlockSelection blockSel, int orient)
		{
			//var siteList = siteListByFacing[orient];

			var ba = api.World.BlockAccessor;
			bool placeable = true;

			// 9 x 3 x 4
			var cpos = blockSel.Position;

			BlockPos mingPos = siteList[0].AddCopy(0, 1, 0).Add(cpos);
			BlockPos maxgPos = siteList[1].AddCopy(-1, 0, -1).Add(cpos);
			maxgPos.Y = mingPos.Y; // Only need to check 1 block ground

			// Below: Solid
			api.World.BlockAccessor.WalkBlocks(mingPos, maxgPos, (block, x, y, z) => {
				if (!block.SideIsSolid(new BlockPos(x, y, z), BlockFacing.UP.Index))
				{
					placeable = false;
				}
			});
			if (!placeable) return false;

			// Above: Free
			BlockPos minPos = siteList[0].AddCopy(0, 1, 0).Add(cpos);
			BlockPos maxPos = siteList[1].AddCopy(-1, 1, -1).Add(cpos);
			api.World.BlockAccessor.WalkBlocks(minPos, maxPos, (block, x, y, z) => {
				var cboxes = block.GetCollisionBoxes(ba, new BlockPos(x, y, z));
				if (cboxes != null && cboxes.Length > 0) 
					placeable = false;
			});

			return placeable;
		}
	}
}
