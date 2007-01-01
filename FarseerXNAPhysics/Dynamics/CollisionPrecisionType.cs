using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerGames.FarseerXNAPhysics.Dynamics {
    /// <summary>
    /// Determines how the collision grid is constructed.  
    /// <para>If 'Relative', the collision precision will be multiplied
    /// by the length of the shortest edge of the geometries AABB.  This value will then be used for the grid's cell size</para> 
    /// <para>If 'Absolute', the collision precision value will be used directly for the grid cell size.</para>
    /// </summary>
    public enum CollisionPrecisionType {
        Relative,
        Absolute
    }
}
