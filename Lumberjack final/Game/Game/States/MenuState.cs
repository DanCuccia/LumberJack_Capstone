using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Managers;

#pragma warning disable 0168 //unused exception e

namespace Game.States
{
    class MenuState : State
    {

        #region Member Variables

        WorldManager            world;   //ref
        Camera                  camera;  //ref
        AudioManager            audio;   //ref

        SpriteFont              font;

        UncontrolledSprite      title;

        TextureManager          texMan = new TextureManager();
        SpriteManager           spriteMan;

        AnimatedMesh            andy;
        float                   searchTime = 22000;
        float                   searchEvery = 25000;

        Texture2D               loadingTex;
        Texture2D               fadeTexture;
        bool                    isFadingOut = false;
        bool                    isFadingIn = true;
        int                     currentAlpha = 255;

        #endregion Member Variables


        #region Initialization

        /// <summary>Default CTOR</summary>
        public MenuState(Game1 g, StateManager sm)
            : base(g, "menu", sm)
        {
            spriteMan = new SpriteManager(g.spriteBatch);
        }

        /// <summary>load assets and initialize variables </summary>
        public override void initialize() 
        {
            world = WorldManager.getInstance();
            world.InitializeForMainMenu(base.content, base.device);
            world.BuildForMainMenu(base.content);

            camera = world.Camera;
            Renderer.getInstance().Camera = camera;

            font = Renderer.getInstance().GameFont;

            audio = AudioManager.getInstance();

            camera.CameraType = CameraType.CAM_STATIONARY;
            camera.Position = new Vector3(6066.645f, 553.0523f, 7765.482f);
            camera.LookAtTarget = new Vector3(6066.164f, 552.8821f, 7764.622f);

            initAndy();
            initTitle();
            initButtons();

            audio.LoadSound("audio\\wind", "wind");
            audio.LoadSound("audio\\birds", "birds");
            audio.LoadSound("audio\\negative", "negative");

            SoundClip snd = audio.Play2DSound("wind", true);
            snd.soundInstance.Volume = .4f;

            snd = audio.Play2DSound("birds", true);
            snd.soundInstance.Volume = .6f;

            loadingTex = base.content.Load<Texture2D>("sprites\\loading");
            fadeTexture = base.content.Load<Texture2D>("textures\\blank");
            currentAlpha = 255;

            base.isInitialized = true;
        }

        /// <summary>init the title image</summary>
        private void initTitle()
        {
            title = new UncontrolledSprite();
            Texture2D tex;
            if (Game1.runLowDef == true)
            {
                tex = base.content.Load<Texture2D>("sprites\\mmTitle1024");
            }
            else
            {
                tex = base.content.Load<Texture2D>("sprites\\mmTitle1280");
            }
            title.Initialize(new Point(tex.Width, tex.Height), new Point(1, 1), tex);
            title.Position = new Vector2(base.device.Viewport.Width / 2, title.FrameSize.Y / 2);
        }

        /// <summary>Build a temporary Andy animatedMesh, privately called in Initialize()</summary>
        private void initAndy()
        {
            andy = new AnimatedMesh();
            andy.Position = new Vector3(6000f, 429.336f, 7712.851f);
            andy.RotationY = -205f;
            andy.Initialize(base.content, "models\\player\\andyMMAnims", "textures\\andy_diffuse");
            andy.BeginAnimation("idle");
        }

        /// <summary>initialize all of the buttons</summary>
        private void initButtons()
        {
            Vector2 pos = new Vector2((
                Renderer.getInstance().OriginalViewport.Width / 4) * 3,
                Renderer.getInstance().OriginalViewport.Height / 2);

            Texture2D tex = base.content.Load<Texture2D>("sprites\\btn_newGame");
            texMan.addTexture("btn_newGame", tex);
            Sprite btn = new Sprite("btn_newGame", pos, "btn_newGame", spriteMan, texMan, newGameCallback);
            spriteMan.addSprite(btn);

            pos.X += 5;
            pos.Y += 5;

            btn = new Sprite("btn_newGame_back", pos, "btn_newGame", spriteMan, texMan);
            btn.tint = Color.Black;
            btn.Clickable = false;
            spriteMan.SpriteList.Insert(0, btn);

            pos.X -= 5;
            pos.Y -= 5;
            pos.Y += tex.Height;

            tex = base.content.Load<Texture2D>("sprites\\btn_continueGame");
            texMan.addTexture("btn_continueGame", tex);
            btn = new Sprite("btn_continueGame", pos, "btn_continueGame", spriteMan, texMan, continueGameCallback);
            spriteMan.addSprite(btn);

            pos.X += 5;
            pos.Y += 5;

            btn = new Sprite("btn_continueGame_back", pos, "btn_continueGame", spriteMan, texMan, continueGameCallback);
            btn.tint = Color.Black;
            btn.Clickable = false;
            spriteMan.SpriteList.Insert(0, btn);

            pos.X -= 5;
            pos.Y -= 5;
            pos.Y += tex.Height;

            tex = base.content.Load<Texture2D>("sprites\\btn_quitGame");
            texMan.addTexture("btn_quitGame", tex);
            btn = new Sprite("btn_quitGame", pos, "btn_quitGame", spriteMan, texMan, quitGameCallback);
            spriteMan.addSprite(btn);

            pos.X += 5;
            pos.Y += 5;

            btn = new Sprite("btn_quitGame_back", pos, "btn_quitGame", spriteMan, texMan);
            btn.tint = Color.Black;
            btn.Clickable = false;
            spriteMan.SpriteList.Insert(0, btn);
        }

        #endregion Initialization


        #region API

        /// <summary>Main Input call</summary>
        public override void input(KeyboardState kb, MouseState ms)
        {
            camera.Input(kb, ms);
            camera.Update(Vector3.Zero);
        }

        /// <summary>Main update call</summary>
        public override void update(GameTime time) 
        {
            searchTime += time.ElapsedGameTime.Milliseconds;
            if (searchTime >= searchEvery)
            {
                andy.BeginAnimation("search", false, searchCompleteCallback);
                searchTime = 0;
            }
            world.Update(time);
            andy.Update(time);

            spriteMan.update(time);

            updateFade();
        }

        /// <summary>Update the black fade in & out</summary>
        private void updateFade()
        {
            if (isFadingOut)
                if (currentAlpha < 255)
                    currentAlpha += 3;
                else
                    fadeComplete();
            else if (isFadingIn)
                if (currentAlpha > 0)
                    currentAlpha -= 3;
                else
                {
                    currentAlpha = 0;
                    isFadingIn = false;
                }
        }

        /// <summary>Main draw 2D call</summary>
        public override void render2D(GameTime time, SpriteBatch batch) 
        { 
            title.Draw(batch);
            spriteMan.renderSprites();

            if (isFadingOut)
                drawFadeOut(batch);
            if (isFadingIn)
                drawFadeIn(batch);
        }

        /// <summary> When fading out of the state, this will draw that</summary>
        private void drawFadeOut(SpriteBatch batch)
        {
            batch.Draw(this.fadeTexture,
                new Rectangle(0, 0, Renderer.getInstance().OriginalViewport.Width, Renderer.getInstance().OriginalViewport.Height),
                new Rectangle(0, 0, 64, 64), Color.FromNonPremultiplied(0, 0, 0, currentAlpha));
            batch.Draw(this.loadingTex,
                new Vector2(Renderer.getInstance().OriginalViewport.Width - loadingTex.Width,
                    Renderer.getInstance().OriginalViewport.Height - loadingTex.Height),
                    Color.White);
        }

        /// <summary>When fading in from a state, this will draw that</summary>
        private void drawFadeIn(SpriteBatch batch)
        {
            batch.Draw(this.fadeTexture,
                new Rectangle(0, 0, Renderer.getInstance().OriginalViewport.Width, Renderer.getInstance().OriginalViewport.Height),
                new Rectangle(0, 0, 64, 64), Color.FromNonPremultiplied(0, 0, 0, currentAlpha));
        }

        /// <summary>Main draw 3D call</summary>
        public override void render3D(GameTime time) 
        {
            world.Draw();
            andy.Draw();
        }

        /// <summary>Dispose and null loaded content</summary>
        public override void close() 
        {
            isFadingOut = false;
            isFadingIn = true;
            currentAlpha = 255;
            loadingTex = null;
            fadeTexture = null;
            andy = null;
            world.ClearAll();
            spriteMan.empty();
            isInitialized = false;

            audio.ClearAll();
        }

        #endregion API


        #region Callbacks

        /// <summary>the new game button calls this when clicked</summary>
        public void newGameCallback()
        {
            isFadingOut = true;
            GameState state = (GameState)base.stateMan.getState("game");
            state.ContinueGame = false;
        }

        /// <summary>continue game btn calls this when clicked, only if playerSave.xml exists</summary>
        public void continueGameCallback()
        {
            try
            {
                TextReader reader = new StreamReader("playerSave.xml");
            }
            catch (Exception e)
            {
                audio.Play2DSound("negative", false);
                return;
            }

            isFadingOut = true;
            GameState state = (GameState)base.stateMan.getState("game");
            state.ContinueGame = true;
        }

        /// <summary>When the fade is completed in update, this finally gets called</summary>
        public void fadeComplete()
        {
            stateMan.setCurrentState("game");
        }

        /// <summary>quit button calls this when clicked</summary>
        public void quitGameCallback()
        {
            audio.ClearAll();
            stateMan.forceClosed();
        }

        /// <summary>Animation calls this when completed the "search" animation</summary>
        public void searchCompleteCallback()
        {
            andy.BeginAnimation("idle");
            searchTime = 0;
        }

        #endregion Callbacks
    }
}
