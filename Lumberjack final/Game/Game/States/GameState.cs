using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using Game.Game_Objects;
using Game.Game_Objects.Build_System;
using Game.Managers;
using Game.Managers.Factories;
using Game.Math_Physics;

namespace Game.States
{
    public class GameState : State
    {
        #region Member Variables

        WorldManager            world;   //ref
        Camera                  camera;  //ref
        AudioManager            audio;   //ref

        bool                    loadContinued       = false;
        Player                  player;

        bool                    levelEditorActive   = false;
        LevelEditor             levelEditor;
        
        InfoboxManager          uiManager;

        //Keys                    pressedKey          = Keys.None;

        Texture2D               fadeTex;
        int                     currentAlpha        = 255;
        bool                    isFading            = true;

        List<CompleteObject>    userPlacedObjectList;
        List<LogicProp>         logicPropList;
        GoalMarker3D            goalMarker;

        public GameTime         runningTime;

        bool IsPositioningObject;
        CompleteObject CurrentCustomObject;

        #endregion Member Variables


        #region Initialization

        public GameState(Game1 g, StateManager sm)
            : base(g, "game", sm)
        {        }

        public override void initialize()
        {
            world = WorldManager.getInstance();
            world.Initialize(base.content, base.device, base.stateMan);

            world.BuildLand(base.content);
            world.BuildWater(base.content);
            world.BuildProps(base.content);
            world.BuildSkybox(base.content);
            world.BuildParticleEmitters(base.content);

            uiManager = new InfoboxManager();
            userPlacedObjectList = new List<CompleteObject>();
            logicPropList = new List<LogicProp>();

            player = new Player(base.device, base.stateMan);
            player.Initialize(base.content);

            initAudio();

            camera = world.Camera;
            camera.CameraType = CameraType.CAM_CHASE;
            Renderer.getInstance().Camera = camera;

            levelEditor = new LevelEditor();
            levelEditor.Initialize(this, base.content, base.device, camera);

            if (loadContinued)
            {
                loadLogicPropsContinued();
                player.Load();
                this.loadCompletedObjects();
                world.BuildPlayerTriggers(base.content);
                loadLogicPropsContinued();
            }
            else
            {
                world.BuildTriggers(base.content);
                loadLogicPropsNew();
                beginCameraFlyby();
            }

            fadeTex = base.content.Load<Texture2D>("textures\\blank");

            goalMarker = new GoalMarker3D();
            goalMarker.Initialize(base.content);

            base.isInitialized = true;
        }

        /// <summary>load the game state's audio, and begin playback</summary>
        private void initAudio()
        {
            audio = AudioManager.getInstance();
            audio.LoadSound("audio\\footstep", "foley");
            audio.LoadSound("audio\\wind", "wind");
            audio.LoadSound("audio\\birds", "birds");
            audio.LoadSound("audio\\pling", "pling");
            audio.LoadSound("audio\\waterfall", "waterfall");
            audio.LoadSound("audio\\creek", "river");

            SoundClip clip = audio.Play2DSound("wind", true);
            clip.soundInstance.Volume = AudioManager.WindVolume;

            clip = audio.Play2DSound("birds", true);
            clip.soundInstance.Volume = AudioManager.BirdsVolume;

            clip = audio.Play3DSound("waterfall", new Vector3(-26630.72f, -3401.587f, -55083.07f));
            clip.soundInstance.Volume = AudioManager.WaterfallVolume;

            clip = audio.Play3DSound("river", new Vector3(-25359.83f, -2759.538f, -21362.94f));
            clip.soundInstance.Volume = AudioManager.RiverVolume;
        }

        /// <summary>load the game's special props</summary>
        private void loadLogicPropsContinued()
        {
            List<LogicPropXmlMedium> inputList = new List<LogicPropXmlMedium>();

            TextReader reader = new StreamReader("playerLogicPropList.xml");
            XmlSerializer xmlin = new XmlSerializer(inputList.GetType());
            inputList = (List<LogicPropXmlMedium>)xmlin.Deserialize(reader);
            reader.Close();

            LogicPropFactory factory = new LogicPropFactory();

            foreach (LogicPropXmlMedium input in inputList)
            {
                this.logicPropList.Add(factory.GetCompleteSpecialProp(base.content, input));
            }
        }

        private void loadLogicPropsNew()
        {
            List<LogicPropXmlMedium> inputList = new List<LogicPropXmlMedium>();

            TextReader reader = new StreamReader("content\\data\\gameLogicPropList.xml");
            XmlSerializer xmlin = new XmlSerializer(inputList.GetType());
            inputList = (List<LogicPropXmlMedium>)xmlin.Deserialize(reader);
            reader.Close();

            LogicPropFactory factory = new LogicPropFactory();

            foreach (LogicPropXmlMedium input in inputList)
            {
                this.logicPropList.Add(factory.GetCompleteSpecialProp(base.content, input));
            }
        }

        /// <summary>Save the list of game special props</summary>
        public void saveLogicProps()
        {
            List<LogicPropXmlMedium> outputList = new List<LogicPropXmlMedium>();

            foreach (LogicProp prop in this.logicPropList)
            {
                outputList.Add(prop.GetLogicPropXmlMedium());
            }

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("gameLogicPropList.xml");
            xmlout.Serialize(writer, outputList);
            writer.Close();
        }

        /// <summary>save the logic prop list, for player</summary>
        public void savePlayerLogicProps()
        {
            List<LogicPropXmlMedium> outputList = new List<LogicPropXmlMedium>();

            foreach (LogicProp prop in this.logicPropList)
            {
                outputList.Add(prop.GetLogicPropXmlMedium());
            }

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("playerLogicPropList.xml");
            xmlout.Serialize(writer, outputList);
            writer.Close();
        }

        /// <summary>loads player's completed object list</summary>
        private void loadCompletedObjects()
        {
            userPlacedObjectList = new List<CompleteObject>();
            List<CompleteObjXmlMedium> inputList = new List<CompleteObjXmlMedium>();

            TextReader reader = new StreamReader("placedCompleteObjects.xml");
            XmlSerializer xmlin = new XmlSerializer(inputList.GetType());
            inputList = (List<CompleteObjXmlMedium>)xmlin.Deserialize(reader);
            reader.Close();

            foreach (CompleteObjXmlMedium obj in inputList)
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
                    piece.Rotation = buildObj.Rotation;
                    piece.AddRotation = new Vector3(buildObj.Rotation.X, buildObj.Rotation.Y, buildObj.Rotation.Z);
                    input.addObject(piece);
                }

                input.AddRotation(Vector3.Zero);
                this.userPlacedObjectList.Add(input);
            }
        }

        /// <summary>closing logic</summary>
        public override void close()
        {
            isFading = true;
            currentAlpha = 255;
            fadeTex = null;
            if (userPlacedObjectList != null)
                userPlacedObjectList.Clear();
        }

        #endregion Initialization


        #region API

        /// <summary> main input call</summary>
        public override void input(KeyboardState kb, MouseState ms)
        {
            if (levelEditorActive == false && !IsPositioningObject)
                player.Input(kb, ms);

            if (!IsPositioningObject)
            {
                camera.Input(kb, ms);
            }
            camera.Update(player.Position);

            //temp
            if (camera.CameraType == CameraType.CAM_AUTOMATED)
            {
                if (kb.IsKeyDown(Keys.Escape))
                {
                    flyby_Step8();
                }
            }

            if (kb.IsKeyDown(Keys.OemPlus))
            {
                camera.CameraType = CameraType.CAM_CHASE;
                Game1.drawDevelopment = 
                    levelEditorActive = false;
            }
            if (kb.IsKeyDown(Keys.OemMinus))
            {
                camera.CameraType = CameraType.CAM_FREE;
                Game1.drawDevelopment =
                    levelEditorActive = true;
            }
            if (kb.IsKeyDown(Keys.F9))
                Game1.drawDevelopment = true;
            if (kb.IsKeyDown(Keys.F10))
                Game1.drawDevelopment = false;

            if (levelEditorActive == true)
                levelEditor.Input(kb, ms);
        }

        /// <summary>receieve a new complete object into the world</summary>
        public void AddUserObject(CompleteObject obj)
        {
            if (obj == null)
                return;

            if (obj.name == "Dam Fix")
            {
                this.lowerRiver();
                this.turnOffTriggerBlock();
                LogicProp prop = getLogicProp(GameIDList.LogicProp_RiverDam);
                obj.position = prop.Position;
                obj.Rotation = prop.Rotation + new Vector3(0, -20f, 0);
                obj.ChangeMajorScale(new Vector3(2.5f));
                obj.AddRotation(Vector3.Zero);
                IsPositioningObject = false;
                this.userPlacedObjectList.Add(obj);
            }
            else if (obj.name == "House Fix")
            {
                this.enableBoat();
                LogicProp prop = getLogicProp(GameIDList.LogicProp_BrokenHouse);
                obj.position = prop.Position;
                obj.Rotation = prop.Rotation;
                IsPositioningObject = false;
                userPlacedObjectList.Add(obj);
            }
            else
            {
                uiManager.Add(Infobox.MakeMessage("Use the arrow keys to move the object. Up moves forward,", "ibxCOP1", 15000, runningTime));
                uiManager.Add(Infobox.MakeMessage("Down moves backward, Left moves left, Right move right.", "ibxCOP2", 15000, runningTime, 24));
                uiManager.Add(Infobox.MakeMessage("The number pad is also used for movement. 8 moves up,", "ibxCOP3", 15000, runningTime, 48));
                uiManager.Add(Infobox.MakeMessage("2 moves down, and 1 and 3 rotate the object.", "ibxCOP4", 15000, runningTime, 72));

                userPlacedObjectList.Add(obj);
                IsPositioningObject = true;
                CurrentCustomObject = obj;
            }

        }

        /// <summary>turns on BoatA, when the player fixes the house</summary>
        private void enableBoat()
        {
            LogicProp prop = getLogicProp(GameIDList.LogicProp_BoatA);
            prop.Enabled = true;
        }

        /// <summary>called when the player fixes the damn, unblocks him from crossing</summary>
        private void turnOffTriggerBlock()
        {
            foreach (Trigger trig in world.TriggerList)
            {
                if (trig.ID == 23)
                {
                    trig.HasTriggered = true;
                    trig.Repeatable = false;
                    return;
                }
            }
        }

        /// <summary>called when the player places the dam fix</summary>
        private void lowerRiver()
        {
            if (player.riverIsLowered == true)
                return;

            player.riverIsLowered = true;
            world.WaterList[(int)WaterIndices.WATER_FLOODED_RIVER].PositionY -= 300f;
        }

        /// <summary>New function for checkking when to place an object (will be removed or remapped)</summary>
        private void pollForPlacement(KeyboardState kb)
        {
        }

        /// <summary>place a completed object in the world</summary>
        /// <param name="objectIndex">index of the object in inventory's list</param>
        /// <param name="position">position is worldspace</param>
        private void placeCompletedObject(int objectIndex, Vector3 position)
        {
            if (objectIndex >= player.Inventory.customObjects.Length ||
                objectIndex < 0)
                return;

            CompleteObject temp = player.Inventory.customObjects[objectIndex];

            if (temp != null)
            {
                CompleteObject objectToPlace = new Game_Objects.Build_System.CompleteObject(temp);

                objectToPlace.stride = temp.stride;
                objectToPlace.position = position;

                userPlacedObjectList.Add(objectToPlace);
            }
        }

        /// <summary>main update call</summary>
        public override void update(GameTime time)
        {
            updateAudio();
            uiManager.Update(time);
            goalMarker.Update(time);

            if (isFading)
                if (currentAlpha > 0)
                    currentAlpha -= 3;
                else
                {
                    isFading = false;
                    currentAlpha = 0;
                }

            foreach (CompleteObject c in userPlacedObjectList)
                c.update(time);

            runningTime = time;

            if (IsPositioningObject)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    IsPositioningObject = !IsPositioningObject;
                }
                MoveCurrentCustomObject();
            }
            else
            {
                player.Update(time);
                world.Update(time);
                levelEditor.Update(time);
            }
        }

        public void MoveCurrentCustomObject()
        {
            if (CurrentCustomObject != null)
            {
                Vector3 distance = Vector3.Zero;
                Vector3 rotation = Vector3.Zero;
                float speed = 1.0f;

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    distance.Z += speed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    distance.Z -= speed;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    distance.X += speed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    distance.X -= speed;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.NumPad8))
                {
                    distance.Y += speed;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad2))
                {
                    distance.Y -= speed;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.NumPad1))
                {
                    rotation.Y += speed;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.NumPad3))
                {
                    rotation.Y -= speed;
                }

                CurrentCustomObject.Move(distance);
                CurrentCustomObject.AddRotation(rotation);
            }
            else
            {
                IsPositioningObject = false;
            }
        }

        /// <summary>update the audio listener parameters</summary>
        private void updateAudio()
        {
            audio.Listener.Position = camera.Position;
            audio.Listener.Forward = camera.RotationMatrix.Forward;
            audio.Listener.Up = camera.RotationMatrix.Up;
        }

        /// <summary> main draw 2D call</summary>
        public override void render2D(GameTime time, SpriteBatch batch) 
        {
            if(levelEditorActive == true)
                levelEditor.DrawText(batch);
            uiManager.Draw(batch);

            if(isFading)
                drawFade(batch);
        }

        /// <summary>draws the i/o fade</summary>
        private void drawFade(SpriteBatch batch)
        {
            batch.Draw(this.fadeTex,
                new Rectangle(0, 0, Renderer.getInstance().OriginalViewport.Width, Renderer.getInstance().OriginalViewport.Height),
                new Rectangle(0, 0, 64, 64), Color.FromNonPremultiplied(0, 0, 0, currentAlpha));
        }

        /// <summary>main draw 3D call</summary>
        public override void render3D(GameTime time) 
        {
            world.Draw();
            player.Draw();
            if(Vector3.Distance(goalMarker.Position, camera.Position) <= WorldManager.cullSettings.PropCullDistance)
                goalMarker.Draw();

            foreach (CompleteObject obj in userPlacedObjectList)
            {
                if(Vector3.Distance(obj.position, camera.Position) <= WorldManager.cullSettings.PropCullDistance)
                {
                    obj.render();
                    if (Game1.drawDevelopment)
                    {
                        obj.drawOBB();
                    }
                }
            }

            foreach (LogicProp prop in logicPropList)
            {
                if (prop != null)
                {
                    if (Vector3.Distance(prop.Position, camera.Position) <= WorldManager.cullSettings.PropCullDistance)
                    {
                        prop.Draw();
                    }
                }
            }

            if(levelEditorActive == true)
                levelEditor.Draw();
        }

        /// <summary>Saves the list of placed completed objects</summary>
        public void Save()
        {
            List<CompleteObjXmlMedium> outputList = new List<CompleteObjXmlMedium>();
            foreach (CompleteObject obj in this.CompleteObjectList)
            {
                outputList.Add(obj.getXmlMedium());
            }

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("placedCompleteObjects.xml");
            xmlout.Serialize(writer, outputList);
            writer.Close();
        }

        /// <summary>receive a new logic prop into the world</summary>
        public void AddLogicalProp(LogicProp prop)
        {
            if(prop != null)
                logicPropList.Add(prop);
        }

        #endregion API


        #region Trigger Callbacks

        public void tut0_callback()
        {
            //use WASD to move (or click terrain if possible)
            uiManager.Add(Infobox.MakeMessage("Use W to move Andy forward, S for backward, and rotate him left and right with A and D", "ibxMovement1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("Use Spacebar to jump, and Shift to toggle between running and walking.", "ibxMovement2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void tut1_callback()
        {
            //use middle mouse to move camera
            uiManager.Add(Infobox.MakeMessage("To rotate the camera press and hold the middle mouse", "ibxMovement3", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("button while moving the mouse. To zoom in and out", "ibsMovement4", 15000, runningTime, 24));
            uiManager.Add(Infobox.MakeMessage("use the scroll wheel. Press TAB to view your inventory,", "ibsMovement5", 15000, runningTime, 48));
            uiManager.Add(Infobox.MakeMessage("build objects, view the map, and more.", "ibsMovement6", 15000, runningTime, 72));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }
        //new
        public void tut1_2_callback()
        {
            //click or press Q to chop down a tree
            uiManager.Add(Infobox.MakeMessage("Chop down trees to get lumber! Lumber is used to make", "ibxLumber1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("objects in the Build App. To chop down trees left-click", "ibxLumber2", 15000, runningTime, 24));
            uiManager.Add(Infobox.MakeMessage("a tree or press Q when you're near one to chop it down.", "ibxLumber3", 15000, runningTime, 48));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }
        
        public void ravineFall_callback()
        {
            //the player fell down the ravine
            player.Position = new Vector3(-43349.73f, -1318.808f, -34307.091f); //hardcode position
            player.canJump = player.animPlaying = false;
            uiManager.Add(Infobox.MakeMessage("There's more to explore on the other side of this river", "ibxRavine1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("but it's way too deep to cross. I need something...", "ibxRavine2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void tip0_callback()
        {
            //first obstacle tip - cliff
            uiManager.Add(Infobox.MakeMessage("I could never climb this ridiculous cliff.", "ibxCliff1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("I'd need a big ladder or something else.", "ibxCliff2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void tip1_callback()
        {
            //second obstacle tip - ravine
            uiManager.Add(Infobox.MakeMessage("There's more to explore on the other side of this river", "ibxRavine1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("but it's way too rough to cross. I need something...", "ibxRavine2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void tip2_callback()
        {
            //third obstacle tip - flooded river
            uiManager.Add(Infobox.MakeMessage("Agh, a bridge would never hold up to this river.", "ibxFlood1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("I just need something to slow the flow.", "ibxFlood2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void tip3_callback()
        {
            //fourth obstacle tip - cliff downwards
            uiManager.Add(Infobox.MakeMessage("This is so steep. A ladder would be tough to position.", "ibxCliff3", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("Maybe some stairs or something would be better.", "ibsCliff4", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void tip4_callback()
        {
            //fif obstacle tip - another clipp upwards
            uiManager.Add(Infobox.MakeMessage("I've done this before. I think a ladder is in order here.", "ibxCliff5", 15000, runningTime));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void goal_callback()
        {
            //you win the game
            uiManager.Add(Infobox.MakeMessage("Whew I made it back safely. Good thing I'm so good", "ibxWin1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("at inventing.", "ibxWin2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }
        //new...
        public void guide0_callback()
        {
            //up the first cliff, stick to the path
            uiManager.Add(Infobox.MakeMessage("Hey there's a path here. I bet I could follow", "ibxFollowPath1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("this and safely make it back home.", "ibxFollowPath2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;

        }

        public void guide1_callback()
        {
            //over the ravine, you hear a river in the distance
            uiManager.Add(Infobox.MakeMessage("I think I can hear a river in the distance. I'm", "ibxRiver1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("going to trust my ears and try to find it.", "ibxRiver2", 15000, runningTime, 24));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void guide2_callback()
        {
            //explore!, if you get lost consult your map
            uiManager.Add(Infobox.MakeMessage("Woah this forest is so big. I could spend a lot", "ibxExplore1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("of time roaming around here. I wonder what there", "ibxExplore2", 15000, runningTime, 24));
            uiManager.Add(Infobox.MakeMessage("is out here...", "ibxExplore3", 15000, runningTime, 48));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        public void guide3_callback()
        {
            //tree line is too thick to cross, look around for help
            uiManager.Add(Infobox.MakeMessage("This series of trees is way too thick for me to", "ibxTreeline1", 15000, runningTime));
            uiManager.Add(Infobox.MakeMessage("fit my burly body through, even if I chopped them", "ibxTreeline2", 15000, runningTime, 24));
            uiManager.Add(Infobox.MakeMessage("all down! Better stick to the path.", "ibxTreeline3", 15000, runningTime, 48));
            SoundClip clip = audio.Play2DSound("pling", false);
            clip.soundInstance.Volume = AudioManager.PlingVolume;
        }

        #endregion Trigger Callbacks


        #region Camera Flyby Steps
        /// <summary>
        /// This is a series of camera.SmoothSteps, that takes the player's camera into a 
        /// a flyby throughout the entire level, and most major points.
        /// Each smooth step when complete will trigger another smoothstep to the next point
        /// </summary>

        ///at start to goal
        private void beginCameraFlyby()
        {
            camera.Position = new Vector3(-29010.01f, -439.3148f, -49362.82f);
            camera.LookAtTarget = new Vector3(-28293.79f, -623.827f, -49387.83f);

            player.LockInput = true;

            camera.SmoothStepTo(
                new Vector3(-10705.42f, -784.0347f, -54551.15f),
                new Vector3(-11367.2f, -670.3306f, -54443.84f),
                flyby_Step1,
                .06f, 350f);
        }

        //at goal to house
        public void flyby_Step1()
        {
            camera.SmoothStepTo(
                new Vector3(-4319.8f, -965.5f, -45258.4f),
                new Vector3(-5068.1f, -547.1f, -45628.8f),
                flyby_Step2,
                .06f, 350f);
        }

        /// <summary>at house to trail</summary>
        public void flyby_Step2()
        {
            camera.SmoothStepTo(
                new Vector3(-4348.4f, -3021.7f, -42001.9f),
                new Vector3(-3672.6f, -2924.8f, -42419.3f),
                flyby_Step3,
                .06f, 350f);
        }

        /// <summary>at trail to cliff</summary>
        public void flyby_Step3()
        {
            camera.SmoothStepTo(
                new Vector3(-28448.97f, 774.03f, -42125.11f),
                new Vector3(-28427.4f, 1047.1f, -42725.7f),
                flyby_Step4,
                .06f, 350f);
        }

        /// <summary>at cliff to dam</summary>
        public void flyby_Step4()
        {
            camera.SmoothStepTo(
                new Vector3(-26208.6f, -423.7f, -27686.4f),
                new Vector3(-26145.1f, -166.5f, -28224.6f),
                flyby_Step5,
                .06f, 350f);
        }

        /// <summary>at dam to ravine</summary>
        public void flyby_Step5()
        {
            camera.SmoothStepTo(
                new Vector3(-36619.7f, -907.225f, -32623.8f),
                new Vector3(-35967.4f, -713.2052f, -32669.12f),
                flyby_Step6,
                .06f, 350f);
        }

        /// <summary>at ravine to cliff</summary>
        public void flyby_Step6()
        {
            camera.SmoothStepTo(
                new Vector3(-49929.69f, -566.96f, -32242.46f),
                new Vector3(-49867.88f, -360.0489f, -33271.46f),
                flyby_Step7,
                .06f, 350f);
        }

        /// <summary>at clif to player</summary>
        public void flyby_Step7()
        {
            camera.SmoothStepTo(
                new Vector3(-56448.4f, -2665.7f, -16156.7f),
                new Vector3(-56434.8f, -2610.6f, -16625.7f),
                flyby_Step8,
                .06f, 350f);
        }

        /// <summary>control to player</summary>
        public void flyby_Step8()
        {
            camera.CameraType = CameraType.CAM_CHASE;
            player.LockInput = false;
            this.tut0_callback();
        }

        #endregion Camera Flyby Steps


        #region Mutators

        public Player getPlayer()
        {
            return player;
        }
        public bool ContinueGame
        {
            get { return this.loadContinued; }
            set { this.loadContinued = value; }
        }
        public List<CompleteObject> CompleteObjectList
        {
            get { return userPlacedObjectList; }
        }
        public List<LogicProp> LogicPropList
        {
            get { return this.logicPropList; }
        }
        public LogicProp getLogicProp(int id)
        {
            foreach (LogicProp prop in logicPropList)
            {
                if (prop.ID == id)
                    return prop;
            }
            return null;
        }

        #endregion Mutators
    }
}