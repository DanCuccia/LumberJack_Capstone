using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Managers;
using Game.Game_Objects.Build_System;

namespace Game.States
{
    class InventoryState : State
    {
        #region Member Variables

        AudioManager            audio; //ref

        Viewport                viewport;
        Player                  player;

        Keys                    pressedKey      = Keys.None;

        SpriteManager spriteManager;
        TextureManager texManager;

        SpriteFont pericles;

        #endregion Member Variables


        #region Initialization

        /// <summary>Default CTOR</summary>
        public InventoryState(Game1 g, StateManager man)
            : base(g, "inventory", man)
        {
            spriteManager = new SpriteManager(g.spriteBatch);
            texManager = new TextureManager();
        }

        /// <summary>Initialize member variables</summary>
        public override void initialize()
        {
            viewport = base.device.Viewport;
            viewport = Game1.GetTabletViewport(Renderer.getInstance().OriginalViewport);

            pericles = Renderer.getInstance().GameFont;

            audio = AudioManager.getInstance();

            GameState gameState = base.stateMan.getState("game") as GameState;
            player = gameState.getPlayer();

            initButtons();

            base.isInitialized = true;
        }

        /// <summary>creates any buttons for this state</summary>
        private void initButtons()
        {
            //Background
            Texture2D backTex = base.content.Load<Texture2D>("sprites\\inventoryBackground");
            texManager.addTexture("texInventoryBackground", backTex);
            Sprite backSpr = new Sprite("sprInventoryBackground", new Vector2(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2), "texInventoryBackground", spriteManager, texManager);
            spriteManager.addSprite(backSpr);

            //go back btn
            Texture2D tex = base.content.Load<Texture2D>("sprites\\btn_goBack");
            texManager.addTexture("btn_goBack", tex);
            Sprite sprite = new Sprite("btn_goBack",
                new Vector2(viewport.X + viewport.Width - (tex.Width / 2) - 6, viewport.Y + (tex.Height / 2) + 6),
                "btn_goBack", spriteManager, texManager, BtnCallback_goBack);
            spriteManager.addSprite(sprite);

            //close tablet btn
            tex = base.content.Load<Texture2D>("sprites\\btn_closeTablet");
            texManager.addTexture("btn_closeTablet", tex);
            sprite = new Sprite("btn_closeTablet", new Vector2(viewport.X + (tex.Width / 2), viewport.Y + (tex.Height / 2)), 
                "btn_closeTablet", spriteManager, texManager, BtnCallback_closeTablet);
            spriteManager.addSprite(sprite);

            //TExtures for the make and recycle buttons loaded once
            tex = base.content.Load<Texture2D>("sprites\\inventoryMakeButton");
            texManager.addTexture("btn_make", tex);
            tex = base.content.Load<Texture2D>("sprites\\inventoryRecycleButton");
            texManager.addTexture("btn_recycle", tex);
            
            //Use offsets for dynamic resizing
            Vector2 rowHeight = new Vector2(0, 43);
            Vector2 columnOffset = new Vector2(160, 0);
            Vector2 btnPos = new Vector2(viewport.X + 604, viewport.Y + 187);
            Vector2 currentButtonPos = Vector2.Zero;

            //Make buttons
            currentButtonPos = btnPos + (0*rowHeight) + (0*columnOffset);
            sprite = new Sprite("btn_makeBlock", currentButtonPos, "btn_make", spriteManager, texManager, player.MakeBlock);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (1*rowHeight) + (0*columnOffset);
            sprite = new Sprite("btn_makePlank", currentButtonPos, "btn_make", spriteManager, texManager, player.MakePlank);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (2*rowHeight) + (0*columnOffset);
            sprite = new Sprite("btn_makeRod", currentButtonPos, "btn_make", spriteManager, texManager, player.MakeRod);
            spriteManager.addSprite(sprite);
            
            currentButtonPos = btnPos + (3*rowHeight) + (0*columnOffset);
            sprite = new Sprite("btn_makeDisk", currentButtonPos, "btn_make", spriteManager, texManager, player.MakeDisk);
            spriteManager.addSprite(sprite);

            //Recycle Buttons
            currentButtonPos = btnPos + (0* rowHeight) + (1 * columnOffset);
            sprite = new Sprite("btn_recycleBlock", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecycleBlock);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (1 * rowHeight) + (1 * columnOffset);
            sprite = new Sprite("btn_recyclePlank", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecyclePlank);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (2 * rowHeight) + (1 * columnOffset);
            sprite = new Sprite("btn_recycleRod", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecycleRod);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (3 * rowHeight) + (1 * columnOffset);
            sprite = new Sprite("btn_recycleDisk", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecycleDisk);
            spriteManager.addSprite(sprite);

            //Lower region
            Vector2 lowerRegionOffset = new Vector2(0, 48);

            //Make buttons
            currentButtonPos = btnPos + (4 * rowHeight) + (0 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_makeCustomOne", currentButtonPos, "btn_make", spriteManager, texManager, MakeCustomOne);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (5 * rowHeight) + (0 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_makeCustomTwo", currentButtonPos, "btn_make", spriteManager, texManager, MakeCustomTwo);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (6 * rowHeight) + (0 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_makeCustomThree", currentButtonPos, "btn_make", spriteManager, texManager, MakeCustomThree);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (7 * rowHeight) + (0 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_makeCustomFour", currentButtonPos, "btn_make", spriteManager, texManager, MakeCustomFour);
            spriteManager.addSprite(sprite);


            //Recycle buttons
            currentButtonPos = btnPos + (4 * rowHeight) + (1 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_recycleCustomOne", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecycleCustomOne);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (5 * rowHeight) + (1 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_recycleCustomTwo", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecycleCustomTwo);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (6 * rowHeight) + (1 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_recycleCustomThree", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecycleCustomThree);
            spriteManager.addSprite(sprite);

            currentButtonPos = btnPos + (7 * rowHeight) + (1 * columnOffset) + lowerRegionOffset;
            sprite = new Sprite("btn_recycleCustomFour", currentButtonPos, "btn_recycle", spriteManager, texManager, player.RecycleCustomFour);
            spriteManager.addSprite(sprite);

        }

        public void MakeCustomOne()
        {
            MakeCompletedObject(0);
        }

        public void MakeCustomTwo()
        {
            MakeCompletedObject(1);
        }

        public void MakeCustomThree()
        {
            MakeCompletedObject(2);
        }

        public void MakeCustomFour()
        {
            MakeCompletedObject(3);
        }

        public void MakeCompletedObject(int index)
        {
            if (index >= player.Inventory.customObjects.Length ||
                index < 0)
                return;

            CompleteObject temp = player.Inventory.customObjects[index];

            if (temp != null)
            {
                CompleteObject objectToPlace = new CompleteObject(temp);

                objectToPlace.stride = temp.stride;
                objectToPlace.position = player.Position;

                GameState gs = stateMan.getState("game") as GameState;

                if (gs != null)
                {
                    //Change the objects scale because its normally tiny
                    //Add it to the gamestate's list of placed objects
                    //Set its available instance count to zero
                    //Clear out that inventory spot
                    objectToPlace.ChangeMajorScale(new Vector3(10, 10, 10));
                    
                    gs.AddUserObject(objectToPlace);

                    player.Inventory.customObjects[index].availableInstances = 0;
                    player.Inventory.customObjects[index] = null;

                    Camera gameCam = WorldManager.getInstance().Camera;

                    gameCam.SmoothStepTo(player.camOrigTarget, player.camOrigPosition, player.closeTabletCompleteCallback);
                    player.AnimatedMesh.BeginAnimation("closeTablet", false, player.defaultCallback);
                    player.Tablet.IsVisible = false;
                    player.CanExecute = false;
                    player.AnimPlaying = true;
                    stateMan.closeOverlapState();                    
                }
                else
                {
                    throw new Exception("Gamestate has disappeared!");
                }
            }
        }

        /// <summary>closing logic</summary>
        public override void close()
        {
            texManager.empty();
            spriteManager.empty();
            pressedKey = Keys.None;
            player = null;
            this.isInitialized = false;
        }

        #endregion Initialization


        #region API

        /// <summary>menu user-input called before update()</summary>
        public override void input(KeyboardState kb, MouseState ms)
        {
            #region close menu
            if (kb.IsKeyDown(Keys.Escape))
            {
                if (pressedKey == Keys.None)
                    audio.Play2DSound("btn_down", false);
                pressedKey = Keys.Escape;
            }
            else if (kb.IsKeyUp(Keys.Escape) && pressedKey == Keys.Escape)
            {
                pressedKey = Keys.None;
                audio.Play2DSound("btn_up", false);
                this.BtnCallback_goBack();
            }

            if (kb.IsKeyDown(Keys.Tab))
            {
                if (pressedKey == Keys.None)
                    audio.Play2DSound("btn_down", false);
                pressedKey = Keys.Tab;
            }
            else if (kb.IsKeyUp(Keys.Tab) && pressedKey == Keys.Tab)
            {
                pressedKey = Keys.None;
                audio.Play2DSound("btn_up", false);
                BtnCallback_closeTablet();
            }
            #endregion close menu
        }

        /// <summary>main update called after input()</summary>
        public override void update(GameTime time)
        {
            spriteManager.update(time);
        }

        #endregion API


        #region Drawing

        /// <summary>Draw all 2D</summary>
        public override void render2D(GameTime time, SpriteBatch batch)
        {
            spriteManager.renderSprites();

            //Render Text
            Vector2 textStart = new Vector2( viewport.X + 400, viewport.Y + 125 );
            Vector2 rowOffset = new Vector2(0, 44);
            Vector2 textPosition;

            textPosition = textStart + (0*rowOffset);
            batch.DrawString(pericles, player.Inventory.lumber.ToString(), textPosition, Color.White);

            textPosition = textStart + (1 * rowOffset);
            batch.DrawString(pericles, player.Inventory.blocks.ToString(), textPosition, Color.White);

            textPosition = textStart + (2 * rowOffset);
            batch.DrawString(pericles, player.Inventory.planks.ToString(), textPosition, Color.White);

            textPosition = textStart + (3 * rowOffset);
            batch.DrawString(pericles, player.Inventory.rods.ToString(), textPosition, Color.White);

            textPosition = textStart + (4 * rowOffset);
            batch.DrawString(pericles, player.Inventory.disks.ToString(), textPosition, Color.White);

            Vector2 lowerRegionOffset = new Vector2(0, 48);
            Vector2 completeNameOffset = new Vector2(viewport.X + 35, viewport.Y + 125);

            int index = 0;
            foreach (Game_Objects.Build_System.CompleteObject c in player.Inventory.customObjects)
            {
                if (c != null)
                {
                    textPosition = textStart + ((5 + index) * rowOffset) + lowerRegionOffset;
                    batch.DrawString(pericles, c.availableInstances.ToString(), textPosition, Color.White);
                    textPosition = completeNameOffset + ((5 + index) * rowOffset) + lowerRegionOffset;
                    batch.DrawString(pericles, c.name, textPosition, Color.White);
                }
                index++;
            }
        }

        /// <summary>Draw all 3D</summary>
        public override void render3D(GameTime time)
        {
        }

        #endregion Drawing


        #region Button Callbacks

        /// <summary>when the player clicks the goBack btn, or presses escape</summary>
        public void BtnCallback_goBack()
        {
            base.stateMan.closeOverlapState();
            base.stateMan.showOverlapState("gameMenu");
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
