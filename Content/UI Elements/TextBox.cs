using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using ArenaInABottle.Content.Misc;

namespace ArenaInABottle.Content.UI_Elements;

public class TextBox : UIElement
{
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    
    public bool Focused;
    private Rectangle _size;
    public bool Hovering = false;
    public int Timer;
    public bool Inverse;
    private int TexBoxIndex { get; }
    public readonly bool IsLocked = true;
    
    private string _drawString = "not found";
    private string _drawStringExplain = "not found";

    private readonly SoundStyle _menuInvalid = new("ArenaInABottle/Content/Sounds/MenuInvalid");
    private readonly Texture2D _lock = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Lock", AssetRequestMode.ImmediateLoad).Value;
    
    private ButtonState _oldMouseState;

    private readonly Color _hoveringColor = new(1f, 1f, 1f);
    private readonly Color _notHoveringColor = new(0.8f, 0.8f, 0.8f);
    
    private readonly List<(int, string)> _textBoxVariables = new();

    public bool IsActive { get; private set; }

    public TextBox(int textBoxIndex)
    {
        TexBoxIndex = textBoxIndex;
        Width.Set(60,0);
        Height.Set(25,0);
        HAlign = 0.05f;
        VAlign = 0.15f;
        Top.Set(30 + (30 * textBoxIndex), 0);
    }
        
    public void DeactivateTextBox()
    {
        Remove();
        IsActive = false;
    }

    public void ActivateTextBox(UIElement parentPanel)
    {
        parentPanel.Append(this);
        IsActive = true;
    }

    public override void OnInitialize()
    {
        for (int i = 0; i <= 5; i++)
        {
            _textBoxVariables.Add((i,"TextBox" + i));
        }
        base.OnInitialize();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Texture2D whiteTex = ModContent.Request<Texture2D>("ArenaInABottle/Content/Images/Input").Value;
        CalculatedStyle dimensions = GetDimensions();
        Point point1 = new((int)dimensions.X, (int)dimensions.Y);
        int width = (int)Math.Ceiling(dimensions.Width);
        int height = (int)Math.Ceiling(dimensions.Height);
        _size = new Rectangle(point1.X, point1.Y, width, height);
        
        
        spriteBatch.Draw(whiteTex, _size, Hovering ? 
            ArenaPlayer.UiColor.MultiplyRGB(_hoveringColor) :
            ArenaPlayer.UiColor.MultiplyRGB(_notHoveringColor));

        Timer += 1;

        if (Timer >= 25)
        {
            Inverse = !Inverse;
            Timer = 0;
        }

        if (IsLocked)
        {
            spriteBatch.Draw(_lock, _size,null,Color.White,0,new Vector2(-40, 0),SpriteEffects.None,0);
        }


        spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.ItemFont), _drawString, point1.ToVector2() + new Vector2(4, 4), Color.Black);

        if (!Inverse && Focused && _drawString != null)
        {
            spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.ItemFont), "|", point1.ToVector2() + new Vector2(Helpers.ProvideFont(Helpers.Fonts.ItemFont).MeasureString(_drawString).X + 2, 4),
                Color.Black);
        }
        
        spriteBatch.DrawString(Helpers.ProvideFont(Helpers.Fonts.ItemFont), _drawStringExplain, point1.ToVector2() + new Vector2(78, 4), Color.Black);
    }

    public override void Update(GameTime gameTime)
    {
        _drawString = TexBoxIndex switch
        {
            0 => ArenaPlayer.TextBox0,
            1 => ArenaPlayer.TextBox1,
            2 => ArenaPlayer.TextBox2,
            3 => ArenaPlayer.TextBox3,
            4 => ArenaPlayer.TextBox4,
            5 => ArenaPlayer.TextBox5,
            _ => _drawString
        };
        _drawStringExplain = TexBoxIndex switch
        {
            0 => ArenaPlayer.TextBoxesExplained[0],
            1 => ArenaPlayer.TextBoxesExplained[1],
            2 => ArenaPlayer.TextBoxesExplained[2],
            3 => ArenaPlayer.TextBoxesExplained[3],
            4 => ArenaPlayer.TextBoxesExplained[4],
            5 => ArenaPlayer.TextBoxesExplained[5],
            _ => _drawString
        };

        Rectangle dim = _size;
        MouseState mouse = Mouse.GetState();
        bool mouseOver = mouse.X > dim.X && mouse.X < dim.X + dim.Width && mouse.Y > dim.Y &&
                         mouse.Y < dim.Y + dim.Height;

        if (mouse.LeftButton == ButtonState.Pressed && _oldMouseState == ButtonState.Released)
        {
            if (!ArenaPlayer.ItemLockedIn)
            {
                LeftClick(mouseOver);
            }
            else
            {
                if (mouseOver)
                    SoundEngine.PlaySound(_menuInvalid);
            }
        }

        if (Focused && !ArenaPlayer.ItemLockedIn)
        {
            HandleTextInput();
        }

        _oldMouseState = mouse.LeftButton;
        base.Update(gameTime);
    }
    
    private void LeftClick(bool mouseOver)
    {
        Focused = Focused switch
        {
            false when mouseOver => true,
            true when !mouseOver => false,
            _ => Focused
        };
    }

    private void HandleTextInput()
    {
        PlayerInput.WritingText = true;
        if (Keyboard.GetState().GetPressedKeys().Length == 0) return;
        
        Array allKeys = Enum.GetValues(typeof(Keys));

        foreach (Keys key in allKeys)
        {
            bool isPressed = Helpers.KeyTyped(key);
            if (!isPressed) continue;
            
            switch (key)
            {
                case Keys.Enter or Keys.Escape:
                    Focused = false;
                    return;
                case Keys.Back or Keys.Delete:
                    DeleteString();
                    return;
                case Keys.Tab:
                    ScrollFocus();
                    return;
            }

            char num = key.ToString()[(key.ToString().Length - 1)..].ToCharArray().First();

            if (!int.TryParse(num.ToString(), out _)) continue;

            string temp = (string)typeof(ArenaPlayer).GetField(_textBoxVariables[TexBoxIndex].Item2)?.GetValue(ArenaPlayer);
            
            if (temp?.Length > 4) return;
            
            typeof(ArenaPlayer).GetField(_textBoxVariables[TexBoxIndex].Item2)?.SetValue(ArenaPlayer, temp + num);
        }
        //OnKeyPressedUpdateRequirements();
    }

    private void ScrollFocus()
    {
        
    }

    private void DeleteString()
    {
        string temp = (string)typeof(ArenaPlayer).GetField(_textBoxVariables[TexBoxIndex].Item2)?.GetValue(ArenaPlayer);
        
        if (temp == null)
            throw new NullReferenceException($"text box variable of index {TexBoxIndex} couldn't be found!");
        
        if (temp.Length == 0) return;
            
        typeof(ArenaPlayer).GetField(_textBoxVariables[TexBoxIndex].Item2)?.SetValue(ArenaPlayer, temp[..^1]);
    }

    private void OnKeyPressedUpdateRequirements()
    {
        throw new NotImplementedException();
    }
}