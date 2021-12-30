using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Game_Objects.Trees;
using Game.Managers;
using Game.Math_Physics;
using Game.Game_Objects.Build_System;


namespace Game
{

    /// <summary>THE GAME</summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        #region Game Declarations

        public GraphicsDeviceManager    graphics;
        public SpriteBatch              spriteBatch;

        Renderer                        render;

        public static Random            random              = new Random();

        public static bool              drawDevelopment     = false;
        public static bool              runLowDef           = false;
        public static bool              noCollision         = false;

        StateManager stateMan;

        #endregion Game Declarations


        #region Initialization

        /// <summary>Game Constructor</summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "Lumber Jack";
            Window.AllowUserResizing = false;

            switch (Game1.runLowDef)
            {
                case true:
                    graphics.PreferredBackBufferWidth = 1024;
                    graphics.PreferredBackBufferHeight = 768;
                    break;

                case false:
                    graphics.PreferredBackBufferWidth = 1280;
                    graphics.PreferredBackBufferHeight = 800;
                    break;
            }
            

            graphics.ApplyChanges();

            IsMouseVisible = true;
        }

        /// <summary>Allows the game to perform any initialization it needs to before starting to run.</summary>
        protected override void Initialize()
        {
            base.Initialize();

            render = Renderer.getInstance();
            render.Initialize(graphics.GraphicsDevice, Content, System.Environment.TickCount);

            AudioManager audio = AudioManager.getInstance();
            audio.Initialize(Content);

            stateMan = new StateManager(this);

            Infobox.background = Content.Load<Texture2D>("sprites\\tooltipBack");
            Infobox.content = Content;
        }
        
        /// <summary>LoadContent will be called once per game and is the place to load</summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }


        /// <summary>UnloadContent will be called once per game and is the place to unload</summary>
        protected override void UnloadContent() 
        {
            Content.Unload();
        }


        #endregion init


        #region Update & Draw

        /// <summary>Game Logic</summary>
        protected override void Update(GameTime gameTime)
        {

            KeyboardState kb = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            stateMan.input(kb, ms);
            stateMan.update(gameTime);

            render.Update(gameTime);

            //Debug render modes
            //if (kb.IsKeyDown(Keys.NumPad0))
            //    render.DebugRenderMode = DebugRenderMode.RENDER_DEFAULT;
            //if (kb.IsKeyDown(Keys.NumPad1))
            //    render.DebugRenderMode = DebugRenderMode.RENDER_DIFFUSE;
            //if (kb.IsKeyDown(Keys.NumPad2))
            //    render.DebugRenderMode = DebugRenderMode.RENDER_NORMALS;
            //if (kb.IsKeyDown(Keys.NumPad3))
            //    render.DebugRenderMode = DebugRenderMode.RENDER_SILHOUETTE;
            //if (kb.IsKeyDown(Keys.NumPad4))
            //    render.DebugRenderMode = DebugRenderMode.RENDER_DEPTH;


            base.Update(gameTime);
        }


        /// <summary>This is called when the game should draw itself.</summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            render.BeginDiffuseScene();
            render.Begin3D();
            stateMan.render3D(gameTime);

            render.BeginDepthScene();
            stateMan.render3D(gameTime);
            render.PresentRender();

            render.Begin2D(spriteBatch);
            stateMan.render2D(gameTime, spriteBatch);
            render.End2D(spriteBatch);

            base.Draw(gameTime);
        }


        #endregion Update & Draw


        #region Static API
        
        /// <summary>get the current mouse ray according to the elaborte game-gam</summary>
        /// <param name="viewport">the current viewport</param>
        /// <param name="camera">the game-cam</param>
        /// <returns>current mouse ray</returns>
        public static Ray GetMouseRay(Viewport viewport, Camera camera)
        {
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            Vector3 nearPoint = new Vector3(mousePosition, 0);
            Vector3 farPoint = new Vector3(mousePosition, 1);

            nearPoint = viewport.Unproject(nearPoint, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        /// <summary>get the current mouse ray according to the slim build-cam</summary>
        /// <param name="viewport">current viewport</param>
        /// <param name="camera">build-cam</param>
        /// <returns>current mouse ray</returns>
        public static Ray GetMouseRay(Viewport viewport, BuildCamera camera)
        {
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            Vector3 nearPoint = new Vector3(mousePosition, 0);
            Vector3 farPoint = new Vector3(mousePosition, 1);

            nearPoint = viewport.Unproject(nearPoint, camera.projection, camera.view, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, camera.projection, camera.view, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        /// <summary>Get the viewport used for the tablet, dynamically sized to current screen settings</summary>
        /// <param name="entireScreen">the full-viewport</param>
        /// <returns>the viewport sized for the tablet</returns>
        public static Viewport GetTabletViewport(Viewport entireScreen)
        {
            Viewport output = entireScreen;

            switch (runLowDef)
            {
                case true:
                    output.X = 98;
                    output.Y = 126;
                    output.Width = 832;
                    output.Height = 530;
                    break;

                case false:
                    output.X = 206;
                    output.Y = 128;
                    output.Width = 870;
                    output.Height = 555;
                    break;
            }

            return output;
        }

        #endregion Static API

    }

        
}
