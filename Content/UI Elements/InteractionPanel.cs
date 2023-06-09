using System;
using log4net.Appender;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle.Content.UI_Elements;


public class InteractionPanel : UIElement
{
    private readonly Texture2D _mainTex = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/interactionPanel", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D _perlinTexture = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Shader/PerlinNoise", AssetRequestMode.ImmediateLoad).Value;
    
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    private Rectangle _size;
    private readonly Color _defaultColor = new(0.9f, 0.9f, 0.9f);

    public override void OnActivate()
    {
        
        base.OnActivate();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {

        CalculatedStyle dimensions = GetDimensions();
        Point point1 = new((int)dimensions.X, (int)dimensions.Y);
        int width = (int)Math.Ceiling(dimensions.Width);
        int height = (int)Math.Ceiling(dimensions.Height);
        _size = new Rectangle(point1.X, point1.Y, width, height);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.graphics.GraphicsDevice.RasterizerState, null, Main.UIScaleMatrix);
        GameShaders.Misc["PerlinNoise"].Shader.Parameters["sampleTexture"].SetValue(_perlinTexture);
        GameShaders.Misc["PerlinNoise"].Shader.Parameters["noiseScalar"].SetValue(2.5f);
        GameShaders.Misc["PerlinNoise"].Apply();
        spriteBatch.Draw(_mainTex, _size, ArenaPlayer.UiColor.MultiplyRGB(_defaultColor));
        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        
    }
}