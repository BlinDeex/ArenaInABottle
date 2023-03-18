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

public class HellBookmark : BookmarkBase
{
    public HellBookmark()
    {
        Width.Set(260, 0);
        Height.Set(32, 0);
        VAlign = 0.5f;
        HAlign = 0.5f;
        Left.Set(14, 0);
        Top.Set(-75, 0);
        BookmarkName = "Hellevator";
        BookmarkTitle = "Hellevator Builder";
        TextBoxesNeeded = new[]{ (0, "Width")};
        CheckBoxesNeeded = new[] { (0, "Place rope"), (1, "Water at the end")};
        DrawColor = new Color(0.9f, 0.45f, 0.1f);
    }

    public sealed override string BookmarkName { get; set; }
    protected sealed override string BookmarkTitle { get; set; }
    protected sealed override (int, string)[] TextBoxesNeeded { get; set; }
    protected sealed override (int, string)[] CheckBoxesNeeded { get; set; }
    protected sealed override Color DrawColor { get; set; }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
            base.DrawSelf(spriteBatch);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["sampleTexture"].SetValue(PerlinTexture); //TODO I doubt I need to set these every frame but crashing otherwise
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["noiseScalar"].SetValue(5f); // higher = more compact tex on both x and y
            GameShaders.Misc["PerlinNoise"].Apply();
            
            spriteBatch.Draw(BookmarkSquare, Size, DrawColor);
            spriteBatch.Draw(BookmarkFrame, Size, DrawColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            //GameShaders.Misc["Frac"].Apply();
            
            spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.ItemFont), BookmarkName, Point1.ToVector2() + new Vector2(176, 7), DrawColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.graphics.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
            //Main.instance.LoadItem(ItemID.WaterBolt);
            //Texture2D sword = TextureAssets.Item[ItemID.WaterBolt].Value;
            //spriteBatch.Draw(sword, boxSize, Color.White);
            
            if (Hovering)
            {
                spriteBatch.Draw(BookmarkHighlight, Size, Color.Lerp(DrawColor, Color.Black, (float)Math.Sin(Main.time / 10)));
            }
    }
}