using System;
using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle.Content.UI_Elements;

public class TitleString : UIElement
{
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    public TitleString()
    {
        Width.Set(25,0);
        Height.Set(25,0);
        HAlign = 0.046f;
        VAlign = 0.046f;
    }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Point point1 = new((int)dimensions.X, (int)dimensions.Y);
        spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.DeathFont), ArenaPlayer.CurrentTitle, point1.ToVector2(), Color.Black, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
    }
}