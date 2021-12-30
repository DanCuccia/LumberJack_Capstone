using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using Game.Drawing_Objects;
using Game.Managers;
using Game.Game_Objects.Build_System;
using Game.Game_Objects.Trees;
using Game.Math_Physics;
using Game.States;

using XNAnimation;
using XNAnimation.Controllers;

namespace Game.Game_Objects
{

    /// <summary>This is everything that the player saves</summary>
    [Serializable]
    public class PlayerXmlMedium
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public int Lumber;
        public int Blocks;
        public int Planks;
        public int Rods;
        public int Disks;
        public List<CompleteObjXmlMedium> CompletedObjectList;

        public bool riverIsLowered;
        public bool houseIsFixed;
        public bool playerOverTreeline;
    }

    /// <summary>hold all the data reguarding the player's inventory</summary>
    public class Inventory
    {
        public Inventory()
        {
            lumber = 20;
            blocks = planks = rods = disks = 0;
        }
        public int lumber;
        public int blocks;
        public int planks;
        public int rods;
        public int disks;

        public CompleteObject[] customObjects = new CompleteObject[4];
    }

    /// <summary>
    /// The main Player Class
    /// Animations:
    ///    IDLE, WALK, RUN, CHOP, OPEN_TABLET, HOLD_TABLET,
    ///    CLOSE_TABLET, DECLINE, WAVE, JUMP
    /// </summary>
    public class Player
    {

        #region Member Variables

        ContentManager              content;   //ref
        GraphicsDevice              device;    //ref
        WorldManager                world;     //ref
        StateManager                stateMan;  //ref
        AudioManager                audio;     //ref
        GameState                   game;      //ref

        // -- main player models & world location
        Point                       worldIndicies;
        AnimatedMesh                animatedModel;
        StaticMesh                  feetBox;
        BoneControlledModel         axeModel;

        // -- tablet info
        StaticMesh                  tabletModel;
        Vector3                     tabletTargetOffset;
        Vector3                     tabletCameraOffset;
        bool                        lockInput               = false;
        public Vector3              camOrigPosition         = Vector3.Zero;
        public Vector3              camOrigTarget           = Vector3.Zero;

        // -- player movement
        const float                 movementSpeed           = 10.0f;
        const float                 rotationSpeed           = 2.5f;
        public bool                 animPlaying             = false;
        bool                        isWalking               = false;
        public bool                 canJump                 = true;
        Vector3                     originalPosition;
        bool                        lastDirectionForward    = true;
        short                       lastUsefullLocation     = 0;

        // -- foley timing
        const int                   foleyRunDelay           = 450;
        const int                   foleyWalkDelay          = 450;
        bool                        foleyCanPlay            = true;
        int                         currentFoleyTime        = 0;

        // -- player physics
        const float                 gravity                 = -1.4f;
        float                       yVelocity               = 0f;
        const float                 initialVelocity         = 25f;
        Vector3                     beforeJump              = Vector3.Zero;

        // -- inventory
        Inventory                   inventory               = new Inventory();

        // -- automation
        bool                        automationEnabled       = false;
        bool                        atLocation              = false;
        public delegate void        AutomationCallback();
        public AutomationCallback   automationCallback      = null;
        Prop                        automationTargetProp    = null;
        const float                 automationThreshold     = 100f;
        const float                 chopReachDistance       = 2000f;

        // -- execution limiting
        bool                        canExecute              = true;
        int                         executeTime             = 0;
        int                         executeWaitTime         = 150;
        Keys                        pressedKey              = Keys.None;

        // -- gameplay flags
        public bool                 riverIsLowered          = false;
        public bool                 houseIsFixed            = false;
        public bool                 playerOverTreeline      = false;

        public bool                 onTopOfComplete         = false;
        
        #endregion Member Variables


        #region Initialization

        /// <summary>Default CTOR </summary>
        /// <param name="device">reference to graphics device</param>
        /// <param name="world">reference to the world manger</param>
        public Player(GraphicsDevice device, StateManager stateManager)
        {
            this.device = device;
            this.world = WorldManager.getInstance();
            this.stateMan = stateManager;
            this.audio = AudioManager.getInstance();
            this.game = (GameState)stateMan.getState("game");
        }

        /// <summary>Init Andy</summary>
        public void Initialize(ContentManager content)
        {
            this.content = content;

            this.animatedModel = new AnimatedMesh();
            this.animatedModel.Initialize(content, "models\\player\\andyAnims", "textures\\andy_diffuse");
            this.animatedModel.BeginAnimation("idle");

            this.tabletModel = new StaticMesh();
            this.tabletModel.Initialize(content, "models\\player\\tablet", "textures\\tablet_diffuse");
            this.tabletModel.IsVisible = false;

            this.feetBox = new StaticMesh();
            this.feetBox.Initialize(content, "models\\player\\feetBox", MyColors.BlankWhite);
            this.feetBox.ScaleY = 7f;
            this.feetBox.GenerateBoundingBox();
            this.feetBox.Position = world.GetPlayerStartGamePosition();
            this.feetBox.RotationY = 180;

            this.axeModel = new BoneControlledModel();
            this.axeModel.InitializeBoneModel(content, "models\\player\\axe", "textures\\axe_diffuse", this.animatedModel, "Bone_RMiddle1");
            this.axeModel.IsVisible = false;
        }

        #endregion Initialization


        #region API

        /// <summary>Main Player-logic Update</summary>
        public void Update(GameTime gameTime) 
        {
            updateExecuteTimer(gameTime);
            updateFoleyTimer(gameTime);

            if (automationEnabled)
                updateAutomation();

            updateHeight();
            this.animatedModel.Update(gameTime);
            axeModel.IsDirty = true;
            updateFeetAndModel();

            world.TestTriggerCollisions(this.feetBox);
        }

        /// <summary>Times the foley to roughly line up with the animation's footsteps</summary>
        public void updateFoleyTimer(GameTime time)
        {
            currentFoleyTime += time.ElapsedGameTime.Milliseconds;

            if (isWalking == true)
            {
                if (currentFoleyTime >= Player.foleyWalkDelay)
                {
                    currentFoleyTime = 0;
                    foleyCanPlay = true;
                }
            }
            else
            {
                if (currentFoleyTime >= Player.foleyRunDelay)
                {
                    currentFoleyTime = 0;
                    foleyCanPlay = true;
                }
            }
        }

        /// <summary>Updates the main execution limiter</summary>
        private void updateExecuteTimer(GameTime gameTime)
        {
            if (this.canExecute == false)
            {
                this.executeTime += gameTime.ElapsedGameTime.Milliseconds;
                if (this.executeTime >= this.executeWaitTime)
                {
                    this.executeTime = 0;
                    this.canExecute = true;
                }
            }
        }

        /// <summary>update the player's position to be ontop of completed objects</summary>
        private void collideWithComplete()
        {
            List<CompleteObject> testList = game.CompleteObjectList;
            float newHeight = feetBox.PositionY;
            onTopOfComplete = false;

            foreach (CompleteObject obj in testList)
            {
                if (Math.Abs(Vector3.Distance(feetBox.Position, obj.position)) <= 5000)
                {
                    foreach (BuildableObject piece in obj.PiecesList)
                    {
                        piece.UpdateBoundingBox();

                        if (piece.OBB.Intersects(feetBox.OBB))
                        {
                            newHeight = piece.Position.Y;
                            onTopOfComplete = true;
                        }
                    }
                }
            }
            this.feetBox.PositionY = newHeight;
        }

        /// <summary>If the player is currently automating to a location, this will update that</summary>
        private void updateAutomation()
        {
            if (atLocation)
                return;

            feetBox.Position += -feetBox.WorldMatrix.Forward * (movementSpeed / 2);

            if (world.TestBoundaryCollision(this.worldIndicies, this.feetBox))
            {
                declineCallback();
            }

            if (Vector3.Distance(feetBox.Position, automationTargetProp.PPosition) <= automationThreshold * automationTargetProp.PScaleX)
            {
                atLocation = true;
                automationCallback();
            }

        }

        /// <summary>Jumping update-logic is in here, privately called in Update()</summary>
        private void updateHeight()
        {
            if (this.canJump == false)
                this.yVelocity += gravity;

            float terrainHeight;
            this.worldIndicies = world.getTerrainHeight(this.feetBox.Position, out terrainHeight);

            this.feetBox.PositionY += this.yVelocity;

            if (this.feetBox.PositionY <= terrainHeight && this.canJump == false)
            {
                this.beforeJump = Vector3.Zero;
                this.yVelocity = 0f;
                this.feetBox.PositionY = terrainHeight;
                this.canJump = true;
                this.animPlaying = false;
                this.animatedModel.BeginAnimation("idle");
            }
            else if (this.canJump == true && !onTopOfComplete)
            {
                this.feetBox.PositionY = terrainHeight;
            }

            collideWithComplete();
        }

        /// <summary>All movement and movement-animation logic is found in here</summary>
        private void inputMovementAnimation(KeyboardState kb, MouseState ms)
        {
            // walking/running
            bool origIsWalking = isWalking;
            if (kb.IsKeyDown(Keys.LeftShift))
            {
                pressedKey = Keys.LeftShift;
            }
            else if (kb.IsKeyUp(Keys.LeftShift) && pressedKey == Keys.LeftShift && canExecute)
            {
                isWalking = !isWalking;
                canExecute = false;
                pressedKey = Keys.None;
            }

            // forward movement && animation
            if (kb.IsKeyDown(Keys.W))
            {
                lastDirectionForward = true;
                if ((canJump == true && animPlaying == false) || (origIsWalking != isWalking))
                {
                    if (isWalking)
                        animatedModel.BeginAnimation("walk");
                    else
                        animatedModel.BeginAnimation("run");

                    animatedModel.SetPlaybackMode(PlaybackMode.Forward);
                    animPlaying = true;
                    
                }

                if (this.foleyCanPlay == true)
                {
                    audio.Play3DSound("foley", this.feetBox.Position, false);
                    foleyCanPlay = false;
                }

                if (isWalking)
                    feetBox.Position -= feetBox.WorldMatrix.Forward * (movementSpeed / 2);
                else
                    feetBox.Position -= feetBox.WorldMatrix.Forward * movementSpeed;

                Prop prop;
                if (world.TestPropCollisions(worldIndicies, feetBox, out prop) && Game1.noCollision == false)
                {
                    if (GameIDList.IsBorder(prop.ID))
                    {
                        feetBox.Position = originalPosition;
                    }
                    else
                    {
                        this.pushBack(prop.PPosition);
                    }
                }
            }
            // backward movement && animation
            else if (kb.IsKeyDown(Keys.S))
            {
                lastDirectionForward = false;
                if (canJump == true && animPlaying == false || (origIsWalking != isWalking))
                {
                    if (isWalking)
                        animatedModel.BeginAnimation("walk");
                    else
                        animatedModel.BeginAnimation("run");

                    animatedModel.SetPlaybackMode(PlaybackMode.Backward);
                    animPlaying = true;
                    
                }

                if ( this.foleyCanPlay == true)
                {
                    audio.Play3DSound("foley", this.feetBox.Position, false);
                    foleyCanPlay = false;
                }

                if (isWalking)
                    feetBox.Position -= feetBox.WorldMatrix.Backward * (movementSpeed / 2);
                else
                    feetBox.Position -= feetBox.WorldMatrix.Backward * movementSpeed;

                Prop prop;
                if (world.TestPropCollisions(worldIndicies, feetBox, out prop) && Game1.noCollision == false)
                {
                    if (GameIDList.IsBorder(prop.ID))
                    {
                        feetBox.Position = originalPosition;
                    }
                    else
                    {
                        this.pushForward(prop.PPosition);
                    }
                }
            }
            else if (canJump == false)
            { }
            else
            {
                testIdlePropCollisions();
                animatedModel.BeginAnimation("idle");
                animatedModel.SetPlaybackMode(PlaybackMode.Forward);
                animPlaying = false;
                audio.StopSound("foley");
            }
        }

        /// <summary>used elsewhere in game logic, will undo the position change of this cycle</summary>
        public void blockTriggerCallback()
        {
            if (isWalking)
                feetBox.Position -= feetBox.WorldMatrix.Backward * (movementSpeed / 2);
            else
                feetBox.Position -= feetBox.WorldMatrix.Backward * movementSpeed;
        }

        /// <summary>jumps the player through the world's list of "usefull locations"</summary>
        private void incrementUsefullLocation()
        {
            if (Game1.drawDevelopment == false)
                return;

            if (lastUsefullLocation >= world.UsefullLocations.Count)
                lastUsefullLocation = 0;

            feetBox.Position = world.UsefullLocations[lastUsefullLocation];
            canJump = canExecute = animPlaying = false;
            animatedModel.BeginAnimation("jump");
            
            lastUsefullLocation++;
        }

        /// <summary>User to Player input logic</summary>
        public void Input(KeyboardState kb, MouseState ms)
        {
            if (automationEnabled == true)
            {
                if (kb.IsKeyDown(Keys.Escape))
                {
                    automationEnabled =
                        atLocation =
                        isWalking = false;
                    automationCallback = null;
                    axeModel.IsVisible = false;
                }
                return;
            }

            if (lockInput == true)
                return;

            originalPosition = feetBox.Position;

            #region movement/jump
            inputMovementAnimation(kb, ms);
            
            // jump 
            if (kb.IsKeyDown(Keys.Space) && 
                this.canJump == true &&
                automationEnabled == false)
            {
                this.canJump = false;
                this.yVelocity += initialVelocity;
                this.beforeJump = feetBox.Position;
                this.animatedModel.BeginAnimation("jump");
            }
            if (canExecute && canJump == true)
            {
                if (kb.IsKeyDown(Keys.LeftControl) && kb.IsKeyDown(Keys.NumPad9))
                    incrementUsefullLocation();
            }
            #endregion movement/jump

            #region turn/rotate
            if (kb.IsKeyDown(Keys.A))
                this.feetBox.RotationY += rotationSpeed;
            else if (kb.IsKeyDown(Keys.D))
                this.feetBox.RotationY -= rotationSpeed;

            if (kb.IsKeyDown(Keys.C) && !kb.IsKeyDown(Keys.W) && !kb.IsKeyDown(Keys.S) && canJump)
            {
                this.animatedModel.BeginAnimation("wave", false, defaultCallback);
                this.animatedModel.SetPlaybackMode(PlaybackMode.Forward);
                this.animPlaying = true;
                this.canExecute = false;
            }
            #endregion turn/rotate

            #region openTablet
            if (this.canExecute)
                if (kb.IsKeyDown(Keys.Tab) && pressedKey == Keys.None)
                {
                    pressedKey = Keys.Tab;
                    audio.Play2DSound("btn_down", false);
                }
                else if(kb.IsKeyUp(Keys.Tab) && pressedKey == Keys.Tab)
                {
                    pressedKey = Keys.None;
                    audio.Play2DSound("btn_up", false);

                    OpenTablet();
                }
            #endregion openTablet

            #region chop tree & click logic prop

            if (this.clickLogicProp(ms))
                return; //don't check trees

            if (ms.LeftButton == ButtonState.Pressed && canExecute)
            {
                this.automationTargetProp = getPickedProp();
                if (automationTargetProp != null)
                {
                    if (IsChoppable(automationTargetProp.ID))
                    {
                        this.EnableWalkToAutomation(automationTargetProp.PPosition, toTreeCallback);
                        this.canExecute = false;
                    }
                }
            }

            if (kb.IsKeyDown(Keys.Q) && canExecute)
            {
                this.automationTargetProp = GetClosestTree();
                if (automationTargetProp != null)
                {
                    if (IsChoppable(automationTargetProp.ID))
                    {
                        this.EnableWalkToAutomation(automationTargetProp.PPosition, toTreeCallback);
                        this.canExecute = false;
                    }
                }
            }
            #endregion chop tree

        }

        /// <summary>
        /// calls open tablet animation
        /// </summary>
        public void OpenTablet(bool forBuild = false)
        {
            this.animatedModel.WorldMatrix.Forward.Normalize();
            this.animatedModel.WorldMatrix.Left.Normalize();

            camOrigPosition = this.world.Camera.Position;
            camOrigTarget = this.world.Camera.LookAtTarget;

            this.tabletTargetOffset = this.animatedModel.Position +
                (-this.animatedModel.WorldMatrix.Forward * 9) +
                new Vector3(0, 84.75f, 0);

            this.tabletCameraOffset = this.animatedModel.Position +
                (-this.animatedModel.WorldMatrix.Forward * 4.75f) +
                new Vector3(0, 88.5f, 0);

            if (forBuild)
            {
                this.world.Camera.SmoothStepTo(this.tabletTargetOffset, this.tabletCameraOffset, openTabletCompleteCallbackBuild, 0.20f);
            }
            else
            {
                this.world.Camera.SmoothStepTo(this.tabletTargetOffset, this.tabletCameraOffset, openTabletCompleteCallback, 0.20f);
            }
            this.animatedModel.BeginAnimation("openTablet", false, openTabletAnimationCompleteCallback);

            this.lockInput = true;
            this.canExecute = false;
        }

        /// <summary>determine whether a prop ID is a choppable tree or not</summary>
        /// <param name="propID">the id to test</param>
        /// <returns>yay/nay is a choppable tree</returns>
        private bool IsChoppable(int propID)
        {
            switch (propID)
            {
                case GameIDList.Prop_LargeFullTree:
                    return true;
                case GameIDList.Prop_SmallFullTree:
                    return true;
                case GameIDList.Prop_ThinTree:
                    return true;
                case GameIDList.Prop_PineTree:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Main Draw</summary>
        public void Draw()
        {
            if (Game1.drawDevelopment == true)
            {
                this.feetBox.Draw();
                if (WorldManager.cullSettings.OBBCullMode == PropCullMode.CULL_DISTANCE)
                    this.feetBox.DrawOBB();
            }
            this.animatedModel.Draw();
            this.tabletModel.Draw();
            this.axeModel.Draw();
        }

        /// <summary>search the current node, if not, 
        /// get the 8 surrounding nodes, and find the picked prop</summary>
        /// <returns>The prop that has been picked</returns>
        private Prop getPickedProp()
        {
            Ray msRay = Game1.GetMouseRay(Renderer.getInstance().OriginalViewport, world.Camera);
            float nearPoint, farPoint;

            //check our current node first
            List<Prop> plist = this.world.GetNodePropList(this.worldIndicies);
            foreach (Prop prop in plist)
            {
                if(prop.GetBoundingModel().OBB.Intersects(msRay, out nearPoint, out farPoint) != -1)
                {
                    if (nearPoint <= Player.chopReachDistance)
                        return prop;
                    else
                        return null;
                }
            }

            //check the 8 surrounding nodes
            List<Point> ilist = this.world.GetSurroundingNodes(this.worldIndicies);
            foreach (Point index in ilist)
            {
                List<Prop> nplist = this.world.GetNodePropList(index);
                foreach (Prop prop in nplist)
                {
                    if (prop.GetBoundingModel().OBB.Intersects(msRay, out nearPoint, out farPoint) != -1)
                    {
                        if (nearPoint <= Player.chopReachDistance)
                            return prop;
                        else
                            return null;
                    }
                }
            }

            return null;
        }

        /// <summary>Search through all trees within the 9 nodes, 
        /// and find the closest one to the player</summary>
        /// <returns>the closest choppable tree to the player</returns>
        private Prop GetClosestTree()
        {
            //check our current node first
            List<Prop> plist = this.world.GetNodePropList(this.worldIndicies);
            float shortestDistance = float.MaxValue;

            Prop closestProp = null;

            foreach (Prop prop in plist)
            {
                if (Math.Abs(Vector3.Distance(prop.PPosition, Position)) < shortestDistance )
                {
                    shortestDistance = Math.Abs(Vector3.Distance(prop.PPosition, Position));
                    closestProp = prop;
                }
            }

            List<Point> ilist = this.world.GetSurroundingNodes(this.worldIndicies);

            foreach (Point index in ilist)
            {
                List<Prop> nplist = this.world.GetNodePropList(index);
                foreach (Prop prop in nplist)
                {
                    if (Math.Abs(Vector3.Distance(prop.PPosition, Position)) < shortestDistance)
                    {
                        shortestDistance = Math.Abs(Vector3.Distance(prop.PPosition, Position));
                        closestProp = prop;
                    }
                }
            }

            return closestProp;
        }

        /// <summary>Update the feet reference to the player's daya</summary>
        private void updateFeetAndModel()
        {
            this.animatedModel.Position = this.feetBox.Position;
            this.animatedModel.Rotation = this.feetBox.Rotation;
            this.feetBox.UpdateBoundingBox();
        }

        /// <summary>will make the player walk to a position and run a callback </summary>
        /// <param name="position">the desired position the player will walk to</param>
        /// <param name="callback">the callback when the player gets there (will default to idle)</param>
        private void EnableWalkToAutomation(Vector3 position, AutomationCallback callback = null)
        {
            float yRotationToTarget = (float)Math.Atan2(automationTargetProp.PPosition.X - feetBox.Position.X, 
                automationTargetProp.PPosition.Z - feetBox.Position.Z);

            feetBox.RotationY = MathHelper.ToDegrees(yRotationToTarget);
            this.isWalking = true;
            this.automationEnabled = true;
            animatedModel.BeginAnimation("walk");

            if (callback == null)
                this.automationCallback = defaultCallback;
            else
                this.automationCallback = callback;
        }

        /// <summary>if the player is idling, tests collisions against props</summary>
        private void testIdlePropCollisions()
        {
            Prop prop;
            if (world.TestPropCollisions(this.worldIndicies, this.feetBox, out prop, true))
            {
                if (GameIDList.IsTree(prop.ID) || GameIDList.IsRock(prop.ID))
                {
                    pushBack(prop.PPosition);
                }
                else if (GameIDList.IsBorder(prop.ID))
                {
                    if (lastDirectionForward)
                        pushForward(feetBox.Position + feetBox.WorldMatrix.Forward);
                    else
                        pushBack(feetBox.Position + feetBox.WorldMatrix.Forward);
                }
            }
        }

        /// <summary>pushes the player backwards when inside of a prop/border</summary>
        private void pushBack(Vector3 objPos)
        {

            Vector3 vecToObj = objPos - feetBox.Position;
            vecToObj.Normalize();

            if(isWalking)
                feetBox.Position -= (movementSpeed / 2) * vecToObj;
            else
                feetBox.Position -= movementSpeed  * vecToObj;
        }

        /// <summary>pushes the player forwards when inside of a prop/border</summary>
        private void pushForward(Vector3 objPos)
        {
            Vector3 vecToObj = objPos - feetBox.Position;
            vecToObj.Normalize();

            if (isWalking)
                feetBox.Position += (movementSpeed / 2) * vecToObj;
            else
                feetBox.Position += movementSpeed  * vecToObj;
        }

        /// <summary>iterate the game's list of logic props, 
        /// check if the player clicked on any of them, and act accordingly</summary>
        private bool clickLogicProp(MouseState ms)
        {
            if (ms.LeftButton != ButtonState.Pressed)
                return false;

            Ray msRay = Game1.GetMouseRay(Renderer.getInstance().OriginalViewport, world.Camera);
            float nearPoint, farPoint;

            BuildState build;

            bool testTrue = false;
            foreach (LogicProp prop in game.LogicPropList)
            {
                if (prop.GetBoundingModel().OBB.Intersects(msRay, out nearPoint, out farPoint) != -1)
                {
                    if (nearPoint <= Player.chopReachDistance)
                    {
                        switch (prop.ID)
                        {
                            case GameIDList.LogicProp_BrokenHouse:
                                build = (BuildState)stateMan.getState("build");
                                build.InitEnvironmentObject(GameIDList.LogicProp_BrokenHouse, (prop.Scale/2.5f/10f));
                                this.OpenTablet(true);
                                testTrue = true;
                                break;
                            case GameIDList.LogicProp_BoatA:
                                moveTo(new Vector3(-28531.76f, -3025.173f, -52656.77f));
                                player180();
                                this.playerOverTreeline = true;
                                enableBoatB();
                                testTrue = true;
                                break;
                            case GameIDList.LogicProp_BoatB:
                                moveTo(new Vector3(-30809.26f, -2782.926f, -48025.14f));
                                player180();
                                this.playerOverTreeline = false;
                                enableBoatA();
                                testTrue = true;
                                break;
                            case GameIDList.LogicProp_RiverDam:
                                build = (BuildState)stateMan.getState("build");
                                build.InitEnvironmentObject(GameIDList.LogicProp_RiverDam, (prop.Scale/2.5f/10f));
                                this.OpenTablet(true);
                                testTrue = true;
                                break;
                        }
                    }
                }

                if (testTrue)
                {
                    canExecute = false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>turn the player and the camera around</summary>
        private void player180()
        {
            Camera gameCam = world.Camera;
            gameCam.ChaseAngle += 180f;
            feetBox.RotationY += 180f;
        }

        /// <summary>toggle the boats</summary>
        private void enableBoatB()
        {
            LogicProp prop = game.getLogicProp(GameIDList.LogicProp_BoatB);
            prop.Enabled = true;
            prop = game.getLogicProp(GameIDList.LogicProp_BoatA);
            prop.Enabled = false;
        }

        /// <summary>toggle the boats</summary>
        private void enableBoatA()
        {
            LogicProp prop = game.getLogicProp(GameIDList.LogicProp_BoatB);
            prop.Enabled = false;
            prop = game.getLogicProp(GameIDList.LogicProp_BoatA);
            prop.Enabled = true;
        }

        /// <summary>snaps the player to a specific location</summary>
        /// <param name="position">world position you want the player to snap to</param>
        private void moveTo(Vector3 position)
        {
            feetBox.Position = position;
            canJump = canExecute = animPlaying = false;
            animatedModel.BeginAnimation("jump");
        }
        
        #endregion API


        #region Saving & Loading

        /// <summary>Save the current player data to xml</summary>
        public void Save()
        {
            PlayerXmlMedium output = this.getXmlMedium();
            XmlSerializer xmlout = new XmlSerializer(output.GetType());
            TextWriter writer = new StreamWriter("playerSave.xml");
            xmlout.Serialize(writer, output);
            writer.Close();

            world.SaveTriggerList();
            game.Save();
        }

        /// <summary>Creates and sets up the xml struct that will be serialized</summary>
        /// <returns>the class that will be serialized</returns>
        private PlayerXmlMedium getXmlMedium()
        {
            PlayerXmlMedium output = new PlayerXmlMedium();

            output.Position = this.feetBox.Position;
            output.Rotation = this.feetBox.Rotation;
            output.Blocks = this.inventory.blocks;
            output.Disks = this.inventory.disks;
            output.Lumber = this.inventory.lumber;
            output.Planks = this.inventory.planks;
            output.Rods = this.inventory.rods;

            output.houseIsFixed = this.houseIsFixed;
            output.riverIsLowered = this.riverIsLowered;
            output.playerOverTreeline = this.playerOverTreeline;

            List<CompleteObjXmlMedium> completeObjList = new List<CompleteObjXmlMedium>();
            foreach (CompleteObject obj in this.inventory.customObjects)
            {
                if(obj != null)
                    completeObjList.Add(obj.getXmlMedium());
            }
            output.CompletedObjectList = completeObjList;

            return output;
        }

        /// <summary>Load the Player Data from xml</summary>
        public void Load()
        {
            PlayerXmlMedium input = new PlayerXmlMedium();

            TextReader reader = new StreamReader("playerSave.xml");
            XmlSerializer xmlIn = new XmlSerializer(input.GetType());
            input = (PlayerXmlMedium)xmlIn.Deserialize(reader);
            reader.Close();

            this.loadFromXmlMedium(input);
        }

        /// <summary>will set the player parameters according to the xml input</summary>
        /// <param name="xmlInput">the medium from xml</param>
        private void loadFromXmlMedium(PlayerXmlMedium xmlInput)
        {
            this.feetBox.Position = xmlInput.Position;
            this.feetBox.Rotation = xmlInput.Rotation;
            this.inventory.rods = xmlInput.Rods;
            this.inventory.planks = xmlInput.Planks;
            this.inventory.lumber = xmlInput.Lumber;
            this.inventory.disks = xmlInput.Disks;
            this.inventory.blocks = xmlInput.Blocks;

            this.houseIsFixed = xmlInput.houseIsFixed;
            this.riverIsLowered = xmlInput.riverIsLowered;
            this.playerOverTreeline = xmlInput.playerOverTreeline;

            foreach (CompleteObjXmlMedium obj in xmlInput.CompletedObjectList)
            {
                CompleteObject input = new CompleteObject();
                input.position = obj.Position;
                input.stride = obj.Stride;

                input.availableInstances = obj.AvailableInstances;
                input.blocks = obj.Blocks;
                input.disks = obj.Disks;
                input.majorScale = obj.MajorScale;
                input.name = obj.Name;
                input.planks = obj.Planks;
                input.rods = obj.Rods;

                foreach (BuildableObjectXmlMedium buildObj in obj.BuildableObjectList)
                {
                    Model model = content.Load<Model>(buildObj.ModelFilepath);
                    Point3 origin = new Point3((int)buildObj.Origin.X, (int)buildObj.Origin.Y, (int)buildObj.Origin.Z);
                    Game.Game_Objects.Build_System.PrimitiveType primType = (Game.Game_Objects.Build_System.PrimitiveType)buildObj.Type;

                    BuildableObject piece = new BuildableObject(model, buildObj.ModelFilepath, buildObj.WorldPosition, origin, primType);
                    piece.Scale = input.majorScale;

                    input.addObject(piece);
                }

                this.AddObject(input);
            }

            editEnvironmentPerLoad();
        }

        /// <summary>once the environemnt and player is loaded, edit any environmental changes,
        /// according to the player's xml save data</summary>
        private void editEnvironmentPerLoad()
        {
            if (riverIsLowered == true)
            {
                world.WaterList[(int)WaterIndices.WATER_FLOODED_RIVER].PositionY -= 300f;
                SoundClip clip = audio.GetSoundClip("river");
                clip.soundInstance.Volume = AudioManager.DammedRiverVolume;
            }

            if (houseIsFixed == true)
            {
                if (playerOverTreeline == true)
                {
                    game.getLogicProp(GameIDList.LogicProp_BoatA).Enabled = false;
                    game.getLogicProp(GameIDList.LogicProp_BoatB).Enabled = true;
                }
                else
                {
                    game.getLogicProp(GameIDList.LogicProp_BoatA).Enabled = true;
                    game.getLogicProp(GameIDList.LogicProp_BoatB).Enabled = false;
                }
            }
        }

        #endregion Saving & Loading


        #region Callbacks

        /// <summary>Camera calls this when it gets to the tablet position</summary>
        public void openTabletCompleteCallback()
        {
            this.animatedModel.BeginAnimation("holdTablet");
            this.world.Camera.CameraType = CameraType.CAM_STATIONARY;
            this.stateMan.showOverlapState("gameMenu");
        }

        /// <summary>Camera calls this when it gets to the tablet position,
        /// into build</summary>
        public void openTabletCompleteCallbackBuild()
        {
            this.animatedModel.BeginAnimation("holdTablet");
            this.world.Camera.CameraType = CameraType.CAM_STATIONARY;
            this.stateMan.showOverlapState("build");
        }

        /// <summary>The animationController calls this when the "openTablet" animation is complete</summary>
        public void openTabletAnimationCompleteCallback()
        {
            this.animatedModel.BeginAnimation("holdTablet");

            this.tabletModel.Position = this.animatedModel.Position +
                (-this.animatedModel.WorldMatrix.Forward * 16.5f) + 
                new Vector3(0, 78, 0);

            this.tabletModel.Rotation = this.animatedModel.Rotation + new Vector3(-70, 0, 0);
            this.tabletModel.IsVisible = true;
        }

        /// <summary>Camera calls this when it's back to it's original position</summary>
        public void closeTabletCompleteCallback()
        {
            this.lockInput = false;
            this.canExecute = false;

            this.world.Camera.CameraType = CameraType.CAM_CHASE;
        }

        /// <summary>player.updateAutomation calls this when he gets to his location</summary>
        public void toTreeCallback()
        {
            animatedModel.BeginAnimation("chop", false, chopTreeCompleteCallback);
            axeModel.Position = animatedModel.Position;
            axeModel.Rotation = animatedModel.Rotation;
            axeModel.IsVisible = true;
        }

        /// <summary>player's chopping animation calls this when it completes</summary>
        public void chopTreeCompleteCallback()
        {
            this.axeModel.IsVisible = false;

             Tree stump = null;

             //Add lumber when a tree is chopped down
             if (GameIDList.IsTree(automationTargetProp.ID))
             {
                 switch (automationTargetProp.ID)
                 {
                     case GameIDList.Prop_PineTree:
                         stump = new PineTreeStump(this.content);
                         AddLumber(15);
                         break;

                     case GameIDList.Prop_LargeFullTree:
                         stump = new BigTreeStump(this.content);
                         AddLumber(10);
                         break;

                     case GameIDList.Prop_SmallFullTree:
                         stump = new SmallTreeStump(this.content);
                         AddLumber(5);
                         break;
                     case GameIDList.Prop_ThinTree:
                         stump = new ThinTreeStump(this.content);
                         AddLumber(3);
                         break;
                 }
            }

            if(stump !=null)
            {
                stump.Position = automationTargetProp.PPosition;
                stump.Rotation = automationTargetProp.PRotation;
                stump.Scale = automationTargetProp.PScale;

                WorldLocation loc = automationTargetProp.LocationData;
                stump.LocationData = loc;

                world[loc.worldIndicies.X, loc.worldIndicies.Y].PropList[loc.listIndex] = stump;
                world.RegisterStump(loc);
            }
            this.automationEnabled = 
                this.atLocation = 
                this.isWalking = false;
        }

        /// <summary>Default idle callback</summary>
        public void defaultCallback()
        {
            this.animatedModel.BeginAnimation("idle");
            this.animPlaying = false;
            this.automationEnabled = false;
        }

        public void declineCallback()
        {
            this.animatedModel.BeginAnimation("decline", false, defaultCallback);
            this.animatedModel.SetPlaybackMode(PlaybackMode.Forward);
            this.animPlaying = true;
            this.automationEnabled = false;
        }

        #endregion Callbacks


        #region INVENTORY

        public void AddLumber(int amount)
        {
            inventory.lumber += amount;
        }

        public void MakeBlock()
        {
            if (inventory.lumber >= 1)
            {
                inventory.blocks += 1;
                inventory.lumber -= 1;
            }
        }

        public void MakePlank()
        {
            if (inventory.lumber >= 3)
            {
                inventory.planks += 1;
                inventory.lumber -= 3;
            }
        }

        public void MakeRod()
        {
            if (inventory.lumber >= 2)
            {
                inventory.rods += 1;
                inventory.lumber -= 2;
            }
        }

        public void MakeDisk()
        {
            if (inventory.lumber >= 2)
            {
                inventory.disks += 1;
                inventory.lumber -= 2;
            }
        }

        public void RecycleBlock()
        {
            if (inventory.blocks > 0)
            {
                inventory.blocks--;
                inventory.lumber++;
            }
        }

        public void RecyclePlank()
        {
            if (inventory.planks > 0)
            {
                inventory.planks--;
                inventory.lumber += 3;
            }
        }

        public void RecycleRod()
        {
            if (inventory.rods > 0)
            {
                inventory.rods--;
                inventory.lumber += 2;
            }
        }

        public void RecycleDisk()
        {
            if (inventory.disks > 0)
            {
                inventory.disks--;
                inventory.lumber += 2;
            }
        }

        public void RecycleCustomOne()
        {
            if (inventory.customObjects[0] != null)
            {
                inventory.customObjects[0].Recycle(this);
                inventory.customObjects[0] = null;
            }
        }

        public void RecycleCustomTwo()
        {
            if (inventory.customObjects[1] != null)
            {
                inventory.customObjects[1].Recycle(this);
                inventory.customObjects[1] = null;
            }
        }

        public void RecycleCustomThree()
        {
            if (inventory.customObjects[2] != null)
            {
                inventory.customObjects[2].Recycle(this);
                inventory.customObjects[2] = null;
            }
        }

        public void RecycleCustomFour()
        {
            if (inventory.customObjects[3] != null)
            {
                inventory.customObjects[3].Recycle(this);
                inventory.customObjects[3] = null;
            }
        }

        public void AddObject(CompleteObject c)
        {
            for (int index = 0; index < 4; index++)
            {
                if (inventory.customObjects[index] == null)
                {
                    c.availableInstances = 1;
                    inventory.customObjects[index] = c;
                    inventory.blocks -= c.blocks;
                    inventory.planks -= c.planks;
                    inventory.rods -= c.rods;
                    inventory.disks -= c.disks;
                    break;
                }
            }
        }

        #endregion INVENTORY


        #region Mutators

        public Vector3 Position
        {
            get { return this.feetBox.Position; }
            set { this.feetBox.Position = value; }
        }
        public bool LockInput
        {
            set { this.lockInput = value; }
            get { return this.lockInput; }
        }
        public AnimatedMesh AnimatedMesh
        {
            get { return this.animatedModel; }
        }
        public bool CanExecute
        {
            get { return this.canExecute; }
            set { this.canExecute = value; }
        }
        public bool AnimPlaying
        {
            get { return this.animPlaying; }
            set { this.animPlaying = value; }
        }
        public StaticMesh Tablet
        {
            get { return this.tabletModel; }
        }
        public Point WorldIndices
        {
            get { return this.worldIndicies; }
        }
        public Matrix WorldMatrix
        {
            get { return feetBox.WorldMatrix; }
        }
        public Inventory Inventory
        {
            get { return this.inventory; }
        }

        #endregion Mutators

    }
}
