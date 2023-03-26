using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArenaInABottle;

public class ArenaPlayer : ModPlayer
{

    #region choosePhase

    public Color UiColor = new(0, 115, 230);
    
    
    public string TextBox0 = "", TextBox1 = "", TextBox2 = "", TextBox3 = "", TextBox4 = "", TextBox5 = "";
    public readonly string[] TextBoxesExplained = { "", "", "", "", "", "" };

    public bool CheckBox1, CheckBox2, CheckBox3, CheckBox4, CheckBox5, CheckBox6;
    public readonly string[] CheckBoxesExplained = { "", "", "", "", "", "" };
    public string CurrentTitle = "Arena Builder";
    public bool ItemLockedIn = false;
    public bool BlueprintPlaced { get; set; } = false;

    #endregion
    
    public Item ItemInSlot = new(ItemID.None);
    public bool IsUiOpen = false;


    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (Keybinds.OpenBuilderUiKeybind.JustPressed)
        {
            ArenaModSystem.Instance.ToggleStatsUi();
            
        }
    }

    public override void PreUpdate()
    {
        if (!Main.dayTime)
        {
            Main.dayTime = true;
        }
        
        Player.delayUseItem = IsUiOpen;
        base.PreUpdate();
    }
}