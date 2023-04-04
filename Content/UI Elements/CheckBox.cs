using System;
using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle.Content.UI_Elements;

public class CheckBox : UIElement
{
    private readonly Texture2D _checkBoxTex = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/CheckBox", AssetRequestMode.ImmediateLoad).Value;
    private Rectangle _size, _lockSize;
    private static readonly SpriteBatch DrawBatch = new(Main.graphics.GraphicsDevice);
    
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    private readonly SoundStyle _menuInvalid = new("ArenaInABottle/Content/Sounds/MenuInvalid");
    private readonly Texture2D _lock = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Lock", AssetRequestMode.ImmediateLoad).Value;
    
    private ButtonState _oldMouseState;
    public int CheckBoxIndex { get; }

    private bool _mouseOver;
    
    public bool IsActive { get; private set; }
    public bool IsLocked { get; set; }
    
    private readonly Color _hoveringColor = new(1f, 1f, 1f);
    private readonly Color _notHoveringColor = new(0.8f, 0.8f, 0.8f);
    public string DrawStringExplain;

    public void DeactivateCheckBox()
    {
        Remove();
        IsActive = false;
    }

    public void ActivateCheckBox(UIElement parentPanel)
    {
        parentPanel.Append(this);
        IsActive = true;
    }
    
    public CheckBox(int index)
    {
        CheckBoxIndex = index;
        IsLocked = false;
        IsActive = false;
        Width.Set(25,0);
        Height.Set(25,0);
        HAlign = 0.64f;
        VAlign = 0.15f;
        Top.Set(30 + 30 * CheckBoxIndex, 0);
    }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Point point1 = new((int)dimensions.X, (int)dimensions.Y);
        int width = (int)Math.Ceiling(dimensions.Width);
        int height = (int)Math.Ceiling(dimensions.Height);
        _size = new Rectangle(point1.X, point1.Y, width, height);
        _lockSize = new Rectangle(point1.X, point1.Y, width + 30, height);
        spriteBatch.Draw(_checkBoxTex, _size, _mouseOver ? ArenaPlayer.UiColor.MultiplyRGB(_hoveringColor) : ArenaPlayer.UiColor.MultiplyRGB(_notHoveringColor));
        if (CheckStatus())
        {
            spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.ItemFont), "X", point1.ToVector2() + new Vector2(7, 4), Color.Black);
        }
        
        if (IsLocked)
        {
            spriteBatch.Draw(_lock, _lockSize,null,Color.White,0,new Vector2(-8, 0),SpriteEffects.None,0);
        }

        DrawStringExplain = CheckBoxCurrentExplain();
        
        spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.ItemFont), DrawStringExplain, point1.ToVector2() + new Vector2(42, 4), Color.Black);
    }

    public override void Update(GameTime gameTime)
    {
        Rectangle dim = _size;
        MouseState mouse = Mouse.GetState();
        _mouseOver = mouse.X > dim.X && mouse.X < dim.X + dim.Width && mouse.Y > dim.Y &&
                         mouse.Y < dim.Y + dim.Height;

        if (mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Released)
        {
            if (_mouseOver)
            {
                if (!ArenaPlayer.ItemLockedIn)
                {
                    LeftClick();
                }
                else
                {
                    SoundEngine.PlaySound(_menuInvalid);
                }
            }
        }
        
        _oldMouseState = mouse.LeftButton;
    }

    private string CheckBoxCurrentExplain()
    {
        return ArenaPlayer.CheckBoxesExplained[CheckBoxIndex];
    }
    
    private void LeftClick()
    {
        IsLocked = !IsLocked;
        switch (CheckBoxIndex)
        {
            case 0:
                ArenaPlayer.CheckBox1 = !ArenaPlayer.CheckBox1;
                break;
            case 1:
                ArenaPlayer.CheckBox2 = !ArenaPlayer.CheckBox2;
                break;
            case 2:
                ArenaPlayer.CheckBox3 = !ArenaPlayer.CheckBox3;
                break;
            case 3:
                ArenaPlayer.CheckBox4 = !ArenaPlayer.CheckBox4;
                break;
            case 4:
                ArenaPlayer.CheckBox5 = !ArenaPlayer.CheckBox5;
                break;
            case 5:
                ArenaPlayer.CheckBox6 = !ArenaPlayer.CheckBox6;
                break;
            default:
                throw new Exception($"Checkbox status of index {_checkBoxTex} couldn't be found!");
        }
    }
    
    

    private bool CheckStatus()
    {
        return CheckBoxIndex switch
        {
            0 => ArenaPlayer.CheckBox1,
            1 => ArenaPlayer.CheckBox2,
            2 => ArenaPlayer.CheckBox3,
            3 => ArenaPlayer.CheckBox4,
            4 => ArenaPlayer.CheckBox5,
            5 => ArenaPlayer.CheckBox6,
            _ => throw new Exception($"Checkbox status of index {_checkBoxTex} couldn't be found!")
        };
    }
}