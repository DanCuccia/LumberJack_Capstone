using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Managers;

namespace Game.Game_Objects
{
    public abstract class State
    {
        public string name;
        public bool isInitialized;
        public ContentManager content;  //ref
        public StateManager stateMan;   //ref
        public GraphicsDevice device;   //ref

        public State(Game1 g, String name, StateManager sm)
        {
            content = g.Content;
            this.device = g.GraphicsDevice;
            this.name = name;
            this.stateMan = sm;
            isInitialized = false;
        }

        public abstract void initialize();
        public abstract void input(KeyboardState kb, MouseState ms);
        public abstract void update(GameTime time);
        public abstract void render2D(GameTime time, SpriteBatch batch);
        public abstract void render3D(GameTime time);
        public abstract void close();
    }
}
