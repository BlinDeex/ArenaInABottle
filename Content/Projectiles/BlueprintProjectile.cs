using System;
using System.Collections.Generic;
using ArenaInABottle.Content.Items;
using ArenaInABottle.Content.Misc;
using IL.Terraria.WorldBuilding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Modules;

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
    
    private readonly Color _highlightColorDragging = new(0.6f, 0.6f, 1f, 0.3f);
    private readonly Color _highlightColorNotDragging = new(1f, 1f, 1f, 0.3f);
    private readonly Color _outlineColorDragging = new(0.4f, 0.4f, 1f, 0.9f);
    private readonly Color _outlineColorNotDragging = new(1f, 1f, 1f, 0.9f);
    private readonly Color _checkBoxColorHovering = new(0.4f, 1f, 0.4f, 0.9f);
    private readonly Color _checkBoxColorNotHovering = new(0.4f, 1f, 0.4f, 0.5f);
    
    #region MainRectangle
    
    //bottom corner
    private int _bottomRightX;
    private int _bottomRightY;
    private bool _bottomCornerArrowDragging;
    private bool _bottomCornerArrowHovering;
    private Rectangle _bottomCornerArrowRectangle;
    private Vector2 _totalBottomOffset = new(16 * 2, 16 * 2);
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
    
    #endregion


    private ButtonState _oldMouseState;
    
    private Vector2 _previousMousePos;
    
    private Vector2 _currentPlayerTilePos = Vector2.Zero;
    private Vector2 _trailingPlayerTilePos = Vector2.Zero;

    private bool _outlineHovering;
    private bool _outlineDragging;
    private bool _checkBoxHovering;

    private Rectangle _checkBoxRectangle;
    private Rectangle _mainRectangle;

    private bool _progressStarted;
    private float _progress;
    
    private (ushort, int, int, ushort,(int,byte))[] _area;
    
    private int _height;
    
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
        SpriteBatch old = Main.spriteBatch;


        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
        if (_progressStarted)
        {
            _progress += 0.02f;
            GameShaders.Misc["Frac"].Shader.Parameters["progress"].SetValue(_progress);
            
            if (_progress >= 1f)
            {
                AssembleItem(_area,_height);
                Projectile.Kill();
                ArenaPlayer.BlueprintPlaced = false;
            }
        }
        
        if(_progressStarted) GameShaders.Misc["Frac"].Apply();
        DrawMainRectangle(Main.spriteBatch);
        Main.spriteBatch.End();
        old.Begin();
        
        return false;
    }

    private void DrawMainRectangle(SpriteBatch sb)
    {
        CheckInput();
        CheckMovement();

        _topLeftX = -8 + (int)_initialPosX - (int)Main.screenPosition.X + Helpers.RoundDownToNearest(_totalTopOffset.X, 16);
        _topLeftY = -8 + (int)_initialPosY - (int)Main.screenPosition.Y + Helpers.RoundDownToNearest(_totalTopOffset.Y, 16);
        
        
        //TODO for now I couldnt come up with solution to fix rounding asymmetry when dragging top arrow
        _bottomRightX = 16 + Helpers.RoundDownToNearest(_totalBottomOffset.X, 16);
        _bottomRightY = 16 + Helpers.RoundDownToNearest(_totalBottomOffset.Y, 16);
        
        //_mainRectangle = new Rectangle(_topLeftX + OutlineWidth, _topLeftY + OutlineWidth, _bottomRightX - OutlineWidth * 2, _bottomRightY - OutlineWidth);
        _mainRectangle = new(_topLeftX + OutlineWidth, _topLeftY + OutlineWidth, _bottomRightX - OutlineWidth, _bottomRightY - OutlineWidth);
        
        
        
        sb.Draw(_dot, _mainRectangle, null,
            _topCornerArrowDragging || _bottomCornerArrowDragging
                ? _highlightColorDragging
                : _highlightColorNotDragging, 0, Vector2.Zero, SpriteEffects.None, 0);
        if (_progressStarted) return;
        //298
        DrawOutlines(sb);
        DrawArrows(sb);
        DrawCheckBox(sb);
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
            _totalBottomOffset.X += differenceX * 16;

            _totalTopOffset.Y -= differenceY * 16;
            _totalBottomOffset.Y += differenceY * 16;
        }

        if (!_bottomCornerArrowDragging) return;


        _totalBottomOffset.X -= differenceX * 16;

        _totalBottomOffset.Y -= differenceY * 16;
    }

    private void AssembleItem((ushort,int,int,ushort,(int,byte))[] area, int height)
    {
        Item item = Main.player[Projectile.owner].QuickSpawnItemDirect(Terraria.Entity.GetSource_None(), ModContent.ItemType<StructuralCapsule>());
        StructuralCapsule capsule = (StructuralCapsule)item.ModItem;
        capsule.TilesCopiedXy = area;
        capsule.Height = height;
    }

    private void DrawCheckBox(SpriteBatch sb)
    {
        int x = _mainRectangle.X + _mainRectangle.Width - 16;
        int y = _mainRectangle.Y + _mainRectangle.Height + OutlineWidth;
        _checkBoxRectangle = new Rectangle(x, y, 16, 16);
        
        _checkBoxHovering = _checkBoxRectangle.Contains(Main.MouseScreen.ToPoint());
        sb.Draw(_checkBox, _checkBoxRectangle, null,
            _checkBoxHovering
                ? _checkBoxColorHovering
                : _checkBoxColorNotHovering, 0, Vector2.Zero, SpriteEffects.None, 0);
    }

    private void CheckInput()
    {
        MouseState mouse = Mouse.GetState();
        bool clicked = mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Released;
        bool holding = mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Pressed;
        if (clicked)
        {
            bool operationDone = false;
            if (_bottomCornerArrowHovering)
            {
                _bottomCornerArrowDragging = true;
                _previousMousePos = Main.MouseScreen;
                operationDone = true;
            }

            if (_topCornerArrowHovering && !operationDone)
            {
                _topCornerArrowDragging = true;
                _previousMousePos = Main.MouseScreen;
                operationDone = true;
            }
            
            if (_outlineHovering && !operationDone)
            {
                _outlineDragging = true;
                _previousMousePos = Main.MouseScreen;
                operationDone = true;
            }

            if (_checkBoxHovering)
            {
                StartCopying();
            }
        }
        
        if ((_topCornerArrowDragging || _bottomCornerArrowDragging || _outlineDragging) && mouse.LeftButton == ButtonState.Released)
        {
            _topCornerArrowDragging = _bottomCornerArrowDragging = _outlineDragging = false;
            //_trailingPlayerTilePos = Main.player[Projectile.owner].position;
        }
        
        if (holding)
        {
            bool operationDone = false;
            if (_topCornerArrowDragging)
            {
                _totalTopOffset -= _previousMousePos - Main.MouseScreen;
                _totalBottomOffset += _previousMousePos - Main.MouseScreen;
                _previousMousePos = Main.MouseScreen;
                operationDone = true;
            }
            if(_bottomCornerArrowDragging && !operationDone)
            {
                _totalBottomOffset -= _previousMousePos - Main.MouseScreen;
                _previousMousePos = Main.MouseScreen;
                operationDone = true;
            }
            
            if (_outlineDragging && !operationDone)
            {
                _totalTopOffset -= _previousMousePos - Main.MouseScreen;
                _previousMousePos = Main.MouseScreen;
            }
        }
        _oldMouseState = mouse.LeftButton;
    }

    private void StartCopying()
    {
        Vector2 topLeft = new(_topLeftX + Main.screenPosition.X, _topLeftY + Main.screenPosition.Y);
        Vector2 topLeftTile = topLeft.ToTileCoordinates().ToVector2();
        int recHeight = _mainRectangle.Height / 16 + 1;
        int recWidth = _mainRectangle.Width / 16 + 1;
        int size = recHeight * recWidth;
        int index = 0;
        (ushort, int, int, ushort,(int,byte))[] tiles = new (ushort, int, int,ushort,(int,byte))[size];
        for (int i = 0; i < recWidth; i++)
        {
            for (int j = 0; j < recHeight; j++)
            {
                Vector2 thisTileVec = new(topLeftTile.X + i, topLeftTile.Y + j);
                        
                Tile thisTile = Main.tile[thisTileVec.ToPoint()];
                        
                tiles[index] = thisTile.HasTile ?
                    (thisTile.TileType,i,j,thisTile.WallType,(thisTile.LiquidType,thisTile.LiquidAmount)) 
                    : (ushort.MaxValue,i,j,thisTile.WallType,(thisTile.LiquidType,thisTile.LiquidAmount));
                
                index++;
            }
        }
                
        _area = tiles;
        _height = recHeight;
        _progressStarted = true;
        int x = _mainRectangle.X + _mainRectangle.Width - 16;
        int y = _mainRectangle.Y + _mainRectangle.Height + OutlineWidth;

        for (int i = 0; i < 50; i++)
        {
                    
            Vector2 newVec = new(x + Main.screenPosition.X + 8 + Main.rand.NextFloat(-8f,8f), y + Main.screenPosition.Y + 8 + Main.rand.NextFloat(-8f,8f));
            Dust.NewDust(newVec, 0, 0, 298, 0, 0, 100, Color.White, 1f);
        }
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

    private static bool MouseOverOutline(Rectangle top, Rectangle bottom, Rectangle left, Rectangle right)
    {
        return top.Contains(Main.MouseScreen.ToPoint()) ||
               bottom.Contains(Main.MouseScreen.ToPoint()) ||
               left.Contains(Main.MouseScreen.ToPoint()) ||
               right.Contains(Main.MouseScreen.ToPoint());
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