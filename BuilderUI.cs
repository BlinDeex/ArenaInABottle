using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using ArenaInABottle.Content.Items;
using ArenaInABottle.Content.Misc;
using ArenaInABottle.Content.UI_Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using UIState = Terraria.UI.UIState;

namespace ArenaInABottle;

public class BuilderUi : UIState
{
    public static BuilderUi Instance;
    private readonly UIPanel _panel = new UIPanel();
    private UIPanel _masterPanel;
    private InteractionPanel _masterInteractionPanel;
    private bool _dragging = false;
    private GalaxySlot _gs;
    private GalaxySlotEffects _gsEffects;
    private ArenaBookmark _arenaBookmark;
    private PoolBookmark _poolBookmark;
    private HellBookmark _hellBookmark;
    private CustomBookmark _customBookmark;
    private readonly TextBox _textBox1 = new(0), _textBox2 = new(1),_textBox3 = new(2), _textBox4 = new(3),_textBox5 = new(4), _textBox6 = new(5);
    private readonly CheckBox _checkBox0 = new(0), _checkBox1 = new(1),_checkBox2 = new(2), _checkBox3 = new(3),_checkBox4 = new(4), _checkBox5 = new(5);
    public TitleString Title = new();
    public List<TextBox> TextBoxes;
    public List<CheckBox> CheckBoxes;

    private RequirementsFrame _requirementsFrame;
    
    private ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();

    private Vector2 _masterPanelOffset;
    
    public override void OnInitialize()
    {
        Instance = this;
        TextBoxes = new List<TextBox> { _textBox1, _textBox2, _textBox3, _textBox4, _textBox5, _textBox6 };
        CheckBoxes = new List<CheckBox> { _checkBox0, _checkBox1, _checkBox2, _checkBox3, _checkBox4, _checkBox5 };
        _masterPanel = new UIPanel();
        _masterPanel.Width.Set(800, 0);
        _masterPanel.Height.Set(400, 0);
        _masterPanel.VAlign = 0.5f;
        _masterPanel.HAlign = 0.5f;
        _masterPanel.BorderColor = new Color(0f, 0f, 0f, 0f);
        _masterPanel.BackgroundColor = new Color(0f, 0f, 0f, 0f);
        Append(_masterPanel);
        Bookmarks(_masterPanel);
        InteractionPanel(_masterPanel);
        
    }

    public void ChangeUiState(IEnumerable<(int, string)> requestedTextBoxes, IEnumerable<(int, string)> requestedCheckBoxes, string title, Color uiColor)
    {
        if (title == ArenaPlayer.CurrentTitle) return;
        
        List<int> totalTextBoxes = new(){ 0, 1, 2, 3, 4, 5 };
        List<int> totalCheckBoxes = new() { 0, 1, 2, 3, 4, 5 };
        
        foreach ((int, string) requestedTextBox in requestedTextBoxes)
        {
            ArenaPlayer.TextBoxesExplained[requestedTextBox.Item1] = requestedTextBox.Item2;
            
            if (TextBoxes[requestedTextBox.Item1].IsActive)
            {
                totalTextBoxes.Remove(requestedTextBox.Item1);
                continue;
            }
            TextBoxes[requestedTextBox.Item1].ActivateTextBox(_masterInteractionPanel);
            totalTextBoxes.Remove(requestedTextBox.Item1);
        }

        foreach ((int, string) requestedCheckBox in requestedCheckBoxes)
        {
            ArenaPlayer.CheckBoxesExplained[requestedCheckBox.Item1] = requestedCheckBox.Item2;
            
            if (CheckBoxes[requestedCheckBox.Item1].IsActive)
            {
                
                totalCheckBoxes.Remove(requestedCheckBox.Item1);
                continue;
            }
            CheckBoxes[requestedCheckBox.Item1].ActivateCheckBox(_masterInteractionPanel);
            totalCheckBoxes.Remove(requestedCheckBox.Item1);
        }
        
        foreach (int extraTextBox in totalTextBoxes)
        {
            TextBoxes[extraTextBox].DeactivateTextBox();
        }

        foreach (int extraCheckBox in totalCheckBoxes)
        {
            CheckBoxes[extraCheckBox].DeactivateCheckBox();
        }

        ArenaPlayer.CurrentTitle = title;
        ArenaPlayer.UiColor = uiColor;
    }

    private void InteractionPanel(UIElement boundingPanel)
    {
        _masterInteractionPanel = new InteractionPanel();
        _masterInteractionPanel.Width.Set(500, 0);
        _masterInteractionPanel.Height.Set(400, 0);
        _masterInteractionPanel.VAlign = 0.5f;
        _masterInteractionPanel.HAlign = 0f;
        _masterInteractionPanel.OnMouseDown += MasterInteractionPanelOnMouseDown;
        _masterInteractionPanel.OnMouseUp += MasterInteractionPanelOnMouseUp;
        boundingPanel.Append(_masterInteractionPanel);
        GalaxySlot(_masterInteractionPanel);
        AppendTextBoxes(_masterInteractionPanel);
        AppendCheckBoxes(_masterInteractionPanel);
        AppendTitle(_masterInteractionPanel);
        AppendRequirementsList(_masterInteractionPanel);
    }

    private void AppendRequirementsList(UIElement parentPanel)
    {
        _requirementsFrame = new RequirementsFrame();
        parentPanel.Append(_requirementsFrame);
    }

    private void AppendTitle(UIElement parentPanel)
    {
        parentPanel.Append(Title);
    }

    private void AppendTextBoxes(UIElement parentPanel)
    {
        foreach (TextBox textBox in TextBoxes)
        {
            textBox.OnMouseOver += GlobalOnMouseOver;
            textBox.OnMouseOut += GlobalOnMouseOut;
            textBox.OnClick += ArenaTexBoxOnClick;
            parentPanel.Append(textBox);
        }
    }

    private void AppendCheckBoxes(UIElement parentPanel)
    {
        foreach (CheckBox checkBox in CheckBoxes)
        {
            parentPanel.Append(checkBox);
        }
    }

    private void GlobalOnMouseOut(UIMouseEvent evt, UIElement listeningelement)
    {
        if (listeningelement == _textBox1)
        {
            _textBox1.Hovering = false;
        }
        else if (listeningelement == _textBox2)
        {
            _textBox2.Hovering = false;
        }
        else if (listeningelement == _textBox3)
        {
            _textBox3.Hovering = false;
        }
        else if (listeningelement == _textBox4)
        {
            _textBox4.Hovering = false;
        }
        else if (listeningelement == _textBox5)
        {
            _textBox5.Hovering = false;
        }
        else if (listeningelement == _textBox6)
        {
            _textBox6.Hovering = false;
        }
    }

    void AssembleRequirements(StructuralBottle item)
    {
        TileInfo[] tiles = item.AreaCopied;

        ushort[] distinctTiles = tiles.Select(x => x.TileType).Distinct().ToArray();
        ushort[] distinctWalls = tiles.Select(x => x.WallType).Distinct().ToArray();

        Dictionary<ushort, int> requiredTiles = distinctTiles.ToDictionary(type => type, _ => 0);
        Dictionary<ushort, int> requiredWalls = distinctWalls.ToDictionary(type => type, _ => 0);
        Main.NewText($"Tile types: {requiredTiles.Keys.Count} Wall types: {requiredWalls.Keys.Count}");
        foreach (TileInfo tile in tiles)
        {
            ushort tileType = tile.TileType;
            
            if (tileType != ushort.MaxValue)
            {
                requiredTiles[tileType]++;
            }

            ushort wallType = tile.WallType;
            
            if (wallType != ushort.MaxValue)
            {
                requiredWalls[wallType]++;
            }
        }
    }

    private void GlobalOnMouseOver(UIMouseEvent evt, UIElement listeningelement)
    {
        if (listeningelement == _textBox1)
        {
            _textBox1.Hovering = true;
            return;
        }
        if (listeningelement == _textBox2)
        {
            _textBox2.Hovering = true;
            return;
        }
        if (listeningelement == _textBox3)
        {
            _textBox3.Hovering = true;
            return;
        }
        if (listeningelement == _textBox4)
        {
            _textBox4.Hovering = true;
            return;
        }
        if (listeningelement == _textBox5)
        {
            _textBox5.Hovering = true;
            return;
        }
        if (listeningelement == _textBox6)
        {
            _textBox6.Hovering = true;
            return;
        }
    }
    
    private void ArenaTexBoxOnClick(UIMouseEvent evt, UIElement listeningElement)
    {
        foreach (TextBox textBox in TextBoxes)
        {
            textBox.Inverse = false;
            textBox.Timer = 0;
        }
        
        if(!ArenaPlayer.ItemLockedIn)
            SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void Bookmarks(UIPanel parentPanel)
    {
        _arenaBookmark = new ArenaBookmark();
        parentPanel.Append(_arenaBookmark);
        
        _poolBookmark = new PoolBookmark();
        parentPanel.Append(_poolBookmark);
        
        _hellBookmark = new HellBookmark();
        parentPanel.Append(_hellBookmark);
        
        _customBookmark = new CustomBookmark();
        parentPanel.Append(_customBookmark);
    }
    
    private void GalaxySlot(UIElement mainPanel)
    {
        _gs = new GalaxySlot();
        _gs.Width.Set(80, 0);
        _gs.Height.Set(80, 0);
        _gs.VAlign = 0.92f;
        _gs.HAlign = 0.94f;
        mainPanel.Append(_gs);

        _gsEffects = new GalaxySlotEffects();
        _gsEffects.Width.Set(500, 0);
        _gsEffects.Height.Set(500, 0);
        _gsEffects.VAlign = HAlign = 0.5f;
        _gs.Append(_gsEffects);
    }

    private void MasterInteractionPanelOnMouseUp(UIMouseEvent evt, UIElement listeningelement)
    {
        Vector2 end = evt.MousePosition;
        _dragging = false;

        _masterPanel.Left.Set(end.X - _masterPanelOffset.X, 0f);
        _masterPanel.Top.Set(end.Y - _masterPanelOffset.Y, 0f);
    }

    private void MasterInteractionPanelOnMouseDown(UIMouseEvent evt, UIElement listeningelement)
    {
        _masterPanelOffset = new Vector2(evt.MousePosition.X - _masterPanel.Left.Pixels, evt.MousePosition.Y - _masterPanel.Top.Pixels);
        _dragging = true;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        if (_dragging)
        {
            _masterPanel.Left.Set(Main.mouseX - _masterPanelOffset.X, 0f);
            _masterPanel.Top.Set(Main.mouseY - _masterPanelOffset.Y, 0f);
        }

        if (PlayerInput.WritingText)
        {
            ScrollFocus();
        }
        
        _gs.OverflowHidden = true;
    }

    private void ScrollFocus()
    {
        if (!Helpers.KeyTyped(Keys.Tab)) return;
        
        int focusedIndex = 0;
        
        for (int i = 0; i < TextBoxes.Count; i++)
        {
            if (!TextBoxes[i].Focused) continue;
            focusedIndex = i;
            break;
        }
                
        foreach (TextBox textBox in TextBoxes)
        {
            textBox.Focused = false;
        }
                
        if (focusedIndex < TextBoxes.Count - 1)
        {
            TextBoxes[focusedIndex + 1].Focused = true;
        }
        else
        {
            TextBoxes[0].Focused = true;
        }
    }
}