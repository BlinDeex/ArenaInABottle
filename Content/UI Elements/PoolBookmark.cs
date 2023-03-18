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

public class PoolBookmark : UIElement
{
    private readonly Texture2D BookmarkBase = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkBase", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D BookmarkFrame = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkFrame", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D BookmarkHighlight = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/BookmarkHighlight", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D PerlinTexture = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Shader/PerlinNoise", AssetRequestMode.ImmediateLoad).Value;
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    Rectangle _size, _boxSize;
    public bool Hovering = false;
    readonly string _text = "Arena";
    readonly Color _drawColor = new(0.84f, 0, 0.9f);
    private float _bookmarkOffset;
    private const string BookmarkTitle = "Pool Builder";
    
    private float[] _screenSize = new float[2];
    public static PoolBookmark Instance { get; private set; }

    private readonly (int, string)[] _textBoxesNeeded = {(0, "Width"),(1,"Height")};
    private readonly (int, string)[] _checkBoxesNeeded = {(0, "Clear area")};

    public PoolBookmark()
    {
        Width.Set(260, 0);
        Height.Set(32, 0);
        VAlign = 0.5f;
        HAlign = 0.5f;
        Left.Set(14, 0);
        Top.Set(-117, 0);
    }

    public override void OnInitialize()
    {
        Instance = this;
        _screenSize[0] = Main.screenWidth;
        _screenSize[1] = Main.screenHeight;
    }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            Point point1 = new((int)dimensions.X, (int)dimensions.Y);
            int width = (int)Math.Ceiling(dimensions.Width);
            int height = (int)Math.Ceiling(dimensions.Height);
            _size = new Rectangle(point1.X, point1.Y, width, height);
            _boxSize = new Rectangle(point1.X + 232, point1.Y + 4, width - 236, height - 8);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["sampleTexture"].SetValue(PerlinTexture); //TODO I doubt I need to set these every frame but crashing otherwise
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["noiseScalar"].SetValue(5f); // higher = more compact tex on both x and y
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["screenSize"].SetValue(_screenSize); // higher = more compact tex on both x and y
            GameShaders.Misc["PerlinNoise"].Apply();
            
            spriteBatch.Draw(BookmarkBase, _size, _drawColor);
            spriteBatch.Draw(BookmarkFrame, _size, _drawColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            //GameShaders.Misc["Frac"].Apply();
            
            spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.ItemFont), _text, point1.ToVector2() + new Vector2(176, 7), _drawColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.graphics.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
            //Main.instance.LoadItem(ItemID.WaterBolt);
            //Texture2D sword = TextureAssets.Item[ItemID.WaterBolt].Value;
            //spriteBatch.Draw(sword, boxSize, Color.White);
            
            if (Hovering)
            {
                spriteBatch.Draw(BookmarkHighlight, _size, Color.Lerp(_drawColor, Color.Black, (float)Math.Sin(Main.time / 10)));
            }
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
            BuilderUi.Instance.ChangeUiState(_textBoxesNeeded,_checkBoxesNeeded,BookmarkTitle,_drawColor);
        }
}