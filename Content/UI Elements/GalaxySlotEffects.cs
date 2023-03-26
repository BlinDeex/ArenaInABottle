using System;
using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle.Content.UI_Elements
{
    
    internal class GalaxySlotEffects : UIElement
    {
        private Texture2D _dotRing = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/DotRing", AssetRequestMode.ImmediateLoad).Value;
        private Texture2D _itemSlotBorder = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/ItemSlotUpdated", AssetRequestMode.ImmediateLoad).Value;
        private Texture2D _itemSlotBottom = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/ItemSlotBottom", AssetRequestMode.ImmediateLoad).Value;
        private Texture2D _itemSlotTop = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/ItemSlotTop", AssetRequestMode.ImmediateLoad).Value;
        private Texture2D _galaxyTex = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/GalaxyBackground", AssetRequestMode.ImmediateLoad).Value;
        private readonly Texture2D _perlinTexture = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Shader/PerlinNoise", AssetRequestMode.ImmediateLoad).Value;
        private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
        private Rectangle _size, _size2;

        private float _timer = 0.66f;
        private float _inverseTimer = 0.33f;
        private bool _timerDir = true;
        private bool _inverseTimerDir = true;
        private Vector2 _currentOffset1 = Vector2.Zero;
        private Vector2 _randomPoint1 = Vector2.Zero;
        private Vector2 _currentOffset2 = Vector2.Zero;
        private Vector2 _randomPoint2 = Vector2.Zero;

        private float _itemOutputAnimationTimer;
        private const float AnimationSpeed = 0.015f;
        private const float MinimumItemOutputDoorsOffset = 0f;
        private const float MaxItemOutputDoorsOffset = 410f;
        private float _currentItemOutputDoorsOffset;

        private CalculatedStyle _dimensions;
        private Point _point1;
        private int _width;
        private int _height;
        private Rectangle _sourceRectangle;
        
        private float[] _screenSize = new float[2];

        private readonly Color _hoveringColor = new(1f, 1f, 1f);
        private readonly Color _notHoveringColor = new(0.8f, 0.8f, 0.8f);
        private readonly Color _defaultColor = new(0.9f, 0.9f, 0.9f);
        
        private static ArenaPlayer ModArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();

        public override void OnInitialize()
        {
            _sourceRectangle = new Rectangle(0, 0, 412, 412);
            _screenSize[0] = Main.screenWidth;
            _screenSize[1] = Main.screenHeight;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            SpriteBatch backup = Main.spriteBatch;


            DimensionsCalc();
            ItemOutputSlotDoorsOffsetCalc();
            NoiseCalcs();
            DrawOutputSlotGalaxyEffects(spriteBatch);
            
            if(ModArenaPlayer.ItemInSlot.type != ItemID.None) DrawItem();

            DrawOutputSlotDoors(spriteBatch);
            
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            spriteBatch.Draw(_itemSlotBorder, _size,
                ModArenaPlayer.ItemInSlot.type != ItemID.None ? ArenaPlayer.UiColor.MultiplyRGB(_hoveringColor) : ArenaPlayer.UiColor.MultiplyRGB(_notHoveringColor));
            
            spriteBatch.End();
            backup.Begin();
        }

        private void DimensionsCalc()
        {
            _dimensions = GetDimensions();
            _point1 = new Point((int)_dimensions.X, (int)_dimensions.Y);
            _width = (int)Math.Ceiling(_dimensions.Width);
            _height = (int)Math.Ceiling(_dimensions.Height);
            _size = new Rectangle(_point1.X, _point1.Y, _width, _height);
            _size2 = new Rectangle(_point1.X + 40, _point1.Y + 40, _width + 40, _height + 40);
        }
        
        private void ItemOutputSlotDoorsOffsetCalc()
        {
            _itemOutputAnimationTimer = ModArenaPlayer.ItemInSlot.IsAir 
                ? Utils.Clamp(_itemOutputAnimationTimer -= AnimationSpeed, 0f, 1f) 
                : Utils.Clamp(_itemOutputAnimationTimer += AnimationSpeed, 0f, 1f);
            
            _currentItemOutputDoorsOffset =
                Utils.Clamp(Helpers.ParametricBlend(_itemOutputAnimationTimer) * MaxItemOutputDoorsOffset,
                    MinimumItemOutputDoorsOffset, MaxItemOutputDoorsOffset);
        }

        private void DrawOutputSlotDoors(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.graphics.GraphicsDevice.RasterizerState, null, Main.UIScaleMatrix);
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["sampleTexture"].SetValue(_perlinTexture); //TODO I doubt I need to set these every frame but crashing otherwise
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["noiseScalar"].SetValue(2.5f);
            GameShaders.Misc["PerlinNoise"].Shader.Parameters["screenSize"].SetValue(_screenSize); // higher = more compact tex on both x and y
            GameShaders.Misc["PerlinNoise"].Apply();
            spriteBatch.Draw(_itemSlotTop, _size, null,
                ArenaPlayer.UiColor.MultiplyRGB(_defaultColor), 0, new Vector2
                (0, 410 + -_currentItemOutputDoorsOffset), SpriteEffects.FlipHorizontally, 0);
            
            
            spriteBatch.Draw(_itemSlotBottom, _size, null,
                ArenaPlayer.UiColor.MultiplyRGB(_defaultColor), 0, new Vector2
                (0, -410 + _currentItemOutputDoorsOffset), SpriteEffects.FlipHorizontally, 0);
        }

        private void DrawOutputSlotGalaxyEffects(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, Main.graphics.GraphicsDevice.RasterizerState, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(_galaxyTex, _size2, _sourceRectangle,  new Color(255, 255, 255, MathHelper.Lerp(0, 1, _timer)), 0, new Vector2(180, 150) + _currentOffset1, SpriteEffects.None, 1);
            spriteBatch.Draw(_galaxyTex, _size2, _sourceRectangle, new Color(255, 255, 255, MathHelper.Lerp(0, 1, _inverseTimer)), 0, new Vector2(230, 200) + _currentOffset2, SpriteEffects.FlipHorizontally, 1);

            spriteBatch.Draw(_dotRing, _size2, _sourceRectangle,
                ModArenaPlayer.ItemInSlot.type != ItemID.None
                    ? new Color(0, 0, Main.DiscoB, 255)
                    : new Color(142, 255, 255, 100), (float)(Main.time / 102), new Vector2(206, 206),
                SpriteEffects.None, 1);
        }
        
        

        private void DrawItem()
        {
            Item item = ModArenaPlayer.ItemInSlot;
            Main.instance.LoadItem(item.type);
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            Rectangle rectangle2 = Main.itemAnimations[item.type]?.GetFrame(itemTexture) ?? itemTexture.Frame();

            Main.EntitySpriteDraw(
            itemTexture, _size.Center.ToVector2(),
            rectangle2, Color.White, 0,
            rectangle2.Size() / 2, 1, SpriteEffects.None, 0);
        }

        public override void Click(UIMouseEvent evt) //TODO wtf is this shit why it only works exactly like this
        {
            ArenaPlayer modArenaPlayer = Main.LocalPlayer.GetModPlayer<ArenaPlayer>();

            Item itemSlot = modArenaPlayer.ItemInSlot;
            Item itemOnMouse = Main.LocalPlayer.HeldItem;

            switch (itemSlot.IsAir)
            {
                //if (Main.dedServ) return;
                // both item slot and mouse has items which are of same type and mouse item is not at max stack
                case false when !itemOnMouse.IsAir && itemSlot.type == itemOnMouse.type && itemOnMouse.maxStack > itemOnMouse.stack:
                    modArenaPlayer.ItemInSlot = new Item(ItemID.None);
                    Main.LocalPlayer.HeldItem.stack++;
                    return;
                // item slot has item and mouse item doesnt
                case false when itemOnMouse.IsAir:
                {
                    Item tempItem = modArenaPlayer.ItemInSlot.Clone();
                    modArenaPlayer.ItemInSlot.TurnToAir();
                    Main.mouseItem = tempItem;
                    return;
                }
                // item slot doesnt have item and mouse does
                case true when !Main.LocalPlayer.HeldItem.IsAir:
                {
                    if (Main.LocalPlayer.HeldItem.stack > 1)
                    {
                        modArenaPlayer.ItemInSlot = new Item(Main.LocalPlayer.HeldItem.type);
                        Main.LocalPlayer.HeldItem.stack -= 1; // wont reduce
                    }
                    else
                    {
                        Item tempItem = Main.LocalPlayer.HeldItem.Clone();
                        Main.LocalPlayer.HeldItem.TurnToAir();
                        modArenaPlayer.ItemInSlot = tempItem;
                    }

                    break;
                }
            }
        }
        
        
        // galaxy effect textures eventually drift off from the center, this resets those textures coords
        public override void OnActivate()
        {
            _randomPoint1 = Main.rand.NextVector2Circular(0.5f, 0.5f);
            _randomPoint2 = Main.rand.NextVector2Circular(0.5f, 0.5f);
        }

        private void NoiseCalcs()
        {
            if (_timer >= 0.99f)
            {
                _timerDir = false;
            }

            if (_inverseTimer >= 0.99f)
            {
                _inverseTimerDir = false;
            }

            if (_timer <= 0.1f)
            {
                _timerDir = true;
            }
            if (_inverseTimer <= 0.1f)
            {
                _inverseTimerDir = true;
            }


            if (_timerDir)
            {
                Utils.Clamp(_timer += 0.0035f * Main.rand.NextFloat(1f, 1.5f), 0, 1);
            }
            else
            {
                Utils.Clamp(_timer -= 0.0035f * Main.rand.NextFloat(1f, 1.5f), 0, 1);
            }

            if (_inverseTimerDir)
            {
                Utils.Clamp(_inverseTimer += 0.0015f * Main.rand.NextFloat(0.5f, 1.5f), 0, 1);
            }
            else
            {
                Utils.Clamp(_inverseTimer -= 0.0015f * Main.rand.NextFloat(0.5f, 1.5f), 0, 1);
            }

            if (Main.rand.NextBool(60))
            {
                _randomPoint1 = Vector2.Zero + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
            _currentOffset1 += Vector2.Zero.DirectionTo(_randomPoint1) / 15f;

            if (Main.rand.NextBool(60))
            {
                _randomPoint2 = Vector2.Zero + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
            _currentOffset2 += Vector2.Zero.DirectionTo(_randomPoint2) / 15f;
        }
    }
}
