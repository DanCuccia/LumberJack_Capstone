using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Drawing_Objects
{
    class SkyBox
    {
        #region Member Variables

        GraphicsDevice          m_device;
        Camera                  m_camera;

        TextureCube             m_texture;
        Model                   m_cube;
        Effect                  m_effect;
        const float             m_scale = 90f;

        #endregion Member Variables



        #region Initialization & RunTime

        public SkyBox() { }

        public void Initialize(GraphicsDevice device, Camera camera, ContentManager content, string texFilename)
        {
            m_device = device;
            m_camera = camera;

            m_texture = content.Load<TextureCube>(texFilename);
            if (m_texture == null)
                throw new ContentLoadException("SkyBox::Initialize: cannot load texture");

            m_cube = content.Load<Model>(@"skybox\Cube");
            if (m_cube == null)
                throw new ContentLoadException("SkyBox::Initialize: cannot load cube model");

            Renderer render = Renderer.getInstance();
            m_effect = render.EffectManager.GetEffect(RenderEffect.RFX_TOON);
            if (m_effect == null)
                throw new ContentLoadException("SkyBox::Initialize: cannot load skybox shader");

            m_effect.Parameters["SkyBoxTexture"].SetValue(m_texture);
            m_cube.Meshes[0].MeshParts[0].Effect = m_effect;
        }

        public void Draw()
        {
            m_device.DepthStencilState = DepthStencilState.DepthRead;
            m_device.RasterizerState = RasterizerState.CullClockwise;

            m_effect.CurrentTechnique = m_effect.Techniques["SkyBox"];

            m_effect.Parameters["World"].SetValue(Matrix.CreateScale(m_scale) * 
                Matrix.CreateTranslation(m_camera.Position));

            m_effect.CurrentTechnique.Passes[0].Apply();
            m_cube.Meshes[0].Draw();

            m_device.RasterizerState = RasterizerState.CullCounterClockwise;
            m_device.DepthStencilState = DepthStencilState.Default;
        }

        #endregion Initialization & RunTime



        #region Mutators

        public TextureCube Texture
        {
            get { return m_texture; }
        }
        public Camera Camera
        {
            set { m_camera = value; }
        }

        #endregion Mutators
    }
}
