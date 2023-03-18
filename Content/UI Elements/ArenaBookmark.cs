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

public class ArenaBookmark : BookmarkBase
{
    public sealed override string BookmarkName { get; set; }
    protected sealed override string BookmarkTitle { get; set; }
    protected sealed override (int, string)[] TextBoxesNeeded { get; set; }
    protected sealed override (int, string)[] CheckBoxesNeeded { get; set; }
    protected sealed override Color DrawColor { get; set; }
    public ArenaBookmark()
    {
        Width.Set(260, 0);
        Height.Set(32, 0);
        VAlign = 0.5f;
        HAlign = 0.5f;
        Left.Set(14, 0);
        Top.Set(-159, 0);
        BookmarkName = "Arena";
        BookmarkTitle = "Arena Builder";
        TextBoxesNeeded = new[]{ (0, "Width"),(1,"Height"),(2,"Floors"),(3,"Solid block spacing")};
        CheckBoxesNeeded = new[] { (0, "Place campfires"), (1, "Place heart lanterns"), (2, "Clear area") };
        DrawColor = new Color(0, 0.45f, 0.9f);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            
            base.DrawSelf(spriteBatch);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["sampleTexture"].SetValue(PerlinTexture); //TODO I doubt I need to set these every frame but crashing otherwise or maybe it just gets cached idk
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["noiseScalar"].SetValue(5f); // higher = more compact tex on both x and yGameShaders.Misc["PerlinNoise"].Apply();
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