using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Cairo;
using Vintagestory.API.MathTools;

#nullable disable
namespace VSTutorial.Blocks
{
	public class GuiDialogTub : GuiDialogBlockEntity
	{
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
					.AddItemSlotGrid(Inventory, SendInvPacket, 2, ElementBounds.Fixed(65, 20 + GuiStyle.TitleBarHeight, 102, 102), "itemslotgrid0")
					.AddInset(ElementBounds.Fixed(10, 15 + GuiStyle.TitleBarHeight, 35, 200).ForkBoundingParent(2, 2, 2, 2), 2)
					.AddDynamicCustomDraw(ElementBounds.Fixed(10, 15 + GuiStyle.TitleBarHeight, 35, 200), (ctx, surface, bounds) =>
					{
						// TODO: replace this fixed fraction with your real liquid level, e.g.
						// liquidSlot.StackSize / itemsPerLitre / capacityLitres
						double fillFraction = 0;
						double offY = (1 - fillFraction) * bounds.InnerHeight;
						ctx.Rectangle(0, offY, bounds.InnerWidth, bounds.InnerHeight - offY);
						ctx.SetSourceRGBA(0.2, 0.4, 0.8, 0.8); // TODO: replace with your liquid's real color/texture
						ctx.Fill();
					}, "liquidgauge2")
					.AddStaticText("recipe result goes here?", CairoFont.WhiteDetailText(), ElementBounds.Fixed(65, 135 + GuiStyle.TitleBarHeight, 150, 30), "statictext4")
					.AddStaticText("Mouse over me for info.", CairoFont.WhiteDetailText(), ElementBounds.Fixed(10, 235 + GuiStyle.TitleBarHeight, 150, 30), "statictext6")
					.AddHoverText("Insert hides of any size/amount. They must all be on the same processing step.", CairoFont.WhiteDetailText(), 200, ElementBounds.Fixed(10, 225 + GuiStyle.TitleBarHeight, 150, 30), "hovertext7")
					.AddHoverText("fill amount & liquid type goes here?", CairoFont.WhiteDetailText(), 200, ElementBounds.Fixed(10, 15 + GuiStyle.TitleBarHeight, 35, 200), "hovertext8")
				.EndChildElements()
				.Compose()
			;
		}
		private void OnInventorySlotModified(int slotId)
		{
			//this.UpdateContents();
		}

		private void OnTitleBarClose()
		{
			TryClose();
		}

		private void SendInvPacket(object packet)
		{
			this.capi.Network.SendBlockEntityPacket(base.BlockEntityPosition.X, base.BlockEntityPosition.Y, base.BlockEntityPosition.Z, packet);
		}

		private bool OnSealClick()
		{
			this.capi.Network.SendBlockEntityPacket(base.BlockEntityPosition, 1337, null);
			this.TryClose();
			return true;
		}

		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			//this.UpdateContents();
		}

		public override void OnGuiClosed()
		{
			base.Inventory.SlotModified -= this.OnInventorySlotModified;
			base.OnGuiClosed();
		}
	}
}