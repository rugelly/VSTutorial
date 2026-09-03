using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Common;
using Vintagestory.GameContent;
#nullable disable
namespace VSTutorial.Blocks
{
	public class BlockTub : BlockLiquidContainerBase
	{
		public override bool AllowHeldLiquidTransfer => false;
		public override int GetContainerSlotId(BlockPos pos) => 4;
		public override int GetContainerSlotId(ItemStack containerStack) => 4;

		// copy paste of BASE implementation + custom fluid / total text
		// ** normal barrel calls base + then does its custom stuff after
		// so make my own 'base' then do a similar thing like the barrel does
		public string CustomBaseGetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
		{
			float currentLitres = GetCurrentLitres(pos);
			StringBuilder stringBuilder = new StringBuilder();
			BlockEntityTub betub = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityTub;
			if (betub != null)
			{
				if (currentLitres <= 0f)
				{
					stringBuilder.AppendLine(Lang.Get("Empty"));
				}
				else
				{
					ItemSlot itemSlot = betub.Inventory[GetContainerSlotId(pos)];
					ItemStack itemstack = itemSlot.Itemstack;
					string text = Lang.Get(itemstack.Collectible.Code.Domain + ":incontainer-" + itemstack.Class.ToString().ToLowerInvariant() + "-" + itemstack.Collectible.Code.Path);
					stringBuilder.AppendLine(Lang.Get("Contents:"));
					stringBuilder.AppendLine(" " + Lang.Get("{0}" + " / " + betub.CapacityLitres + " " + "litres of {1}", currentLitres, text));
					string text2 = PerishableInfoCompact(api, itemSlot, 0f, withStackName: false);
					if (text2.Length > 2)
					{
						stringBuilder.AppendLine(text2.Substring(2));
					}
				}
			}

			StringBuilder stringBuilder2 = new StringBuilder();
			BlockBehavior[] blockBehaviors = BlockBehaviors;
			foreach (BlockBehavior blockBehavior in blockBehaviors)
			{
				stringBuilder2.Append(blockBehavior.GetPlacedBlockInfo(world, pos, forPlayer));
			}

			if (stringBuilder2.Length > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(stringBuilder2.ToString());
			}

			return stringBuilder.ToString();
		}

		// now the barrel version of it that calls above custom BASE in the same way
		public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
		{
			string text = CustomBaseGetPlacedBlockInfo(world, pos, forPlayer);
			string aftertext = "";
			int j = text.IndexOfOrdinal(Environment.NewLine + Environment.NewLine);
			if (j > 0)
			{
				aftertext = text.Substring(j);
				text = text.Substring(0, j);
			}

			float litres = GetCurrentLitres(pos);

			if (litres <= 0) text = "";

			BlockEntityTub betub = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityTub;
			if (betub != null)
			{
				// display the non fluid slots
				for (int i = 0; i < betub.Inventory.Count - 1; i++)
				{
					ItemSlot slot = betub.Inventory[i];
					if (!slot.Empty)
					{
						if (text.Length > 0) text += " ";
						else text += Lang.Get("Contents:") + "\n";

						text += Lang.Get("{0}x {1}", slot.Itemstack.StackSize, slot.Itemstack.GetName());
						text += PerishableInfoCompact(api, slot, 0, false);
						text += "\n";
					}
				}

				//if (betub.Sealed && betub.CurrentRecipe != null)
				//{
				//	double hoursPassed = world.Calendar.TotalHours - betub.SealedSinceTotalHours;
				//	if (hoursPassed < 3) hoursPassed = Math.Max(0, hoursPassed + 0.2);  // Small addition to deal with possible server/client calendar desync
				//	string timePassedText = hoursPassed > 24 ? Lang.Get("{0} days", Math.Floor(hoursPassed / api.World.Calendar.HoursPerDay * 10) / 10) : Lang.Get("{0} hours", Math.Floor(hoursPassed));
				//	string timeTotalText = betub.CurrentRecipe.SealHours > 24 ? Lang.Get("{0} days", Math.Round(betub.CurrentRecipe.SealHours / api.World.Calendar.HoursPerDay, 1)) : Lang.Get("{0} hours", Math.Round(betub.CurrentRecipe.SealHours));
				//	text += "\n" + Lang.Get("Sealed for {0} / {1}", timePassedText, timeTotalText);
				//}
			}

			return text + aftertext;
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
				{
					blockEntityTub.OnPlayerRightClick(byPlayer);
				}
				return true;
			}
			return flag;
		}
	}
}
