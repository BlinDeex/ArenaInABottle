using System;
using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle.Content.UI_Elements;

public abstract class BookmarkBase : UIElement
{
    private readonly Texture2D _bookmarkSquare = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkBase", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D _bookmarkFrame = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkFrame", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D _bookmarkHighlight = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkHighlight", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D _perlinTexture = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Shader/PerlinNoise", AssetRequestMode.ImmediateLoad).Value;

    protected abstract string BookmarkName { get; set; }
    protected abstract string BookmarkTitle { get; set; }
    
    protected abstract (int, string)[] TextBoxesNeeded { get; set; }
    protected abstract (int, string)[] CheckBoxesNeeded { get; set; }

    protected bool Custom { get; init; }
    
    protected abstract Color DrawColor { get; set; }
    private bool _hovering;
    private Point _point1;

    private Rectangle _size;
    private float _bookmarkOffset;
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        _point1 = new Point((int)dimensions.X, (int)dimensions.Y);
        int width = (int)Math.Ceiling(dimensions.Width);
        int height = (int)Math.Ceiling(dimensions.Height);
        _size = new Rectangle(_point1.X, _point1.Y, width, height);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            
        GameShaders.Misc["PerlinNoise"].Shader.Parameters["sampleTexture"].SetValue(_perlinTexture); //TODO I doubt I need to set these every frame but crashing otherwise
        GameShaders.Misc["PerlinNoise"].Shader.Parameters["noiseScalar"].SetValue(5f); // higher = more compact tex on both x and y
        GameShaders.Misc["PerlinNoise"].Apply();

        spriteBatch.Draw(_bookmarkSquare, _size, Custom ? Main.DiscoColor : DrawColor);
        spriteBatch.Draw(_bookmarkFrame, _size, Custom ? Main.DiscoColor : DrawColor);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            
        //GameShaders.Misc["Frac"].Apply();

        spriteBatch.DrawString(
            Helpers.ProvideFont(Helpers.Fonts.ItemFont), BookmarkName,
            _point1.ToVector2() + new Vector2(218 - Helpers.ProvideFont(Helpers.Fonts.ItemFont).MeasureString(BookmarkName).X, 7),
            Custom ? Main.DiscoColor : DrawColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        
        //Main.instance.LoadItem(ItemID.WaterBolt);
        //Texture2D sword = TextureAssets.Item[ItemID.WaterBolt].Value;
        //spriteBatch.Draw(sword, boxSize, Color.White);
            
        if (_hovering)
        {
            spriteBatch.Draw(_bookmarkHighlight, _size, Color.Lerp(Custom ? Main.DiscoColor : DrawColor, Color.Black, (float)Math.Sin(Main.time / 10)));
        }
        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        
    }
    
    
    public override void Update(GameTime gameTime)
    {
        if (_hovering)
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
        _hovering = true;
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        _hovering = false;
    }

    public override void Click(UIMouseEvent evt)
    {
        BuilderUi.Instance.ChangeUiState(TextBoxesNeeded,CheckBoxesNeeded,BookmarkTitle,Custom ? Main.DiscoColor : DrawColor);
    }
}