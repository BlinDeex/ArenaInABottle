using ArenaInABottle.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArenaInABottle.Content.Items;

public class Blueprint : ModItem
{
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("Blueprint for Assembler");
        Tooltip.SetDefault("");
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.maxStack = 99;
        Item.rare = ItemRarityID.Blue;
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
        Main.NewText("consuming");
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage,
        ref float knockback)
    {
        position = Main.MouseWorld;
        
        base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }
}