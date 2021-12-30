using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using SkinnedModel;

using Game.Drawing_Objects;

namespace Game.Managers
{
    /// <summary>All of the different shaders we can possibly draw with</summary>
    public enum RenderEffect
    {
        RFX_BILLBOARD,
        RFX_GEOMETRY,
        RFX_TOON,
        RFX_TOON_POST_PROCESS
    }

    /// <summary>This is held public static so all objects can check what render phase
    /// the renderer is on, if we're on depth phase, objects won't change the technique</summary>
    public enum RenderPhase
    {
        PHASE_DIFFUSE,
        PHASE_DEPTH
    }

    /// <summary>Debug render modes</summary>
    public enum DebugRenderMode
    {
        RENDER_DEFAULT,
        RENDER_NORMALS,
        RENDER_DEPTH,
        RENDER_DIFFUSE,
        RENDER_SILHOUETTE
    }

    /// <summary>This class is designed to be the meca of drawing
    /// Encapsulates Rendering Algorithms for 2D and 3D drawing,
    /// Is a singleton, Accessable anywhere</summary>
    public class Renderer
    {

        private static Renderer m_myInstance;
        public static Renderer getInstance()
        {
            if (m_myInstance == null)
                m_myInstance = new Renderer();
            return m_myInstance;
        }

        #region Member Variables

        int                         m_gameStart;
        GameTime                    m_currentGameTime;

        GraphicsDeviceManager       m_device;       //ref
        Camera                      m_camera;       //ref

        RenderTarget2D              m_diffuseTarget; 
        RenderTarget2D              m_depthTarget;

        bool                        m_debugRenderWireFrame = false;
        DebugRenderMode             m_debugRenderMode = DebugRenderMode.RENDER_DEFAULT;

        Vector2                     m_halfPixel;

        TextureCube                 m_skyBoxCubeMap; //ref

        ScreenAlignedQuad           m_screenAlignedQuad;

        EffectManager               m_effectMan;
        public static RenderPhase   m_renderPhase;

        int                         m_currentHold = 0;
        bool                        m_holdSwitch = false;

        #endregion Member Variables


        #region Initialization

        /// <summary>Null Constructor, must call Initialize()</summary>
        private Renderer() { }

        /// <summary>Main Initialization </summary>
        /// <param name="device">the device we'll be rendering with</param>
        /// <param name="startTime">the time our renderer turned on</param>
        public void Initialize(GraphicsDeviceManager graphics, ContentManager content, int startTime)
        {
            m_device = graphics;
            m_gameStart = startTime;
            m_halfPixel = new Vector2(
                .5f / (float)m_device.GraphicsDevice.Viewport.Width,
                .5f / (float)m_device.GraphicsDevice.Viewport.Height);

            CreateRenderTargets();

            LoadShaders(content);

            m_screenAlignedQuad = new ScreenAlignedQuad(graphics.GraphicsDevice);
        }


        #endregion Init


        #region Run-Time

        /// <summary> Standard dump of the backbuffer</summary>
        public void ClearBackBuffer()
        {
            m_device.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Bisque, 1.0f, 0);
        }


        /// <summary>Sets the device's render target to our main presentation texture</summary>
        public void BeginDiffuseScene()
        {
            m_device.GraphicsDevice.SetRenderTarget(m_diffuseTarget);
            this.ClearBackBuffer();
            m_renderPhase = RenderPhase.PHASE_DIFFUSE;
        }


        /// <summary>Sets the device's render target to the depth texture</summary>
        public void BeginDepthScene()
        {
            m_device.GraphicsDevice.SetRenderTarget(m_depthTarget);
            this.ClearBackBuffer();
            m_renderPhase = RenderPhase.PHASE_DEPTH;
            Effect fx = m_effectMan.GetEffect(RenderEffect.RFX_TOON);
            fx.CurrentTechnique = fx.Techniques["NormalDepth"];
        }

        
        /// <summary>begin's the spriteBatch</summary>
        /// <param name="spriteBatch">drawing api</param>
        public void Begin2D(SpriteBatch sBatch)
        {
            sBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        }


        /// <summary>Sets the device's render states correctly to draw 3D</summary>
        public void Begin3D()
        {
            m_device.GraphicsDevice.BlendState = BlendState.Opaque;
            m_device.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }


        /// <summary>Resets the device's render target and ends the spriteBatch</summary>
        /// <param name="spriteBatch">the spritebatch to end()</param>
        public void End2D(SpriteBatch sBatch)
        {
            //m_device.GraphicsDevice.SetRenderTarget(null);
            sBatch.End();
        }


        /// <summary>The Final composition is presented to the screen.</summary>
        /// <param name="sprite">spriteBatch to draw the texture with</param>
        public void PresentRender(SpriteBatch sBatch)
        {
            m_device.GraphicsDevice.SetRenderTarget(null);
            Effect postfx = m_effectMan.GetEffect(RenderEffect.RFX_TOON_POST_PROCESS);

            postfx.Parameters["SceneTexture"].SetValue(m_diffuseTarget);
            postfx.Parameters["NormalDepthTexture"].SetValue(m_depthTarget);

            switch (m_debugRenderMode)
            {
                case DebugRenderMode.RENDER_DEFAULT:
                    postfx.CurrentTechnique = postfx.Techniques["EdgeDetect"];
                    break;

                case DebugRenderMode.RENDER_DEPTH:
                    postfx.CurrentTechnique = postfx.Techniques["DebugShowDepth"];
                    break;

                case DebugRenderMode.RENDER_NORMALS:
                    postfx.CurrentTechnique = postfx.Techniques["DebugShowNormals"];
                    break;

                case DebugRenderMode.RENDER_DIFFUSE:
                    postfx.CurrentTechnique = postfx.Techniques["DebugShowDiffuse"];
                    break;

                case DebugRenderMode.RENDER_SILHOUETTE:
                    postfx.CurrentTechnique = postfx.Techniques["DebugShowSilhouette"];
                    break;
            }
            
            postfx.CurrentTechnique.Passes[0].Apply();
            m_screenAlignedQuad.Draw();
   
        }


        /// <summary>updates the renderer's time</summary>
        /// <param name="time">time is supplied for any special timing in this class</param>
        public void Update(GameTime time)
        {
            m_currentGameTime = time;
            if (m_holdSwitch)
            {
                m_currentHold += time.ElapsedGameTime.Milliseconds;
                if (m_currentHold >= 500)
                {
                    m_currentHold = 0;
                    m_holdSwitch = false;
                }
            }

            Effect toonMaster = m_effectMan.GetEffect(RenderEffect.RFX_TOON);

            toonMaster.Parameters["View"].SetValue(m_camera.ViewMatrix);
            toonMaster.Parameters["ViewInverse"].SetValue(Matrix.Invert(m_camera.ViewMatrix));
            toonMaster.Parameters["Projection"].SetValue(m_camera.ProjectionMatrix);
            toonMaster.Parameters["CameraPosition"].SetValue(m_camera.Position);
            toonMaster.Parameters["Time"].SetValue((float)time.TotalGameTime.TotalSeconds);
        }

        #endregion Run-Time


        #region Drawing Functions

        /// <summary>
        /// Skinned Model Draw Function
        /// </summary>
        /// <param name="model">the model to be drawn</param>
        /// <param name="animController">the animation controller to the model</param>
        public void Draw(Model model, AnimationController animController)
        {
            Matrix[] bones = animController.getSkinTransforms();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);
                    effect.World = m_camera.WorldMatrix * 
                        Matrix.CreateRotationY(MathHelper.ToRadians(110)) * 
                        Matrix.CreateTranslation(new Vector3(200, -15.309803f, -300));    //temporary!!
                    effect.View = m_camera.ViewMatrix;
                    effect.Projection = m_camera.ProjectionMatrix;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

        }

        /// <summary>Draws the BillBoard Particle Emitter</summary>
        /// <param name="emitter">the emitter you wish to draw</param>
        public void DrawBillboardEmitter(BillboardParticleEmitter emitter)
        {
            if (emitter.ParticleCount == 0)
                return;

            Matrix world = Matrix.CreateScale(1f) *
                Matrix.CreateTranslation(emitter.Position);

            Effect emitterfx = m_effectMan.GetEffect(RenderEffect.RFX_BILLBOARD);

            emitterfx.Parameters["World"].SetValue(m_camera.WorldMatrix);
            emitterfx.Parameters["View"].SetValue(m_camera.ViewMatrix);
            emitterfx.Parameters["Projection"].SetValue(m_camera.ProjectionMatrix);
            emitterfx.Parameters["Camera_Position"].SetValue(m_camera.Position);
            emitterfx.Parameters["Rotation_Direction"].SetValue(new Vector3(0, 1, 0));
            emitterfx.Parameters["BillboardTex"].SetValue(emitter.Texture);

            emitterfx.CurrentTechnique = emitterfx.Techniques["BillboardParticle"];
            emitterfx.CurrentTechnique.Passes[0].Apply();

            m_device.GraphicsDevice.SetVertexBuffer(emitter.VertexBuffer);
            m_device.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 
                0, 
                emitter.VertexBuffer.VertexCount/3);

            m_device.GraphicsDevice.BlendState = BlendState.Opaque;
            m_device.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
        }

        public void DrawBillboardSprite(BillboardSprite sprite)
        {
            Effect emitterfx = m_effectMan.GetEffect(RenderEffect.RFX_BILLBOARD);

            emitterfx.Parameters["World"].SetValue(Matrix.Identity);
            emitterfx.Parameters["View"].SetValue(m_camera.ViewMatrix);
            emitterfx.Parameters["Projection"].SetValue(m_camera.ProjectionMatrix);
            emitterfx.Parameters["Camera_Position"].SetValue(m_camera.Position);
            emitterfx.Parameters["BillboardTex"].SetValue(sprite.Texture);

            emitterfx.CurrentTechnique = emitterfx.Techniques["BillboardSprite"];
            emitterfx.CurrentTechnique.Passes[0].Apply();

            m_device.GraphicsDevice.SetVertexBuffer(sprite.VertexBuffer);
            m_device.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList,
                0,
                sprite.VertexBuffer.VertexCount / 3);

            m_device.GraphicsDevice.BlendState = BlendState.Opaque;
            m_device.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        #endregion


        #region Privates

        private void CreateRenderTargets()
        {
            m_diffuseTarget = new RenderTarget2D(m_device.GraphicsDevice,
                m_device.GraphicsDevice.PresentationParameters.BackBufferWidth,
                m_device.GraphicsDevice.PresentationParameters.BackBufferHeight,
                true,
                m_device.GraphicsDevice.DisplayMode.Format,
                DepthFormat.Depth24,
                m_device.GraphicsDevice.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

            m_depthTarget = new RenderTarget2D(m_device.GraphicsDevice,
                m_device.GraphicsDevice.PresentationParameters.BackBufferWidth,
                m_device.GraphicsDevice.PresentationParameters.BackBufferHeight,
                true,
                m_device.GraphicsDevice.DisplayMode.Format,
                DepthFormat.Depth24,
                m_device.GraphicsDevice.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

        }

        private void LoadShaders(ContentManager content)
        {
            m_effectMan = new EffectManager();
            m_effectMan.AddEffect(content.Load<Effect>(@"shaders\Billboard"), RenderEffect.RFX_BILLBOARD);

            Effect loadfx = content.Load<Effect>(@"shaders\ToonMaster");
            loadfx.Parameters["Noise_Tex"].SetValue(content.Load<Texture3D>(@"noisemaps\noiseVolume"));
            loadfx.Parameters["pulse_train_Tex"].SetValue(content.Load<Texture2D>(@"noisemaps\PulseTrain"));
            loadfx.Parameters["lowTex"].SetValue(content.Load<Texture2D>(@"heightmaps\texPack\low_diffuse"));
            loadfx.Parameters["lowMidTex"].SetValue(content.Load<Texture2D>(@"heightmaps\texPack\mid_low_diffuse"));
            loadfx.Parameters["highMidTex"].SetValue(content.Load<Texture2D>(@"heightmaps\texPack\mid_high_diffuse"));
            loadfx.Parameters["WaterNormalMap"].SetValue(content.Load<Texture2D>(@"noisemaps\waves"));
            m_effectMan.AddEffect(loadfx, RenderEffect.RFX_TOON);

            loadfx = content.Load<Effect>(@"shaders\PostprocessEffect");
            loadfx.CurrentTechnique = loadfx.Techniques["EdgeDetect"];
            loadfx.Parameters["ScreenResolution"].SetValue(new Vector2(m_diffuseTarget.Width, m_diffuseTarget.Height));
            loadfx.Parameters["ScreenHalfPixel"].SetValue(m_halfPixel);
            m_effectMan.AddEffect(loadfx, RenderEffect.RFX_TOON_POST_PROCESS);

        }

        #endregion Privates


        #region Mutators

        public EffectManager EffectManager
        {
            get { return m_effectMan; }
        }
        public bool RenderWireFrame
        {
            get { return m_debugRenderWireFrame; }
            set 
            {
                if (m_holdSwitch == false)
                {
                    m_debugRenderWireFrame = value;
                    m_holdSwitch = true;
                }
            }
        }
        public TextureCube SkyBoxTexture
        {
            set { m_skyBoxCubeMap = value; }
        }
        public Camera Camera
        {
            set { m_camera = value; }
        }
        public DebugRenderMode DebugRenderMode
        {
            set { m_debugRenderMode = value; }
        }

        #endregion //Mutators

    }
}
