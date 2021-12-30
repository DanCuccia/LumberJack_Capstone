using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Game_Objects.Build_System;
using Game.Managers;


namespace Game.States
{
    class MapState : State
    {
        #region MemberVariables

        WorldManager                    world;  //ref
        Player                          player; //ref
        Renderer                        render; //ref
        Effect                          toonMaster;//ref
        AudioManager                    audio; //ref

        Viewport                        viewport;

        Point                           worldIndices        = new Point(-1, -1);
        BuildCamera                     camera;

        Vector2                         msLast              = new Vector2(float.MaxValue);
        Vector2                         msVelocity          = Vector2.Zero;
        const float                     msDecelleration     = 0.6f;
        const float                     cameraSnapDistance  = 25000f;
        const float                     camHeightOffset     = 5000f;

        SpriteManager                   spriteManager;
        TextureManager                  texManager;

        UncontrolledSprite              worldMap;
        Sprite                          playerLocator;

        Keys                            pressedKey          = Keys.None;

        #endregion MemberVariables


        #region Initialization

        /// <summary>Default CTOR</summary>
        /// <param name="g">the program pointer</param>
        /// <param name="man">manager's pointer</param>
        public MapState(Game1 g, StateManager man)
            : base(g, "map", man)
        {
            spriteManager = new SpriteManager(g.spriteBatch);
            texManager = new TextureManager();
        }

        /// <summary>init all member variables ready for run time</summary>
        public override void initialize()
        {
            viewport = base.device.Viewport;
            viewport = Game1.GetTabletViewport(viewport);
            initWorldMap();
            initButtons();

            world = WorldManager.getInstance();
            render = Renderer.getInstance();
            toonMaster = Renderer.getInstance().EffectManager.GetEffect(RenderEffect.RFX_TOON);
            audio = AudioManager.getInstance();
            
            GameState game = base.stateMan.getState("game") as GameState;
            player = game.getPlayer();
            this.worldIndices = player.WorldIndices;

            camera = new BuildCamera(player.Position + new Vector3(0, camHeightOffset, 0), 
                player.Position + (player.WorldMatrix.Backward * 5) + new Vector3(0, camHeightOffset-100, 0), 
                farPlane: 7500);
            camera.update();

            base.isInitialized = true;
        }

        private void initWorldMap()
        {
            worldMap = new UncontrolledSprite();
            Texture2D tex = base.content.Load<Texture2D>("sprites\\worldMap");
            worldMap.Initialize(new Point(tex.Width, tex.Height), new Point(1, 1), tex);
            worldMap.Position = new Vector2(viewport.X + (viewport.Width / 2), viewport.Y + (viewport.Height / 2));
            worldMap.Animating = worldMap.Visible = false;
        }

        /// <summary>creates any buttons for this state</summary>
        private void initButtons()
        {
            //go back btn
            Texture2D tex = base.content.Load<Texture2D>("sprites\\btn_goBack");
            texManager.addTexture("btn_goBack", tex);
            Sprite btn = new Sprite("btn_goBack",
                new Vector2(viewport.X + viewport.Width - (tex.Width/2) - 6, viewport.Y + (tex.Height/2) + 6),
                "btn_goBack", spriteManager, texManager, BtnCallback_goBack);
            spriteManager.addSprite(btn);

            tex = base.content.Load<Texture2D>("sprites\\btn_closeTablet");
            texManager.addTexture("btn_closeTablet", tex);
            btn = new Sprite("btn_closeTablet", new Vector2(viewport.X + (tex.Width / 2), viewport.Y + (tex.Height / 2)),
                "btn_closeTablet", spriteManager, texManager, BtnCallback_closeTablet);
            spriteManager.addSprite(btn);

            tex = base.content.Load<Texture2D>("sprites\\btn_worldMap");
            texManager.addTexture("btn_worldMap", tex);
            btn = new Sprite("btn_worldMap", new Vector2(viewport.X + (viewport.Width / 2), viewport.Y + (tex.Height / 2)),
                "btn_worldMap", spriteManager, texManager, BtnCallback_openWorldMap);
            spriteManager.addSprite(btn);

            tex = base.content.Load<Texture2D>("sprites\\btn_exitWorldMap");
            texManager.addTexture("btn_exitWorldMap", tex);
            btn = new Sprite("btn_exitWorldMap", 
                new Vector2(worldMap.Position.X - (worldMap.Texture.Width/2) + (tex.Width/2) + 12, 
                    worldMap.Position.Y - (worldMap.Texture.Height/2) + (tex.Height/2) + 12),
                "btn_exitWorldMap", spriteManager, texManager, BtnCallback_exitWorldMap);
            btn.Visible = btn.Clickable = false;
            spriteManager.addSprite(btn);

            tex = base.content.Load<Texture2D>("sprites\\playerLocator");
            texManager.addTexture("playerLocator", tex);
            playerLocator = new Sprite("playerLocator", Vector2.Zero, "playerLocator", spriteManager, texManager);
            playerLocator.Clickable = playerLocator.Visible = false;
            spriteManager.addSprite(playerLocator);

            tex = base.content.Load<Texture2D>("sprites\\goalMarker");
            texManager.addTexture("goalMarker", tex);
            btn = new Sprite("goalMarker",
                new Vector2(worldMap.Position.X + worldMap.Texture.Width / 2.5f,
                    worldMap.Position.Y - worldMap.Texture.Height / 2.5f), "goalMarker", spriteManager, texManager);
            btn.Clickable = btn.Visible = false;
            spriteManager.addSprite(btn);
        }

        /// <summary>any closing logic and asset releasing in here</summary>
        public override void close()
        {
            worldIndices = new Point(-1, -1);
            camera = null;
            spriteManager.empty();
            texManager.empty();
            pressedKey = Keys.None;
            worldMap = null;

            base.isInitialized = false;

        }

        #endregion Initialization


        #region API

        /// <summary>main user input call</summary>
        public override void input(KeyboardState kb, MouseState ms)
        {
            this.moveMap(ms);

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

        /// <summary>All camera movement logic in here</summary>
        private void moveMap(MouseState ms)
        {
            Vector2 msNow = new Vector2(ms.X, ms.Y);
            Vector3 camPositionOrig = camera.position;
            Vector3 camTargetOrig = camera.target;

            switch (ms.LeftButton)
            {
                case ButtonState.Pressed:

                    if (this.msLast == new Vector2(float.MaxValue))
                    {
                        msLast = msNow;
                        break;
                    }
                    else
                    {
                        Vector2 delta = msNow - msLast;
                        camera.move(new Vector3(delta.X, 0, delta.Y));

                        float height;
                        Point wi = world.getTerrainHeight(camera.position, out height);
                        if (wi == new Point(-1, -1))
                        {
                            camera.position = camPositionOrig;
                            camera.target = camTargetOrig;
                        }
                    }

                    break;

                case ButtonState.Released:

                    if (msLast != new Vector2(float.MaxValue))
                    {
                        Vector2 delta = msNow - msLast;
                        msVelocity += delta;
                    }
                    msLast = new Vector2(float.MaxValue);
                    
                    break;
            }

            
            if (msVelocity != Vector2.Zero)
            {
                //x
                if (msVelocity.X < 0)
                {
                    msVelocity.X += msDecelleration;
                    if (msVelocity.X >= 0)
                        msVelocity.X = 0;
                }
                else if (msVelocity.X > 0)
                {
                    msVelocity.X -= msDecelleration;
                    if (msVelocity.X <= 0)
                        msVelocity.X = 0;
                }
                //y
                if (msVelocity.Y < 0)
                {
                    msVelocity.Y += msDecelleration;
                    if (msVelocity.Y >= 0)
                        msVelocity.Y = 0;
                }
                else if (msVelocity.Y > 0)
                {
                    msVelocity.Y -= msDecelleration;
                    if (msVelocity.Y <= 0)
                        msVelocity.Y = 0;
                }
            }

            if (Vector3.Distance(player.Position, camera.position) > cameraSnapDistance)
            {
                camera.position = player.Position + new Vector3(0, camHeightOffset, 0);
                camera.target = player.Position + (player.WorldMatrix.Backward * 5) + new Vector3(0, camHeightOffset - 100, 0);
                this.msVelocity = Vector2.Zero;
                this.msLast = new Vector2(float.MaxValue);
            }

            camera.move(new Vector3(msVelocity.X, 0, msVelocity.Y));
            float ignore;
            Point wIndex = world.getTerrainHeight(camera.position, out ignore);
            if (wIndex == new Point(-1, -1))
            {
                camera.position = camPositionOrig;
                camera.target = camTargetOrig;
                msVelocity = Vector2.Zero;
            }
            camera.update();
        }

        /// <summary>main update call</summary>
        /// <param name="time"></param>
        public override void update(GameTime time)
        {
            spriteManager.update(time);
        }

        #endregion API


        #region Drawing

        /// <summary>main draw 2D call</summary>
        public override void render2D(GameTime time, SpriteBatch batch)
        {
            spriteManager.renderSprites();
            worldMap.Draw(batch);
            Sprite spr = spriteManager.getSprite("btn_exitWorldMap");
            spr.draw(batch);
            spr = spriteManager.getSprite("playerLocator");
            spr.draw(batch);
            spr = spriteManager.getSprite("goalMarker");
            spr.draw(batch);            
        }

        /// <summary>main draw 3D call</summary>
        public override void render3D(GameTime time)
        {
            base.device.Viewport = viewport;
            Renderer.getInstance().ClearDepthBuffer();
            this.updateShaderMatrices();

            world.Draw();
            player.Draw();

            base.device.Viewport = Renderer.getInstance().OriginalViewport;
        }

        /// <summary>load this state's camera's view & projection matrices into the shader</summary>
        private void updateShaderMatrices()
        {
            Effect toonMaster = Renderer.getInstance().EffectManager.GetEffect(RenderEffect.RFX_TOON);
            toonMaster.Parameters["View"].SetValue(camera.view);
            toonMaster.Parameters["Projection"].SetValue(camera.projection);
        }

        #endregion Drawing


        #region Callbacks

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
            gamePlayer.Tablet.IsVisible = gamePlayer.CanExecute = false;
            gamePlayer.AnimPlaying = true;
            stateMan.closeOverlapState();
        }

        public void BtnCallback_openWorldMap()
        {
            Sprite spr = spriteManager.getSprite("btn_exitWorldMap");
            worldMap.Visible = 
                spr.Clickable = 
                spr.Visible = true;

            spr = spriteManager.getSprite("goalMarker");
            spr.Visible = true;

            Vector2 posScalar;

            float nodeStride = Math.Abs(world[0, 0].Terrain.Position.X - world[0, 0].Terrain.HeightData.WorldSpaceCorners.X * 2);
            float dimension = (nodeStride * 7) - nodeStride/2;

            Vector2 playerActual = new Vector2(
                player.Position.X + (nodeStride / 2) + dimension,
                player.Position.Z - (nodeStride / 2) + dimension );

            posScalar.X = 1 - ((dimension - playerActual.X) / dimension);
            posScalar.Y = 1 - ((dimension - playerActual.Y) / dimension);

            // hardcode: 505px is the size of the actual map in the image
            float borderDif = (worldMap.Texture.Width - 505)/2;

            Vector2 playerLocatorPos = new Vector2(
                (worldMap.Position.X - (worldMap.Texture.Width / 2)) + (505 * posScalar.X) + borderDif,
                (worldMap.Position.Y - (worldMap.Texture.Height / 2)) + (505 * posScalar.Y) + borderDif );

            playerLocator.Position = playerLocatorPos;
            playerLocator.Visible = true;
        }

        public void BtnCallback_exitWorldMap()
        {
            Sprite sprite = spriteManager.getSprite("btn_exitWorldMap");
            worldMap.Visible =
                sprite.Clickable =
                sprite.Visible = 
                playerLocator.Visible = false;
            sprite = spriteManager.getSprite("goalMarker");
            sprite.Visible = false;
        }

        #endregion Callbacks
    }
}
