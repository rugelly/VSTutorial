using Cairo;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

#nullable disable
namespace VSTutorial.Blocks
{
	internal class GuiDialogTub : GuiDialogBlockEntity
	{
		EnumPosFlag screenPos;
		ElementBounds inputSlotBounds;

		protected override double FloatyDialogPosition => 0.6;
		protected override double FloatyDialogAlign => 0.8;

		public override double DrawOrder => 0.2;

		public GuiDialogTub(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) 
			: base(dialogTitle, inventory, blockEntityPos, capi)
		{
			if (this.IsDuplicate) return;
		}

		void SetupDialog()
		{
			ElementBounds barrelBoundsLeft = ElementBounds.Fixed(0, 30, 150, 200);
			ElementBounds barrelBoundsRight = ElementBounds.Fixed(170, 30, 150, 200);

			inputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 30, 1, 1);
			inputSlotBounds.fixedHeight += 10;

			double top = inputSlotBounds.fixedHeight + inputSlotBounds.fixedY;


			ElementBounds fullnessMeterBounds = ElementBounds.Fixed(100, 30, 40, 200);

			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = ElementSizing.FitToChildren;
			bgBounds.WithChildren(barrelBoundsLeft, barrelBoundsRight);

			// 3. Finally Dialog
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
				.WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 0)
				.WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle)
			;


			SingleComposer = capi.Gui
				.CreateCompo("blockentitybarrel" + BlockEntityPosition, dialogBounds)
				.AddShadedDialogBG(bgBounds)
				.AddDialogTitleBar(DialogTitle, OnTitleBarClose)
				.BeginChildElements(bgBounds)
					.AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, inputSlotBounds, "inputSlot")
					//.AddSmallButton(Lang.Get("barrel-seal"), onSealClick, ElementBounds.Fixed(0, 100, 80, 25), EnumButtonStyle.Normal)

					.AddInset(fullnessMeterBounds.ForkBoundingParent(2, 2, 2, 2), 2)
					//.AddDynamicCustomDraw(fullnessMeterBounds, fullnessMeterDraw, "liquidBar")

					//.AddDynamicText(getContentsText(), CairoFont.WhiteDetailText(), barrelBoundsRight, "contentText")

				.EndChildElements()
			.Compose();
		}

		public override void OnGuiOpened()
		{
			base.OnGuiOpened();
			SetupDialog();
		}

		void SendInvPacket(object packet)
		{
			capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, packet);
		}

		public override void OnGuiClosed()
		{
			SingleComposer?.GetSlotGrid("itemSlots")?.OnGuiClosed(capi);
			SingleComposer?.GetSlotGrid("liquidSlot")?.OnGuiClosed(capi);
			base.OnGuiClosed();
		}

		public void UpdateContents()
		{
			SingleComposer.GetCustomDraw("liquidBar").Redraw();
			//SingleComposer.GetDynamicText("contentText").SetNewText(getContentsText());
		}


		private void OnTitleBarClose()
		{
			TryClose();
		}
	}
}
