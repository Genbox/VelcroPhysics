#region File Description

//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System.Collections.Generic;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.DemoBaseXNA.Screens
{
    /// <summary>
    ///   The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class MainMenuScreen : MenuScreen
    {
        private int _id;
        private Dictionary<int, MenuEntry> _mainMenuItems = new Dictionary<int, MenuEntry>();

        /// <summary>
        ///   Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu")
        {
        }

        /// <summary>
        ///   Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex, PlayerIndex index)
        {
            if (_mainMenuItems[entryIndex].IsExitItem)
                ScreenManager.Game.Exit();
            else
            {
                ScreenManager.Camera.SmoothResetCamera();
                ScreenManager.AddScreen(_mainMenuItems[entryIndex].Screen, null);
            }
        }

#if WINDOWS_PHONE
    /// <summary>
    /// Handler for when the user has cancelled the menu.
    /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }
#endif

        public void AddMainMenuItem(string name, GameScreen screen)
        {
            AddMainMenuItem(name, screen, false);
        }

        public void AddMainMenuItem(string name, GameScreen screen, bool isExitItem)
        {
            MenuEntry entry = new MenuEntry(name, screen, isExitItem);
            _mainMenuItems.Add(_id++, entry);
            MenuEntries.Add(entry);
        }
    }
}