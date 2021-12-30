using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Game.Math_Physics
{
    /// <summary>Contains all the data reguarding a heightmap so the user can perform height
    /// movement dynamically with the heightmap used to generate terrain.</summary>
    public class HeightMapData
    {
        float           m_scale; 
        float[,]        m_heightMatrix;
        Vector3[,]      m_normals;
        Vector3         m_heightMapPosition;  
        float           m_width;       
        float           m_height; 


        /// <summary>Default CTOR, init all member variables</summary>
        /// <param name="heights">the the 2d float array of heights</param>
        /// <param name="terrainScale">the global scale, distance between each height</param>
        public HeightMapData(float[,] heights, Vector3[,] normals, float terrainScale)
        {
            if (heights == null)
                throw new ArgumentNullException("HeightMapData::CTOR param:heights is null");

            if (normals == null)
                throw new ArgumentNullException("HeightMapData::CTOR param:normals is null");

            m_scale = terrainScale;
            m_heightMatrix = heights;
            m_normals = normals;

            m_width = (heights.GetLength(0) - 1) * m_scale;
            m_height = (heights.GetLength(1) - 1) * m_scale;

            m_heightMapPosition.X = -(heights.GetLength(0) - 1) / 2f * m_scale;
            m_heightMapPosition.Z = -(heights.GetLength(1) - 1) / 2f * m_scale;
        }

        /// <summary>Take in the worldspace vector3 position and determine whether or not
        /// it is on the heightMap</summary>
        /// <param name="position">worldspace position of your object</param>
        /// <returns>yay/nay whether the object is on the map</returns>
        public bool IsOnHeightMap(Vector3 position)
        {
            Vector3 posOnMap = position - m_heightMapPosition;

            return (posOnMap.X > 0 &&
                posOnMap.X < m_width &&
                posOnMap.Z > 0 &&
                posOnMap.Z < m_height);
        }

        /// <summary>Takes in a position and returns the height at that point,
        /// uses Lerp to return a smooth position between points
        /// Error when position isn't on the map</summary>
        /// <param name="position">the vector3 worldspace position</param>
        /// <returns>the height of that object at that position</returns>
        public float GetHeight(Vector3 position)
        {
            Vector3 posOnMap = position - m_heightMapPosition;

            if (!IsOnHeightMap(posOnMap))
                return position.Y;

            int left = (int)posOnMap.X / (int)m_scale;
            int top = (int)posOnMap.Z / (int)m_scale;

            //float xNormal = (posOnMap.X % m_scale) / m_scale;
            //float zNormal = (posOnMap.Z % m_scale) / m_scale;

            //float topHeight = MathHelper.Lerp(m_heightMatrix[left, top], m_heightMatrix[left + 1, top], xNormal);
            //float botHeight = MathHelper.Lerp(m_heightMatrix[left, top + 1], m_heightMatrix[left + 1, top + 1], xNormal);

            //return MathHelper.Lerp(topHeight, botHeight, zNormal);

            return MathHelper.Lerp(
                MathHelper.Lerp(m_heightMatrix[left, top], m_heightMatrix[left + 1, top], (posOnMap.X % m_scale) / m_scale),
                MathHelper.Lerp(m_heightMatrix[left, top + 1], m_heightMatrix[left + 1, top + 1], (posOnMap.X % m_scale) / m_scale),
                (posOnMap.Z % m_scale) / m_scale);
        }

        public void GetHeight(Vector3 position, out float height)
        {
            Vector3 positionOnMap = position - m_heightMapPosition;

            int left, top;
            left = (int)positionOnMap.X / (int)m_scale;
            top = (int)positionOnMap.Z / (int)m_scale;

            float xNorm = (positionOnMap.X % m_scale) / m_scale;
            float zNorm = (positionOnMap.Z % m_scale) / m_scale;

            float topHeight = MathHelper.Lerp(
                m_heightMatrix[left, top],
                m_heightMatrix[left + 1, top],
                xNorm);

            float bottomHeight = MathHelper.Lerp(
                m_heightMatrix[left, top + 1],
                m_heightMatrix[left + 1, top + 1],
                xNorm);

            height = MathHelper.Lerp(topHeight, bottomHeight, zNorm);
        }

        #region Mutators

        public float Width
        {
            get { return m_width; }
        }
        public float Height
        {
            get { return m_height; }
        }
        public Vector3 WorldSpaceCorners
        {
            get { return m_heightMapPosition; }
        }
        public float Scale
        {
            get { return m_scale; }
        }

        #endregion Mutators

        
    }

    /// <summary>This class takes care of loading the HeightMapData object during load time</summary>
    public class HeightMapDataReader : ContentTypeReader<HeightMapData>
    {
        protected override HeightMapData Read(ContentReader input, HeightMapData existingInstance)
        {
            float scale = input.ReadSingle();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            float[,] heights = new float[width, height];
            Vector3[,] normals = new Vector3[width, height];

            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    heights[x, z] = input.ReadSingle();

            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    normals[x, z] = input.ReadVector3();
            return new HeightMapData(heights, normals, scale);
        }
    }

   
}
