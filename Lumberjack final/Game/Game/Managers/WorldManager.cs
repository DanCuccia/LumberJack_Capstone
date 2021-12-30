using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Xml.Serialization;
using System.IO;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Game_Objects.Trees;
using Game.Managers.Factories;
using Game.States;

#pragma warning disable 0168 //fileNotFound excecption not being used

namespace Game.Managers
{
    public enum NodeCullMode
    {
        CULL_NONE
    }

    public enum PropCullMode
    {
        CULL_NONE,
        CULL_DISTANCE
    }

    public enum WaterIndices
    {
        WATER_POND = 0,
        WATER_STILL_LAKE,
        WATER_FORK_RIVER,
        WATER_FLOODED_RIVER,
        WATER_WATERFALL_FEED,
        WATER_WATERFALL,
        COUNT
    }

    /// <summary>This holds culling parameters, worldManager holds it public static</summary>
    public class CullSettings
    {
        public NodeCullMode     NodeCullMode                = NodeCullMode.CULL_NONE;
        public PropCullMode     PropCullMode                = PropCullMode.CULL_DISTANCE;
        public PropCullMode     OBBCullMode                 = PropCullMode.CULL_DISTANCE;
        public float            PropCullDistance            = 25000;
        public float            OBBCullDistance             = 10000;
        public float            OBBCollisionDistance        = 2000;
        public float            ParticleEmitterUpdateRange  = 15000;

        public CullSettings(){}

        public CullSettings(NodeCullMode nodeMode, PropCullMode propMode, float distance)
        {
            NodeCullMode = nodeMode;
            PropCullMode = propMode;
            PropCullDistance = distance;
        }
    }

    /// <summary>WorldManager is the World. It holds each world node, 
    /// which holds the props, the land, the water etc...
    /// Is a singlton, accessable anywhere.</summary>
    class WorldManager
    {
        private static WorldManager m_myInstance;
        public static WorldManager getInstance() 
        {
            if (m_myInstance == null)
                m_myInstance = new WorldManager();
            return m_myInstance; 
        }


        #region Member Variables


        GraphicsDevice                      device;   //ref
        Camera                              camera;   //not a ref
        ContentManager                      content;  //ref
        StateManager                        stateMan; //ref

        SkyBox                              skybox;
        Point3D                             point;

        int                                 worldSize;
        WorldNode[,]                        worldNodes;

        List<WaterVolume>                   waterList;
        List<WorldLocation>                 stumpList;
        List<Trigger>                       triggerList;
        List<Vector3>                       usefulLocations;
        List<BillboardParticleEmitter>      particleEmitterList;
        List<Prop>                          specialProps;

        public static CullSettings          cullSettings = new CullSettings();


        #endregion Member Variables


        #region Initialization

        /// <summary> World Manager is a Singleton</summary>
        private WorldManager(){}

        /// <summary>Main Initialize, must be called </summary>
        /// <param name="device">reference to graphice device</param>
        /// <param name="worldSize">how big x,y your world in nodes is</param>
        public void Initialize(ContentManager content, GraphicsDevice device, StateManager stateMan)
        {
            this.content = content;
            this.device = device;
            worldSize = 8;        //hardcode
            this.stateMan = stateMan;

            camera = new Camera(device.Viewport);
            camera.Initialize(CameraType.CAM_FREE, device.Viewport.AspectRatio);

            worldNodes = new WorldNode[worldSize, worldSize];

            waterList = new List<WaterVolume>();
            stumpList = new List<WorldLocation>();
            triggerList = new List<Trigger>();
            usefulLocations = new List<Vector3>();
            particleEmitterList = new List<BillboardParticleEmitter>();
            specialProps = new List<Prop>();

            loadUsefulLocations();
            point = new Point3D(device);
        }

        /// <summary>Main menu is a single populated node</summary>
        public void InitializeForMainMenu(ContentManager content, GraphicsDevice device)
        {
            this.content = content;
            this.device = device;
            worldSize = 1;

            camera = new Camera(device.Viewport);
            camera.Initialize(CameraType.CAM_FREE, device.Viewport.AspectRatio);

            worldNodes = new WorldNode[worldSize, worldSize];

            waterList = new List<WaterVolume>();
            stumpList = new List<WorldLocation>();
            triggerList = new List<Trigger>();
            specialProps = new List<Prop>();
        }

        /// <summary>Clear all objects, ready for next use of the WorldManager(it is singleton)</summary>
        public void ClearAll()
        {
            worldNodes = null;
            if(waterList != null)
                waterList.Clear();
            waterList = null;
            if(triggerList != null)
                triggerList.Clear();
            triggerList = null;
            camera = null;
            if (usefulLocations != null)
                usefulLocations.Clear();
            if (particleEmitterList != null)
                particleEmitterList.Clear();
            if (specialProps != null)
                specialProps.Clear();
        }

        #endregion Initialization


        #region API

        /// <summary>Main Update Call, individual nodes hold their own logic</summary>
        public void Update(GameTime gameTime) 
        {
            for (int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                    if (worldNodes[x, y] != null)
                        worldNodes[x, y].Update(gameTime);
            
            updateStumps(gameTime);
            updateEmitters(gameTime);
        }

        private void updateEmitters(GameTime time)
        {
            if (particleEmitterList == null)
                return;

            foreach (BillboardParticleEmitter emitter in particleEmitterList)
            {
                if (Vector3.Distance(camera.Position, emitter.Position) <= cullSettings.ParticleEmitterUpdateRange)
                {
                    emitter.Animate(time);
                    emitter.Update(time);
                }
            }
        }

        /// <summary>update the registered world locations</summary>
        private void updateStumps(GameTime gameTime)
        {
            foreach (WorldLocation loc in stumpList)
            {
                Tree stump = (Tree) worldNodes[loc.worldIndicies.X, loc.worldIndicies.Y].PropList[loc.listIndex];

                if (stump == null)
                    continue;

                stump.stumpTime += gameTime.ElapsedGameTime.Milliseconds;
                if (stump.stumpTime >= Tree.StumpDuration)
                {
                    Tree tree = null;
                    switch (stump.ID)
                    {
                        case GameIDList.Prop_SmallStump:
                            tree = new SmallFullTree(content);
                            break;
                        case GameIDList.Prop_BigStump:
                            tree = new LargeFullTree(content);
                            break;
                        case GameIDList.Prop_ThinTreeStump:
                            tree = new ThinTree(content);
                            break;
                    }
                    if (tree != null)
                    {
                        tree.Position = stump.PPosition;
                        tree.Rotation = stump.PRotation;
                        tree.Scale = stump.PScale;
                        tree.LocationData = stump.LocationData;
                        Matrix orig = tree.GetBoundingModel().WorldMatrix;
                        tree.GetBoundingModel().WorldMatrix = Matrix.CreateScale(new Vector3(.5f, 1f, .5f)) * orig;
                        tree.GetBoundingModel().GenerateBoundingBox(false);
                        tree.GetBoundingModel().UpdateBoundingBox();
                        tree.GetBoundingModel().WorldMatrix = orig;
                        worldNodes[loc.worldIndicies.X, loc.worldIndicies.Y].PropList[loc.listIndex] = tree;
                    }
                }
            }
        }

        /// <summary>Main Draw Call, draws the world and everything in it</summary>
        public void Draw()
        {
            if (Renderer.m_renderPhase != RenderPhase.PHASE_DEPTH)
                skybox.Draw();

            switch (cullSettings.NodeCullMode)
            {
                case NodeCullMode.CULL_NONE:

                    for (int x = 0; x < worldSize; x++)
                        for (int y = 0; y < worldSize; y++)
                            if (worldNodes[x, y] != null)
                            {
                                worldNodes[x, y].DrawLand();
                                worldNodes[x, y].DrawProps();
                            }

                    foreach (Prop prop in specialProps)
                        prop.Draw();

                    foreach (WaterVolume water in waterList)
                        water.Draw();
                    
                    drawEmitters();

                    if (Game1.drawDevelopment)
                    {
                        drawTriggerList();
                        drawUsefulLocations();
                    }
                    break;
            }
            
        }

        /// <summary>draw particle emitters within range</summary>
        private void drawEmitters()
        {
            if (particleEmitterList == null)
                return;
            foreach (BillboardParticleEmitter emitter in particleEmitterList)
            {
                if(Vector3.Distance(emitter.Position, camera.Position) <= cullSettings.ParticleEmitterUpdateRange)
                    emitter.Draw();
            }
        }

        /// <summary>draw all trigger list</summary>
        private void drawTriggerList()
        {
            foreach (Trigger trigger in triggerList)
            {
                trigger.Draw();
                if (cullSettings.OBBCullMode == PropCullMode.CULL_DISTANCE)
                    trigger.Model.DrawOBB();
            }
        }

        /// <summary>draw all the "useful location" points</summary>
        private void drawUsefulLocations()
        {
            if (usefulLocations != null)
            {
                foreach (Vector3 pos in usefulLocations)
                {
                    point.Position = pos;
                    point.Draw();
                }
            }
        }

        /// <summary>this will output height from the given position, it will return
        /// what world node that position belongs to</summary>
        /// <param name="position">input position to test for</param>
        /// <param name="height">the output variable of the terrain height found</param>
        /// <returns>indices of what node the position is within</returns>
        public Point getTerrainHeight(Vector3 position, out float height)
        {
            //todo: refine...
            for (int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                    if (worldNodes[x, y] != null)
                    {
                        Vector3 pos = position - worldNodes[x, y].Position;
                        if (worldNodes[x, y].Terrain.HeightData.IsOnHeightMap(pos))
                        {
                            worldNodes[x, y].Terrain.HeightData.GetHeight(pos, out height);
                            return new Point(x, y);
                        }
                    }

            height = position.Y;
            return new Point(-1, -1);
        }

        /// <summary>If you are definate that you know what world nodes your in, use this faster overload</summary>
        /// <param name="worldIndicies">indices to has into worldNodes</param>
        /// <param name="position">the position to test</param>
        /// <returns>the height of the terrain at that point</returns>
        public float getTerrainHeight(Point worldIndicies, Vector3 position)
        {
            float h;
            worldNodes[worldIndicies.X, worldIndicies.Y].Terrain.HeightData.GetHeight(position, out h);
            return h;
        }

        /// <summary>get the prop count of a particular node</summary>
        public int GetNodePropCount(Point node)
        {
            return worldNodes[node.X, node.Y].GetPropCount();
        }

        /// <summary>Get the total prop count of the world</summary>
        public int GetTotalPropCount()
        {
            int total = 0;
            for (int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                    total += worldNodes[x, y].GetPropCount();
            return total;
        }

        /// <summary>Get a node's propList using indicies</summary>
        /// <param name="worldIndicies">the node indicies of the world</param>
        /// <returns>the node's populated propList, if any</returns>
        public List<Prop> GetNodePropList(Point worldIndicies)
        {
            return worldNodes[worldIndicies.X, worldIndicies.Y].PropList;
        }

        /// <summary>Get a node's propList using only world position data</summary>
        /// <param name="position">the world position, will search through node's</param>
        /// <returns>the propList of the node in which the position is within</returns>
        public List<Prop> GetNodePropList(Vector3 position)
        {
            for(int x=0; x<worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                {
                    if (worldNodes[x, y].Terrain.HeightData.IsOnHeightMap(position))
                    {
                        return worldNodes[x, y].PropList;
                    }
                }
            return null;
        }

        /// <summary>The Starting point of the player, this could be elaborated on</summary>
        /// <returns>the player's 'new game' starting point</returns>
        public Vector3 GetPlayerStartGamePosition()
        {
            return worldNodes[7, 2].Position;
        }

        /// <summary>Get the 8 world node indices surrounding a specified index</summary>
        /// <param name="indicies">the center index of the 9 patches you want</param>
        /// <returns>up to 8 world node indicies</returns>
        public List<Point> GetSurroundingNodes(Point indicies)
        {
            List<Point> output = new List<Point>();

            Point index = indicies;
            index.Y--;
            index.X--;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Point temp = new Point(index.X + x, index.Y + y);
                    if (temp.X >= 0 && temp.X < worldSize &&
                        temp.Y >= 0 && temp.Y < worldSize )
                        if(temp != indicies)
                            output.Add(temp);
                }
            }

            return output;
        }

        /// <summary>Run collision tests on all props in your initial node, and the 8 surrounding nodes</summary>
        /// <param name="nodeIndex">node index of the model your testing</param>
        /// <param name="model">the model your testing against the props</param>
        /// <returns>true for collision</returns>
        public bool TestPropCollisions(Point nodeIndex, StaticMesh model, out Prop collidedProp, bool oneNode = false)
        {
            collidedProp = null;
            if (nodeIndex == new Point(-1, -1))
                return false;

            List<Point> nodeIndices = this.GetSurroundingNodes(nodeIndex);
            nodeIndices.Insert(0, nodeIndex);

            foreach (Point worldIndex in nodeIndices)
            {
                foreach (Prop prop in worldNodes[worldIndex.X, worldIndex.Y].PropList)
                {
                    if (Vector3.Distance(prop.PPosition, model.Position) <= cullSettings.OBBCollisionDistance)
                    {
                        if (prop.GetBoundingModel().OBB.Intersects(model.OBB))
                        {
                            collidedProp = prop;
                            return true;
                        }
                    }
                }
                if (oneNode)
                    return false;
            }
            return false;
        }

        /// <summary>Will run the same collision test against within the propList, 
        /// but exclue anything that is not a world boundary</summary>
        /// <param name="nodeIndex">node index of the model your testing</param>
        /// <param name="model">the model your testing against the props</param>
        /// <returns>true for collision</returns>
        public bool TestBoundaryCollision(Point nodeIndex, StaticMesh model)
        {
            if (nodeIndex == new Point(-1, -1))
                return false;

            List<Point> nodeIndices = this.GetSurroundingNodes(nodeIndex);
            nodeIndices.Insert(0, nodeIndex);

            foreach (Point worldIndex in nodeIndices)
            {
                foreach (Prop prop in worldNodes[worldIndex.X, worldIndex.Y].PropList)
                {
                    if(GameIDList.IsBorder(prop.ID) && Vector3.Distance(prop.PPosition, model.Position) <= cullSettings.OBBCollisionDistance)
                        if (prop.GetBoundingModel().OBB.Intersects(model.OBB))
                            return true;

                }
            }
            return false;
        }

        /// <summary>Test for any collisions within the world, 
        /// will test distance first, same as object cullingDistance</summary>
        /// <param name="nodeIndex">what node in the world the model is in</param>
        /// <param name="model">the model to test against</param>
        /// <returns>true/false a collision occured, the trigger will execute itself</returns>
        public bool TestTriggerCollisions(StaticMesh model)
        {
            if (model == null)
                return false;

            foreach (Trigger trigger in triggerList)
            {
                if (Vector3.Distance(model.Position, trigger.Model.Position) <= cullSettings.OBBCollisionDistance)
                    if (trigger.Model.OBB.Intersects(model.OBB))
                    {
                        trigger.Execute();
                        return true;
                    }
            }

            return false;
        }

        /// <summary>add a world location to be updated by the world manager</summary>
        /// <param name="location">a tree's location in the world</param>
        public void RegisterStump(WorldLocation location)
        {
            stumpList.Add(location);
        }

        #endregion API


        #region Building

        /// <summary>Main Menu has only 1 node, so do all building with this call</summary>
        public void BuildForMainMenu(ContentManager content)
        {
            worldNodes[0, 0] = new WorldNode(this);
            worldNodes[0, 0].Initialize(content, "heightmaps\\mainMenuTerrainMap", Vector3.Zero);

            WaterVolume water = new WaterVolume(device);
            float stride = worldNodes[0,0].Terrain.HeightData.WorldSpaceCorners.X * 2;
            water.Initialize(content, stride, stride);
            water.Parameters = WaterSettings.GetSettingsTwo();
            water.PositionY -= 1750;    //hardcode!
            water.PositionX += (worldNodes[0, 0].Terrain.HeightData.WorldSpaceCorners.X);
            water.PositionZ -= (worldNodes[0, 0].Terrain.HeightData.WorldSpaceCorners.X);
            waterList.Add(water);

            this.BuildSkybox(content);
        }

        /// <summary>Create all of the terrain patches, and correctly position them in world space </summary>
        public void BuildLand(ContentManager content)
        {
            float stride = 0f;
            for(int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                {
                    string filename = "heightmaps\\worldHeightMaps\\" + (7-x) + (7-y);
                    worldNodes[x, y] = new WorldNode(this, (x * 10) + y);

                    Vector3 pos = new Vector3(x * stride, 0, y * stride);
                    worldNodes[x, y].Initialize(content, filename, pos);

                    stride = worldNodes[x, y].Terrain.HeightData.WorldSpaceCorners.X * 2;
                }
        }

        /// <summary> Load all node propLists from xml. Individual nodes do their own loading.</summary>
        public void BuildProps(ContentManager content)
        {
            for (int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                {
                    worldNodes[x, y].LoadPropList(content);
                }
        }

        /// <summary>Builds the list of water volume objects from XML</summary>
        public void BuildWater(ContentManager content)
        {
            List<WaterXMLStruct> inputList = new List<WaterXMLStruct>();

            TextReader reader = new StreamReader("content\\data\\worldWaterList.xml");
            XmlSerializer xmlIn = new XmlSerializer(inputList.GetType());
            inputList = (List<WaterXMLStruct>)xmlIn.Deserialize(reader);

            WaterFactory factory = new WaterFactory();

            foreach (WaterXMLStruct xmlWater in inputList)
                waterList.Add(factory.GetCompleteWaterVolume(content, device, xmlWater));
            
            reader.Close();
        }

        /// <summary>Load all triggers, including their states, from XML</summary>
        public void BuildTriggers(ContentManager content)
        {
            List<TriggerXmlMedium> inputList = new List<TriggerXmlMedium>();

            TextReader reader = new StreamReader("content\\data\\worldTriggerList.xml");
            XmlSerializer xmlIn = new XmlSerializer(inputList.GetType());
            inputList = (List<TriggerXmlMedium>)xmlIn.Deserialize(reader);

            TriggerFactory factory = new TriggerFactory((GameState)stateMan.getState("game"));

            foreach (TriggerXmlMedium xmlTrigger in inputList)
                triggerList.Add(factory.GetTrigger(content, xmlTrigger));
            
            reader.Close();
        }

        public void BuildPlayerTriggers(ContentManager content)
        {
            try
            {
                List<TriggerXmlMedium> inputList = new List<TriggerXmlMedium>();

                TextReader reader = new StreamReader("playerTriggers.xml");
                XmlSerializer xmlIn = new XmlSerializer(inputList.GetType());
                inputList = (List<TriggerXmlMedium>)xmlIn.Deserialize(reader);

                TriggerFactory factory = new TriggerFactory((GameState)stateMan.getState("game"));

                foreach (TriggerXmlMedium xmlTrigger in inputList)
                    triggerList.Add(factory.GetTrigger(content, xmlTrigger));

                reader.Close();
            }
            catch (FileNotFoundException e)
            {
                //there is no player save
            }
        }

        /// <summary>build the skybox object</summary>
        public void BuildSkybox(ContentManager content)
        {
            skybox = new SkyBox();
            if(Game1.runLowDef)
                skybox.Initialize(device, camera, content, "skybox\\Cubemap_LostValley_512");
            else
                skybox.Initialize(device, camera, content, "skybox\\Cubemap_LostValley_1024");
        }

        /// <summary>load the world's particle effects</summary>
        public void BuildParticleEmitters(ContentManager content)
        {
            particleEmitterList = new List<BillboardParticleEmitter>();

            List<ParticleEmitterXmlMedium> inputList = new List<ParticleEmitterXmlMedium>();

            TextReader reader= new StreamReader("content\\data\\worldEmitterList.xml");
            XmlSerializer xmlIn = new XmlSerializer(inputList.GetType());
            inputList = (List<ParticleEmitterXmlMedium>)xmlIn.Deserialize(reader);

            ParticleEmitterFactory factory = new ParticleEmitterFactory(device);

            foreach (ParticleEmitterXmlMedium input in inputList)
                particleEmitterList.Add(factory.GetCompleteEmitter(content, input));

            reader.Close();
        }

        public void BuildSpecialProps(ContentManager content)
        {
            //scooby-doo
        }

        #endregion Building


        #region Saving

        /// <summary>Save All current props in the node prop lists, Individual nodes do their own saving,
        /// this is used for the player's save </summary>
        public void Save()
        {
            for (int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                {
                    worldNodes[x, y].SavePropList();
                }

            //SaveWaterList();
            //SaveTriggerList();
        }

        /// <summary>Save the list of usefull locations</summary>
        public void saveUsefulLocations()
        {
            XmlSerializer xmlout = new XmlSerializer(usefulLocations.GetType());
            TextWriter writer = new StreamWriter("content\\data\\worldUsefulLocations.xml");
            xmlout.Serialize(writer, usefulLocations);
            writer.Close();
        }

        /// <summary>Load the list of usefull locations</summary>
        public void loadUsefulLocations()
        {
            TextReader reader = new StreamReader("content\\data\\worldUsefulLocations.xml");
            XmlSerializer xmlin = new XmlSerializer(usefulLocations.GetType());
            usefulLocations = (List<Vector3>)xmlin.Deserialize(reader);
            reader.Close();
        }

        /// <summary>Save the prop, trigger, and usefull location list, used by LevelEditor</summary>
        public void LevelEditorSave()
        {
            for (int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                {
                    worldNodes[x, y].SavePropList();
                }

            SaveTriggerList();
            SaveEmitterList();

            GameState game = (GameState)stateMan.getState("game");
            game.saveLogicProps();

            //saveUsefulLocations();
        }

        /// <summary>Use this for saving the Main Menu version</summary>
        public void SaveForMainMenu()
        {
            worldNodes[0, 0].SavePropList();
        }

        /// <summary>Was used with level-editor in early development periods, 
        /// not needed anymore -- DO NOT CALL</summary>
        private void SaveWaterList()
        {
            List<WaterXMLStruct> outputList = new List<WaterXMLStruct>();
            foreach (WaterVolume water in waterList)
                outputList.Add(water.GetXmlStruct());

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("cotnent\\data\\worldWaterList.xml");
            xmlout.Serialize(writer, outputList);
            writer.Close();
        }

        /// <summary>Save the triggers in the list, including their states</summary>
        public void SaveTriggerList()
        {
            List<TriggerXmlMedium> outputList = new List<TriggerXmlMedium>();
            foreach (Trigger trigger in triggerList)
                outputList.Add(trigger.GetXmlMedium());

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("playerTriggers.xml");
            xmlout.Serialize(writer, outputList);
            writer.Close();
        }

        public void SaveEmitterList()
        {
            List<ParticleEmitterXmlMedium> outputList = new List<ParticleEmitterXmlMedium>();
            foreach (BillboardParticleEmitter emitter in particleEmitterList)
                outputList.Add(emitter.GetXmlMedium());

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("worldEmitterList.xml");
            xmlout.Serialize(writer, outputList);
            writer.Close();
        }

        public void SaveSpecialPropList()
        {
            //todo
        }

        #endregion Saving


        #region Mutators

        public Camera Camera
        {
            get { return camera; }
        }
        public GraphicsDevice Device
        {
            get { return device; }
        }
        public WorldNode this[int x, int y]
        {
            get { return worldNodes[x, y]; }
        }
        public List<Trigger> TriggerList
        {
            get { return triggerList; }
        }
        public List<WaterVolume> WaterList
        {
            get { return waterList; }
        }
        public List<Vector3> UsefullLocations
        {
            get { return usefulLocations; }
        }

        #endregion Mutators


        #region Level Editing

        /// <summary>Was used in early stages of level editing, not used anymore</summary>
        public void addWaterVolume(WaterVolume water)
        {
            if(water != null)
                waterList.Add(water);
        }

        /// <summary>Used by LevelEditor, finds the correct node of the incoming prop, 
        /// and adds it to that node's prop list</summary>
        /// <param name="prop">The fully constructed and placed prop</param>
        public void addProp(Prop prop)
        {
            for (int x = 0; x < worldSize; x++)
                for (int y = 0; y < worldSize; y++)
                    if (worldNodes[x, y] != null)
                    {
                        Vector3 pos = prop.PPosition - worldNodes[x, y].Position;
                        if (worldNodes[x, y].Terrain.HeightData.IsOnHeightMap(pos))
                        {
                            worldNodes[x, y].ReceiveNewProp(prop);
                            return;
                        }
                    }
            throw new Exception("WorldManger::addProp prop is not within a node");
        }

        /// <summary>Add a new trigger to the world trigger list
        /// note: it will not have any callback until you code it in TriggerFactory and restart the game</summary>
        /// <param name="trigger">a fully contrusted trigger, from LevelEditor</param>
        public void AddTrigger(Trigger trigger)
        {
            if (trigger != null && trigger.ID != -1)
                triggerList.Add(trigger);
        }

        /// <summary>Add a new usefull location to the list</summary>
        public void AddUsefullLocation(Vector3 location)
        {
            if (location != null && location != Vector3.Zero && usefulLocations != null)
                usefulLocations.Add(location);
        }

        public void AddEmitter(BillboardParticleEmitter emitter)
        {
            if (emitter != null)
                particleEmitterList.Add(emitter);
        }

        #endregion Level Editing

    }
}
