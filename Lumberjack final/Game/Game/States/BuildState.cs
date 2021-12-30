using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;
using Game.Managers;
using Game.Managers.Factories;
using Game.Game_Objects;
using Game.Game_Objects.Build_System;
using Game.Math_Physics;

namespace Game.States
{
    public class BuildState : State
    {
        #region Member Variables

        BuildCamera             buildCamera;
        Viewport                viewport;
        AudioManager            audio;
        Renderer                renderer;

        SpriteManager           spriteMan;
        TextureManager          textureMan;
        SpriteFont              font;

        Keys                    pressedKey = Keys.None;

        BuildController         builder;
        List<CompleteObject>    placedObjects;

        List<Vector3>           cameraPositions;
        int                     cameraIndex = 0;

        StaticMesh              floor;

        LogicProp               environmentObject = null;

        Vector2 uiOffset;
        Vector2 uiDataOffset;
        bool showMenu;

        Player player;
        bool isHelpVisible = false;

        int hideHelpTimer = 0;

        #endregion Member Variables 


        #region Initialization

        /// <summary> Default CTOR</summary>
        public BuildState(Game1 g, StateManager sm)
            : base(g, "build", sm)
        {
            spriteMan = new SpriteManager(g.spriteBatch);
            textureMan = new TextureManager();
            renderer = Renderer.getInstance();
        }

        /// <summary>main init </summary>
        public override void initialize()
        {
            textureMan.addTexture("texTest", content.Load<Texture2D>("sprites\\testMenuTitle"));
            font = Renderer.getInstance().GameFont;
            audio = AudioManager.getInstance();

            GameState gs = stateMan.getState("game") as GameState;
            player = gs.getPlayer();

            //Camera setup
            cameraPositions = new List<Vector3>();

            cameraPositions.Add(new Vector3(175.0f, 175.0f, 175.0f));
            cameraPositions.Add(new Vector3(-50.0f, 50.0f, 50.0f));
            cameraPositions.Add(new Vector3(-50.0f, 50.0f, -50.0f));
            cameraPositions.Add(new Vector3(50.0f, 50.0f, -50.0f));

            buildCamera = new BuildCamera(cameraPositions[0], new Vector3(0.0f, 0.0f, 0.0f));
            Renderer.getInstance().BuildCamera = buildCamera;

            BuildControllerSetup setup = new BuildControllerSetup();
            setup.dimensions = new Point3(25, 25, 25);
            setup.origin = new Vector3(0, 0, 0);
            setup.stride = new Vector3(3.0f, 3.0f, 3.0f);
            setup.sMan = spriteMan;
            setup.tMan = textureMan;

            builder = new BuildController(base.content, setup, base.stateMan);
            viewport = base.device.Viewport;
            viewport = Game1.GetTabletViewport(viewport);
            builder.Viewport = this.viewport;

            placedObjects = new List<CompleteObject>();

            floor = new StaticMesh();
            floor.Initialize(base.content, "models\\building\\gridFloor", "textures\\gridTex");
            floor.Scale *= 3.0f;

            #region UI Button Setup
            //UI Setup
            uiOffset = new Vector2(viewport.X + viewport.Width - 75, viewport.Y + 80);

            Texture2D titleBack = content.Load<Texture2D>("sprites\\buildTitleBack");
            Texture2D buttonBack = content.Load<Texture2D>("sprites\\buildButtonBack");

            textureMan.addTexture("texTitleBack", titleBack);
            textureMan.addTexture("texButtonBack", buttonBack);

            Sprite menuButton = new Sprite("sprMenuButton", uiOffset + new Vector2(0, -55), "texTitleBack", spriteMan, textureMan, ToggleMenu);
            spriteMan.addSprite(menuButton);

            Sprite moveTitle = new Sprite("sprMoveTitle", uiOffset, "texTitleBack", spriteMan, textureMan);
            spriteMan.addSprite(moveTitle);

            Sprite temp = new Sprite("sprMoveLeft", uiOffset + new Vector2(0, 50) + (new Vector2(0, -4)), "texButtonBack", spriteMan, textureMan, builder.MoveLeft);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMoveRight", uiOffset + new Vector2(0, 50) + (new Vector2(0, 27)), "texButtonBack", spriteMan, textureMan, builder.MoveRight);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMoveForward", uiOffset + new Vector2(0, 50) + (new Vector2(0, 58)), "texButtonBack", spriteMan, textureMan, builder.MoveForward);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMoveBackward", uiOffset + new Vector2(0, 50) + (new Vector2(0, 89)), "texButtonBack", spriteMan, textureMan, builder.MoveBackward);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMoveUp", uiOffset + new Vector2(0, 50) + (new Vector2(0, 120)), "texButtonBack", spriteMan, textureMan, builder.MoveUp);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMoveDown", uiOffset + new Vector2(0, 50) + (new Vector2(0, 151)), "texButtonBack", spriteMan, textureMan, builder.MoveDown);
            spriteMan.addSprite(temp);

            Sprite rotateTitle = new Sprite("sprRotateTitle", uiOffset + new Vector2(0, 300), "texTitleBack", spriteMan, textureMan);
            spriteMan.addSprite(rotateTitle);

            temp = new Sprite("sprRotateLeft", uiOffset + new Vector2(0, 350) + (new Vector2(0, -4)), "texButtonBack", spriteMan, textureMan, builder.RotateLeft);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprRotateRight", uiOffset + new Vector2(0, 350) + (new Vector2(0, 27)), "texButtonBack", spriteMan, textureMan, builder.RotateRight);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprRotateForward", uiOffset + new Vector2(0, 350) + (new Vector2(0, 58)), "texButtonBack", spriteMan, textureMan, builder.RotateForward);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprRotateBackward", uiOffset + new Vector2(0, 350) + (new Vector2(0, 89)), "texButtonBack", spriteMan, textureMan, builder.RotateBackward);
            spriteMan.addSprite(temp);

            uiDataOffset = new Vector2(viewport.X + 85, viewport.Y + 525);

            temp = new Sprite("sprBlockCount", uiDataOffset, "texTitleBack", spriteMan, textureMan, player.MakeBlock);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprPlankCount", uiDataOffset + new Vector2(155, 0), "texTitleBack", spriteMan, textureMan, player.MakePlank);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprRodCount", uiDataOffset + new Vector2(310, 0), "texTitleBack", spriteMan, textureMan, player.MakeRod);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprDiskkCount", uiDataOffset + new Vector2(465, 0), "texTitleBack", spriteMan, textureMan, player.MakeDisk);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprCursorPosition", uiDataOffset + new Vector2(0, -500), "texTitleBack", spriteMan, textureMan);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMakeBlock", uiDataOffset + new Vector2(0, -450), "texButtonBack", spriteMan, textureMan, builder.BuildBlock);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMakePlank", uiDataOffset + new Vector2(0, -400), "texButtonBack", spriteMan, textureMan, builder.BuildPlank);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMakeRod", uiDataOffset + new Vector2(0, -350), "texButtonBack", spriteMan, textureMan, builder.BuildRod);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprMakeDisk", uiDataOffset + new Vector2(0, -300), "texButtonBack", spriteMan, textureMan, builder.BuildDisk);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprHelp", uiDataOffset + new Vector2(155, -500), "texTitleBack", spriteMan, textureMan, ShowHelp);
            spriteMan.addSprite(temp);

            temp = new Sprite("sprComplete", uiDataOffset + new Vector2(310, -500), "texTitleBack", spriteMan, textureMan, builder.finalizeObject);
            spriteMan.addSprite(temp);

            if (Game1.runLowDef == true)
            {
                textureMan.addTexture("texHelpScreen", content.Load<Texture2D>("sprites\\buildHelpSmall"));
            }
            else
            {
                textureMan.addTexture("texHelpScreen", content.Load<Texture2D>("sprites\\buildHelpBig"));
            }
            temp = new Sprite("sprHelpScreen", new Vector2(viewport.X + viewport.Width / 2 + 2, viewport.Y + viewport.Height / 2 + 2), "texHelpScreen", spriteMan, textureMan, HideHelp);

            temp.Visible = false;
            spriteMan.addSprite(temp);

            showMenu = true;
            ToggleMenu();

            #endregion UI Setup

            isInitialized = true;
        }

        /// <summary>load the build state with a logic prop from the environment</summary>
        /// <param name="logicPropID">the ID of the logic prop</param>
        /// <param name="scaling">it's world-scaling</param>
        public void InitEnvironmentObject(int logicPropID, Vector3 scaling)
        {
            switch (logicPropID)
            {
                case GameIDList.LogicProp_RiverDam:
                    environmentObject = new RiverDam(base.content);
                    break;
                case GameIDList.LogicProp_BrokenHouse:
                    environmentObject = new BrokenHouse(base.content);
                    break;
            }
            if (environmentObject != null)
            {
                environmentObject.Scale = scaling;
            }
        }

        public void ShowHelp()
        {
            Sprite help = spriteMan.getSprite("sprHelpScreen");
            help.Visible = true;
            isHelpVisible = true;
            hideHelpTimer = 30;
        }

        public void HideHelp()
        {
            if (hideHelpTimer == 0)
            {
                Sprite help = spriteMan.getSprite("sprHelpScreen");
                help.Visible = false;
                isHelpVisible = false;
            }
        }

        public void ToggleMenu()
        {
            showMenu = !showMenu;

            foreach (Sprite s in spriteMan.SpriteList)
            {
                if (s.name == "sprMoveTitle" ||
                    s.name == "sprMoveLeft" ||
                    s.name == "sprMoveRight" ||
                    s.name == "sprMoveForward" ||
                    s.name == "sprMoveBackward" ||
                    s.name == "sprMoveUp" ||
                    s.name == "sprMoveDown" ||
                    s.name == "sprRotateLeft" ||
                    s.name == "sprRotateRight" ||
                    s.name == "sprRotateForward" ||
                    s.name == "sprRotateBackward" ||
                    s.name == "sprRotateTitle")
                {
                    s.Visible = showMenu;
                }
            }
        }

        /// <summary>closing logic in here</summary>
        public override void close()
        {
            textureMan.empty();
            spriteMan.empty();
            font = null;
            builder = null;
            isHelpVisible = false;
            isInitialized = false;
            environmentObject = null;
        }

        #endregion Initialization


        #region API

        /// <summary>Main input</summary>
        public override void input(KeyboardState kb, MouseState ms)
        {
            if (kb.IsKeyDown(Keys.Q) && placedObjects.Count < 1)
            {
                CompleteObject temp = new CompleteObject(builder.builtObjects[0]);
                temp.position = new Vector3(0.0f, 0.0f, 0.0f);
                temp.stride = builder.getStride();
                placedObjects.Add(temp);
            }

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

        /// <summary>main update</summary>
        public override void update(GameTime time)
        {
            spriteMan.update(time);

            builder.update(time, buildCamera);
            controlCamera();

            if (hideHelpTimer > 0)
                hideHelpTimer--;

            buildCamera.update();
        }

        private void controlCamera()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.End) && buildCamera.position == cameraPositions[cameraIndex])
            {
                cameraIndex++;
                if (cameraIndex == cameraPositions.Count)
                {
                    cameraIndex = 0;
                }
            }
        }

        #endregion API


        #region Drawing

        /// <summary>main draw 2D call</summary>
        public override void render2D(GameTime time, SpriteBatch batch)
        {
            spriteMan.renderSprites();

            if (!isHelpVisible)
            {
                #region Text
                spriteMan.drawText("Menu", Color.White, font, uiOffset + new Vector2(-50, -68));

                if (showMenu)
                {
                    spriteMan.drawText("Move", Color.White, font, uiOffset + new Vector2(-50, -18));
                    spriteMan.drawText("Left", Color.White, font, uiOffset + new Vector2(-45, 32));
                    spriteMan.drawText("Right", Color.White, font, uiOffset + new Vector2(-45, 63));
                    spriteMan.drawText("Forward", Color.White, font, uiOffset + new Vector2(-45, 94));
                    spriteMan.drawText("Backward", Color.White, font, uiOffset + new Vector2(-45, 125));
                    spriteMan.drawText("Up", Color.White, font, uiOffset + new Vector2(-45, 156));
                    spriteMan.drawText("Down", Color.White, font, uiOffset + new Vector2(-45, 187));

                    spriteMan.drawText("Rotate", Color.White, font, uiOffset + new Vector2(-50, 285));
                    spriteMan.drawText("Left", Color.White, font, uiOffset + new Vector2(-45, 335));
                    spriteMan.drawText("Right", Color.White, font, uiOffset + new Vector2(-45, 366));
                    spriteMan.drawText("Forward", Color.White, font, uiOffset + new Vector2(-45, 397));
                    spriteMan.drawText("Backward", Color.White, font, uiOffset + new Vector2(-45, 428));
                }

                int blockValue = (player.Inventory.blocks - builder.blocksRequired);
                int plankValue = (player.Inventory.planks - builder.planksRequired);
                int rodValue = (player.Inventory.rods - builder.rodsRequired);
                int diskValue = (player.Inventory.disks - builder.disksRequired);

                Color blockColor = Color.Maroon;
                Color plankColor = Color.Maroon;
                Color rodColor = Color.Maroon;
                Color diskColor = Color.Maroon;

                if (blockValue >= 0)
                {
                    blockColor = Color.LawnGreen;
                }

                if (plankValue >= 0)
                {
                    plankColor = Color.LawnGreen;
                }

                if (rodValue >= 0)
                {
                    rodColor = Color.LawnGreen;
                }

                if (diskValue >= 0)
                {
                    diskColor = Color.LawnGreen;
                }

                spriteMan.drawText("Blocks: " + blockValue.ToString(), blockColor, font, uiDataOffset + new Vector2(-60, -15));
                spriteMan.drawText("Planks: " + plankValue.ToString(), plankColor, font, uiDataOffset + new Vector2(95, -15));
                spriteMan.drawText("Rods: " + rodValue.ToString(), rodColor, font, uiDataOffset + new Vector2(250, -15));
                spriteMan.drawText("Disks: " + diskValue.ToString(), diskColor, font, uiDataOffset + new Vector2(405, -15));

                spriteMan.drawText("Position:\n", Color.White, font, uiDataOffset + new Vector2(-60, -525));

                spriteMan.drawText("BLOCK", Color.White, font, uiDataOffset + new Vector2(-60, -465));
                spriteMan.drawText("PLANK", Color.White, font, uiDataOffset + new Vector2(-60, -415));
                spriteMan.drawText("ROD", Color.White, font, uiDataOffset + new Vector2(-60, -365));
                spriteMan.drawText("DISK", Color.White, font, uiDataOffset + new Vector2(-60, -315));

                spriteMan.drawText(builder.GetPositionAsString(), Color.White, font, uiDataOffset + new Vector2(-50, -505));
                spriteMan.drawText("Help", Color.White, font, uiDataOffset + new Vector2(105, -510));

                Color canComplete = Color.Maroon;
                Sprite completeButton = spriteMan.getSprite("sprComplete");
                if (builder.getLiveObjectCount() > 0 &&
                    blockValue >= 0 &&
                    plankValue >= 0 &&
                    rodValue >= 0 &&
                    diskValue >= 0)
                {
                    canComplete = Color.LawnGreen;
                    completeButton.Clickable = true;
                }
                else
                {
                    completeButton.Clickable = false;
                }

                spriteMan.drawText("Complete", canComplete, font, uiDataOffset + new Vector2(255, -510));
                #endregion Text
            }
            else
            {
                //Do something for the help screen
            }
        }

        /// <summary>main draw 3D call</summary>
        public override void render3D(GameTime time)
        {
            base.device.Viewport = viewport;
            renderer.ClearDepthBuffer();
            
            UpdateBuildCamMatricies();

            floor.Draw();
            builder.render();

            foreach (CompleteObject c in placedObjects)
            {
                c.render();
            }

            player.Draw();

            if (environmentObject != null)
                environmentObject.Draw();
            
            base.device.Viewport = renderer.OriginalViewport;
        }

        /// <summary>loads the new build camera matrices into the shader</summary>
        private void UpdateBuildCamMatricies()
        {
            Effect toonMaster = Renderer.getInstance().EffectManager.GetEffect(RenderEffect.RFX_TOON);
            toonMaster.Parameters["View"].SetValue(buildCamera.view);
            toonMaster.Parameters["Projection"].SetValue(buildCamera.projection);
        }

        #endregion Drawing


        #region Callbacks

        /// <summary>when the player pressed tab, or close btn</summary>
        public void BtnCallback_closeTablet()
        {
            GameState gameState = base.stateMan.getState("game") as GameState;
            Player gamePlayer = gameState.getPlayer();
            Camera gameCam = WorldManager.getInstance().Camera;

            gameCam.SmoothStepTo(gamePlayer.camOrigTarget, gamePlayer.camOrigPosition, gamePlayer.closeTabletCompleteCallback);
            gamePlayer.AnimatedMesh.BeginAnimation("closeTablet", false, gamePlayer.defaultCallback);
            gamePlayer.Tablet.IsVisible = gamePlayer.CanExecute = false;
            gamePlayer.AnimPlaying = true;
            stateMan.closeOverlapState();
        }

        /// <summary>when the player clicks the goBack btn, or presses escape</summary>
        public void BtnCallback_goBack()
        {
            base.stateMan.closeOverlapState();
            base.stateMan.showOverlapState("gameMenu");
        }

        #endregion Callbacks


        #region Mutators

        public LogicProp EnvironmentObject
        {
            get { return this.environmentObject; }
        }

        #endregion Mutators
    }
}
