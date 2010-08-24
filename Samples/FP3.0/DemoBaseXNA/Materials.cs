using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA
{
    /**
    * Helper class for setting consistent material properties on physics objects. The values
    * are inspired by real values.
    *
    * Original source: http://box2d.org/forum/viewtopic.php?f=3&t=3333
    * 
    * Note: Density (in kg/m3) was divided by 1000 to group them around the unofficial default
    * Iron = 7874 kg/m3
    * Mild steel = 7850 kg/m3
    * Granite = 2750 kg/m3
    * Concrete = 2400 kg/m3
    * Dry Sand = 1780 kg/m3
    * Pine = 530 kg/m3
    * Average Glass = 2500 kg/m3
    * Manufactured Rubber = 1500 kg/m3
    * Ice = 920 kg/m3
    * Pumice = 250 kg/m3
    * Expanded version for Styrofoam density = 100 kg/m3
    * Cotton = 30 kg/m3 (actually 5-30)
    * Commercial low-density Sponge = 18 kg/m3
    * Air = 1.204 kg/m3 (@ 20 C at sea level)
    * Helium = 0.1786 kg/m3.
    */

    public class Material
    {
        //                                           Density  Friction  COR   Grab   Crush  Explode  Color
        public static Material Default = new Material(1.00f, 0.30f, 0.1f, false, true, true, Color.Gray);

        public static Material Metal = new Material(7.85f, 0.20f, 0.2f, false, false, false, Color.LightGray);
                               // Heavy, inert.

        public static Material Stone = new Material(2.40f, 0.50f, 0.1f, false, false, false, Color.DarkGray);
                               // Heavy, inert.

        public static Material Wood = new Material(0.53f, 0.40f, 0.15f, false, true, false, new Color(150, 98, 0));
                               // Medium weight, mostly inert.

        public static Material Glass = new Material(2.50f, 0.10f, 0.2f, false, true, true, new Color(0, 0, 220, 128));
                               // Heavy, transparent.

        public static Material Rubber = new Material(1.50f, 0.80f, 0.4f, false, false, false, new Color(20, 20, 20));
                               // Medium weight, inert, bouncy.

        public static Material Ice = new Material(0.92f, 0.01f, 0.1f, false, true, true, new Color(0, 146, 220, 200));
                               // Medium weight, slippery surface.

        public static Material Pumice = new Material(0.25f, 0.60f, 0.0f, false, true, true, Color.White);
                               // Light, fragile.

        public static Material Polystyrene = new Material(0.10f, 0.60f, 0.05f, false, true, true, Color.White);
                               // Light, fragile.

        public static Material Fabric = new Material(0.03f, 0.60f, 0.1f, true, true, true, Color.Pink);
                               // Medium weight, grabbable.

        public static Material Sponge = new Material(0.018f, 0.90f, 0.05f, true, true, true, Color.Yellow);
                               // Light, fragile, grabbable.

        public static Material Air = new Material(0.001f, 0.90f, 0.0f, true, true, true, new Color(142, 171, 255, 128));
                               // No gravity?

        public static Material Helium = new Material(0.0001f, 0.9f, 0.0f, true, true, true,
                                                     new Color(142, 171, 255, 128)); // Negative gravity?

        public static Material Player = new Material(1.00f, 0f, 0.0f, false, true, true, Color.Yellow);

        public Material(float density, float friction, float restitution, bool isGrabbable,
                        bool isCrushable, bool isExplodable, Color color)
        {
            Density = density;
            Friction = friction;
            Restitution = restitution;
            IsGrabbable = isGrabbable;
            IsCrushable = isCrushable;
            IsExplodable = isExplodable;
            Color = color;
        }

        //Measure of mass in kg/m^3. Used to calculate the mass of a body.
        public float Density { get; private set; }

        //Measure of how easily a shape slides across a surface. Typically between 0 and 1. 0 means no friction.
        public float Friction { get; private set; }

        //Coefficient of Restitution (COR) or how much velocity a shape retains when colliding with another
        //(i.e. bounciness). COR is a ratio represented by a value between 0 and 1. 1 being more bouncy than 0.
        public float Restitution { get; private set; }

        //Game specific. Whether player can grab and hold onto the shape.
        public bool IsGrabbable { get; private set; }

        //Game specific. Whether a shape is crushed when a strong force is applied to it.
        public bool IsCrushable { get; private set; }

        //Game specific. Whether the material is affected by explosions (deforms the shape).
        public bool IsExplodable { get; private set; }

        // Rendering engine specific. Color used to represent the shape when displaying.
        public Color Color { get; private set; }
    }
}