using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using Game.Drawing_Objects;
using Game.Managers;

namespace Game.Game_Objects
{
    class Player
    {
        ContentManager m_content;   //ref
        GraphicsDevice m_device;    //ref
        WorldManager   m_world;     //ref

        Point m_worldIndicies;

        StaticMesh m_model;

        float m_movementSpeed = 5.0f;

        /// <summary>
        /// Default CTOR
        /// </summary>
        /// <param name="device">reference to graphics device</param>
        /// <param name="world">reference to the world manger</param>
        public Player(GraphicsDevice device)
        {
            this.m_device = device;
            this.m_world = WorldManager.getInstance();
        }

        public void Initialize(ContentManager content)
        {
            this.m_content = content;
            m_model = new StaticMesh();
            m_model.Initialize(content, "models\\mushroom", "textures\\ground_diffuse");
        }

        public void Update(GameTime gameTime) { }

        /// <summary>All User to Player input logic in here</summary>
        /// <param name="kb">current keyboard state</param>
        /// <param name="ms">current mouse state</param>
        /// <param name="gp">current gamepad state</param>
        public void Input(KeyboardState kb, MouseState ms, GamePadState gp)
        {
            if (kb.IsKeyDown(Keys.Up))
                m_model.Position += m_model.WorldMatrix.Forward * m_movementSpeed;
            else if (kb.IsKeyDown(Keys.Down))
                m_model.Position += m_model.WorldMatrix.Backward * m_movementSpeed;

            if (kb.IsKeyDown(Keys.Left))
                m_model.Position += m_model.WorldMatrix.Left * m_movementSpeed;
            else if (kb.IsKeyDown(Keys.Right))
                m_model.Position += m_model.WorldMatrix.Right * m_movementSpeed;

            float h;
            m_worldIndicies = m_world.getTerrainHeight(m_model.Position, out h);
            m_model.PositionY = h;

        }

        /// <summary>Main Draw</summary>
        public void Draw()
        {
            m_model.Draw();
        }
    }
}
