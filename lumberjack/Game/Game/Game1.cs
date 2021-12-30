using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SkinnedModel;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Game_Objects.Trees;
using Game.Managers;
using Game.Math_Physics;

namespace Game
{

    /// <summary>THE GAME</summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        #region Game Declarations

        public GraphicsDeviceManager    graphics;
        public SpriteBatch              spriteBatch;

        Renderer                        render;
        Camera                          camera;
        public static Random            random  = new Random();


        WorldManager                    world;
        Player                          player;

        LevelEditor                     levelEditor;

        //---------------------------------------SAVING

        bool canSave = true;
        int saveWaitCount = 0;
        int saveMaxWait = 1000;

        //---------------------------------------SAVING


        #endregion Game Declarations


        #region Initialization

        /// <summary>Game Constructor</summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "A Work In Progess...";
            Window.AllowUserResizing = false;

            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1440;

            graphics.ApplyChanges();
        }



        /// <summary>Allows the game to perform any initialization it needs to before starting to run.</summary>
        protected override void Initialize()
        {
            camera = new Camera(graphics.GraphicsDevice.Viewport);
            camera.Initialize(CameraType.CAM_FREE, graphics.GraphicsDevice.Viewport.AspectRatio);
            
            //must set camera before initialize
            render = Renderer.getInstance();
            render.Camera = camera;
            render.Initialize(graphics, Content, System.Environment.TickCount);

            world = WorldManager.getInstance();

            player = new Player(graphics.GraphicsDevice);

            levelEditor = new LevelEditor();

            base.Initialize();
        }



        /// <summary>LoadContent will be called once per game and is the place to load</summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            world.Initialize(Content, graphics.GraphicsDevice, camera);
            world.BuildLand(Content);
            world.BuildWater(Content);
            world.BuildProps(Content);

            player.Initialize(Content);

            levelEditor.Initialize(Content, graphics.GraphicsDevice, camera);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                (Keyboard.GetState().IsKeyDown(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)))
                this.Exit();

            KeyboardState kb = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            GamePadState gp = GamePad.GetState(0);

            UpdateSaveTimer(gameTime);

            camera.FreeCamInput(gameTime, kb, ms, gp);
            camera.Update(Vector3.Zero);

            render.Update(gameTime);

            world.Update(gameTime);

            //Debug render modes
            if (kb.IsKeyDown(Keys.NumPad0))
                render.DebugRenderMode = DebugRenderMode.RENDER_DEFAULT;
            if (kb.IsKeyDown(Keys.NumPad1))
                render.DebugRenderMode = DebugRenderMode.RENDER_DIFFUSE;
            if (kb.IsKeyDown(Keys.NumPad2))
                render.DebugRenderMode = DebugRenderMode.RENDER_NORMALS;
            if (kb.IsKeyDown(Keys.NumPad3))
                render.DebugRenderMode = DebugRenderMode.RENDER_SILHOUETTE;
            if (kb.IsKeyDown(Keys.NumPad4))
                render.DebugRenderMode = DebugRenderMode.RENDER_DEPTH;

            if (kb.IsKeyDown(Keys.F11) && canSave == true)
            {
                world.Save(Content);
                canSave = false;
            }

            levelEditor.Input(kb, ms);
            levelEditor.Update(gameTime);


            base.Update(gameTime);
        }


        /// <summary>This is called when the game should draw itself.</summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            render.BeginDiffuseScene();
            render.Begin3D();


            world.Draw();
            player.Draw();
            levelEditor.Draw();


            render.BeginDepthScene();


            world.Draw();
            player.Draw();
            levelEditor.Draw();

            
            render.PresentRender(spriteBatch);

            render.Begin2D(spriteBatch);

            levelEditor.DrawText(spriteBatch);
            DrawText();

            render.End2D(spriteBatch);

            base.Draw(gameTime);
        }



        private void UpdateSaveTimer(GameTime time)
        {
            if (canSave == false)
            {
                saveWaitCount += time.ElapsedGameTime.Milliseconds;
                if (saveWaitCount >= saveMaxWait)
                {
                    saveWaitCount = 0;
                    canSave = true;
                }
            }
        }

        private void DrawText()
        {
            if (canSave == false)
            {
                spriteBatch.DrawString(levelEditor.Font, "SAVE COMPLETED", new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, 20), Color.DarkRed);
            }
        }


        
    }

        #endregion Update & Draw
}
