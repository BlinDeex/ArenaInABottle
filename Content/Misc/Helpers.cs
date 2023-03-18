using System;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace ArenaInABottle.Content.Misc;

public static class Helpers
{
    private static readonly Asset<DynamicSpriteFont> DeathFontAsset = FontAssets.DeathText;
    private static readonly Asset<DynamicSpriteFont> ItemFontAsset = FontAssets.ItemStack;
    private static readonly Asset<DynamicSpriteFont> MouseFontAsset = FontAssets.MouseText;
    private static readonly Asset<DynamicSpriteFont> CombatFontAsset = FontAssets.CombatText[0];
    private static readonly DynamicSpriteFont DeathFont = DeathFontAsset.Value;
    private static readonly DynamicSpriteFont ItemFont = ItemFontAsset.Value;
    private static readonly DynamicSpriteFont MouseFont = MouseFontAsset.Value;
    private static readonly DynamicSpriteFont CombatFont0 = CombatFontAsset.Value;

    public enum Fonts
    {
        DeathFont,
        ItemFont,
        MouseFont,
        CombatFont0
    }
    
    public static bool KeyTyped(Keys key) => Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);

    public static DynamicSpriteFont ProvideFont(Fonts font)
    {
        return font switch
        {
            Fonts.DeathFont => DeathFont,
            Fonts.ItemFont => ItemFont,
            Fonts.MouseFont => MouseFont,
            Fonts.CombatFont0 => CombatFont0,
            _ => throw new Exception("ProvideFont failed!")
        };
    }
    
    public static int RoundDownToNearest(float passednumber, float roundto)
    {
        if (roundto == 0)
        {
            return (int)passednumber;
        }
        
        return (int)(Math.Floor(passednumber / roundto) * roundto);
    }
    

    public static float ParametricBlend(float t)
    {
        float sqt = t * t;
        return sqt / (2.0f * (sqt - t) + 1.0f);
    }
}