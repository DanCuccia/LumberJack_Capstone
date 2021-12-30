using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Managers;

namespace Game.States
{
    class GameMenuState : State
    {
        #region Member Variables

        Viewport viewport;
        AudioManager audio; //ref

        TextureManager texManager = new TextureManager();
        SpriteManager spriteManager = null;

        Keys pressedKey = Keys.None;


        Texture2D fadeTexture;
        bool isFadingOut = false;
        short currentAlpha = 0;

        UncontrolledSprite savingImg = null;
        bool saveFadingIn = true;
        const int saveDuration = 4000;
        int currentDuration = 0;
        bool saveImageComplete = false;

        SpriteFont tabletFont;

        #endregion Member Variables


        #region Initialization

        /// <summary>Default CTOR</summary>
        public GameMenuState(Game1 g, StateManager man)
            : base(g, "gameMenu", man)
        {
            spriteManager = new SpriteManager(g.spriteBatch);
            audio = AudioManager.getInstance();
        }

        /// <summary>Initialize member variables</summary>
        public override void initialize()
        {
            viewport = base.device.Viewport;
            viewport = Game1.GetTabletViewport(viewport);

            texManager.addTexture("texTabletBack", content.Load<Texture2D>("sprites\\tabletBack"));
            spriteManager.addSprite(new Sprite("sprTabletBack", new Vector2(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2), "texTabletBack", spriteManager, texManager));

            initButtons();
            fadeTexture = base.content.Load<Texture2D>("textures\\blank");

            initSavingImage();

            tabletFont = content.Load<SpriteFont>("fonts\\tabletText");

            base.isInitialized = true;
        }

        /// <summary>initialize all buttons for this state</summary>
        private void initButtons()
        {
            Vector2 btnPos = new Vector2(viewport.X + 115, viewport.Height + 80);

            //build btn
            Texture2D tex = base.content.Load<Texture2D>("sprites\\buildApp");
            texManager.addTexture("btn_build", tex);
            Sprite btn = new Sprite("btn_build", btnPos, "btn_build",
                spriteManager, texManager, BtnCallback_Build);
            spriteManager.addSprite(btn);

            btnPos.X += 128;

            //inventory btn
            tex = base.content.Load<Texture2D>("sprites\\inventoryApp");
            texManager.addTexture("btn_inventory", tex);
            btn = new Sprite("btn_inventory", btnPos, "btn_inventory",
                spriteManager, texManager, BtnCallback_Inventory);
            spriteManager.addSprite(btn);

            btnPos.X += 128;

            //map btn
            tex = base.content.Load<Texture2D>("sprites\\mapApp");
            texManager.addTexture("btn_map", tex);
            btn = new Sprite("btn_map", btnPos, "btn_map",
                spriteManager, texManager, BtnCallback_Map);
            spriteManager.addSprite(btn);

            btnPos.X += 128;

            //save game btn
            tex = base.content.Load<Texture2D>("sprites\\saveApp");
            texManager.addTexture("btn_save", tex);
            btn = new Sprite("btn_save",
                btnPos,
                "btn_save", spriteManager, texManager, BtnCallback_Save);
            spriteManager.addSprite(btn);

            btnPos.X += 128;

            //quit to menu btn
            tex = base.content.Load<Texture2D>("sprites\\quitApp");
            texManager.addTexture("btn_quit", tex);
            btn = new Sprite("btn_quit",
                btnPos,
                "btn_quit", spriteManager, texManager, BtnCallback_Quit);
            spriteManager.addSprite(btn);

            btnPos.X += 128;

            tex = base.content.Load<Texture2D>("sprites\\closeApp");
            texManager.addTexture("btn_closeTablet", tex);
            btn = new Sprite("btn_closeTablet", btnPos,
                "btn_closeTablet", spriteManager, texManager, BtnCallback_closeTablet);
            spriteManager.addSprite(btn);

        }

        /// <summary>initialize the "saving..." image</summary>
        private void initSavingImage()
        {
            savingImg = new UncontrolledSprite();
            Texture2D tex = base.content.Load<Texture2D>("sprites\\saving");
            savingImg.Initialize(new Point(tex.Width, tex.Height), new Point(1, 1), tex);
            savingImg.Visible = false;
            savingImg.Opacity = 0;
            savingImg.Position = new Vector2(
                Renderer.getInstance().OriginalViewport.Width - (tex.Width/2),
                Renderer.getInstance().OriginalViewport.Height - (tex.Height/2));
        }

        /// <summary>Close the game menu</summary>
        public override void close()
        {
            spriteManager.empty();
            this.isInitialized = false;
            currentAlpha = 0;
            isFadingOut = false;
        }

        #endregion Initialization


        #region Run-Time

        /// <summary>menu user-input called before update()</summary>
        public override void input(KeyboardState kb, MouseState ms)
        {
            #region close menu
            if (kb.IsKeyDown(Keys.Tab))
            {
                pressedKey = Keys.Tab;
            }
            else if (kb.IsKeyUp(Keys.Tab) && pressedKey == Keys.Tab)
            {
                pressedKey = Keys.None;
                this.BtnCallback_closeTablet();
            }
            #endregion close menu

        }

        /// <summary>main update called after input()</summary>
        public override void update(GameTime time)
        {

            if (isFadingOut)
                if (currentAlpha < 255)
                    currentAlpha += 3;
                else
                {
                    base.stateMan.closeOverlapState();
                    base.stateMan.setCurrentState("menu");
                }

            spriteManager.update(time);
            updateSavingImage(time);
        }

        /// <summary>Draw all 2D</summary>
        public override void render2D(GameTime time, SpriteBatch batch)
        {
            spriteManager.renderSprites();

            drawButtonNames(batch);
            if (isFadingOut)
                drawFade(batch);

            String dateString = null;
            String timeString = null;

            if (!isFadingOut)
            {
                dateString = DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + DateTime.Now.Year;
                timeString = DateTime.Now.Hour.ToString() + ":";
            

                if (DateTime.Now.Minute < 10)
                {
                    timeString += "0";
                }

                timeString += DateTime.Now.Minute.ToString();

                batch.DrawString(content.Load<SpriteFont>("fonts\\Arial14"), dateString, new Vector2(viewport.X + 691, viewport.Y + 6), Color.Black);
                batch.DrawString(content.Load<SpriteFont>("fonts\\Arial14"), dateString, new Vector2(viewport.X + 690, viewport.Y + 5), Color.White);

                batch.DrawString(content.Load<SpriteFont>("fonts\\Arial14"), timeString, new Vector2(viewport.X + 801, viewport.Y + 6), Color.Black);
                batch.DrawString(content.Load<SpriteFont>("fonts\\Arial14"), timeString, new Vector2(viewport.X + 800, viewport.Y + 5), Color.White);
            }

            savingImg.Draw(batch);
        }

        /// <summary>Draw the black fade out if we exit the game</summary>
        private void drawFade(SpriteBatch batch)
        {
            batch.Draw(this.fadeTexture,
                new Rectangle(0, 0, Renderer.getInstance().OriginalViewport.Width, Renderer.getInstance().OriginalViewport.Height),
                new Rectangle(0, 0, 64, 64), Color.FromNonPremultiplied(0, 0, 0, currentAlpha));
        }

        /// <summary>Update the flashing saving image, if the player saves his game</summary>
        private void updateSavingImage(GameTime time)
        {
            if (savingImg.Visible == false)
                return;

            if (saveImageComplete == false)
            {
                currentDuration += time.ElapsedGameTime.Milliseconds;
                if (currentDuration >= saveDuration)
                {
                    saveImageComplete = true;
                    currentDuration = 0;
                }
            }

            if (saveFadingIn == true)
            {
                savingImg.Opacity += 4;
                if (savingImg.Opacity >= 255)
                {
                    savingImg.Opacity = 255;
                    saveFadingIn = false;
                }
            }
            else
            {
                savingImg.Opacity -= 4;
                if (savingImg.Opacity <= 0)
                {
                    savingImg.Opacity = 0;
                    saveFadingIn = true;
                    if (saveImageComplete == true)
                    {
                        savingImg.Visible = false;
                        saveImageComplete = false;
                    }
                }
            }
        }

        /// <summary>Draw all 3D</summary>
        public override void render3D(GameTime time)
        { }

        /// <summary>w.i.p. -- draw button names, dynamically under the logo</summary>
        private void drawButtonNames(SpriteBatch batch)
        {
            batch.DrawString(tabletFont, "Build", new Vector2(viewport.X + 101, viewport.Height + 109), Color.Black);
            batch.DrawString(tabletFont, "Inventory", new Vector2(viewport.X + 216, viewport.Height + 109), Color.Black);
            batch.DrawString(tabletFont, "Map", new Vector2(viewport.X + 359, viewport.Height + 109), Color.Black);
            batch.DrawString(tabletFont, "Save", new Vector2(viewport.X + 486, viewport.Height + 109), Color.Black);
            batch.DrawString(tabletFont, "Quit", new Vector2(viewport.X + 614, viewport.Height + 109), Color.Black);
            batch.DrawString(tabletFont, "Close", new Vector2(viewport.X + 738, viewport.Height + 109), Color.Black);

            batch.DrawString(tabletFont, "Build", new Vector2(viewport.X + 100, viewport.Height + 108), Color.White);
            batch.DrawString(tabletFont, "Inventory", new Vector2(viewport.X + 215, viewport.Height + 108), Color.White);
            batch.DrawString(tabletFont, "Map", new Vector2(viewport.X + 358, viewport.Height + 108), Color.White);
            batch.DrawString(tabletFont, "Save", new Vector2(viewport.X + 485, viewport.Height + 108), Color.White);
            batch.DrawString(tabletFont, "Quit", new Vector2(viewport.X + 613, viewport.Height + 108), Color.White);
            batch.DrawString(tabletFont, "Close", new Vector2(viewport.X + 737, viewport.Height + 108), Color.White);
        }

        #endregion Run-Time


        #region Button Callbacks
        /// <summary>push the build btn</summary>
        public void BtnCallback_Build()
        {
            base.stateMan.closeOverlapState();
            base.stateMan.showOverlapState("build");
        }
        /// <summary>push the quit button</summary>
        public void BtnCallback_Quit()
        {
            isFadingOut = true;
            currentAlpha = 0;
            audio.ClearAll();

        }
        /// <summary>push the save button</summary>
        public void BtnCallback_Save()
        {
            savingImg.Visible = true;
            WorldManager.getInstance().SaveTriggerList();

            GameState gameState = base.stateMan.getState("game") as GameState;
            Player gamePlayer = gameState.getPlayer();
            gamePlayer.Save();

            gameState.savePlayerLogicProps();
        }
        /// <summary>push the inventory btn</summary>
        public void BtnCallback_Inventory()
        {
            base.stateMan.closeOverlapState();
            base.stateMan.showOverlapState("inventory");
        }
        /// <summary>pushed the map state button</summary>
        public void BtnCallback_Map()
        {
            base.stateMan.closeOverlapState();
            base.stateMan.showOverlapState("map");
        }

        /// <summary>when the player pressed tab, or close btn</summary>
        public void BtnCallback_closeTablet()
        {
            GameState gameState = base.stateMan.getState("game") as GameState;
            Player gamePlayer = gameState.getPlayer();
            Camera gameCam = WorldManager.getInstance().Camera;

            gameCam.SmoothStepTo(gamePlayer.camOrigTarget, gamePlayer.camOrigPosition, gamePlayer.closeTabletCompleteCallback);
            gamePlayer.AnimatedMesh.BeginAnimation("closeTablet", false, gamePlayer.defaultCallback);
            gamePlayer.Tablet.IsVisible = false;
            gamePlayer.CanExecute = false;
            gamePlayer.AnimPlaying = true;
            stateMan.closeOverlapState();
        }

        #endregion Button Callbacks

    }
}
