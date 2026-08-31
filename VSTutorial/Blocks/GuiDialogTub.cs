using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using static System.Net.Mime.MediaTypeNames;

#nullable disable
namespace VSTutorial.Blocks
{
	public class GuiDialogTub : GuiDialogBlockEntity
	{
		// this enitere class is mostly copied from barrel gui dialog but with some tiny tweaks here and there
		public GuiDialogTub(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
		{
			if (base.IsDuplicate) return;
			SetupDialog();
			base.Inventory.SlotModified += this.OnInventorySlotModified;
		}

		public override string ToggleKeyCombinationCode => "tub";

		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterTop);
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = ElementSizing.FitToChildren;
			
			SingleComposer = capi.Gui.CreateCompo("tub", dialogBounds)
				.AddShadedDialogBG(bgBounds)
				.AddDialogTitleBar("Tub", OnTitleBarClose)
				.BeginChildElements(bgBounds)
					// new int[] {0, 1, 2, 3} so that it only draws the 4 item slots
					.AddItemSlotGrid(Inventory, SendInvPacket, 2, new int[] {0, 1, 2, 3}, ElementBounds.Fixed(65, 20 + GuiStyle.TitleBarHeight, 102, 102), "itemslotgrid0")
					.AddInset(ElementBounds.Fixed(10, 15 + GuiStyle.TitleBarHeight, 35, 200).ForkBoundingParent(2, 2, 2, 2), 2)
					.AddDynamicCustomDraw(ElementBounds.Fixed(10, 15 + GuiStyle.TitleBarHeight, 35, 200), fullnessMeterDraw, "liquidBar")
					.AddDynamicText(getContentsText(), CairoFont.WhiteDetailText(), ElementBounds.Fixed(65, 135 + GuiStyle.TitleBarHeight, 250, 200), "contentText")
					.AddSmallButton(Lang.Get("barrel-seal"), onSealClick, ElementBounds.Fixed(0, 300, 80, 25), EnumButtonStyle.Normal)
				.EndChildElements()
				.Compose()
			;
		}
		private void OnInventorySlotModified(int slotId)
		{
			this.UpdateContents();
		}
		public void UpdateContents()
		{
			SingleComposer.GetCustomDraw("liquidBar").Redraw();
			SingleComposer.GetDynamicText("contentText").SetNewText(getContentsText());
		}

		private void OnTitleBarClose()
		{
			TryClose();
		}

		private void SendInvPacket(object packet)
		{
			this.capi.Network.SendBlockEntityPacket(base.BlockEntityPosition.X, base.BlockEntityPosition.Y, base.BlockEntityPosition.Z, packet);
		}

		private bool onSealClick()
		{
			BlockEntityTub betub = capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition) as BlockEntityTub;
			if (betub == null || betub.Sealed) return true;

			if (!betub.GetCanSeal(capi.World.Player)) return true;

			betub.SealTub();

			capi.Network.SendBlockEntityPacket(BlockEntityPosition, 1337);
			capi.World.PlaySoundAt(new AssetLocation("sounds/player/seal"), BlockEntityPosition, 0.4, null);

			TryClose();

			return true;
		}

		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			this.UpdateContents();
		}

		public override void OnGuiClosed()
		{
			base.Inventory.SlotModified -= this.OnInventorySlotModified;
			base.OnGuiClosed();
		}

		private void fullnessMeterDraw(Context ctx, ImageSurface surface, ElementBounds currentBounds)
		{
			// use the last index
			ItemSlot liquidSlot = Inventory[^1];
			if (liquidSlot.Empty) return;

			BlockEntityTub betub = capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition) as BlockEntityTub;
			float itemsPerLitre = 1f;
			int capacity = betub.CapacityLitres;

			WaterTightContainableProps props = BlockLiquidContainerBase.GetContainableProps(liquidSlot.Itemstack);
			if (props != null)
			{
				itemsPerLitre = props.ItemsPerLitre;
				capacity = Math.Max(capacity, props.MaxStackSize);
			}

			float fullnessRelative = liquidSlot.StackSize / itemsPerLitre / capacity;

			double offY = (1 - fullnessRelative) * currentBounds.InnerHeight;

			ctx.Rectangle(0, offY, currentBounds.InnerWidth, currentBounds.InnerHeight - offY);

			CompositeTexture tex = props?.Texture ?? liquidSlot.Itemstack.Collectible.Attributes?["inContainerTexture"].AsObject<CompositeTexture>(null, liquidSlot.Itemstack.Collectible.Code.Domain);
			if (tex != null)
			{
				ctx.Save();
				Matrix m = ctx.Matrix;
				m.Scale(GuiElement.scaled(3), GuiElement.scaled(3));
				ctx.Matrix = m;

				AssetLocation loc = tex.Base.Clone().WithPathAppendixOnce(".png");
				GuiElement.fillWithPattern(capi, ctx, loc, true, false, tex.Alpha);

				ctx.Restore();
			}
			return;
		}

		string getContentsText()
		{
			string contents = Lang.Get("Contents:");
			// get tub BE so we can display total fluid capacity in the text too
			BlockEntityTub betub = capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition) as BlockEntityTub;

			if (Inventory.Empty) contents += "\n" + Lang.Get("nobarrelcontents");
			else
			{
				// loop all item slots
				for (int i = 0; i < Inventory.Count - 1; i++)
				{
					if (Inventory[i].Empty) continue;

					ItemStack stack = Inventory[i].Itemstack;
					contents += "\n" + Lang.Get("barrelcontents-items", stack.StackSize, stack.GetName());
				}

				// then do the fluid slot
				if (!Inventory[^1].Empty)
				{
					ItemStack stack = Inventory[^1].Itemstack;
					WaterTightContainableProps props = BlockLiquidContainerBase.GetContainableProps(stack);

					if (props != null)
					{
						string incontainername = Lang.Get(stack.Collectible.Code.Domain + ":incontainer-" + stack.Class.ToString().ToLowerInvariant() + "-" + stack.Collectible.Code.Path);
						contents += "\n" + Lang.Get(props.MaxStackSize > 0 ? "barrelcontents-items" : "barrelcontents-liquid", (float)stack.StackSize / props.ItemsPerLitre + " / " + betub.CapacityLitres, incontainername);
					}
					else
					{
						contents += "\n" + Lang.Get("barrelcontents-items", stack.StackSize, stack.GetName());
					}
				}


				if (betub.CurrentRecipes != null)
				{
					foreach (var (recipe, outsize, usingSlots) in betub.CurrentRecipes)
					{
						if (recipe == null) continue;

						ItemStack outStack = recipe.RecipeOutput.ResolvedItemStack;
						WaterTightContainableProps props = BlockLiquidContainerBase.GetContainableProps(outStack);

						string timeText = recipe.SealHours > 24 ? Lang.Get("{0} days", Math.Round(recipe.SealHours / capi.World.Calendar.HoursPerDay, 1)) : Lang.Get("{0} hours", recipe.SealHours);

						if (props != null)
						{
							string incontainername = Lang.Get(outStack.Collectible.Code.Domain + ":incontainer-" + outStack.Class.ToString().ToLowerInvariant() + "-" + outStack.Collectible.Code.Path);
							float litres = (float)outsize / props.ItemsPerLitre;
							contents += "\n\n" + Lang.Get("Will turn into {0} litres of {1} after {2} of sealing.", litres, incontainername, timeText);
						}
						else
						{
							contents += "\n\n" + Lang.Get("Will turn into {0}x {1} after {2} of sealing.",outsize, outStack.GetName(), timeText);
						}
					}

				}
			}

			return contents;
		}
	}
}