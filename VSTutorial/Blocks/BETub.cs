using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
		public (BarrelRecipe recipe, int outsize, ItemSlot[] usingSlots)[] CurrentRecipes;
		public bool Sealed;
		public double SealedSinceTotalHours;
		public double SealHoursNeeded;
		private bool ignoreChange = false; // stops Inventory_SlotModified from doing anything during wanted moments
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
			inventory.OnAcquireTransitionSpeed += Inventory_OnAcquireTransitionSpeed1;
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			if (api.Side == EnumAppSide.Server)
			{
				RegisterGameTickListener(OnEvery3Second, 3000);
			}
			(inventory[FluidSlot] as ItemSlotLiquidOnly).CapacityLitres = (float)CapacityLitres;
		}

		protected float Inventory_OnAcquireTransitionSpeed1(EnumTransitionType transType, ItemStack stack, float mul)
		{
			// Don't spoil while sealed, otherwise no multiplication either way
			return Sealed && CurrentRecipes[0].recipe.SealHours > 0 ? 0 : 1;
		}

		protected void OnEvery3Second(float dt)
		{
			Api.Logger.Event($"seal?:{Sealed} [1]");
			// *********maybe we just want to find recipes when a slot is modified??
			if (!inventory.Empty && CurrentRecipes == null)
			{
				FindMatchingRecipe();
			}

			if (CurrentRecipes != null)
			{
				Api.Logger.Event($"seal?:{Sealed} [2]");
				if (Sealed)
				{
					Api.Logger.Event($"seal?:{Sealed} [3]");
					float elapsed = (float)(Api.World.Calendar.TotalHours - SealedSinceTotalHours);
					//Api.Logger.Event($"sealed check: elapsed={elapsed:F2}, needed={SealHoursNeeded}, recipes={CurrentRecipes?.Length ?? 0}");
					if (elapsed >= SealHoursNeeded)
					{
						Api.Logger.Event($"seal?:{Sealed} [4]");
						//ignoreChange = true; // dont trigger Inventory_SlotModified
						//*******//var recipesToCraft = ((BarrelRecipe recipe, int outsize, ItemSlot[] usingSlots)[])CurrentRecipes.Clone(); // this didnt seem to make a difference??
						foreach (var (recipe, outsize, usingSlots) in CurrentRecipes)
						{
							Api.Logger.Event($"seal?:{Sealed} [5]");
							if (recipe == null || outsize == 0 || usingSlots == null) 
							{
								Api.Logger.Event($"this recipe slot is null or something - skip it");
								continue;
							}
							if (recipe.TryCraftNow(Api, elapsed, usingSlots))
							{
								Api.Logger.Event($"seal?:{Sealed} [6.1]");
								Sealed = false;
								SealHoursNeeded = 0;
								Api.Logger.Event($"**SUCCESS trycraftnow**{recipe.RecipeIngredients.Last().ResolvedItemStack} -> {recipe.RecipeOutput.ResolvedItemStack}");
								MarkDirty(true);
								Api.World.BlockAccessor.MarkBlockEntityDirty(Pos);
								Api.Logger.Event($"seal?:{Sealed} [6.2]");
							}
							else
							{
								Api.Logger.Event($"seal?:{Sealed} [6.a]");
								Api.Logger.Event($"****trycraftnow FAIL****!{recipe.RecipeIngredients.Last().ResolvedItemStack} -> {recipe.RecipeOutput.ResolvedItemStack}");
							}
							Api.Logger.Event($"seal?:{Sealed} [7]");
						}
						Api.Logger.Event($"seal?:{Sealed} [8]");
						ignoreChange = false;
						CurrentRecipes = null;
					}
				}
			}
			else
			{
				if (Sealed)
				{
					Api.Logger.Event($"seal?:{Sealed} [9.1]");
					Sealed = false;
					MarkDirty(true);
					Api.Logger.Event($"seal?:{Sealed} [9.2]");
				}
			}
		}

		public void OnPlayerRightClick(IPlayer byPlayer)
		{
			Api.Logger.Event($"seal?:{Sealed} [r.1]");
			if (Sealed)
				return;

			FindMatchingRecipe();

			if (invDialog != null) invDialog = null; // shitty bugfix? // doesnt seem to do anything?

			if (Api.Side == EnumAppSide.Client)
			{
				Api.Logger.Event($"seal?:{Sealed} [r.2]");
				ToggleInventoryDialogClient(byPlayer); 
			}
			Api.Logger.Event($"seal?:{Sealed} [r.3]");
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
			CurrentRecipes = new (BarrelRecipe recipe, int outsize, ItemSlot[] usingSlots)[ItemSlots];

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
							SealHoursNeeded = recipe.SealHours; // since theres only 1 liquid, all matching recipes SHOULD always have the same time... right???
							//***//Api.Logger.Event($"{recipe.RecipeIngredients.Last().ResolvedItemStack} -> {recipe.RecipeOutput.ResolvedItemStack} | {recipe.RecipeIngredients.First().ResolvedItemStack.StackSize} | {recipe.SealHours}h");
							CurrentRecipes[i] = (recipe, outsize, selectedSlots);
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
						break; // found a match so done recipe loops for this itemslot
					}
				}
			}
		}

		public bool GetCanSeal(IPlayer byPlayer)
		{
			FindMatchingRecipe(byPlayer);

			foreach (var combo in CurrentRecipes)
			{
				if (combo.recipe != null && combo.recipe.SealHours > 0) 
				{
					
					return true; 
				}
			}

			return false;
		}

		public void SealTub()
		{
			Api.Logger.Event($"seal?:{Sealed} [s.1]");
			if (Sealed) return;

			Sealed = true;
			SealedSinceTotalHours = Api.World.Calendar.TotalHours;
			
			MarkDirty(true);
			Api.Logger.Event($"seal?:{Sealed} [s.2]");
		}
	}

}