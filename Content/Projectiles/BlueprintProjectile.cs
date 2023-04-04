using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ArenaInABottle.Content.Items;
using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Light;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ArenaInABottle.Content.Projectiles;

public class BlueprintProjectile : ModProjectile
{
    private const int OutlineWidth = 2;
    
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    
    private readonly Texture2D _arrow = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/CornerArrow", AssetRequestMode.ImmediateLoad).Value;

    private readonly Texture2D _dot = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/Dot", AssetRequestMode.ImmediateLoad).Value;
    
    private readonly Texture2D _checkBox = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/checkbox", AssetRequestMode.ImmediateLoad).Value;
    
    private readonly Texture2D _numberBox = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/numberBox", AssetRequestMode.ImmediateLoad).Value;

    private readonly Texture2D _plus = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/plus", AssetRequestMode.ImmediateLoad).Value;
    
    private readonly Texture2D _minus = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/minus", AssetRequestMode.ImmediateLoad).Value;
    
    private readonly SoundStyle _menuInvalid = new("ArenaInABottle/Content/Sounds/MenuInvalid");
    private readonly SoundStyle _loading = new("ArenaInABottle/Content/Sounds/loading");

    #region Colors
    
    private readonly Color _highlightColorDragging = new(0.6f, 0.6f, 1f, 0.3f);
    private readonly Color _highlightColorNotDragging = new(1f, 1f, 1f, 0.3f);
    private readonly Color _outlineColorDragging = new(0.4f, 0.4f, 1f, 0.9f);
    private readonly Color _outlineColorNotDragging = new(1f, 1f, 1f, 0.9f);
    private readonly Color _checkBoxColorHovering = new(0.2f, 0.8f, 0.2f, 0.9f);
    private readonly Color _checkBoxColorNotHovering = new(0.2f, 0.8f, 0.2f, 0.3f);
    
    private readonly Color _plusColorHovering = new(0.2f, 0.8f, 0.2f, 0.9f);
    private readonly Color _plusColorNotHovering = new(0.2f, 0.8f, 0.2f, 0.3f);
    private readonly Color _minusColorHovering = new(0.8f, 0.2f, 0.2f, 0.9f);
    private readonly Color _minusColorNotHovering = new(0.8f, 0.2f, 0.2f, 0.3f);
    #endregion
    
    
    #region MainRectangle
    
    //bottom corner
    private int _bottomRightX;
    private int _bottomRightY;
    private bool _bottomCornerArrowDragging;
    private bool _bottomCornerArrowHovering;
    private Rectangle _bottomCornerArrowRectangle;
    private Vector2 _totalBottomOffset = new(16 * 3, 16 * 3);
    //top corner
    private int _topLeftX;
    private int _topLeftY;
    private bool _topCornerArrowDragging;
    private bool _topCornerArrowHovering;
    private Rectangle _topCornerArrowRectangle;
    private Vector2 _totalTopOffset;

    // starting rectangle
    private float _initialPosX;
    private float _initialPosY;
    
    private Rectangle _mainRectangle;
    
    #endregion

    #region Input

    private ButtonState _oldMouseState;
    private Vector2 _previousMousePos;
    
    private Vector2 _currentPlayerTilePos = Vector2.Zero;
    private Vector2 _trailingPlayerTilePos = Vector2.Zero;

    #endregion
    

    private bool _outlineHovering;
    private bool _outlineDragging;
    private bool _checkBoxHovering;

    private Rectangle _checkBoxRectangle;
    private Rectangle _plusRectangle;
    private Rectangle _numberBoxRectangle;
    private Rectangle _minusRectangle;

    

    private bool _plusHovering;
    private bool _minusHovering;

    #region Shader

    private bool _progressStarted;
    private float _progress;

    #endregion
    

    #region Item

    private TileInfo[] _areaStruct;
    private int _height;
    
    #endregion

    private int _capsuleCount = 1;

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.timeLeft = int.MaxValue;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _initialPosX = Projectile.position.X;
        _initialPosY = Projectile.position.Y;
        _currentPlayerTilePos = Main.player[Projectile.owner].position.ToTileCoordinates().ToVector2();
    }

    public override bool CanHitPvp(Player target)
    {
        return false;
    }

    public override bool? CanHitNPC(NPC target)
    {
        return false;
    }
        
    
    
    public override bool PreDraw(ref Color lightColor)
    {

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        if (_progressStarted)
        {
            _progress += 0.02f;
            GameShaders.Misc["Frac"].Shader.Parameters["progress"].SetValue(_progress);
            GameShaders.Misc["Frac"].Apply();
            if (_progress >= 1f)
            {
                AssembleItem(_areaStruct,_height);
                ArenaPlayer.BlueprintPlaced = false;
                Projectile.Kill();
            }
        }
        
        MainDraw(Main.spriteBatch);
        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
    }

    private void MainDraw(SpriteBatch sb)
    {
        float oldTotalTopOffsetX = Helpers.RoundDownToNearest(_totalTopOffset.X, 16);
        float oldTotalTopOffsetY = Helpers.RoundDownToNearest(_totalTopOffset.Y, 16);
        Vector2 oldTotalTopOffset = new(oldTotalTopOffsetX, oldTotalTopOffsetY);

        CheckInput();
        CheckMovement();

        float newTotalTopOffsetX = Helpers.RoundDownToNearest(_totalTopOffset.X, 16);
        float newTotalTopOffsetY = Helpers.RoundDownToNearest(_totalTopOffset.Y, 16);
        Vector2 newTotalTopOffset = new(newTotalTopOffsetX, newTotalTopOffsetY);
        
        if(oldTotalTopOffset != newTotalTopOffset) TopCornerChanged(oldTotalTopOffset, newTotalTopOffset);
        
        _topLeftX = -8 + (int)_initialPosX - (int)Main.screenPosition.X + Helpers.RoundDownToNearest(_totalTopOffset.X, 16);
        _topLeftY = -8 + (int)_initialPosY - (int)Main.screenPosition.Y + Helpers.RoundDownToNearest(_totalTopOffset.Y, 16);
        
        //TODO for now I couldn't come up with solution to fix snapping asymmetry when dragging top arrow
        _bottomRightX = 16 + Helpers.RoundDownToNearest(_totalBottomOffset.X, 16);
        _bottomRightY = 16 + Helpers.RoundDownToNearest(_totalBottomOffset.Y, 16);
        
        
        _mainRectangle = new Rectangle(_topLeftX + OutlineWidth, _topLeftY + OutlineWidth, _bottomRightX - OutlineWidth * 2, _bottomRightY - OutlineWidth);
        
        
        
        sb.Draw(_dot, _mainRectangle, null,
            _topCornerArrowDragging || _bottomCornerArrowDragging
                ? _highlightColorDragging
                : _highlightColorNotDragging, 0, Vector2.Zero, SpriteEffects.None, 0);
        
        if (_progressStarted) return;
        DrawOutlines(sb);
        DrawArrows(sb);
        DrawCheckBox(sb);
        DrawCountSelection(sb);
    }

    private void TopCornerChanged(Vector2 old, Vector2 current)
    {
        _totalBottomOffset.X += 16 * ((old.X - current.X) / 16);
        _totalBottomOffset.Y += 16 * ((old.Y - current.Y) / 16);
    }
    
    private void CheckInput()
    {
        MouseState mouse = Mouse.GetState();
        bool clicked = mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Released;
        bool holding = mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Pressed;
        if (clicked)
        {
            if (_bottomCornerArrowHovering)
            {
                _bottomCornerArrowDragging = true;
                _previousMousePos = Main.MouseScreen;
            }

            if (_topCornerArrowHovering)
            {
                _topCornerArrowDragging = true;
                _previousMousePos = Main.MouseScreen;
            }
            
            if (_outlineHovering)
            {
                _outlineDragging = true;
                _previousMousePos = Main.MouseScreen;
            }

            if (_checkBoxHovering)
            {
                SoundEngine.PlaySound(_loading);
                StartCopying();
            }

            if (_plusHovering)
            {
                if (_capsuleCount < 9)
                {
                    _capsuleCount++;
                    SoundEngine.PlaySound(new SoundStyle(SoundID.MenuTick.SoundPath));
                }
                else
                {
                    SoundEngine.PlaySound(_menuInvalid);
                }
            }
            
            if (_minusHovering)
            {
                if (_capsuleCount > 1)
                {
                    _capsuleCount--;
                    SoundEngine.PlaySound(new SoundStyle(SoundID.MenuTick.SoundPath));
                }
                else
                {
                    SoundEngine.PlaySound(_menuInvalid);
                }
            }
        }
        
        if ((_topCornerArrowDragging || _bottomCornerArrowDragging || _outlineDragging) && mouse.LeftButton == ButtonState.Released)
        {
            _topCornerArrowDragging = _bottomCornerArrowDragging = _outlineDragging = false;
        }
        
        if (holding)
        {
            if (_topCornerArrowDragging)
            {
                Vector2 difference = _previousMousePos - Main.MouseScreen;

                switch (difference.X)
                {
                    case > 0:
                        _totalTopOffset.X -= difference.X;
                        break;
                    case < 0:
                        if (_mainRectangle.Width > 4 * 16) _totalTopOffset.X -= difference.X;
                        break;
                }

                switch (difference.Y)
                {
                    case > 0:
                        _totalTopOffset.Y -= difference.Y;
                        break;
                    case < 0:
                        if (_mainRectangle.Height > 4 * 16) _totalTopOffset.Y -= difference.Y;
                        break;
                }
                
                _previousMousePos = Main.MouseScreen;
            }
            if(_bottomCornerArrowDragging)
            {
                Vector2 difference = _previousMousePos - Main.MouseScreen;

                switch (difference.X)
                {
                    case > 0:
                        if (_mainRectangle.Width > 4 * 16) _totalBottomOffset.X -= difference.X;
                        break;
                    case < 0:
                        _totalBottomOffset.X -= difference.X;
                        break;
                }

                switch (difference.Y)
                {
                    case > 0:
                        if (_mainRectangle.Height > 4 * 16) _totalBottomOffset.Y -= difference.Y;
                        break;
                    case < 0:
                        _totalBottomOffset.Y -= difference.Y;
                        break;
                }

                _previousMousePos = Main.MouseScreen;
            }
            
            if (_outlineDragging)
            {
                _totalTopOffset -= _previousMousePos - Main.MouseScreen;
                _previousMousePos = Main.MouseScreen;
            }
        }
        _oldMouseState = mouse.LeftButton;
    }

    private void CheckMovement()
    {
        if (!_topCornerArrowDragging && !_bottomCornerArrowDragging) return;

        _trailingPlayerTilePos = _currentPlayerTilePos;

        _currentPlayerTilePos = Main.player[Projectile.owner].position.ToTileCoordinates().ToVector2();

        float differenceX = _trailingPlayerTilePos.X - _currentPlayerTilePos.X;
        float differenceY = _trailingPlayerTilePos.Y - _currentPlayerTilePos.Y;

        if (_topCornerArrowDragging)
        {
            _totalTopOffset.X -= differenceX * 16;

            _totalTopOffset.Y -= differenceY * 16;
        }

        if (_bottomCornerArrowDragging)
        {
            _totalBottomOffset.X -= differenceX * 16;

            _totalBottomOffset.Y -= differenceY * 16;
        }

        if (!_outlineDragging) return;
        
        _totalTopOffset.X -= differenceX * 16;
        _totalBottomOffset.X += differenceX * 16;

        _totalTopOffset.Y -= differenceY * 16;
        _totalBottomOffset.Y += differenceY * 16;
    }
    
    private void DrawOutlines(SpriteBatch sb)
    {
        Rectangle topRectangle = TopLine(_topLeftX, _topLeftY, _bottomRightX);
        Rectangle bottomRectangle = BottomLine(_topLeftX, _topLeftY, _bottomRightX, _bottomRightY);
        Rectangle leftRectangle = LeftLine(_topLeftX, _topLeftY, _bottomRightY);
        Rectangle rightRectangle = RightLine(_topLeftX, _topLeftY, _bottomRightX, _bottomRightY);

        _outlineHovering = MouseOverOutline(topRectangle, bottomRectangle, leftRectangle, rightRectangle);
        Color outlineColor = _outlineHovering ||
                             _topCornerArrowDragging ||
                             _bottomCornerArrowDragging
            ? _outlineColorDragging
            : _outlineColorNotDragging;


        sb.Draw(_dot, topRectangle, null,
            outlineColor, 0, Vector2.Zero, SpriteEffects.None, 0);


        sb.Draw(_dot, leftRectangle, null,
            outlineColor, 0, Vector2.Zero, SpriteEffects.None, 0);

        sb.Draw(_dot, rightRectangle, null,
            outlineColor, 0, Vector2.Zero, SpriteEffects.None, 0);
        
        sb.Draw(_dot, bottomRectangle, null,
            outlineColor, 0, Vector2.Zero, SpriteEffects.None, 0);
    }
    
    private void DrawArrows(SpriteBatch sb)
    {
        _bottomCornerArrowHovering = _bottomCornerArrowRectangle.Contains(Main.MouseScreen.ToPoint());
        _topCornerArrowHovering = _topCornerArrowRectangle.Contains(Main.MouseScreen.ToPoint());

        Color topArrowColor = Color.Green;
        Color bottomArrowColor = Color.Green;

        if (_topCornerArrowHovering) topArrowColor = Color.Lime;
        if (_topCornerArrowDragging) topArrowColor = Color.Aqua;

        if (_bottomCornerArrowHovering) bottomArrowColor = Color.Lime;
        if (_bottomCornerArrowDragging) bottomArrowColor = Color.Aqua;

        _bottomCornerArrowRectangle =
            new Rectangle(_topLeftX + _bottomRightX - 18, _topLeftY + _bottomRightY - 16, 16, 16);

        _topCornerArrowRectangle = new Rectangle(_topLeftX + 2, _topLeftY + 2, 12 + 2, 12 + 2);


        sb.Draw(_arrow, _bottomCornerArrowRectangle, null, bottomArrowColor,
            0, Vector2.Zero, SpriteEffects.None, 0);

        sb.Draw(_arrow, _topCornerArrowRectangle, null, topArrowColor,
            0, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);
    }
    
    private void DrawCheckBox(SpriteBatch sb)
    {
        int x = _mainRectangle.X + _mainRectangle.Width - 14;
        int y = _mainRectangle.Y + _mainRectangle.Height + OutlineWidth;
        _checkBoxRectangle = new Rectangle(x, y, 16, 16);
        
        _checkBoxHovering = _checkBoxRectangle.Contains(Main.MouseScreen.ToPoint());
        sb.Draw(_checkBox, _checkBoxRectangle, null,
            _checkBoxHovering
                ? _checkBoxColorHovering
                : _checkBoxColorNotHovering, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    private void DrawCountSelection(SpriteBatch sb)
    {
        
        int plusX = _mainRectangle.X + _mainRectangle.Width - 30;
        int plusY = _mainRectangle.Y + _mainRectangle.Height + OutlineWidth;
        _plusRectangle = new Rectangle(plusX, plusY, 16, 16);
        _plusHovering = _plusRectangle.Contains(Main.MouseScreen.ToPoint());
        
        int numberBoxX = _mainRectangle.X + _mainRectangle.Width - 46;
        int numberBoxY = _mainRectangle.Y + _mainRectangle.Height + OutlineWidth;
        _numberBoxRectangle = new Rectangle(numberBoxX, numberBoxY, 16, 16);
        
        int minusX = _mainRectangle.X + _mainRectangle.Width - 62;
        int minusY = _mainRectangle.Y + _mainRectangle.Height + OutlineWidth;
        _minusRectangle = new Rectangle(minusX, minusY, 16, 16);
        _minusHovering = _minusRectangle.Contains(Main.MouseScreen.ToPoint());
        
        sb.Draw(_plus, _plusRectangle, null,
            _plusHovering
                ? _plusColorHovering
                : _plusColorNotHovering, 0, Vector2.Zero, SpriteEffects.None, 0);
        
        
        sb.Draw(_minus, _minusRectangle, null,
            _minusHovering
                ? _minusColorHovering
                : _minusColorNotHovering, 0, Vector2.Zero, SpriteEffects.None, 0);
        
        sb.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
        
        
        
        Vector2 topLeft = new(plusX + Main.screenPosition.X, plusY + Main.screenPosition.Y);
        Vector2 topLeftTile = topLeft.ToTileCoordinates().ToVector2();
        float brightness = Lighting.Brightness((int)topLeftTile.X, (int)topLeftTile.Y);
        Color lerpedColor = Color.Lerp(Color.White, Color.Black, brightness);
        

        sb.DrawString(Helpers.ProvideFont(Helpers.Fonts.CombatFont0), _capsuleCount.ToString(),
            _numberBoxRectangle.Center() -
            new Vector2(Helpers.ProvideFont(Helpers.Fonts.CombatFont0)
                .MeasureString(_capsuleCount.ToString()).X - 6,10),
            lerpedColor,0,Vector2.Zero,
            0.8f,SpriteEffects.None,0);
        
    }
    
    private void StartCopying()
    {
        Vector2 topLeft = new(_topLeftX + Main.screenPosition.X, _topLeftY + Main.screenPosition.Y);
        Vector2 topLeftTile = topLeft.ToTileCoordinates().ToVector2();
        int recHeight = _mainRectangle.Height / 16 + 1;
        int recWidth = _mainRectangle.Width / 16 + 1;
        int size = recHeight * recWidth;
        int index = 0;
        TileInfo[] tiles = new TileInfo[size];
        for (int i = 0; i < recWidth; i++)
        {
            for (int j = 0; j < recHeight; j++)
            {
                Vector2 thisTileVec = new(topLeftTile.X + i, topLeftTile.Y + j);
                        
                Tile thisTile = Main.tile[thisTileVec.ToPoint()];
                
                int style = 0, alt = 0;
                TileObjectData.GetTileInfo(thisTile, ref style, ref alt);
                TileObjectData tileData = TileObjectData.GetTileData(thisTile.TileType, style, alt);
                bool specialTile = false;
                bool originTile = false;
                
                if (tileData != null)
                {
                    originTile = thisTileVec.ToPoint16() == GetTileWorldOrigin((int)thisTileVec.X, (int)thisTileVec.Y, tileData);
                    Point16 difference = thisTileVec.ToPoint16() - GetTileWorldOrigin((int)thisTileVec.X, (int)thisTileVec.Y, tileData);
                    Main.NewText(difference);
                    if (originTile)
                    {
                        Dust.QuickDust(thisTileVec.ToPoint(), Color.Aqua);
                        
                    }
                    specialTile = true;
                }

                TileInfo tile = new()
                {
                    TileType = thisTile.HasTile ? thisTile.TileType : ushort.MaxValue,
                    TileX = i,
                    TileY = j,
                    WallType = thisTile.WallType,
                    LiquidType = thisTile.LiquidType,
                    LiquidAmount = thisTile.LiquidAmount,
                    IsSpecialTile = specialTile,
                    OriginTile = originTile,
                    Style = style
                };
                
                tiles[index] = tile;
                index++;
            }
        }

        _areaStruct = tiles;
        _height = recHeight;
        _progressStarted = true;
        int x = _mainRectangle.X + _mainRectangle.Width - 16;
        int y = _mainRectangle.Y + _mainRectangle.Height + OutlineWidth;

        for (int i = 0; i < 50; i++)
        {
            
            Vector2 newVec = new(x + Main.screenPosition.X + 8 + Main.rand.NextFloat(-8f,8f), y + Main.screenPosition.Y + 8 + Main.rand.NextFloat(-8f,8f));
            Dust.NewDust(newVec, 0, 0, 298, 0, 0, 100, Color.White);
        }
    }
    
    private static Point16 GetTileWorldOrigin(int i, int j, TileObjectData tileObjectData)
    {
        Tile tile = Framing.GetTileSafely(i, j);
        Point16 coord = new(i, j);
        //Point16 frame = new(tile.TileFrameX / 18, tile.TileFrameY / 18);
        
        int size = 16 + tileObjectData.CoordinatePadding;
        int frameX = tile.TileFrameX % (size * tileObjectData.Width) / size;
        int frameY = tile.TileFrameY % (size * tileObjectData.Height) / size;
        Point16 addition = new(frameX, frameY);
        Point16 origin = tileObjectData.Origin;
        return coord - addition + origin;
    }
    
    private void AssembleItem(TileInfo[] area, int height)
    {
        Item[] items = new Item[_capsuleCount];

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = Main.player[Projectile.owner].QuickSpawnItemDirect(Terraria.Entity.GetSource_None(), ModContent.ItemType<StructuralBottle>());
        }
        
        StructuralBottle[] capsules = new StructuralBottle[_capsuleCount];

        List<string> loadedMods = ModLoader.Mods.Select(mod => mod.Name).ToList();

        for (int i = 0; i < items.Length; i++)
        {
            capsules[i] = (StructuralBottle)items[i].ModItem;
            capsules[i].AreaCopied = area;
            capsules[i].Height = height;
            capsules[i].OriginalMods = loadedMods;
        }
    }

    private static bool MouseOverOutline(Rectangle top, Rectangle bottom, Rectangle left, Rectangle right)
    {
        return top.Contains(Main.MouseScreen.ToPoint()) ||
               bottom.Contains(Main.MouseScreen.ToPoint()) ||
               left.Contains(Main.MouseScreen.ToPoint()) ||
               right.Contains(Main.MouseScreen.ToPoint());
    }

    private static Rectangle TopLine(int topLeftX, int topLeftY, int bottomRightX)
    {
        return new Rectangle(topLeftX, topLeftY, bottomRightX, OutlineWidth);
    }

    private static Rectangle RightLine(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY)
    {
        return new Rectangle(topLeftX + bottomRightX - OutlineWidth, topLeftY, OutlineWidth, bottomRightY);
    }

    private static Rectangle LeftLine(int topLeftX, int topLeftY, int bottomRightY)
    {
        return new Rectangle(topLeftX, topLeftY, OutlineWidth, bottomRightY);
    }

    private static Rectangle BottomLine(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY)
    {
        return new Rectangle(topLeftX, topLeftY + bottomRightY, bottomRightX, OutlineWidth);
    }
}