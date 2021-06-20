using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input
{
    public class MouseManager
    {
        public MouseState OldState;
        public MouseState NewState;

        public int DeltaScrollValue => NewState.ScrollWheelValue - OldState.ScrollWheelValue;
        public Vector2 DeltaPosition => new Vector2(NewState.X - OldState.X, NewState.Y - OldState.Y);

        public Vector2 NewPosition => new Vector2(NewState.X, NewState.Y);
        public Vector2 OldPosition => new Vector2(OldState.X, OldState.Y);

        public bool IsNewButtonClick(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return OldState.LeftButton == ButtonState.Released && NewState.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return OldState.RightButton == ButtonState.Released && NewState.RightButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return OldState.MiddleButton == ButtonState.Released && NewState.MiddleButton == ButtonState.Pressed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        public bool IsNewButtonRelease(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return OldState.LeftButton == ButtonState.Pressed && NewState.LeftButton == ButtonState.Released;
                case MouseButton.Right:
                    return OldState.RightButton == ButtonState.Pressed && NewState.RightButton == ButtonState.Released;
                case MouseButton.Middle:
                    return OldState.MiddleButton == ButtonState.Pressed && NewState.MiddleButton == ButtonState.Released;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        public bool IsButtonDown(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return NewState.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return NewState.RightButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return NewState.MiddleButton == ButtonState.Pressed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        public void Update()
        {
            OldState = NewState;
            NewState = Mouse.GetState();
        }
    }
}