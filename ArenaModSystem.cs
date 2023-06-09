using System.Collections.Generic;
using ArenaInABottle.Content.UI_Elements;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle;

public class ArenaModSystem : ModSystem
{
    private UserInterface _builderInterface;
    private BuilderUi _builderUi;
    public static ArenaModSystem Instance => ModContent.GetInstance<ArenaModSystem>();
    private GameTime _lastGameTime;
    

    public override void Load()
    {
        if (!Main.dedServ)
        {
            _builderInterface = new UserInterface();

            _builderUi = new BuilderUi();
            _builderUi.Activate();
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        _lastGameTime = gameTime;
        if (_builderInterface?.CurrentState != null)
        {
            _builderInterface.Update(gameTime);
        }

        base.UpdateUI(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
            "TerrariaRPG: Stats",
            delegate
            {
                if (_lastGameTime != null && _builderInterface?.CurrentState != null)
                {
                    _builderInterface.Draw(Main.spriteBatch, _lastGameTime);
                }
                return true;
            },
            InterfaceScaleType.UI));
        base.ModifyInterfaceLayers(layers);
    }
    
    internal void ToggleStatsUi()
    {
        if(_builderInterface.CurrentState == null)
        {
            //rPGPlayer.UIOpen = true; //TODO
            ModContent.GetInstance<ArenaPlayer>().IsUiOpen = true;
            _builderInterface?.SetState(_builderUi);
            
        }
        else
        {
            //rPGPlayer.UIOpen = false;
            ModContent.GetInstance<ArenaPlayer>().IsUiOpen = false;
            _builderInterface?.SetState(null);
        }
    }
}