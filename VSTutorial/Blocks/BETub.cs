using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using VSTutorial.utils;

#nullable disable
namespace VSTutorial.Blocks
{
	public class BlockEntityTub : BlockEntityLiquidContainer
	{
		private int ItemSlots = 4;
		private int inventorySizeTotal { get { return ItemSlots + 1; } }
		private int FluidSlot { get { return inventorySizeTotal - 1; } }
		private GuiDialogTub invDialog;
		public int CapacityLitres { get; set; } = 200;
		public List<(BarrelRecipe recipe, int outsize, ItemSlot[] usingSlots)> CurrentRecipes;
		public bool Sealed;
		public double SealedSinceTotalHours;
		public double SealHoursNeeded;
		public override InventoryBase Inventory => this.inventory;
		public override string InventoryClassName => "tub";

		public BlockEntityTub()
		{
			inventory = new InventoryGeneric(inventorySizeTotal, null, null, (id, self) =>
			{
				if (id < ItemSlots) return new ItemSlot(self);
				else return new ItemSlotLiquidOnly(self, CapacityLitres);
			});
			inventory.BaseWeight = 1f;

			inventory.SlotModified += Inventory_SlotModified;
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			if (api.Side == EnumAppSide.Server)
			{
				RegisterGameTickListener(OnEvery3Second, 3000);
			}
			(inventory[FluidSlot] as ItemSlotLiquidOnly).CapacityLitres = (float)CapacityLitres; // ig when init need to makesure val assigned?
		}

		protected void OnEvery3Second(float dt)
		{
			if (!inventory.Empty && CurrentRecipes == null)
			{
				FindMatchingRecipe();
			}

			if (CurrentRecipes != null)
			{
				//if (Sealed && CurrentRecipes.TryCraftNow(Api, Api.World.Calendar.TotalHours - SealedSinceTotalHours, new ItemSlot[] { inventory[0], inventory[1] }) == true)
				//{
				//	MarkDirty(true);
				//	Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
				//	Sealed = false;
				//}

				if (Sealed && Api.World.Calendar.TotalHours - SealedSinceTotalHours > SealHoursNeeded)
				{
					foreach (var (recipe, outsize, usingSlots) in CurrentRecipes)
					{
						if (recipe.TryCraftNow(Api, Api.World.Calendar.TotalHours - SealedSinceTotalHours, usingSlots))
						{
							MarkDirty(true);
							Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
							Sealed = false;
						}
					}
				}
			}
			else
			{
				if (Sealed)
				{
					Sealed = false;
					MarkDirty(true);
				}
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
					SealTub();
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

		public override void OnBlockBroken(IPlayer byPlayer = null)
		{
			if (!Sealed)
			{
				base.OnBlockBroken(byPlayer);
			}

			invDialog?.TryClose();
			invDialog = null;
		}

		bool ignoreChange = false;

		protected void Inventory_SlotModified(int slotId)
		{
			if (ignoreChange) return;

			invDialog?.UpdateContents();
			if (Api?.Side == EnumAppSide.Client)
			{
				//currentMesh = null;   // Trigger a re-tesselation // TODO: make mesh ig
			}

			MarkDirty(true);
			FindMatchingRecipe();
		}

		protected void FindMatchingRecipe()
		{
			FindMatchingRecipe(null);
		}

		protected void FindMatchingRecipe(IPlayer byPlayer)
		{
			System.Collections.Generic.List<BarrelRecipe> barrelRecipes = Api.GetBarrelRecipes();
			//var tubRecipes = barrelRecipes.ConvertAll(x => (TubRecipe)x);
			CurrentRecipes ??= new System.Collections.Generic.List<(BarrelRecipe, int, ItemSlot[])>();
			//CurrentRecipes.Clear();

			// lets try to treat each slot as if this was a normal barrel with 1 slot, just... n times
			for (int i = 0; i < ItemSlots; i++)
			{
				// and since its a barrel we only need to compare current item slot against fluid slot
				ItemSlot[] selectedSlots = [inventory[i], inventory[FluidSlot]];

				// yeahhhh this has just n * total barrel recipes...............
				// TODO: is this a perf issue?!?!?!?
				// TODO: CAN WE SELECT ONLY SPECIFIC BARREL RECIPES?? IE: JUST LEATHERWORKING ONES????????
				foreach (BarrelRecipe recipe in barrelRecipes)
				{
					bool matches;
					int outsize;

					if (byPlayer != null)
					{
						matches = recipe.Matches(byPlayer, selectedSlots, out outsize);
					}
					else
					{
						matches = recipe.Matches(selectedSlots, out outsize);
					}

					if (matches)
					{
						ignoreChange = true;

						if (recipe.SealHours > 0)
						{
							CurrentRecipes.Add((recipe, outsize, selectedSlots));

						}
						else
						{
							if (Api?.Side == EnumAppSide.Server)
							{
								recipe.TryCraftNow(Api, 0, selectedSlots);
								MarkDirty(true);
								Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
							}
						}


						invDialog?.UpdateContents();
						if (Api?.Side == EnumAppSide.Client)
						{
							//currentMesh = null;   // Trigger a re-tesselation // TODO: implement a mesh lol
							MarkDirty(true);
						}

						ignoreChange = false;
						return;
					}
				}
			}
		}

		public bool GetCanSeal(IPlayer byPlayer)
		{
			FindMatchingRecipe(byPlayer);

			foreach (var combo in CurrentRecipes)
			{
				if (combo.recipe != null && combo.recipe.SealHours > 0) return true;
			}

			return false;
		}

		public void SealTub()
		{
			if (Sealed) return;

			Sealed = true;
			SealedSinceTotalHours = Api.World.Calendar.TotalHours;
			MarkDirty(true);
		}
	}

}