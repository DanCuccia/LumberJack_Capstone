using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Game.Drawing_Objects;

namespace Game.Managers
{
    public class SpriteManager
    {
        protected List<Sprite> sprites;
        SpriteBatch sb;

        /// <summary>Default CTOR</summary>
        /// <param name="sb">pointer used for drawing</param>
        public SpriteManager(SpriteBatch sb)
        {
            sprites = new List<Sprite>();
            this.sb = sb;
        }

        /// <summary>get a sprite from the list by name </summary>
        /// <param name="name">the assigned name of the sprite</param>
        /// <returns>sprite from the list or null</returns>
        public Sprite getSprite(String name)
        {
            Sprite temp = null;
            for (int index = 0; index < sprites.Count; index++)
            {
                if (sprites[index].name == name)
                {
                    temp = sprites[index];
                    break;
                }
            }
            return temp;
        }

        /// <summary>add a sprite into the list</summary>
        public void addSprite(Sprite s)
        {
            if (s != null)
            {
                sprites.Add(s);
            }
        }

        /// <summary>remove a sprite from the list by name</summary>
        /// <param name="name">name of the sprite you want to remove</param>
        /// <returns>the sprite you just removed</returns>
        public Sprite removeSprite(String name)
        {
            Sprite temp = null;
            for (int index = 0; index < sprites.Count; index++)
            {
                if (sprites[index].name == name)
                {
                    temp = sprites[index];
                    sprites.Remove(temp);
                    break;
                }
            }
            return temp;
        }

        /// <summary>Main update call, will call every sprite's update</summary>
        public void update(GameTime time)
        {
            for (int index = 0; index < sprites.Count; index++)
            {
                sprites[index].update(time);
            }
        }

        /// <summary>
        /// Draw text to the screen
        /// </summary>
        /// <param name="text">the text to display</param>
        /// <param name="color">the color to display it in</param>
        /// <param name="font">the sprite font you wish to use, (get from renderer)</param>
        /// <param name="position">the on screen location</param>
        public void drawText(String text, Color color, SpriteFont font, Vector2 position)
        {
            sb.DrawString(font, text, position, color);
        }

        /// <summary>draw all the sprites in the list, painter's alg.</summary>
        public void renderSprites()
        {
            for (int index = 0; index < sprites.Count; index++)
            {
                sprites[index].draw(sb);
            }
        }

        /// <summary> dump the sprite list</summary>
        public void empty()
        {
            sprites.Clear();
        }

        
        public List<Sprite> SpriteList
        {
            get { return this.sprites; }
        }
    }
}
