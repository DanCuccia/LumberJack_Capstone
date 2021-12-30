using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Managers
{
    public class SpriteTexture
    {
        public Texture2D texture;
        public String name;

        public SpriteTexture(String name, Texture2D texture)
        {
            this.texture = texture;
            this.name = name;
        }
    }

    public class TextureManager
    {
        protected List<SpriteTexture> textures;

        public TextureManager()
        {
            textures = new List<SpriteTexture>();
        }

        public void addTexture(String name, Texture2D texture)
        {
            if (texture != null)
            {
                SpriteTexture temp = new SpriteTexture(name, texture);

                textures.Add(temp);
            }
        }

        public Texture2D getTexture(String name)
        {
            for (int index = 0; index < textures.Count; index++)
            {
                if (textures[index].name == name)
                {
                    return textures[index].texture;
                }
            }

            return null;
        }

        public Texture2D removeTexture(String name)
        {
            Texture2D temp = null;

            for (int index = 0; index < textures.Count; index++)
            {
                if (textures[index].name == name)
                {
                    temp = textures[index].texture;
                    textures.Remove(textures[index]);
                    return temp;
                }
            }

            return null;
        }

        public void empty()
        {
            textures.Clear();
        }
    }
}
