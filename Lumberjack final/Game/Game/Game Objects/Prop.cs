using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Game.Drawing_Objects;

namespace Game.Game_Objects
{

    /// <summary>This is what gets serialized for each prop in the world</summary>
    [Serializable]
    public class PropXMLStruct2
    {
        public int id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    /// <summary>All indices needed to find a prop within the entire world</summary>
    public class WorldLocation
    {
        public Point worldIndicies;
        public int listIndex;

        public WorldLocation() { }
        public WorldLocation(Point windex, int lindex)
        {
            this.worldIndicies = windex;
            this.listIndex = lindex;
        }
    }

    /// <summary>The Base abstract prop class, all uncontrollable world items inherit from this</summary>
    public abstract class Prop
    {
        private int     id;
        private Vector3 m_position;
        private Vector3 m_rotation;
        private Vector3 m_scale = new Vector3(1f);

        private WorldLocation m_locationData = new WorldLocation();

        public abstract void Draw();
        public abstract StaticMesh GetBoundingModel();

        public PropXMLStruct2 getXMLStruct2()
        {
            PropXMLStruct2 prop = new PropXMLStruct2();
            prop.id = this.id;
            prop.position = this.m_position;
            prop.rotation = this.m_rotation;
            prop.scale = this.m_scale;
            return prop;
        }


        #region Mutators
        public int ID
        {
            set { this.id = value; }
            get { return this.id; }
        }
        public Vector3 PPosition
        {
            set { m_position = value; }
            get { return m_position; }
        }
        public float PPositionX
        {
            set { m_position.X = value; }
            get { return m_position.X; }
        }
        public float PPositionY
        {
            set { m_position.Y = value; }
            get { return m_position.Y; }
        }
        public float PPositionZ
        {
            set { m_position.Z = value; }
            get { return m_position.Z; }
        }
        public Vector3 PRotation
        {
            set { m_rotation = value; }
            get { return m_rotation; }
        }
        public float PRotationY
        {
            set { m_rotation.Y = value; }
            get { return m_rotation.Y; }
        }
        public float PRotationX
        {
            set { m_rotation.X = value; }
            get { return m_rotation.X; }
        }
        public float PRotationZ
        {
            set { m_rotation.Z = value; }
            get { return m_rotation.Z; }
        }
        public Vector3 PScale
        {
            set { m_scale = value; }
            get { return m_scale; }
        }
        public float PScaleX
        {
            set { m_scale.X = value; }
            get { return m_scale.X; }
        }
        public float PScaleY
        {
            set { m_scale.Y = value; }
            get { return m_scale.Y; }
        }
        public float PScaleZ
        {
            set { m_scale.Z = value; }
            get { return m_scale.Z; }
        }
        public WorldLocation LocationData
        {
            set { this.m_locationData = value; }
            get { return this.m_locationData; }
        }
        #endregion Mutators
    }
}
