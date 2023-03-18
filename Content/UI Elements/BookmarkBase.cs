using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle.Content.UI_Elements;

public abstract class BookmarkBase : UIElement
{
    protected readonly Texture2D BookmarkSquare = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkBase", AssetRequestMode.ImmediateLoad).Value;
    protected readonly Texture2D BookmarkFrame = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkFrame", AssetRequestMode.ImmediateLoad).Value;
    protected readonly Texture2D BookmarkHighlight = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkHighlight", AssetRequestMode.ImmediateLoad).Value;
    protected readonly Texture2D PerlinTexture = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Shader/PerlinNoise", AssetRequestMode.ImmediateLoad).Value;

    public abstract string BookmarkName { get; set; }
    protected abstract string BookmarkTitle { get; set; }
    
    protected abstract (int, string)[] TextBoxesNeeded { get; set; }
    protected abstract (int, string)[] CheckBoxesNeeded { get; set; }
    
    protected abstract Color DrawColor { get; set; }
    protected bool Hovering;
    protected Point Point1;
    
    protected Rectangle Size;
    private float _bookmarkOffset;
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Point1 = new Point((int)dimensions.X, (int)dimensions.Y);
        int width = (int)Math.Ceiling(dimensions.Width);
        int height = (int)Math.Ceiling(dimensions.Height);
        Size = new Rectangle(Point1.X, Point1.Y, width, height);
    }
    
    public override void Update(GameTime gameTime)
    {
        if (Hovering)
        {
            if (_bookmarkOffset < 100) _bookmarkOffset += 1 * Math.Clamp(100 - _bookmarkOffset * 2 / 25, 0.5f, 8);
        }
        else
        {
            if (_bookmarkOffset > 0) _bookmarkOffset -= 1 * Math.Clamp(0 + _bookmarkOffset * 2 / 40, 0.5f, 4);
        }
        
        Left.Set(14 + _bookmarkOffset, 0);
        
        base.Update(gameTime);
    }
    
    public override void MouseOver(UIMouseEvent evt)
    {
        Hovering = true;
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        Hovering = false;
    }

    public override void Click(UIMouseEvent evt)
    {
        BuilderUi.Instance.ChangeUiState(TextBoxesNeeded,CheckBoxesNeeded,BookmarkTitle,DrawColor);
    }
}