using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArenaInABottle;

public class Keybinds : ModSystem
{
    public static ModKeybind OpenBuilderUiKeybind { get; private set; }
    
    public static ModKeybind StructuralCapsuleReversal { get; private set; }

    public override void Load()
    {
        OpenBuilderUiKeybind = KeybindLoader.RegisterKeybind(Mod, "BuilderUi", Keys.N);
        StructuralCapsuleReversal = KeybindLoader.RegisterKeybind(Mod, "CapsuleReversal", Keys.M);
    }
    
    public override void Unload()
    {
        OpenBuilderUiKeybind = null;
        StructuralCapsuleReversal = null;
    }
}