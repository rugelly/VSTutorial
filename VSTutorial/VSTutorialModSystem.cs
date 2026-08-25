using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using VSTutorial.Blocks;

namespace VSTutorial
{
	public class VSTutorialModSystem : ModSystem
	{

		// Called on server and client
		// Useful for registering block/entity classes on both sides
		public override void Start(ICoreAPI api)
		{
			base.Start(api);
			api.RegisterBlockClass(Mod.Info.ModID + ".blocktub", typeof(BlockTub));
			api.RegisterBlockEntityClass(Mod.Info.ModID + ".blockentitytub", typeof(BlockEntityTub));
		}

	}
}
