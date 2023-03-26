using System;
using System.Collections.Generic;
using System.Linq;
using ArenaInABottle.Content.Misc;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArenaInABottle.Content.Items;

public class StructuralCapsule : ModItem
{
    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("Structural Capsule");
        Tooltip.SetDefault("");
    }
    
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Pink;
        Item.useAnimation = 50;
        Item.useTime = 1;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item81;
        Item.consumable = true;
    }

    public (ushort, int, int, ushort,(int,byte))[] TilesCopiedXy;
    public int Height;
    private int _width = -1;
    private const int DustCountMultiplier = 4;
    private int _highlightSpeed;
    private int _index;
    private int _arrayLength;

    public override bool OnPickup(Player player)
    {
        _highlightSpeed = (int)(5 + TilesCopiedXy.GetLength(0) * 0.01f);
        _width = TilesCopiedXy.GetLength(0) / Height;
        return true;
    }

    public override void HoldItem(Player player)
    {
        Vector2 mousePos = Main.MouseScreen + Main.screenPosition;
        int x = Helpers.RoundDownToNearest(mousePos.X, 16);
        int y = Helpers.RoundDownToNearest(mousePos.Y, 16) - 2;
        Vector2[] boxInfo = Lines(x, y);
        
        int dustsSpawned = 0;

        while (dustsSpawned < _highlightSpeed)
        {
            if (_index >= boxInfo.Length)
            {
                _index = 0;
                break;
            }
            Dust.NewDust(boxInfo[_index], 0, 0, 278, 0, 0, 0, Color.White, 0.4f);
            dustsSpawned++;
            _index++;
        }
    }

    public override void LoadData(TagCompound tag)
    {
        _arrayLength = tag.GetInt("arrayLength");
        Height = tag.GetInt("Height");
        _highlightSpeed = (int)(5 + _arrayLength * 0.01f);
        _width = _arrayLength / Height;
        TilesCopiedXy = new (ushort, int, int, ushort, (int, byte))[_arrayLength];
        for (int i = 0; i < _arrayLength; i++)
        {
            TilesCopiedXy[i] = LoadEntry(tag, i);
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag.Add("arrayLength",TilesCopiedXy.Length);
        tag.Add("Height",Height);
        
        for (int i = 0; i < TilesCopiedXy.Length; i++)
        {
            (ushort, int, int, ushort, (int, byte)) entry = TilesCopiedXy[i];
            SaveEntry(tag,entry.Item1,entry.Item2,entry.Item3,entry.Item4,entry.Item5.Item1,entry.Item5.Item2,i);
        }
    }
    private static void SaveEntry(TagCompound tag, int tileType, int tileX, int tileY, int wallType, int liquidType, byte liquidAmount, int entryIndex)
    {
        tag.Add($"t{entryIndex}", tileType);
        tag.Add($"x{entryIndex}", tileX);
        tag.Add($"y{entryIndex}", tileY);
        tag.Add($"w{entryIndex}", wallType);
        tag.Add($"l{entryIndex}", liquidType);
        tag.Add($"a{entryIndex}", liquidAmount);
    }

    private static (ushort, int, int, ushort, (int, byte)) LoadEntry(TagCompound tag, int entryIndex)
    {
        int tileType = tag.GetInt($"t{entryIndex}");
        int tileX = tag.GetInt($"x{entryIndex}");
        int tileY = tag.GetInt($"y{entryIndex}");
        int wallType = tag.GetInt($"w{entryIndex}");
        int liquidType = tag.GetInt($"l{entryIndex}");
        byte liquidAmount = tag.GetByte($"a{entryIndex}");

        return ((ushort)tileType, tileX, tileY, (ushort)wallType, ((ushort)liquidType, liquidAmount));
    }


    public override bool? UseItem(Player player)
    {
        int x = Main.MouseWorld.ToTileCoordinates().X;
        int y = Main.MouseWorld.ToTileCoordinates().Y;
        int loopIndex = 0;

        while (loopIndex < TilesCopiedXy.Length)
        {
            int xOffset = x + TilesCopiedXy[loopIndex].Item2;
            int yOffset = y + TilesCopiedXy[loopIndex].Item3 - Height;
            
            
            WorldGen.KillWall(xOffset,yOffset);
            WorldGen.KillTile(xOffset, yOffset,false,false,true);
            WorldGen.EmptyLiquid(xOffset, yOffset);
            
            if (TilesCopiedXy[loopIndex].Item1 != ushort.MaxValue)
            {
                WorldGen.PlaceTile(xOffset, yOffset, TilesCopiedXy[loopIndex].Item1);
            }
            
            ushort wallType = TilesCopiedXy[loopIndex].Item4;
            
            if (wallType != 0)
            {
                WorldGen.PlaceWall(xOffset,yOffset,wallType);
            }

            (int, byte) currentLiquid = TilesCopiedXy[loopIndex].Item5;
            
            WorldGen.PlaceLiquid(xOffset, yOffset, (byte)currentLiquid.Item1, currentLiquid.Item2);
            
            
            loopIndex++;
        }
        
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        return true;
    }
    
    private Vector2[] Lines(int x, int y)
    {
        IEnumerable<Vector2> bottomPositions = BottomLine(x, y);
        IEnumerable<Vector2> topPositions = TopLine(x, y);
        IEnumerable<Vector2> leftPositions = LeftLine(x, y);
        IEnumerable<Vector2> rightPositions = RightLine(x, y);
        Vector2[] fullArray = bottomPositions.Concat(rightPositions).Concat(topPositions.Reverse()).Concat(leftPositions.Reverse())
            .ToArray();
        
        return fullArray;
    }

    private IEnumerable<Vector2> BottomLine(int x, int y)
    {
        Vector2[] positions = new Vector2[_width * DustCountMultiplier];
        
        for (int i = 0; i < _width * DustCountMultiplier; i++)
        {
            positions[i] = new Vector2(x + i * (16 / DustCountMultiplier), y);
            
        }

        return positions;
    }
    
    private IEnumerable<Vector2> TopLine(int x, int y)
    {
        Vector2[] positions = new Vector2[_width * DustCountMultiplier];
        
        for (int i = 0; i < _width * DustCountMultiplier; i++)
        {
            positions[i] = new Vector2(x + i * (16 / DustCountMultiplier), y - Height * 16);
        }

        return positions;
    }
    
    private IEnumerable<Vector2> LeftLine(int x, int y)
    {
        Vector2[] positions = new Vector2[Height * DustCountMultiplier];
        
        for (int i = 0; i < Height * DustCountMultiplier; i++)
        {
            positions[i] = new Vector2(x, y - i * (16 / DustCountMultiplier));
        }

        return positions;
    }
    
    private IEnumerable<Vector2> RightLine(int x, int y)
    {
        Vector2[] positions = new Vector2[Height * DustCountMultiplier];
        
        for (int i = 0; i < Height * DustCountMultiplier; i++)
        {
            positions[i] = new Vector2(x + _width * 16, y - i * (16 / DustCountMultiplier));
        }

        return positions;
    }
}
