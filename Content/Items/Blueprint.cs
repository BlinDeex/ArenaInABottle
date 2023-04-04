using Terraria.ID;
using Terraria.ModLoader;

namespace ArenaInABottle.Content.Items;

public class Blueprint : ModItem
{
    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("Blueprint");
        Tooltip.SetDefault("");
    }
    
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 99;
        Item.rare = ItemRarityID.White;
    }
}