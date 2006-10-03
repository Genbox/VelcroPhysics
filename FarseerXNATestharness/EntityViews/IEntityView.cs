using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerXNATestharness.EntityViews {
    public interface IEntityView {
        void Update(Vector2 position, float orientation);
        void Draw();
    }
}
