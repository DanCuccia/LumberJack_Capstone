using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using System.Reflection;
using System.Xml.Serialization;
using System.IO;

using Game.Drawing_Objects;
using Game.Game_Objects.Trees;
using Game.Managers;
using Game.Math_Physics;
using Game.Managers.Factories;

namespace Game.Game_Objects
{
    class WorldNode
    {

        #region Member Variables

        int                 m_ID;
        WorldManager        m_myManager;    //ref
        Camera              m_camera;       //ref

        Vector3             m_position = Vector3.Zero;
        Vector3             m_rotation = Vector3.Zero;

        GeneratedTerrain    m_land;
        List<Prop>          m_propList;
        

        #endregion Member Variables



        #region Initialization

        /// <summary>Default CTOR for gameState</summary>
        /// <param name="manager">where this is located</param>
        /// <param name="ID">world indices location </param>
        public WorldNode(WorldManager manager, int ID) 
        { 
            m_ID = ID;
            m_myManager = manager;
            m_camera = m_myManager.Camera;
            m_propList = new List<Prop>();
        }

        /// <summary>
        /// Overloaded version for Main Menu State, heavily hardcoded
        /// </summary>
        /// <param name="camera"></param>
        public WorldNode(WorldManager manager)
        {
            m_ID = 99;
            m_myManager = manager;
            m_camera = m_myManager.Camera;
            m_propList = new List<Prop>();
        }

        public void Initialize(ContentManager content, string heightmapFilename, Vector3 position) 
        {
            m_land = new GeneratedTerrain();
            m_land.Initialize(content, m_myManager.Camera, heightmapFilename);
            this.Position = position;
        }

        /// <summary>Load the list of props back in from the xml file</summary>
        public void LoadPropList(ContentManager content)
        {
            List<PropXMLStruct2> inputList = new List<PropXMLStruct2>();

            string id = m_ID.ToString();
            if (m_ID < 10)
                id = "0" + id;

            Point myWIndex = new Point(int.Parse(id[0].ToString()), int.Parse(id[1].ToString()));

            TextReader reader = new StreamReader("content\\data\\propList_" + id + ".xml");
            XmlSerializer xmlIn = new XmlSerializer(inputList.GetType());
            inputList = (List<PropXMLStruct2>)xmlIn.Deserialize(reader);

            TreeFactory treeFactory = new TreeFactory();
            RockFactory rockFactory = new RockFactory();
            BorderFactory borderFactory = new BorderFactory();

            Prop input = null;
            int index = 0;
            foreach (PropXMLStruct2 prop in inputList)
            {
                if (GameIDList.IsTree(prop.id))
                    input = treeFactory.getProp(content, prop);
                else if (GameIDList.IsRock(prop.id))
                    input = rockFactory.getProp(content, prop);
                else if (GameIDList.IsBorder(prop.id))
                    input = borderFactory.getProp(content, prop);

                WorldLocation loc = new WorldLocation(myWIndex, index);
                input.LocationData = loc;

                if (GameIDList.IsStump(input.ID))
                    m_myManager.RegisterStump(input.LocationData);

                if (GameIDList.IsTree(input.ID))
                {
                    Matrix orig = input.GetBoundingModel().WorldMatrix;
                    input.GetBoundingModel().WorldMatrix = Matrix.CreateScale(new Vector3(.5f, 1f, .5f)) * input.GetBoundingModel().WorldMatrix;
                    input.GetBoundingModel().UpdateBoundingBox();
                    input.GetBoundingModel().WorldMatrix = orig;
                }
                else
                {
                    input.GetBoundingModel().UpdateBoundingBox();
                }

                m_propList.Add(input);

                index++;
            }

            reader.Close();
        }

        /// <summary>Export the prop list to this node's xml file</summary>
        public void SavePropList()
        {
            List<PropXMLStruct2> outputList = new List<PropXMLStruct2>();
            foreach (Prop prop in m_propList)
                outputList.Add(prop.getXMLStruct2());

            string id = m_ID.ToString();
            if (m_ID < 10)
                id = "0" + id;

            XmlSerializer xmlout = new XmlSerializer(outputList.GetType());
            TextWriter writer = new StreamWriter("propList_" + id + ".xml");
            xmlout.Serialize(writer, outputList);

            writer.Close();
        }

        #endregion Initialization



        #region API

        public void DrawLand()
        {
            m_land.Draw();
        }

        public void DrawProps()
        {
            switch(WorldManager.cullSettings.PropCullMode)
            {
                case PropCullMode.CULL_DISTANCE:
                    foreach (Prop prop in m_propList)
                    {
                        if (Vector3.Distance(m_camera.Position, prop.PPosition) < WorldManager.cullSettings.PropCullDistance)
                            prop.Draw();
                        if (Vector3.Distance(m_camera.Position, prop.PPosition) < WorldManager.cullSettings.OBBCullDistance && 
                            WorldManager.cullSettings.OBBCullMode == PropCullMode.CULL_DISTANCE && Game1.drawDevelopment)
                            prop.GetBoundingModel().DrawOBB();
                    }
                    break;

                case PropCullMode.CULL_NONE:
                    foreach (Prop prop in m_propList)
                        prop.Draw();
                    break;
            }
        }

        private void Draw_LOD_OBB(Prop prop)
        {
            if (WorldManager.cullSettings.OBBCullMode == PropCullMode.CULL_NONE)
                return;
            prop.GetBoundingModel().DrawOBB();
        }

        public void Update(GameTime gameTime){ }

        public void ReceiveNewProp(Prop prop)
        {
            m_propList.Add(prop);
        }

        public int GetPropCount()
        {
            return m_propList.Count;
        }

        #endregion API



        #region Mutators

        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }
        public Vector3 Position
        {
            get { return m_position; }
            set 
            { 
                m_position = value;
                m_land.Position = value;
            }
        }
        public Vector3 Rotation
        {
            get { return m_rotation; }
            set { m_rotation = value; }
        }
        public List<Prop> PropList
        {
            get { return m_propList; }
        }
        public GeneratedTerrain Terrain
        {
            get { return m_land; }
        }

        #endregion Mutators
    }
}
