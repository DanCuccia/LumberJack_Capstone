using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Game.Game_Objects
{
    /// <summary>This is an intermediary class between xml and run-time</summary>
    [Serializable]
    public class PropXMLStruct
    {
        public int id;
        public Vector3 position;
        public Vector3 rotation;
        public float scale;
    }

    /// <summary>The Base abstract prop class, all uncontrollable world items inherit from this</summary>
    abstract class Prop
    {

        private int     m_ID;
        private Vector3 m_position;
        private Vector3 m_rotation;
        private float   m_scale = 1f;


        public abstract void Draw();

        public PropXMLStruct getXMLStruct() 
        {
            PropXMLStruct prop = new PropXMLStruct();
            prop.id = this.m_ID;
            prop.position = this.m_position;
            prop.rotation = this.m_rotation;
            prop.scale = this.m_scale;
            return prop;
        }


        #region Mutators
        public int ID
        {
            set { m_ID = value; }
            get { return m_ID; }
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
        public float PScale
        {
            set { m_scale = value; }
            get { return m_scale; }
        }
        #endregion Mutators
    }
}
