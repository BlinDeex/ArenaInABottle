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

public sealed class HellBookmark : BookmarkBase
{
    public HellBookmark()
    {
        Width.Set(260, 0);
        Height.Set(32, 0);
        VAlign = 0.5f;
        HAlign = 0.5f;
        Left.Set(14, 0);
        Top.Set(-75, 0);
        BookmarkName = "Hellevator";
        BookmarkTitle = "Hellevator Builder";
        TextBoxesNeeded = new[]{ (0, "Width")};
        CheckBoxesNeeded = new[] { (0, "Place rope"), (1, "Water at the end")};
        DrawColor = new Color(0.9f, 0.45f, 0.1f);
    }

    protected override string BookmarkName { get; set; }
    protected override string BookmarkTitle { get; set; }
    protected override (int, string)[] TextBoxesNeeded { get; set; }
    protected override (int, string)[] CheckBoxesNeeded { get; set; }
    protected override Color DrawColor { get; set; }

}