using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Game.Drawing_Objects;
using Game.Game_Objects.Build_System;

namespace Game.Managers
{
    /// <summary>All of the different shaders we can possibly draw with</summary>
    public enum RenderEffect
    {
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

        GraphicsDevice              m_device;       //ref
        Camera                      m_camera;       //ref
        BuildCamera                 m_buildCamera;  //ref

        Viewport                    m_origViewport;

        RenderTarget2D              m_diffuseTarget; 
        RenderTarget2D              m_depthTarget;

        DebugRenderMode             m_debugRenderMode = DebugRenderMode.RENDER_DEFAULT;
        RasterizerState             m_wireFrameRasterizer;
        RasterizerState             m_solidRasterizer;

        Vector2                     m_halfPixel;

        TextureCube                 m_skyBoxCubeMap; //ref

        ScreenAlignedQuad           m_screenAlignedQuad;

        EffectManager               m_effectMan;
        public static RenderPhase   m_renderPhase;

        int                         m_currentHold = 0;
        bool                        m_holdSwitch = false;

        SpriteFont                  m_font;

        #endregion Member Variables


        #region Initialization

        /// <summary>Null Constructor, must call Initialize()</summary>
        private Renderer() { }

        /// <summary>Main Initialization </summary>
        /// <param name="device">the device we'll be rendering with</param>
        /// <param name="startTime">the time our renderer turned on</param>
        public void Initialize(GraphicsDevice graphics, ContentManager content, int startTime)
        {
            m_device = graphics;
            m_gameStart = startTime;
            m_halfPixel = new Vector2(
                .5f / (float)m_device.Viewport.Width,
                .5f / (float)m_device.Viewport.Height);
            m_origViewport = graphics.Viewport;

            CreateRenderTargets();

            LoadShaders(content);

            m_screenAlignedQuad = new ScreenAlignedQuad(m_device);

            m_font = content.Load<SpriteFont>("fonts\\Pericles");
            if (m_font == null)
                throw new ArgumentNullException("Renderer::Initialize : m_font loaded null");

            m_wireFrameRasterizer = new RasterizerState();
            m_wireFrameRasterizer.FillMode = FillMode.WireFrame;
            m_solidRasterizer = m_device.RasterizerState;
        }


        #endregion Init


        #region Run-Time

        /// <summary>Draw in wireFrame</summary>
        public void FlipToWireframe()
        {
            m_device.RasterizerState = m_wireFrameRasterizer;
        }

        /// <summary> Draw normal solid triangles</summary>
        public void FlipToSolid()
        {
            m_device.RasterizerState = m_solidRasterizer;
        }

        /// <summary> Standard dump of the backbuffer</summary>
        public void ClearBackBuffer()
        {
            m_device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Bisque, 1.0f, 0);
        }

        /// <summary>Dump any data thats in the depthbuffer</summary>
        public void ClearDepthBuffer()
        {
            m_device.Clear(ClearOptions.DepthBuffer, Color.Bisque, 1.0f, 0);
        }

        /// <summary>Sets the device's render target to our main diffuse texture</summary>
        public void BeginDiffuseScene()
        {
            m_device.SetRenderTarget(m_diffuseTarget);
            this.ClearBackBuffer();
            m_renderPhase = RenderPhase.PHASE_DIFFUSE;
        }

        /// <summary>Sets the device's render target to the depth texture,
        /// because every object will draw with the same technique, set it now</summary>
        public void BeginDepthScene()
        {
            m_device.SetRenderTarget(m_depthTarget);
            this.ClearBackBuffer();
            m_renderPhase = RenderPhase.PHASE_DEPTH;
            Effect fx = m_effectMan.GetEffect(RenderEffect.RFX_TOON);
            fx.CurrentTechnique = fx.Techniques["NormalDepth"];
        }

        /// <summary>begin's the spriteBatch</summary>
        public void Begin2D(SpriteBatch sBatch)
        {
            sBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        }

        /// <summary>Sets the device's render states correctly to draw 3D</summary>
        public void Begin3D()
        {
            m_device.BlendState = BlendState.Opaque;
            m_device.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>Resets the device's render target and ends the spriteBatch</summary>
        /// <param name="spriteBatch">the spritebatch to end()</param>
        public void End2D(SpriteBatch sBatch)
        {
            sBatch.End();
        }

        /// <summary>Compile Diffuse & Depth World Textures to the backbuffer.</summary>
        public void PresentRender()
        {
            m_device.SetRenderTarget(null);
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

            if (m_camera != null)
            {
                toonMaster.Parameters["View"].SetValue(m_camera.ViewMatrix);
                toonMaster.Parameters["ViewInverse"].SetValue(Matrix.Invert(m_camera.ViewMatrix));
                toonMaster.Parameters["Projection"].SetValue(m_camera.ProjectionMatrix);
                toonMaster.Parameters["CameraPosition"].SetValue(m_camera.Position);
            }
            toonMaster.Parameters["Time"].SetValue((float)time.TotalGameTime.TotalSeconds);
        }


        #endregion Run-Time


        #region Privates

        /// <summary>initialize the imposter textures</summary>
        private void CreateRenderTargets()
        {
            m_diffuseTarget = new RenderTarget2D(m_device,
                m_device.PresentationParameters.BackBufferWidth,
                m_device.PresentationParameters.BackBufferHeight,
                true,
                m_device.DisplayMode.Format,
                DepthFormat.Depth24,
                m_device.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

            m_depthTarget = new RenderTarget2D(m_device,
                m_device.PresentationParameters.BackBufferWidth,
                m_device.PresentationParameters.BackBufferHeight,
                true,
                m_device.DisplayMode.Format,
                DepthFormat.Depth24,
                m_device.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

        }

        /// <summary>initialize our cartoon shaders</summary>
        private void LoadShaders(ContentManager content)
        {
            m_effectMan = new EffectManager();

            Effect loadfx = content.Load<Effect>(@"shaders\ToonMaster");
            if(Game1.runLowDef == false)
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

        public SpriteFont GameFont
        {
            get { return m_font; }
        }
        public EffectManager EffectManager
        {
            get { return m_effectMan; }
        }
        public Viewport OriginalViewport
        {
            get { return m_origViewport; }
        }
        public TextureCube SkyBoxTexture
        {
            set { m_skyBoxCubeMap = value; }
        }
        public Camera Camera
        {
            set { m_camera = value; }
            get { return m_camera; }
        }
        public BuildCamera BuildCamera
        {
            set { m_buildCamera = value; }
            get { return m_buildCamera; }
        }
        public DebugRenderMode DebugRenderMode
        {
            set { m_debugRenderMode = value; }
        }
        public GraphicsDevice Device
        {
            get { return this.m_device; }
        }

        #endregion //Mutators

    }
}
