using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace FarseerGames.FarseerXNAGame.Components {
    public partial class KeyboardInputComponent : Microsoft.Xna.Framework.GameComponent {

        Keys[] _keysPressed;
        List<Keys> _keysDown = new List<Keys>();
        List<Keys> _keysUp = new List<Keys>();

        //A
        public event EventHandler<KeyEventArgs> AKeyDown;
        public event EventHandler<KeyEventArgs> AKeyUp;
        public event EventHandler<KeyEventArgs> AKeyPressed;
        //Add

        //Alt

        //B

        //Back

        //C

        //Cancel

        //CapsLock

        //Clear

        //Control

        //ControlKey

        //D
        public event EventHandler<KeyEventArgs> DKeyDown;
        public event EventHandler<KeyEventArgs> DKeyUp;
        public event EventHandler<KeyEventArgs> DKeyPressed;

        //D0

        //D1

        //D2

        //D3

        //D4

        //D5

        //D6

        //D7

        //D8

        //D9

        //Decimal

        //Delete

        //Divide

        //Down

        //E

        //End

        //Enter

        //Escape
        public event EventHandler<KeyEventArgs> EscapeKeyDown;
        public event EventHandler<KeyEventArgs> EscapeKeyUp;
        public event EventHandler<KeyEventArgs> EscapeKeyPressed;

        //Execute

        //F

        //F1

        //F10

        //F11

        //F12

        //F13

        //F14

        //F15

        //F16

        //F17

        //F18

        //F19

        //F2

        //F20

        //F21

        //F22

        //F23

        //F24

        //F3

        //F4

        //F5

        //F6

        //F7

        //F8

        //F9

        //G

        //H

        //Help

        //Home

        //I

        //Insert

        //J

        //K

        //L

        //LButton

        //LControlKey

        //LMenu

        //LShiftKey

        //LWin

        //Left
        public event EventHandler<KeyEventArgs> LeftKeyDown;
        public event EventHandler<KeyEventArgs> LeftKeyUp;
        public event EventHandler<KeyEventArgs> LeftKeyPressed;

        //LineFeed

        //M

        //MButton

        //Menu

        //Multiply

        //N

        //NumLock

        //NumPad0

        //NumPad1

        //NumPad2

        //NumPad3

        //NumPad4

        //NumPad5

        //NumPad6

        //NumPad7

        //NumPad8

        //NumPad9

        //O

        //P

        //PageDown

        //PageUp

        //Pause
        public event EventHandler<KeyEventArgs> PauseKeyDown;
        public event EventHandler<KeyEventArgs> PauseKeyUp;
        public event EventHandler<KeyEventArgs> PauseKeyPressed;

        //Play

        //Print

        //PrintScreen

        //Q

        //R

        //RButton

        //RControlKey

        //RMenu

        //RShiftKey

        //RWin

        //Right
        public event EventHandler<KeyEventArgs> RightKeyDown;
        public event EventHandler<KeyEventArgs> RightKeyUp;
        public event EventHandler<KeyEventArgs> RightKeyPressed;

        //S
        public event EventHandler<KeyEventArgs> SKeyDown;
        public event EventHandler<KeyEventArgs> SKeyUp;
        public event EventHandler<KeyEventArgs> SKeyPressed;

        //Separator

        //Shift

        //ShiftKey

        //Space

        //Subtract

        //T

        //Tab

        //U

        //Up

        //V

        //W
        public event EventHandler<KeyEventArgs> WKeyDown;
        public event EventHandler<KeyEventArgs> WKeyUp;
        public event EventHandler<KeyEventArgs> WKeyPressed;

        //X

        //Y

        //Z

        public KeyboardInputComponent() {
            InitializeComponent();
            _keysPressed = Keyboard.GetState().GetPressedKeys();
        }

        public override void Update() {
            base.Update();
            UpdateInput();
            RaiseKeyEvents();
        }

        private void RaiseKeyEvents() {
            _keysDown.ForEach(RaiseKeyDownEvent);
            _keysUp.ForEach(RaiseKeyUpEvent);
            for (int i = 0; i < _keysPressed.Length; i++) {
                RaisKeyPressedEvent(_keysPressed[i]);
            }

        }

        private void RaiseKeyDownEvent(Keys key) {
            switch (key) {
                case Keys.A:
                    if (AKeyDown != null) { AKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.Add:
                    break;
                case Keys.Alt:
                    break;
                case Keys.B:
                    break;
                case Keys.Back:
                    break;
                case Keys.C:
                    break;
                case Keys.Cancel:
                    break;
                case Keys.CapsLock:
                    break;
                case Keys.Clear:
                    break;
                case Keys.Control:
                    break;
                case Keys.ControlKey:
                    break;
                case Keys.D:
                    if (DKeyDown != null) { DKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.D0:
                    break;
                case Keys.D1:
                    break;
                case Keys.D2:
                    break;
                case Keys.D3:
                    break;
                case Keys.D4:
                    break;
                case Keys.D5:
                    break;
                case Keys.D6:
                    break;
                case Keys.D7:
                    break;
                case Keys.D8:
                    break;
                case Keys.D9:
                    break;
                case Keys.Decimal:
                    break;
                case Keys.Delete:
                    break;
                case Keys.Divide:
                    break;
                case Keys.Down:
                    break;
                case Keys.E:
                    break;
                case Keys.End:
                    break;
                case Keys.Enter:
                    break;
                case Keys.Escape:
                    if (EscapeKeyDown != null) { EscapeKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.Execute:
                    break;
                case Keys.F:
                    break;
                case Keys.F1:
                    break;
                case Keys.F10:
                    break;
                case Keys.F11:
                    break;
                case Keys.F12:
                    break;
                case Keys.F13:
                    break;
                case Keys.F14:
                    break;
                case Keys.F15:
                    break;
                case Keys.F16:
                    break;
                case Keys.F17:
                    break;
                case Keys.F18:
                    break;
                case Keys.F19:
                    break;
                case Keys.F2:
                    break;
                case Keys.F20:
                    break;
                case Keys.F21:
                    break;
                case Keys.F22:
                    break;
                case Keys.F23:
                    break;
                case Keys.F24:
                    break;
                case Keys.F3:
                    break;
                case Keys.F4:
                    break;
                case Keys.F5:
                    break;
                case Keys.F6:
                    break;
                case Keys.F7:
                    break;
                case Keys.F8:
                    break;
                case Keys.F9:
                    break;
                case Keys.G:
                    break;
                case Keys.H:
                    break;
                case Keys.Help:
                    break;
                case Keys.Home:
                    break;
                case Keys.I:
                    break;
                case Keys.Insert:
                    break;
                case Keys.J:
                    break;
                case Keys.K:
                    break;
                case Keys.L:
                    break;
                case Keys.LButton:
                    break;
                case Keys.LControlKey:
                    break;
                case Keys.LMenu:
                    break;
                case Keys.LShiftKey:
                    break;
                case Keys.LWin:
                    break;
                case Keys.Left:
                    if (LeftKeyDown != null) { LeftKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.LineFeed:
                    break;
                case Keys.M:
                    break;
                case Keys.MButton:
                    break;
                case Keys.Menu:
                    break;
                case Keys.Multiply:
                    break;
                case Keys.N:
                    break;
                case Keys.NumLock:
                    break;
                case Keys.NumPad0:
                    break;
                case Keys.NumPad1:
                    break;
                case Keys.NumPad2:
                    break;
                case Keys.NumPad3:
                    break;
                case Keys.NumPad4:
                    break;
                case Keys.NumPad5:
                    break;
                case Keys.NumPad6:
                    break;
                case Keys.NumPad7:
                    break;
                case Keys.NumPad8:
                    break;
                case Keys.NumPad9:
                    break;
                case Keys.O:
                    break;
                case Keys.P:
                    break;
                case Keys.PageDown:
                    break;
                case Keys.PageUp:
                    break;
                case Keys.Pause:
                    if (PauseKeyDown != null) { PauseKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.Play:
                    break;
                case Keys.Print:
                    break;
                case Keys.PrintScreen:
                    break;
                case Keys.Q:
                    break;
                case Keys.R:
                    break;
                case Keys.RButton:
                    break;
                case Keys.RControlKey:
                    break;
                case Keys.RMenu:
                    break;
                case Keys.RShiftKey:
                    break;
                case Keys.RWin:
                    break;
                case Keys.Right:
                    if (RightKeyDown != null) { RightKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.S:
                    if (SKeyDown != null) { SKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.Separator:
                    break;
                case Keys.Shift:
                    break;
                case Keys.ShiftKey:
                    break;
                case Keys.Space:
                    break;
                case Keys.Subtract:
                    break;
                case Keys.T:
                    break;
                case Keys.Tab:
                    break;
                case Keys.U:
                    break;
                case Keys.Up:
                    break;
                case Keys.V:
                    break;
                case Keys.W:
                    if (WKeyDown != null) { WKeyDown(this, new KeyEventArgs()); }
                    break;
                case Keys.X:
                    break;
                case Keys.Y:
                    break;
                case Keys.Z:
                    break;
                default:
                    break;
            }
        }

        private void RaiseKeyUpEvent(Keys key) {
            switch (key) {
                case Keys.A:
                    if (AKeyUp != null) { AKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.Add:
                    break;
                case Keys.Alt:
                    break;
                case Keys.B:
                    break;
                case Keys.Back:
                    break;
                case Keys.C:
                    break;
                case Keys.Cancel:
                    break;
                case Keys.CapsLock:
                    break;
                case Keys.Clear:
                    break;
                case Keys.Control:
                    break;
                case Keys.ControlKey:
                    break;
                case Keys.D:
                    if (DKeyUp != null) { DKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.D0:
                    break;
                case Keys.D1:
                    break;
                case Keys.D2:
                    break;
                case Keys.D3:
                    break;
                case Keys.D4:
                    break;
                case Keys.D5:
                    break;
                case Keys.D6:
                    break;
                case Keys.D7:
                    break;
                case Keys.D8:
                    break;
                case Keys.D9:
                    break;
                case Keys.Decimal:
                    break;
                case Keys.Delete:
                    break;
                case Keys.Divide:
                    break;
                case Keys.Down:
                    break;
                case Keys.E:
                    break;
                case Keys.End:
                    break;
                case Keys.Enter:
                    break;
                case Keys.Escape:
                    if (EscapeKeyUp != null) { EscapeKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.Execute:
                    break;
                case Keys.F:
                    break;
                case Keys.F1:
                    break;
                case Keys.F10:
                    break;
                case Keys.F11:
                    break;
                case Keys.F12:
                    break;
                case Keys.F13:
                    break;
                case Keys.F14:
                    break;
                case Keys.F15:
                    break;
                case Keys.F16:
                    break;
                case Keys.F17:
                    break;
                case Keys.F18:
                    break;
                case Keys.F19:
                    break;
                case Keys.F2:
                    break;
                case Keys.F20:
                    break;
                case Keys.F21:
                    break;
                case Keys.F22:
                    break;
                case Keys.F23:
                    break;
                case Keys.F24:
                    break;
                case Keys.F3:
                    break;
                case Keys.F4:
                    break;
                case Keys.F5:
                    break;
                case Keys.F6:
                    break;
                case Keys.F7:
                    break;
                case Keys.F8:
                    break;
                case Keys.F9:
                    break;
                case Keys.G:
                    break;
                case Keys.H:
                    break;
                case Keys.Help:
                    break;
                case Keys.Home:
                    break;
                case Keys.I:
                    break;
                case Keys.Insert:
                    break;
                case Keys.J:
                    break;
                case Keys.K:
                    break;
                case Keys.L:
                    break;
                case Keys.LButton:
                    break;
                case Keys.LControlKey:
                    break;
                case Keys.LMenu:
                    break;
                case Keys.LShiftKey:
                    break;
                case Keys.LWin:
                    break;
                case Keys.Left:
                    if (LeftKeyUp != null) { LeftKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.LineFeed:
                    break;
                case Keys.M:
                    break;
                case Keys.MButton:
                    break;
                case Keys.Menu:
                    break;
                case Keys.Multiply:
                    break;
                case Keys.N:
                    break;
                case Keys.NumLock:
                    break;
                case Keys.NumPad0:
                    break;
                case Keys.NumPad1:
                    break;
                case Keys.NumPad2:
                    break;
                case Keys.NumPad3:
                    break;
                case Keys.NumPad4:
                    break;
                case Keys.NumPad5:
                    break;
                case Keys.NumPad6:
                    break;
                case Keys.NumPad7:
                    break;
                case Keys.NumPad8:
                    break;
                case Keys.NumPad9:
                    break;
                case Keys.O:
                    break;
                case Keys.P:
                    if (PauseKeyUp != null) { PauseKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.PageDown:
                    break;
                case Keys.PageUp:
                    break;
                case Keys.Pause:
                    break;
                case Keys.Play:
                    break;
                case Keys.Print:
                    break;
                case Keys.PrintScreen:
                    break;
                case Keys.Q:
                    break;
                case Keys.R:
                    break;
                case Keys.RButton:
                    break;
                case Keys.RControlKey:
                    break;
                case Keys.RMenu:
                    break;
                case Keys.RShiftKey:
                    break;
                case Keys.RWin:
                    break;
                case Keys.Right:
                    if (RightKeyUp != null) { RightKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.S:
                    if (SKeyUp != null) { SKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.Separator:
                    break;
                case Keys.Shift:
                    break;
                case Keys.ShiftKey:
                    break;
                case Keys.Space:
                    break;
                case Keys.Subtract:
                    break;
                case Keys.T:
                    break;
                case Keys.Tab:
                    break;
                case Keys.U:
                    break;
                case Keys.Up:
                    break;
                case Keys.V:
                    break;
                case Keys.W:
                    if (WKeyUp != null) { WKeyUp(this, new KeyEventArgs()); }
                    break;
                case Keys.X:
                    break;
                case Keys.Y:
                    break;
                case Keys.Z:
                    break;
                default:
                    break;
            }

        }

        private void RaisKeyPressedEvent(Keys key) {
            switch (key) {
                case Keys.A:
                    if (AKeyPressed != null) { AKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.Add:
                    break;
                case Keys.Alt:
                    break;
                case Keys.B:
                    break;
                case Keys.Back:
                    break;
                case Keys.C:
                    break;
                case Keys.Cancel:
                    break;
                case Keys.CapsLock:
                    break;
                case Keys.Clear:
                    break;
                case Keys.Control:
                    break;
                case Keys.ControlKey:
                    break;
                case Keys.D:
                    if (DKeyPressed != null) { DKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.D0:
                    break;
                case Keys.D1:
                    break;
                case Keys.D2:
                    break;
                case Keys.D3:
                    break;
                case Keys.D4:
                    break;
                case Keys.D5:
                    break;
                case Keys.D6:
                    break;
                case Keys.D7:
                    break;
                case Keys.D8:
                    break;
                case Keys.D9:
                    break;
                case Keys.Decimal:
                    break;
                case Keys.Delete:
                    break;
                case Keys.Divide:
                    break;
                case Keys.Down:
                    break;
                case Keys.E:
                    break;
                case Keys.End:
                    break;
                case Keys.Enter:
                    break;
                case Keys.Escape:
                    if (EscapeKeyPressed != null) { EscapeKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.Execute:
                    break;
                case Keys.F:
                    break;
                case Keys.F1:
                    break;
                case Keys.F10:
                    break;
                case Keys.F11:
                    break;
                case Keys.F12:
                    break;
                case Keys.F13:
                    break;
                case Keys.F14:
                    break;
                case Keys.F15:
                    break;
                case Keys.F16:
                    break;
                case Keys.F17:
                    break;
                case Keys.F18:
                    break;
                case Keys.F19:
                    break;
                case Keys.F2:
                    break;
                case Keys.F20:
                    break;
                case Keys.F21:
                    break;
                case Keys.F22:
                    break;
                case Keys.F23:
                    break;
                case Keys.F24:
                    break;
                case Keys.F3:
                    break;
                case Keys.F4:
                    break;
                case Keys.F5:
                    break;
                case Keys.F6:
                    break;
                case Keys.F7:
                    break;
                case Keys.F8:
                    break;
                case Keys.F9:
                    break;
                case Keys.G:
                    break;
                case Keys.H:
                    break;
                case Keys.Help:
                    break;
                case Keys.Home:
                    break;
                case Keys.I:
                    break;
                case Keys.Insert:
                    break;
                case Keys.J:
                    break;
                case Keys.K:
                    break;
                case Keys.L:
                    break;
                case Keys.LButton:
                    break;
                case Keys.LControlKey:
                    break;
                case Keys.LMenu:
                    break;
                case Keys.LShiftKey:
                    break;
                case Keys.LWin:
                    break;
                case Keys.Left:
                    if (LeftKeyPressed != null) { LeftKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.LineFeed:
                    break;
                case Keys.M:
                    break;
                case Keys.MButton:
                    break;
                case Keys.Menu:
                    break;
                case Keys.Multiply:
                    break;
                case Keys.N:
                    break;
                case Keys.NumLock:
                    break;
                case Keys.NumPad0:
                    break;
                case Keys.NumPad1:
                    break;
                case Keys.NumPad2:
                    break;
                case Keys.NumPad3:
                    break;
                case Keys.NumPad4:
                    break;
                case Keys.NumPad5:
                    break;
                case Keys.NumPad6:
                    break;
                case Keys.NumPad7:
                    break;
                case Keys.NumPad8:
                    break;
                case Keys.NumPad9:
                    break;
                case Keys.O:
                    break;
                case Keys.P:
                    break;
                case Keys.PageDown:
                    break;
                case Keys.PageUp:
                    break;
                case Keys.Pause:
                    if (PauseKeyPressed != null) { PauseKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.Play:
                    break;
                case Keys.Print:
                    break;
                case Keys.PrintScreen:
                    break;
                case Keys.Q:
                    break;
                case Keys.R:
                    break;
                case Keys.RButton:
                    break;
                case Keys.RControlKey:
                    break;
                case Keys.RMenu:
                    break;
                case Keys.RShiftKey:
                    break;
                case Keys.RWin:
                    break;
                case Keys.Right:
                    if (RightKeyPressed != null) { RightKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.S:
                    if (SKeyPressed != null) { SKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.Separator:
                    break;
                case Keys.Shift:
                    break;
                case Keys.ShiftKey:
                    break;
                case Keys.Space:
                    break;
                case Keys.Subtract:
                    break;
                case Keys.T:
                    break;
                case Keys.Tab:
                    break;
                case Keys.U:
                    break;
                case Keys.Up:
                    break;
                case Keys.V:
                    break;
                case Keys.W:
                    if (WKeyPressed != null) { WKeyPressed(this, new KeyEventArgs()); }
                    break;
                case Keys.X:
                    break;
                case Keys.Y:
                    break;
                case Keys.Z:
                    break;
                default:
                    break;
            }
        }

        private bool KeyDown(Keys key) {
            if (_keysDown.Contains(key)) {
                return true;
            }
            return false;
        }

        private bool KeyUp(Keys key) {
            if (_keysUp.Contains(key)) {
                return true;
            }
            return false;
        }

        void UpdateInput() {
            // Clear our pressed and released lists.
            _keysDown.Clear();
            _keysUp.Clear();

            // Interpret pressed key data between arrays to
            // figure out just-pressed and just-released keys.
            KeyboardState currentState = Keyboard.GetState();
            Keys[] currentKeys = currentState.GetPressedKeys();

            // First loop, looking for keys just pressed.
            for (int currentKey = 0; currentKey < currentKeys.Length; currentKey++) {
                bool found = false;
                for (int previousKey = 0; previousKey < _keysPressed.Length; previousKey++) {
                    if (currentKeys[currentKey] == _keysPressed[previousKey]) {
                        // The key was pressed both this frame and last; ignore.
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    // The key was pressed this frame, but not last frame; it was just pressed.
                    _keysDown.Add(currentKeys[currentKey]);
                }
            }

            // Second loop, looking for keys just released.
            for (int previousKey = 0; previousKey < _keysPressed.Length; previousKey++) {
                bool found = false;
                for (int currentKey = 0; currentKey < currentKeys.Length; currentKey++) {
                    if (_keysPressed[previousKey] == currentKeys[currentKey]) {
                        // The key was pressed both this frame and last; ignore.
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    // The key was pressed last frame, but not this frame; it was just released.
                    _keysUp.Add(_keysPressed[previousKey]);
                }
            }

            // Set the held state to the current state.
            _keysPressed = currentKeys;
        }
    }

    public class KeyEventArgs : EventArgs {
        //empty for now.
    }
}