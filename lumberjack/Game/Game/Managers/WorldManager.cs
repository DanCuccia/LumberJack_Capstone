using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System.Xml.Serialization;
using System.IO;

using Game.Drawing_Objects;
using Game.Game_Objects;
using Game.Managers.Factories;

namespace Game.Managers
{
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

        GraphicsDevice m_device; //ref
        Camera m_camera;         //ref

        SkyBox m_skybox;

        const int m_worldSize = 8;
        WorldNode[,] m_worldNodes;

        List<WaterVolume> m_waterList;

        #endregion Member Variables



        #region Initialization

        /// <summary> World Manager is a Singleton</summary>
        private WorldManager() { }

        /// <summary>Main Initialize, must be called </summary>
        /// <param name="content">we load the skybox object in here</param>
        /// <param name="device">reference to graphice device</param>
        /// <param name="camera">refernce to main camera</param>
        public void Initialize(ContentManager content, GraphicsDevice device, Camera camera)
        {
            m_device = device;
            m_camera = camera;
            m_worldNodes = new WorldNode[m_worldSize, m_worldSize];

            m_skybox = new SkyBox();
            m_skybox.Initialize(m_device, m_camera, content, "skybox\\Cubemap_LostValley_1024");

            m_waterList = new List<WaterVolume>();
        }

        #endregion Initialization




        #region API

        /// <summary>Main Update Call, individual nodes hold their own logic</summary>
        public void Update(GameTime gameTime) 
        {
            for (int x = 0; x < m_worldSize; x++)
                for (int y = 0; y < m_worldSize; y++)
                    if (m_worldNodes[x, y] != null)
                        m_worldNodes[x, y].Update(gameTime);
        }

        /// <summary>Main Draw Call, draws the world</summary>
        public void Draw()
        {
            if (Renderer.m_renderPhase != RenderPhase.PHASE_DEPTH)
                m_skybox.Draw();

            //todo: cull nodes that aren't viewable...
            for (int x = 0; x < m_worldSize; x++)
                for (int y = 0; y < m_worldSize; y++)
                    if (m_worldNodes[x, y] != null)
                        m_worldNodes[x, y].Draw();

            foreach (WaterVolume water in m_waterList)
                water.Draw();
        }


        /// <summary>this will output height from the given position, it will return
        /// what world node that position belongs to</summary>
        /// <param name="position">input position to test for</param>
        /// <param name="height">the output variable of the terrain height found</param>
        /// <returns>indices of what node the position is within</returns>
        public Point getTerrainHeight(Vector3 position, out float height)
        {
            //todo: refine...
            for (int x = 0; x < m_worldSize; x++)
                for (int y = 0; y < m_worldSize; y++)
                    if (m_worldNodes[x, y] != null)
                    {
                        Vector3 pos = position - m_worldNodes[x, y].Position;
                        if (m_worldNodes[x, y].Terrain.HeightData.IsOnHeightMap(pos))
                        {
                            m_worldNodes[x, y].Terrain.HeightData.GetHeightAndNormal(pos, out height);
                            return new Point(x, y);
                        }
                    }

            height = position.Y;
            return new Point(-1, -1);
        }


        #endregion API


        #region BUILDING

        /// <summary>Create all of the terrain patches, and correctly position them in world space </summary>
        public void BuildLand(ContentManager content)
        {
            float stride = 0f;
            for(int x = 0; x < m_worldSize; x++)
                for (int y = 0; y < m_worldSize; y++)
                {
                    string filename = "heightmaps\\worldHeightMaps\\" + (7-x) + (7-y);
                    m_worldNodes[x, y] = new WorldNode(this, (x * 10) + y);

                    Vector3 pos = new Vector3(x * stride, 0, y * stride);

                    m_worldNodes[x, y].Initialize(content, filename, pos);

                    stride = m_worldNodes[x, y].Terrain.HeightData.WorldSpaceCorners.X * 2;
                    
                }
        }

        /// <summary> Load all node propLists from xml. Individual nodes do their own loading.</summary>
        public void BuildProps(ContentManager content)
        {
            for (int x = 0; x < m_worldSize; x++)
                for (int y = 0; y < m_worldSize; y++)
                {
                    m_worldNodes[x, y].LoadPropList(content);
                }
        }

        /// <summary>Save All current props in the node prop lists, Individual nodes do their own saving</summary>
        public void Save(ContentManager content)
        {
            for (int x = 0; x < m_worldSize; x++)
                for (int y = 0; y < m_worldSize; y++)
                {
                    m_worldNodes[x, y].SavePropList();
                }

            //SaveWaterList();
        }

        private void SaveWaterList()
        {
            List<WaterXMLStruct> outputList = new List<WaterXMLStruct>();
            foreach (WaterVolume water in m_waterList)
                outputList.Add(water.GetXmlStruct());

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("worldWaterList.xml");
            xmlout.Serialize(writer, outputList);
            writer.Close();
        }

        public void BuildWater(ContentManager content)
        {
            List<WaterXMLStruct> inputList = new List<WaterXMLStruct>();


            TextReader reader = new StreamReader("worldWaterList.xml");
            XmlSerializer xmlIn = new XmlSerializer(inputList.GetType());
            inputList = (List<WaterXMLStruct>)xmlIn.Deserialize(reader);

            WaterFactory factory = new WaterFactory();

            foreach (WaterXMLStruct xmlWater in inputList)
                m_waterList.Add(factory.GetCompleteWaterVolume(content, m_device, xmlWater));
            
            reader.Close();
        }

        #endregion BUILDING




        #region Mutators

        public Camera Camera
        {
            get { return m_camera; }
        }

        #endregion Mutators



        #region Level Editing

        public void addWaterVolume(WaterVolume water)
        {
            if(water != null)
                m_waterList.Add(water);
        }

        public void addProp(Prop tree)
        {
            for (int x = 0; x < m_worldSize; x++)
                for (int y = 0; y < m_worldSize; y++)
                    if (m_worldNodes[x, y] != null)
                    {
                        Vector3 pos = tree.PPosition - m_worldNodes[x, y].Position;
                        if (m_worldNodes[x, y].Terrain.HeightData.IsOnHeightMap(pos))
                        {
                            m_worldNodes[x, y].ReceiveNewProp(tree);
                        }
                    }
        }

        #endregion Level Editing
    }
}
