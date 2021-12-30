using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Game_Objects.Trees;
using Game.Game_Objects.Rocks;

namespace Game.Managers
{
    /// <summary>Designed to encapsulate some quick and dirty functionality
    /// for some basic level editing</summary>
    public class LevelEditor
    {
        ContentManager m_content;   //ref
        GraphicsDevice m_device;    //ref
        Camera m_camera;            //ref
        WorldManager m_world;       //ref

        SpriteFont m_font;

        Point m_worldIndicies = new Point(-1,-1);


        bool m_grabObject = true;
        Prop m_prop;


        //input limiting
        int m_waitCounter = 0;
        const int m_waitUntil = 250;
        bool m_canExecute = true;

        bool m_canAddToWorld = true;
        int m_addCount = 0;
        int m_addMaxCount = 1000;

        /// <summary>Default ctor, must call initialize</summary>
        public LevelEditor() { }

        /// <summary>Initialize all pointers to main global objects</summary>
        public void Initialize(ContentManager content, GraphicsDevice device, Camera camera)
        {
            m_content = content;
            m_device = device;
            m_camera = camera;
            m_world = WorldManager.getInstance();

            m_font = content.Load<SpriteFont>("fonts\\verdana");
        }

        /// <summary>hot keys and main key logic go in here</summary>
        public void Input(KeyboardState kb, MouseState ms)
        {
            if (kb.IsKeyDown(Keys.G) && kb.IsKeyDown(Keys.LeftShift))
                m_grabObject = false;

            if (m_canExecute)
            {
                if (kb.IsKeyDown(Keys.F1))
                    StartSmallFullTree();
                if (kb.IsKeyDown(Keys.F2))
                    StartLargeFullTree();
                if (kb.IsKeyDown(Keys.F3))
                    StartSmallBush();
                if (kb.IsKeyDown(Keys.F4))
                    StartBigBush();
                if (kb.IsKeyDown(Keys.F5))
                    StartSpikeyBush();
                if (kb.IsKeyDown(Keys.F6))
                    StartLongRock();
                if (kb.IsKeyDown(Keys.F7))
                    StartBoulderRock();
            }


            if (m_grabObject == false &&  m_prop != null )
            {
                TreeAdjustments(kb, ms);
                RockAdjustments(kb, ms);
            }

            if (m_canExecute)
            {
                if (kb.IsKeyDown(Keys.Enter) && kb.IsKeyDown(Keys.LeftShift))
                {
                    if (m_prop != null)
                    {
                        m_world.addProp(m_prop);
                        m_canExecute = false;
                        m_prop = null;
                        m_canAddToWorld = false;
                    }
                }
            }
        }

        /// <summary>Main gameTime update</summary>
        public void Update(GameTime gameTime) 
        {
            //update object to be in front of camera
            if (m_prop != null && m_grabObject == true)
            {
                if (m_prop.ID > GameIDList.Trees_Begin && m_prop.ID < GameIDList.Trees_End)
                {
                    Tree t_tree = (Tree)m_prop;
                    t_tree.Position = m_camera.Position + m_camera.RotationMatrix.Forward * 500;
                }
                else if (m_prop.ID > GameIDList.Rocks_Begin && m_prop.ID < GameIDList.Rocks_End)
                {
                    Rock t_rock = (Rock)m_prop;
                    t_rock.Position = m_camera.Position + m_camera.RotationMatrix.Forward * 500;
                }
            }

            //update world indicies
            float ignore;
            m_worldIndicies = m_world.getTerrainHeight(m_camera.Position, out ignore);

            //update main execute timer
            if (m_canExecute == false)
            {
                m_waitCounter += gameTime.ElapsedGameTime.Milliseconds;
                if (m_waitCounter >= m_waitUntil)
                {
                    m_canExecute = true;
                    m_waitCounter = 0;
                }
            }

            //update main add to world timer
            if (m_canAddToWorld == false)
            {
                m_addCount += gameTime.ElapsedGameTime.Milliseconds;
                if (m_addCount >= m_addMaxCount)
                {
                    m_addCount = 0;
                    m_canAddToWorld = true;
                }
            }
        }

        /// <summary>Draw the object we're currently working on</summary>
        public void Draw() 
        {
            if(m_prop != null)
                m_prop.Draw();
        }

        /// <summary>Draw Controls to the screen</summary>
        public void DrawText(SpriteBatch batch)
        {
            float stride = 20f;
            Vector2 position = new Vector2(20, 20);
            batch.DrawString(m_font, "FunctionKeys      :   Start New Object", position, Color.FloralWhite);
            
            position.Y += stride;
            batch.DrawString(m_font, "Shift + G           :   Let Go of Object", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "Shift + Enter      :   Add into WorldList", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "F11                   :   Save World Lists", position, Color.FloralWhite);

            position.Y += stride * 2;
            batch.DrawString(m_font, "Ctrl + Up/Down    :   Position.Y up/down", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "Alt + Up/Down     :   Position.Z up/down", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "Alt + Left/Right    :   Position.X up/down", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "Home/End            :   Rotation.Y +/-", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "Insert/Delete        :   Rotation.Z +/-", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "PageUp/PageDown   :   Rotation.X +/-", position, Color.FloralWhite);

            position.Y += stride;
            batch.DrawString(m_font, "Ctrl + PageUp/PageDown   :   Scale +/-", position, Color.FloralWhite);

            position.Y += stride * 2;
            batch.DrawString(m_font, "Current World Node Indicies: (" + m_worldIndicies.X + ", " + m_worldIndicies.Y + ")", position, Color.FloralWhite);

            if (m_canAddToWorld == false)
            {
                position.Y += stride * 2;
                batch.DrawString(m_font, "OBJECT ADDED TO PROP LIST", position, Color.DarkRed);
            }
        }

        private void TreeAdjustments(KeyboardState kb, MouseState ms)
        {
            Tree t_tree = null;
            if (m_prop.ID > GameIDList.Trees_Begin && m_prop.ID < GameIDList.Trees_End)
                t_tree = (Tree)m_prop;
            else
                return;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                t_tree.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                t_tree.PositionY -= 1f;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionZ -= 1f;

            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                t_tree.PositionX -= 1f;

            if (kb.IsKeyDown(Keys.Home))
                t_tree.RotationY += .5f;
            if (kb.IsKeyDown(Keys.End))
                t_tree.RotationY -= .5f;

            if (kb.IsKeyDown(Keys.Insert))
                t_tree.RotationZ += .5f;
            if (kb.IsKeyDown(Keys.Delete))
                t_tree.RotationZ -= .5f;

            if (kb.IsKeyDown(Keys.PageUp))
                t_tree.RotationX += .5f;
            if (kb.IsKeyDown(Keys.PageDown))
                t_tree.RotationX -= .5f;

            if (kb.IsKeyDown(Keys.PageUp) && kb.IsKeyDown(Keys.LeftControl))
                t_tree.Scale += .1f;
            if (kb.IsKeyDown(Keys.PageDown) && kb.IsKeyDown(Keys.LeftControl))
                t_tree.Scale -= .1f;

            if (kb.IsKeyDown(Keys.Back))
                t_tree.Rotation = Vector3.Zero;
        }

        private void RockAdjustments(KeyboardState kb, MouseState ms)
        {
            Rock t_rock = null;

            if (m_prop.ID > GameIDList.Rocks_Begin && m_prop.ID < GameIDList.Rocks_End)
                t_rock = (Rock)m_prop;
            else
                return;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftControl))
                t_rock.PositionY += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftControl))
                t_rock.PositionY -= 1f;

            if (kb.IsKeyDown(Keys.Up) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionZ += 1f;
            if (kb.IsKeyDown(Keys.Down) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionZ -= 1f;

            if (kb.IsKeyDown(Keys.Left) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionX += 1f;
            if (kb.IsKeyDown(Keys.Right) && kb.IsKeyDown(Keys.LeftAlt))
                t_rock.PositionX -= 1f;

            if (kb.IsKeyDown(Keys.Home))
                t_rock.RotationY += .5f;
            if (kb.IsKeyDown(Keys.End))
                t_rock.RotationY -= .5f;

            if (kb.IsKeyDown(Keys.Insert))
                t_rock.RotationZ += .5f;
            if (kb.IsKeyDown(Keys.Delete))
                t_rock.RotationZ -= .5f;

            if (kb.IsKeyDown(Keys.PageUp))
                t_rock.RotationX += .5f;
            if (kb.IsKeyDown(Keys.PageDown))
                t_rock.RotationX -= .5f;

            if (kb.IsKeyDown(Keys.PageUp) && kb.IsKeyDown(Keys.LeftControl))
                t_rock.Scale += .1f;
            if (kb.IsKeyDown(Keys.PageDown) && kb.IsKeyDown(Keys.LeftControl))
                t_rock.Scale -= .1f;

            if (kb.IsKeyDown(Keys.Back))
                t_rock.Rotation = Vector3.Zero;
        }

        private void StartSmallFullTree()
        {
            m_prop = new SmallFullTree(m_content);
            m_grabObject = true;
            m_canExecute = false;
        }

        private void StartLargeFullTree()
        {
            m_prop = new LargeFullTree(m_content);
            m_grabObject = true;
            m_canExecute = false;
        }

        private void StartSmallBush()
        {
            m_prop = new SmallBush(m_content);
            m_grabObject = true;
            m_canExecute = false;
        }

        private void StartBigBush()
        {
            m_prop = new BigBush(m_content);
            m_grabObject = true;
            m_canExecute = false;
        }

        private void StartLongRock()
        {
            m_prop = new LongRock(m_content);
            m_grabObject = true;
            m_canExecute = false;
        }

        private void StartBoulderRock()
        {
            m_prop = new BoulderRock(m_content);
            m_grabObject = true;
            m_canExecute = false;
        }

        private void StartSpikeyBush()
        {
            m_prop = new SpikeyBush(m_content);
            m_grabObject = true;
            m_canExecute = false;
        }

        public SpriteFont Font
        {
            get { return m_font; }
        }
    }
}
