using System.Diagnostics;
using ArenaInABottle.Content.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
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

    public bool ReverseCapsulePlacement;

    public bool CreativeMode;

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (Keybinds.OpenBuilderUiKeybind.JustPressed)
        {
            ArenaModSystem.Instance.ToggleStatsUi();
            
        }

        if (Keybinds.StructuralCapsuleReversal.JustPressed &&
            Player.HeldItem.type == ModContent.ItemType<StructuralBottle>())
        {
            ReverseCapsulePlacement = true;
        }
    }

    public override void PreUpdate()
    {
        if (!Main.dayTime)
        {
            Main.dayTime = true;
        }

        Main.time = 50000;
        
        
        Player.delayUseItem = IsUiOpen;
        base.PreUpdate();
    }
}