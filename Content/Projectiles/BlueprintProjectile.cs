using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArenaInABottle.Content.Projectiles;

public class BlueprintProjectile : ModProjectile
{
    private const int OutlineWidth = 2;

    private readonly Texture2D _arrow = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/CornerArrow", AssetRequestMode.ImmediateLoad).Value;

    private readonly Texture2D _dot = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/Dot", AssetRequestMode.ImmediateLoad).Value;

    private readonly Color _highlightColorDragging = new(0.6f, 0.6f, 1f, 0.3f);

    private readonly Color _highlightColorNotDragging = new(1f, 1f, 1f, 0.3f);
    private readonly Color _outlineColorDragging = new(0.6f, 0.6f, 1f, 0.8f);
    private readonly Color _outlineColorNotDragging = new(1f, 1f, 1f, 0.8f);
    private bool _bottomCornerArrowDragging;
    private bool _bottomCornerArrowHovering;
    private Rectangle _bottomCornerArrowRectangle;
    private int _bottomRightX;
    private int _bottomRightY;
    private Vector2 _currentPlayerTilePos = Vector2.Zero;


    private float _differenceX;
    private float _differenceY;

    private float _initialPosX;
    private float _initialPosY;

    private ButtonState _oldMouseState;

    private Vector2 _previousMousePos;
    private bool _topCornerArrowDragging;


    private bool _topCornerArrowHovering;
    private Rectangle _topCornerArrowRectangle;


    private int _topLeftX;
    private int _topLeftY;
    private Vector2 _totalBottomOffset = new(16 * 2, 16 * 2);

    private Vector2 _totalTopOffset;

    private Vector2 _trailingPlayerTilePos = Vector2.Zero;

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
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
        DrawHighlight(Main.spriteBatch);
        Main.spriteBatch.End();
        old.Begin();

        return false;
    }

    private void DrawHighlight(SpriteBatch sb)
    {
        CheckInput();
        CheckMovement();

        _topLeftX = (int)_initialPosX - (int)Main.screenPosition.X + Helpers.RoundDownToNearest(_totalTopOffset.X, 16);
        _topLeftY = (int)_initialPosY - (int)Main.screenPosition.Y + Helpers.RoundDownToNearest(_totalTopOffset.Y, 16);

        //TODO this is somehow causing flickering when moving top left arrow

        _bottomRightX = 16 + Helpers.RoundDownToNearest(_totalBottomOffset.X, 16);
        _bottomRightY = 16 + Helpers.RoundDownToNearest(_totalBottomOffset.Y, 16);

        Rectangle rectangle = new(_topLeftX, _topLeftY, _bottomRightX, _bottomRightY);

        sb.Draw(_dot, rectangle, null,
            _topCornerArrowDragging || _bottomCornerArrowDragging
                ? _highlightColorDragging
                : _highlightColorNotDragging, 0, Vector2.Zero, SpriteEffects.None, 0);

        DrawOutlines(sb);
        DrawArrows(sb);
    }

    private void CheckMovement()
    {
        if (!_topCornerArrowDragging && !_bottomCornerArrowDragging) return;

        _trailingPlayerTilePos = _currentPlayerTilePos;

        _currentPlayerTilePos = Main.player[Projectile.owner].position.ToTileCoordinates().ToVector2();

        _differenceX = _trailingPlayerTilePos.X - _currentPlayerTilePos.X;
        _differenceY = _trailingPlayerTilePos.Y - _currentPlayerTilePos.Y;

        if (_topCornerArrowDragging)
        {
            _totalTopOffset.X -= _differenceX * 16;
            _totalBottomOffset.X += _differenceX * 16;

            _totalTopOffset.Y -= _differenceY * 16;
            _totalBottomOffset.Y += _differenceY * 16;
        }

        if (!_bottomCornerArrowDragging) return;


        _totalBottomOffset.X -= _differenceX * 16;

        _totalBottomOffset.Y -= _differenceY * 16;
    }

    private void CheckInput()
    {
        MouseState mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Released)
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
        }

        if ((_topCornerArrowDragging || _bottomCornerArrowDragging) && mouse.LeftButton == ButtonState.Released)
            _topCornerArrowDragging = _bottomCornerArrowDragging = false;

        if (mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Pressed &&
            (_topCornerArrowDragging || _bottomCornerArrowDragging))
        {
            if (_topCornerArrowDragging)
            {
                _totalTopOffset -= _previousMousePos - Main.MouseScreen;
                _totalBottomOffset += _previousMousePos - Main.MouseScreen;
                _previousMousePos = Main.MouseScreen;
            }
            else
            {
                _totalBottomOffset -= _previousMousePos - Main.MouseScreen;
                _previousMousePos = Main.MouseScreen;
            }
        }

        _oldMouseState = mouse.LeftButton;
    }

    private void DrawOutlines(SpriteBatch sb)
    {
        sb.Draw(_dot, TopLine(_topLeftX, _topLeftY, _bottomRightX), null,
            _topCornerArrowDragging || _bottomCornerArrowDragging
                ? _outlineColorDragging
                : _outlineColorNotDragging, 0, Vector2.Zero, SpriteEffects.None, 0);

        sb.Draw(_dot, BottomLine(_topLeftX, _topLeftY, _bottomRightX, _bottomRightY),
            null, _topCornerArrowDragging || _bottomCornerArrowDragging
                ? _outlineColorDragging
                : _outlineColorNotDragging, 0, Vector2.Zero, SpriteEffects.None, 0);

        sb.Draw(_dot, LeftLine(_topLeftX, _topLeftY, _bottomRightY), null,
            _topCornerArrowDragging || _bottomCornerArrowDragging
                ? _outlineColorDragging
                : _outlineColorNotDragging, 0, Vector2.Zero, SpriteEffects.None, 0);

        sb.Draw(_dot, RightLine(_topLeftX, _topLeftY, _bottomRightX, _bottomRightY),
            null, _topCornerArrowDragging || _bottomCornerArrowDragging
                ? _outlineColorDragging
                : _outlineColorNotDragging, 0, Vector2.Zero, SpriteEffects.None, 0);
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
            new Rectangle(_topLeftX + _bottomRightX - 16, _topLeftY + _bottomRightY - 16, 16, 16);

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