using Terraria;
using Terraria.ModLoader;

namespace ArenaInABottle.Content.MyDust;

public class NeonDust : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.velocity *= 0.05f; // Multiply the dust's start velocity by 0.4, slowing it down
        dust.noGravity = true; // Makes the dust have no gravity.
        dust.noLight = true; // Makes the dust emit no light.
        dust.scale *= 1.5f; // Multiplies the dust's initial scale by 1.5.
        dust.alpha = 100;
    }

    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.15f;
        dust.scale *= 0.95f;

        float light = 1f * dust.scale;

        Lighting.AddLight(dust.position, light, light, light);

        if (dust.scale < 0.1f)
        {
            dust.active = false;
        }
        return false;
    }
}