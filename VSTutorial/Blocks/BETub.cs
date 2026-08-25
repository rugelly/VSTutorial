using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

#nullable disable
namespace VSTutorial.Blocks
{
	public class BlockEntityTub : BlockEntityLiquidContainer
	{
		GuiDialogTub invDialog;
		protected static SoundAttributes barrelOpen = new(AssetLocation.Create("sounds/block/barrelopen"), true);
		protected static SoundAttributes barrelClose = new(AssetLocation.Create("sounds/block/barrelclose"), true);

		internal new InventoryGeneric inventory;
		public override InventoryBase Inventory => inventory;
		public override string InventoryClassName => "tub";
		private float capacityLitres = 200;

		public BlockEntityTub()
		{
			inventory = new InventoryGeneric(4, null, null, (id, self) =>
			{
				if (id == 0) return new ItemSlotBarrelInput(self);
				else return new ItemSlotLiquidOnly(self, 200);
			});
			inventory.BaseWeight = 1;
			inventory.OnGetSuitability = GetSuitability;


			//inventory.SlotModified += Inventory_SlotModified;
			//inventory.OnAcquireTransitionSpeed += Inventory_OnAcquireTransitionSpeed1;
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			capacityLitres = Block.Attributes?["capacityLitres"]?.AsInt(200) ?? 200;
		}

		public void OnPlayerRightClick(IPlayer byPlayer)
		{
			//if (Sealed) return;

			//FindMatchingRecipe(byPlayer);

			if (Api.Side == EnumAppSide.Client)
			{
				toggleInventoryDialogClient(byPlayer);
			}
		}

		protected void toggleInventoryDialogClient(IPlayer byPlayer)
		{
			if (invDialog == null)
			{
				ICoreClientAPI capi = Api as ICoreClientAPI;
				invDialog = new GuiDialogTub(Lang.Get("Tub"), Inventory, Pos, Api as ICoreClientAPI);
				invDialog.OnClosed += () =>
				{
					invDialog = null;
					capi.Network.SendBlockEntityPacket(Pos, (int)EnumBlockEntityPacketId.Close, null);
					capi.Network.SendPacketClient(Inventory.Close(byPlayer));
				};
				invDialog.OpenSound = Block.Attributes?["openSound"]?.AsObject<SoundAttributes?>(null, Block.Code.Domain, true) ?? barrelOpen;
				invDialog.CloseSound = Block.Attributes?["closeSound"]?.AsObject<SoundAttributes?>(null, Block.Code.Domain, true) ?? barrelClose;

				invDialog.TryOpen();
				capi.Network.SendPacketClient(Inventory.Open(byPlayer));
				capi.Network.SendBlockEntityPacket(Pos, (int)EnumBlockEntityPacketId.Open, null);
			}
			else
			{
				invDialog.TryClose();
			}
		}
		protected float GetSuitability(ItemSlot sourceSlot, ItemSlot targetSlot, bool isMerge)
		{
			// prevent for example rot overflowing into the liquid slot, on a shift-click, when slot[0] is already full of 64 x rot.   Rot can be accepted in the liquidOnly slot because it has containableProps (perhaps it shouldn't?)
			if (targetSlot == inventory[1])
			{
				if (inventory[0].StackSize > 0)
				{
					ItemStack currentStack = inventory[0].Itemstack;
					ItemStack testStack = sourceSlot.Itemstack;
					if (currentStack.Collectible.Equals(currentStack, testStack, GlobalConstants.IgnoredStackAttributes)) return -1;
				}
			}

			// normal behavior
			return (isMerge ? (inventory.BaseWeight + 3) : (inventory.BaseWeight + 1)) + (sourceSlot.Inventory is InventoryBasePlayer ? 1 : 0);
		}
		bool ignoreChange = false;
		protected void Inventory_SlotModified(int slotId)
		{
			if (ignoreChange) return;

			if (slotId == 0 || slotId == 1)
			{
				invDialog?.UpdateContents();
				if (Api?.Side == EnumAppSide.Client)
				{
					//currentMesh = null;   // Trigger a re-tesselation
				}

				MarkDirty(true);
				//FindMatchingRecipe();
			}
		}
	}
}
