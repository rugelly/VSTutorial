using System;
using System.Collections.Generic;
using System.Numerics;
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
		
		private int inventorySizeTotal = 5; // 4 items, 1 fluid
		private int ItemSlots { get { return inventorySizeTotal - 2; } }
		private int FluidSlot { get { return ItemSlots; } }
		private GuiDialogTub invDialog;
		public int CapacityLitres { get; set; } = 200;
		public bool Sealed;
		public double SealedSinceTotalHours;
		public double SealHoursNeeded;
		public override InventoryBase Inventory => this.inventory;
		public override string InventoryClassName => "tub";

		public BlockEntityTub()
		{
			inventory = new InventoryGeneric(inventorySizeTotal - 1, null, null, (id, self) =>
			{
				if (id < ItemSlots) return new ItemSlotBarrelInput(self);
				else return new ItemSlotLiquidOnly(self, CapacityLitres);
			});
			inventory.BaseWeight = 1f;
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			if (api.Side == EnumAppSide.Server)
			{
				RegisterGameTickListener(new Action<float>(SealTick), 500, 0);
			}
			(inventory[FluidSlot] as ItemSlotLiquidOnly).CapacityLitres = (float)CapacityLitres; // ig when init need to makesure val assigned?
		}

		private void SealTick(float dt)
		{
			if (!Sealed)
				return;
			if (Api.World.Calendar.TotalHours - SealedSinceTotalHours >= SealHoursNeeded)
			{
				// finished sealing so...
				// TODO: what has to be done when recipes are completed?
			}
		}

		public void OnPlayerRightClick(IPlayer byPlayer)
		{
			if (Sealed) 
				return;
			if (Api.Side == EnumAppSide.Client) 
				ToggleInventoryDialogClient(byPlayer);
		}

		private void ToggleInventoryDialogClient(IPlayer byPlayer)
		{
			if (invDialog == null)
			{
				ICoreClientAPI capi = Api as ICoreClientAPI;
				invDialog = new GuiDialogTub("test dialog title", Inventory, Pos, capi);
				invDialog.OnClosed += delegate ()
				{
					invDialog = null;
					capi.Network.SendBlockEntityPacket(Pos, 1001, null);
					capi.Network.SendPacketClient(Inventory.Close(byPlayer));
				};
				invDialog.TryOpen();
				capi.Network.SendPacketClient(Inventory.Open(byPlayer));
				capi.Network.SendBlockEntityPacket(Pos, 1000, null);
				return;
			}
			invDialog.TryClose();
		}

		public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
		{
			base.OnReceivedClientPacket(fromPlayer, packetid, data);
			if (packetid >= 1000)
			{
				if (packetid == 1001)
				{
					IPlayerInventoryManager inventoryManager = fromPlayer.InventoryManager;
					if (inventoryManager != null)
					{
						inventoryManager.CloseInventory(Inventory);
					}
				}
				if (packetid == 1000)
				{
					IPlayerInventoryManager inventoryManager2 = fromPlayer.InventoryManager;
					if (inventoryManager2 != null)
					{
						inventoryManager2.OpenInventory(Inventory);
					}
				}
				if (packetid == 1337)
				{
					TrySeal();
				}
				return;
			}
			Inventory.InvNetworkUtil.HandleClientPacket(fromPlayer, packetid, data);
			IWorldChunk chunkAtBlockPos = Api.World.BlockAccessor.GetChunkAtBlockPos(Pos);
			if (chunkAtBlockPos == null)
			{
				return;
			}
			chunkAtBlockPos.MarkModified();
		}

		public void TrySeal()
		{
			if (Sealed || Api.Side != EnumAppSide.Server)
				return;

			if (!CanSeal())
				return;

			// TODO: i guess if every recipe check passes you can do these?
			//this.SealHoursNeeded = 48.0;
			//this.Sealed = true;
			//this.SealedSinceTotalHours = this.Api.World.Calendar.TotalHours;
			//this.MarkDirty(true, null);
		}

		private bool CanSeal()
		{
			// needs fluid to seal. is this ever not the case? could there be combo item only recipes?
			if (inventory[FluidSlot].Empty)
				return false;

			// TODO: looks like recipe matching goes here?
			return false;
		}

		//public override void OnBlockPlaced(ItemStack byItemStack = null)
		//{
		//	base.OnBlockPlaced(byItemStack);

		//	// Deal with situation where the itemStack had some liquid contents, and BEContainer.OnBlockPlaced() placed this into the inputSlot not the liquidSlot
		//	ItemSlot inputSlot = Inventory[0];
		//	ItemSlot liquidSlot = Inventory[FluidSlot];
		//	if (!inputSlot.Empty && liquidSlot.Empty)
		//	{
		//		WaterTightContainableProps liqProps = BlockLiquidContainerBase.GetContainableProps(inputSlot.Itemstack);
		//		if (liqProps != null)
		//		{
		//			Inventory.TryFlipItems(1, inputSlot);
		//		}
		//	}
		//}

		public override void OnBlockBroken(IPlayer byPlayer = null)
		{
			if (!Sealed)
			{
				base.OnBlockBroken(byPlayer);
			}

			invDialog?.TryClose();
			invDialog = null;
		}
	}
}
