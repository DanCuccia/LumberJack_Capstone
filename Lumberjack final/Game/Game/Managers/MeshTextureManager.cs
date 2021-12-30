using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Game.Managers
{
    /// <summary>Depicts what type of texture</summary>
    public enum TextureType
    {
        TEX_DIFFUSE = 0,
        TEX_NORMAL,
        TEX_OPACITY,
        COUNT
    }

    /// <summary>This class takes care of holding all possible textures that a 
    /// Mesh may use.  Because I use my own shaders, I need to call and set
    /// textures very specifically, I must manage them myself, and not .fbx auto-stuff </summary>
    public class MeshTextureManager
    {
        Texture2D[] m_textures = new Texture2D[(int)TextureType.COUNT];

        /// <summary>Default CTOR</summary>
        public MeshTextureManager() { }

        /// <summary>Adds a texture to the list</summary>
        /// <param name="tex">the loaded image</param>
        /// <param name="type">what type it is</param>
        public void AddTexture(Texture2D tex, TextureType type)
        {
            if (tex == null)
                throw new Exception("Manager_MeshTextures::AddTexture:tex is null");
            m_textures[(int)type] = tex;
        }

        /// <summary>Use this when building from xml, defaults to diffuse texture</summary>
        /// <param name="content">what to load with</param>
        /// <param name="filename">the filename</param>
        public void AddTexture(ContentManager content, string filename)
        {
            AddTexture(content.Load<Texture2D>(filename), TextureType.TEX_DIFFUSE);
        }

        /// <summary>Get a loaded texture from the list</summary>
        /// <param name="type">what type of texture you want</param>
        /// <returns>the loaded texture, or null if none was found</returns>
        public Texture2D GetTexture(TextureType type)
        {
            return m_textures[(int)type];
        }
    }
}
