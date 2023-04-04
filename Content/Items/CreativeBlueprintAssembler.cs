using ArenaInABottle.Content.Misc;
using ArenaInABottle.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArenaInABottle.Content.Items;

public class CreativeBlueprintAssembler : ModItem
{
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("Creative Blueprint Assembler");
        Tooltip.SetDefault("");
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.maxStack = 99;
        Item.rare = ItemRarityID.Master;
        Item.useAnimation = 50;
        Item.useTime = 50;
        Item.shoot = ModContent.ProjectileType<BlueprintProjectile>();
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.Item81;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return !ArenaPlayer.BlueprintPlaced;
    }

    public override void OnConsumeItem(Player player)
    {
        ArenaPlayer.BlueprintPlaced = true;
        ArenaPlayer.CreativeMode = true;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage,
        ref float knockback)
    {
        position.X = Helpers.RoundDownToNearest(Main.MouseWorld.X, 16);
        position.Y = Helpers.RoundDownToNearest(Main.MouseWorld.Y, 16);
        base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }
}