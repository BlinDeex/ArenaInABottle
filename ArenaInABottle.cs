using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArenaInABottle
{
	public class ArenaInABottle : Mod
	{
		public override void Load()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				Ref<Effect> fracRef = new(ModContent.Request<Effect>("ArenaInABottle/Content/Shaders/Frac", AssetRequestMode.ImmediateLoad).Value);
				GameShaders.Misc["Frac"] = new MiscShaderData(fracRef, "Frac");
				
				Ref<Effect> perlinNoiseRef = new(ModContent.Request<Effect>("ArenaInABottle/Content/Shaders/PerlinNoise", AssetRequestMode.ImmediateLoad).Value);
				GameShaders.Misc["PerlinNoise"] = new MiscShaderData(perlinNoiseRef, "NoisePass").UseImage1("Images/Misc/Perlin");
			}
		}
	}
}