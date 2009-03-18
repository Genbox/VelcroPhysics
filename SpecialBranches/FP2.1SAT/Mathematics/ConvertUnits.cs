#if (XNA)
using Microsoft.Xna.Framework;
#endif

namespace FarseerGames.FarseerPhysics.Mathematics
{
    /// <summary>
    /// Convert units between display and simulation units.
    /// </summary>
    public static class ConvertUnits
    {
        private static float _displayUnitsToSimUnitsRatio = 50;
        private static float _simUnitsToDisplayUnitsRatio = 1/_displayUnitsToSimUnitsRatio;

        public static void SetDisplayUnitToSimUnitRatio(float displayUnitsPerSimUnit)
        {
            _displayUnitsToSimUnitsRatio = displayUnitsPerSimUnit;
            _simUnitsToDisplayUnitsRatio = 1/displayUnitsPerSimUnit;
        }

        public static float ToDisplayUnits(float simUnits)
        {
            return simUnits*_displayUnitsToSimUnitsRatio;
        }

        public static float ToSimUnits(float displayUnits)
        {
            return displayUnits*_simUnitsToDisplayUnitsRatio;
        }

        public static float ToSimUnits(double displayUnits)
        {
            return (float) displayUnits*_simUnitsToDisplayUnitsRatio;
        }

        public static float ToDisplayUnits(int simUnits)
        {
            return simUnits*_displayUnitsToSimUnitsRatio;
        }

        public static float ToSimUnits(int displayUnits)
        {
            return displayUnits*_simUnitsToDisplayUnitsRatio;
        }

        public static Vector2 ToDisplayUnits(Vector2 simUnits)
        {
            return _displayUnitsToSimUnitsRatio*simUnits;
        }

        public static void ToDisplayUnits(ref Vector2 simUnits, out Vector2 displayUnits)
        {
            Vector2.Multiply(ref simUnits, _displayUnitsToSimUnitsRatio, out displayUnits);
        }

        public static Vector2 ToDisplayUnits(float x, float y)
        {
            return _displayUnitsToSimUnitsRatio*new Vector2(x, y);
        }

        public static void ToDisplayUnits(float x, float y, out Vector2 displayUnits)
        {
            displayUnits = Vector2.Zero;
            displayUnits.X = x*_displayUnitsToSimUnitsRatio;
            displayUnits.Y = y*_displayUnitsToSimUnitsRatio;
        }

        public static Vector2 ToSimUnits(Vector2 displayUnits)
        {
            return _simUnitsToDisplayUnitsRatio*displayUnits;
        }

        public static void ToSimUnits(ref Vector2 displayUnits, out Vector2 simUnits)
        {
            Vector2.Multiply(ref displayUnits, _simUnitsToDisplayUnitsRatio, out simUnits);
        }

        public static Vector2 ToSimUnits(float x, float y)
        {
            return _simUnitsToDisplayUnitsRatio*new Vector2(x, y);
        }

        public static Vector2 ToSimUnits(double x, double y)
        {
            return _simUnitsToDisplayUnitsRatio*new Vector2((float) x, (float) y);
        }

        public static void ToSimUnits(float x, float y, out Vector2 simUnits)
        {
            simUnits = Vector2.Zero;
            simUnits.X = x*_simUnitsToDisplayUnitsRatio;
            simUnits.Y = y*_simUnitsToDisplayUnitsRatio;
        }
    }
}