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
        WorldManager        m_myManager;

        Vector3             m_position = Vector3.Zero;
        Vector3             m_rotation = Vector3.Zero;

        GeneratedTerrain    m_land;
        List<Prop>          m_propList;
        

        #endregion Member Variables



        #region Initialization

        public WorldNode(WorldManager manager, int ID) 
        { 
            m_ID = ID;
            m_myManager = manager;
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
            List<PropXMLStruct> inputList = new List<PropXMLStruct>();

            string id = m_ID.ToString();
            if (m_ID < 10)
                id = "0" + id;

            TextReader reader = new StreamReader("propList_" + id + ".xml");
            XmlSerializer xmlIn = new XmlSerializer(inputList.GetType());
            inputList = (List<PropXMLStruct>)xmlIn.Deserialize(reader);

            TreeFactory treeFactory = new TreeFactory();
            RockFactory rockFactory = new RockFactory();

            foreach (PropXMLStruct prop in inputList)
            {
                if (prop.id > GameIDList.Trees_Begin && prop.id < GameIDList.Trees_End)
                    m_propList.Add(treeFactory.getProp(content, prop));
                else if (prop.id > GameIDList.Rocks_Begin && prop.id < GameIDList.Rocks_End)
                    m_propList.Add(rockFactory.getProp(content, prop));
            }

            reader.Close();
        }

        /// <summary>Export the prop list to this node's xml file</summary>
        public void SavePropList()
        {
            List<PropXMLStruct> outputList = new List<PropXMLStruct>();
            foreach (Prop prop in m_propList)
                outputList.Add(prop.getXMLStruct());

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

        public void Draw()
        {
            m_land.Draw();
            foreach (Prop prop in m_propList)
                prop.Draw();
        }

        public void Update(GameTime gameTime){ }

        public void ReceiveNewProp(Prop prop)
        {
            m_propList.Add(prop);
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
