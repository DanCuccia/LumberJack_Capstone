using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;

namespace ControlledTerrainPipeline
{

    public class ControlledHeightMapData
    {
    
        /// <summary>2D array of floats that holds height of each position</summary>
        float[,] heights;
        public float[,] Heights
        {
            get { return heights; }
        }

        /// <summary>2D array of vectory3 triangle normals</summary>
        Vector3[,] normals;
        public Vector3[,] Normals
        {
            get { return normals; }
            set { normals = value; }
        }


        /// <summary>Distance between each array index</summary>
        private float terrainScale;
        public float TerrainScale
        {
            get { return terrainScale; }
        }


        /// <summary>Default CTOR, init the height array from bitmap info</summary>
        /// <param name="bitmap">the bitmap heightmap</param>
        /// <param name="terrainScale">the scale of the terrain</param>
        /// <param name="terrainBumpiness">bumpiness of the terrain</param>
        public ControlledHeightMapData(MeshContent terrainMesh, PixelBitmapContent<Vector4> bitmap,
            float terrainScale, int terrainWidth, int terrainHeight, float terrainYscale)
        {

            if (terrainMesh == null)
                throw new ArgumentNullException("ControlledHeightMapData::CTOR terrainMesh is null");

            if (terrainWidth <= 0 || terrainHeight <= 0)
                throw new ArgumentNullException("ControlledHeightMapData::CTOR width & height are <= 0");

            this.terrainScale = terrainScale;
            heights = new float[terrainWidth, terrainHeight];
            normals = new Vector3[terrainWidth, terrainHeight];

            GeometryContent geometry = terrainMesh.Geometry[0];

            for (int i = 0; i < geometry.Vertices.VertexCount; i++)
            {
                Vector3 pos = geometry.Vertices.Positions[i];
                Vector3 norm = (Vector3)geometry.Vertices.Channels[VertexChannelNames.Normal()][i];

                int arrayX = (int)((pos.X / terrainScale) + (terrainWidth - 1) / 2f);
                int arrayY = (int)((pos.Y / terrainScale) + (terrainHeight - 1) / 2f);

                heights[arrayX, arrayY] = pos.Y;
                normals[arrayX, arrayY] = norm;
            }

            for (int y = 0; y < terrainHeight; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    heights[x, y] = (bitmap.GetPixel(x, y).W - 1) * terrainYscale;
                }
            }
        }

        /// <summary> This tells the content pipeline how to save the data in heightMapInfo
        /// this must match the reader</summary>
        [ContentTypeWriter]
        public class HeightMapInfoWriter : ContentTypeWriter<ControlledHeightMapData>
        {
            protected override void Write(ContentWriter output, ControlledHeightMapData value)
            {
                output.Write(value.TerrainScale);
                output.Write(value.Heights.GetLength(0));
                output.Write(value.Heights.GetLength(1));

                foreach (float height in value.Heights)
                {
                    output.Write(height);
                }
                foreach (Vector3 normal in value.Normals)
                {
                    output.Write(normal);
                }
            }

            /// <summary>Tells the content pipeline what CLR type the data will be loaded into</summary>
            public override string GetRuntimeType(TargetPlatform targetPlatform)
            {
                return "Game.Math_Physics.HeightMapData, Game, Version=1.0.0.0, Culture=neutral";
            }

            /// <summary>Tells the content pipeline what worker type will be used to load the data</summary>
            public override string GetRuntimeReader(TargetPlatform targetPlatform)
            {
                return "Game.Math_Physics.HeightMapDataReader, Game, Version=1.0.0.0, Culture=neutral";
            }
        }
    }
}
