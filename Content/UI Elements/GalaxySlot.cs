using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle.Content.UI_Elements;

public class GalaxySlot : UIElement
{
    private readonly Texture2D _mainTex = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/WhiteSquare", AssetRequestMode.ImmediateLoad).Value;
    private Rectangle _size;
    private static readonly SpriteBatch DrawBatch = new(Main.graphics.GraphicsDevice);
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Point point1 = new((int)dimensions.X, (int)dimensions.Y);
        int width = (int)Math.Ceiling(dimensions.Width);
        int height = (int)Math.Ceiling(dimensions.Height);
        _size = new Rectangle(point1.X, point1.Y, width, height);
        DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        DrawBatch.Draw(_mainTex, _size, new Color(255, 255, 255) * 1f);
        DrawBatch.End();
    }
}