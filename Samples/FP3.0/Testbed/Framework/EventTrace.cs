/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Box2D.XNA.TestBed.Framework
{
    public interface IEventTrace
    {
        int Register(string label, Color color);
        void BeginTrace(int eventID);
        void EndTrace(int eventID);

        void ResetFrame();
    }

    public class EventTrace : IEventTrace
    {
        public EventTrace(Game game)
        {
            _game = game;
            game.Services.AddService(typeof(IEventTrace), this);
        }

        public int Register(string label, Color color)
        {
            var e = new Event();
            e.Id = _nextId;
            e.Label = label;
            e.Color = color;
            e.TraceTicks = new List<long>(10);

            _registeredEvents[_nextId] = e;
            return _nextId++;
        }

        public void BeginTrace(int eventID)
        {
            if (_stopwatch.IsRunning)
            {
                Event e = _registeredEvents[eventID];
                if (!e.LastTraceWasStart)
                {
                    e.TraceTicks.Add(_stopwatch.ElapsedTicks);
                    e.LastTraceWasStart = true;    
                }
            }
        }

        public void EndTrace(int eventID)
        {
            if (_stopwatch.IsRunning)
            {
                Event e = _registeredEvents[eventID];
                if (e.LastTraceWasStart)
                {
                    e.TraceTicks.Add(_stopwatch.ElapsedTicks);
                    e.LastTraceWasStart = false;   
                }
            }   
        }

        public void ResetFrame()
        {
            if (_stopwatch.IsRunning)
            {
                EnsureContent();

                double totalTime = _stopwatch.Elapsed.TotalSeconds;
                Vector2 offset = new Vector2(50, 100);
                float height = 20;
                float textWidth = 175;
                float timeWidth = 500;

                _spriteBatch.Begin();

                foreach (var e in _registeredEvents.Values)
                {
                    var count = e.TraceTicks.Count;
                    if (count % 2 != 0)
                    {
                        throw new InvalidOperationException();
                    }

                    int intervals = 0;
                    double eventTime = 0;
                    for (int i=0; i<count; i+=2)
                    {
                        double startRatio = (double)e.TraceTicks[i] / (double)Stopwatch.Frequency / totalTime;
                        double endRatio = (double)e.TraceTicks[i + 1] / (double)Stopwatch.Frequency / totalTime;
                        int startOffset = (int)(offset.X + textWidth + startRatio * timeWidth);
                        int endOffset = (int)(offset.X + textWidth + endRatio * timeWidth);
                        if (endOffset <= startOffset)
                            endOffset++;

                        _spriteBatch.Draw(_1x1, new Rectangle(startOffset, (int)offset.Y, endOffset - startOffset, (int)height), e.Color);
                        intervals++;
                        eventTime += (endRatio - startRatio) * totalTime;
                    }

                    _spriteBatch.DrawString(_font, string.Format("{0} ({1}) {2:f}", e.Label, intervals, eventTime * 1000.0), offset, e.Color);

                    e.ResetFrame();
                    offset.Y += height + 5;
                }

                _spriteBatch.End();
            }
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        private void EnsureContent()
        {
            if (_spriteBatch == null)
            {
                _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
            }

            if (_font == null)
            {
                _font = _game.Content.Load<SpriteFont>("font");
            }

            if (_1x1 == null)
            {
                _1x1 = _game.Content.Load<Texture2D>("1x1");
            }
        }

        private Game _game;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _1x1;
        private int _nextId;
        private Stopwatch _stopwatch = new Stopwatch();
        private Dictionary<int, Event> _registeredEvents = new Dictionary<int,Event>();

        public class Event
        {
            public int Id;
            public string Label;
            public Color Color;
            public List<long> TraceTicks;
            public bool LastTraceWasStart;

            internal void ResetFrame()
            {
                if (LastTraceWasStart)
                {
                    throw new InvalidOperationException();
                }

                TraceTicks.Clear();
            }
        }
    }
}
