using System.Collections.Generic;
using System.Linq;
using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArenaInABottle.Content.Items;

public class StructuralBottle : ModItem
{
    private const int Resolution = 16;
    private int _arrayLength;
    private bool _reversed;
    private int _width = -1;


    public TileInfo[] AreaCopied;
    public int Height;
    public List<string> OriginalMods;
    private static ArenaPlayer ArenaPlayer => Main.LocalPlayer.GetModPlayer<ArenaPlayer>();
    
    private readonly Texture2D _dot = ModContent
        .Request<Texture2D>("ArenaInABottle/Content/Projectiles/Dot", AssetRequestMode.ImmediateLoad).Value;

    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("Structural Capsule");
        Tooltip.SetDefault("");
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.maxStack = 99;
        Item.rare = ItemRarityID.Master;
        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item81;
        Item.consumable = true;
    }

    public override bool OnPickup(Player player)
    {
        if (AreaCopied == null)
        {
            Item.TurnToAir();
            Main.NewText("AreaCopied was null! Item destroyed");
            return true;
        }

        // ReSharper disable once PossibleLossOfFraction
        _width = AreaCopied.Length / Height;

        return true;
    }

    public override void HoldItem(Player player)
    {
        if (ArenaPlayer.ReverseCapsulePlacement)
        {
            ArenaPlayer.ReverseCapsulePlacement = false;
            _reversed = !_reversed;

            for (var i = 0; i < AreaCopied.Length; i++) AreaCopied[i].TileX = _width - 1 - AreaCopied[i].TileX;
        }
        
        AssembleRequirements(AreaCopied);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor,
        Vector2 origin, float scale)
    {
        bool holdingBottle = ArenaPlayer.Player.HeldItem.type == ModContent.ItemType<StructuralBottle>();

        if (!holdingBottle)
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        
        
        
        Vector2 mousePos = Main.MouseScreen + Main.screenPosition;
        var x = Helpers.RoundDownToNearest(mousePos.X, 16);
        var y = Helpers.RoundDownToNearest(mousePos.Y, 16) - 2;
        var boxInfo = Lines(x, y);

        var iter = 0;
        var index = 0;
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            
        while (iter < boxInfo.Length)
        {
            //if (_index >= boxInfo.Length) _index = 0;
            Main.spriteBatch.Draw(_dot, boxInfo[index] - Main.screenPosition, !_reversed ? Color.Aqua : Color.Orange);
            //Dust.NewDust(boxInfo[_index], 0, 0, 278, 0, 0, 0, _reversed ? Color.Red : Color.Green, 0.4f);
            iter++;
            index++;
        }
            
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }


    private void AssembleRequirements(TileInfo[] item)
    {
        var distinctTiles = item.Select(x => x.TileType).Distinct().ToArray();
        var distinctWalls = item.Select(x => x.WallType).Distinct().ToArray();
        //Item[] samples = ContentSamples.ItemsByType.Select(kvp => kvp.Value).Where(item => item.createTile == tileType).ToArray();

        var requiredTiles = distinctTiles.Where(tileType => tileType != ushort.MaxValue)
            .ToDictionary(tileType => tileType, _ => 0);

        var requiredWalls = distinctWalls.Where(wallType => wallType != 0).ToDictionary(wallType => wallType, _ => 0);

        foreach (TileInfo tile in item)
        {
            var tileType = tile.TileType;

            if (tileType != ushort.MaxValue) requiredTiles[tileType]++;

            var wallType = tile.WallType;

            if (wallType != 0) requiredWalls[wallType]++;
        }
    }

    private static void DifferentLoadedMods(IEnumerable<string> newMods, IEnumerable<string> missingMods)
    {
        Main.NewText(
            "For safety loaded mods must be same as at the point of creation of this bottle! [c/FFD900:NEW MODS] and [c/FF2D00:MISSING MODS]",
            Color.Orange);
        foreach (var name in missingMods) Main.NewText(name, new Color(255, 45, 0));

        foreach (var name in newMods) Main.NewText(name, new Color(255, 217, 0));
    }

    public override bool CanStack(Item item2)
    {
        return ShouldStack(item2);
    }

    

    public override bool CanStackInWorld(Item item2)
    {
        return ShouldStack(item2);
    }
    
    private bool ShouldStack(Item item2)
    {
        StructuralBottle bottle = (StructuralBottle)item2.ModItem;

        if (bottle.AreaCopied.Length != AreaCopied.Length || bottle._reversed != _reversed) return false;

        var same = true;

        for (var i = 0; i < bottle.AreaCopied.Length; i++)
        {
            if (AreaCopied[i].TileType == bottle.AreaCopied[i].TileType) continue;
            same = false;
            break;
        }

        return same;
    }

    public override void LoadData(TagCompound tag)
    {
        _arrayLength = tag.GetInt("arrayLength");
        Height = tag.GetInt("Height");
        _reversed = tag.GetBool("reversed");
        OriginalMods = tag.Get<List<string>>("loadedMods");
        
        AreaCopied = new TileInfo[_arrayLength];
        
        _width = _arrayLength / Height;
        AreaCopied = new TileInfo[_arrayLength];
        for (var i = 0; i < _arrayLength; i++) AreaCopied[i] = LoadEntry(tag, i);
    }

    public override void SaveData(TagCompound tag)
    {
        tag.Add("arrayLength", AreaCopied.Length);
        tag.Add("Height", Height);
        tag.Add("reversed", _reversed);
        tag["loadedMods"] = OriginalMods;
        
        for (var i = 0; i < AreaCopied.Length; i++)
        {
            TileInfo entry = AreaCopied[i];
            SaveEntry(tag, entry.TileType, entry.TileX, entry.TileY, entry.WallType, entry.LiquidType,
                entry.LiquidAmount, entry.IsSpecialTile, entry.OriginTile, entry.Style, i);
        }
    }

    private static void SaveEntry(TagCompound tag, int tileType, int tileX, int tileY, int wallType, int liquidType,
        byte liquidAmount, bool specialTile, bool originTile, int style, int entryIndex)
    {
        tag.Add($"t{entryIndex}", tileType);
        tag.Add($"x{entryIndex}", tileX);
        tag.Add($"y{entryIndex}", tileY);
        tag.Add($"w{entryIndex}", wallType);
        tag.Add($"l{entryIndex}", liquidType);
        tag.Add($"a{entryIndex}", liquidAmount);
        tag.Add($"s{entryIndex}", specialTile);
        tag.Add($"o{entryIndex}", originTile);
        tag.Add($"st{entryIndex}", style);
    }

    private static TileInfo LoadEntry(TagCompound tag, int entryIndex)
    {
        TileInfo tile = new()
        {
            TileType = (ushort)tag.GetInt($"t{entryIndex}"),
            TileX = tag.GetInt($"x{entryIndex}"),
            TileY = tag.GetInt($"y{entryIndex}"),
            WallType = (ushort)tag.GetInt($"w{entryIndex}"),
            LiquidType = tag.GetInt($"l{entryIndex}"),
            LiquidAmount = tag.GetByte($"a{entryIndex}"),
            IsSpecialTile = tag.GetBool($"s{entryIndex}"),
            OriginTile = tag.GetBool($"o{entryIndex}"),
            Style = tag.GetInt($"st{entryIndex}")
        };

        return tile;
    }


    public override bool? UseItem(Player player)
    {
        var x = Main.MouseWorld.ToTileCoordinates().X;
        var y = Main.MouseWorld.ToTileCoordinates().Y;
        var loopIndex = 0;
        TileInfo[] specialTiles = AreaCopied.Where(tileInfo => tileInfo is { IsSpecialTile: true, OriginTile: true }).ToArray();
        
        while (loopIndex < AreaCopied.Length)
        {
            var xOffset = x + AreaCopied[loopIndex].TileX;
            var yOffset = y + AreaCopied[loopIndex].TileY - Height;


            WorldGen.KillWall(xOffset, yOffset);
            WorldGen.KillTile(xOffset, yOffset, false, false, true);
            WorldGen.EmptyLiquid(xOffset, yOffset);

            if (AreaCopied[loopIndex].TileType != ushort.MaxValue && !AreaCopied[loopIndex].IsSpecialTile)
            {
                WorldGen.PlaceTile(xOffset, yOffset, AreaCopied[loopIndex].TileType);
            }
                

            var wallType = AreaCopied[loopIndex].WallType;

            if (wallType != 0) WorldGen.PlaceWall(xOffset, yOffset, wallType);

            (int, byte) currentLiquid = (AreaCopied[loopIndex].LiquidType, AreaCopied[loopIndex].LiquidAmount);

            WorldGen.PlaceLiquid(xOffset, yOffset, (byte)currentLiquid.Item1, currentLiquid.Item2);
            
            loopIndex++;
        }
        
        foreach (TileInfo tileInfo in specialTiles)
        {
            //Main.NewText(tileInfo.TileType + " " + tileInfo.Style);
            
            WorldGen.PlaceTile(x + tileInfo.TileX, y + tileInfo.TileY - Height, tileInfo.TileType, true, false, -1, tileInfo.Style);
        }
        
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        var newMods = (from mod in ModLoader.Mods where !OriginalMods.Contains(mod.Name) select mod.Name).ToArray();
        var missingMods = OriginalMods.Where(name => !ModLoader.Mods.Select(x => x.Name).Contains(name)).ToArray();

        if (newMods.Length <= 0 && missingMods.Length <= 0) return true;

        DifferentLoadedMods(newMods, missingMods);
        return false;
    }

    private Vector2[] Lines(int x, int y)
    {
        var bottomPositions = BottomLine(x, y);
        var topPositions = TopLine(x, y);
        var leftPositions = LeftLine(x, y);
        var rightPositions = RightLine(x, y);
        var fullArray = bottomPositions.Concat(rightPositions).Concat(topPositions.Reverse())
            .Concat(leftPositions.Reverse())
            .ToArray();

        if (_reversed) fullArray = fullArray.Reverse().ToArray();

        return fullArray;
    }

    private IEnumerable<Vector2> BottomLine(int x, int y)
    {
        var positions = new Vector2[_width * Resolution];

        for (var i = 0; i < _width * Resolution; i++)
            positions[i] = new Vector2(x + i * (16 / Resolution), y);

        return positions;
    }

    private IEnumerable<Vector2> TopLine(int x, int y)
    {
        var positions = new Vector2[_width * Resolution];

        for (var i = 0; i < _width * Resolution; i++)
            positions[i] = new Vector2(x + i * (16 / Resolution), y - Height * 16);

        return positions;
    }

    private IEnumerable<Vector2> LeftLine(int x, int y)
    {
        var positions = new Vector2[Height * Resolution];

        for (var i = 0; i < Height * Resolution; i++)
            positions[i] = new Vector2(x, y - i * (16 / Resolution));

        return positions;
    }

    private IEnumerable<Vector2> RightLine(int x, int y)
    {
        var positions = new Vector2[Height * Resolution];

        for (var i = 0; i < Height * Resolution; i++)
            positions[i] = new Vector2(x + _width * 16, y - i * (16 / Resolution));

        return positions;
    }
}