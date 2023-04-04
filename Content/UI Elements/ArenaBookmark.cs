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

public sealed class ArenaBookmark : BookmarkBase
{
    protected override string BookmarkName { get; set; }
    protected override string BookmarkTitle { get; set; }
    protected override (int, string)[] TextBoxesNeeded { get; set; }
    protected override (int, string)[] CheckBoxesNeeded { get; set; }
    protected override Color DrawColor { get; set; }

    public ArenaBookmark()
    {
        Width.Set(260, 0);
        Height.Set(32, 0);
        VAlign = 0.5f;
        HAlign = 0.5f;
        Left.Set(14, 0);
        Top.Set(-159, 0);
        BookmarkName = "Arena";
        BookmarkTitle = "Arena Builder";
        TextBoxesNeeded = new[]{ (0, "Width"),(1,"Height"),(2,"Floors"),(3,"Solid block spacing")};
        CheckBoxesNeeded = new[] { (0, "Place campfires"), (1, "Place heart lanterns"), (2, "Clear area") };
        DrawColor = new Color(0, 0.45f, 0.9f);
    }
}