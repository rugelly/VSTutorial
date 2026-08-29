using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
#nullable disable
namespace VSTutorial.utils
{
	public class TubRecipe : BarrelRecipe
	{
		public new bool TryCraftNow(ICoreAPI api, double nowSealedHours, ItemSlot[] inputSlots)
		{
			if (SealHours > 0.0 && nowSealedHours < SealHours)
			{
				return false;
			}

			List<(ItemSlot, BarrelRecipeIngredient)> matched = PairInput(inputSlots);
			if (matched.Count == 0) return false;

			int outputStackSize = GetOutputSize(matched);
			if (outputStackSize < 0 || Output?.ResolvedItemStack == null)
			{
				return false;
			}

			//foreach ((ItemSlot slot, BarrelRecipeIngredient ingredient) in matched)
			//{
			//	if (ingredient.ConsumeQuantity == null || slot.Itemstack == null) continue;

			//	ItemStack itemStack = Output.ResolvedItemstack.Clone();
			//	itemStack.StackSize = outputStackSize;
			//}
			ItemStack itemStack = Output.ResolvedItemStack.Clone();
			itemStack.StackSize = outputStackSize;
			CarryOverFreshness(api, itemStack, inputSlots);
			ItemStack itemStack2 = null;
			foreach (var (itemSlot, barrelRecipeIngredient) in matched)
			{
				if (barrelRecipeIngredient.ConsumeQuantity.HasValue && itemSlot.Itemstack != null)
				{
					itemStack2 = itemSlot.Itemstack;
					itemStack2.StackSize -= barrelRecipeIngredient.ConsumeQuantity.Value * (itemStack.StackSize / Output.StackSize);
					if (itemStack2.StackSize <= 0)
					{
						itemStack2 = null;
					}

					break;
				}
			}

			ItemSlot itemSlot2 = inputSlots[0];
			ItemSlot itemSlot3 = inputSlots[1];
			if (ShouldBeInLiquidSlot(itemStack))
			{
				itemSlot2.Itemstack = itemStack2;
				itemSlot3.Itemstack = itemStack;
			}
			else
			{
				itemSlot3.Itemstack = itemStack2;
				itemSlot2.Itemstack = itemStack;
			}

			itemSlot2.MarkDirty();
			itemSlot3.MarkDirty();
			return true;
		}
	}
}
