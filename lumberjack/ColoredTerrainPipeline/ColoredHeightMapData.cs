using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;

namespace ColoredTerrainPipeline
{
    class ColoredHeightMapData
    {
    
        /// <summary>
        /// 2D array of floats that holds height of each position
        /// </summary>
        float[,] height;
        public float[,] Height
        {
            get { return height; }
        }


        /// <summary>
        /// Distance between each array index
        /// </summary>
        private float terrainScale;
        public float TerrainScale
        {
            get { return terrainScale; }
        }


        int dimensionSize;
        public int DimensionSize
        {
            get { return dimensionSize; }
        }


        /// <summary>
        /// Default CTOR, init the height array from bitmap info
        /// </summary>
        /// <param name="bitmap">the bitmap heightmap</param>
        /// <param name="terrainScale">the scale of the terrain</param>
        /// <param name="terrainBumpiness">bumpiness of the terrain</param>
        public ColoredHeightMapData(PixelBitmapContent<Vector4> bitmap,
            float terrainScale, float terrainBumpiness)
        {
            this.terrainScale = terrainScale;
            height = new float[bitmap.Width, bitmap.Height];
            dimensionSize = bitmap.Width;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    height[x, y] = (bitmap.GetPixel(x, y).W - 1) * terrainBumpiness;
                }
            }
        }

        /// <summary>
        /// This tells the content pipeline how to save the data in heightMapInfo
        /// this must match the reader
        /// </summary>
        [ContentTypeWriter]
        public class HeightMapInfoWriter : ContentTypeWriter<ColoredHeightMapData>
        {
            protected override void Write(ContentWriter output, ColoredHeightMapData value)
            {
                output.Write(value.TerrainScale);
                output.Write(value.Height.GetLength(0));
                output.Write(value.Height.GetLength(1));

                foreach (float height in value.Height)
                {
                    output.Write(height);
                }
            }

            /// <summary>
            /// Tells the content pipeline what CLR type the data will be loaded into
            /// </summary>
            public override string GetRuntimeType(TargetPlatform targetPlatform)
            {
                return "Game.HeightMapData, Game, Version=1.0.0.0, Culture=neutral";
            }

            /// <summary>
            /// Tells the content pipeline what worker type will be used to load the data
            /// </summary>
            public override string GetRuntimeReader(TargetPlatform targetPlatform)
            {
                return "Game.HeightMapDataReader, Game, Version=1.0.0.0, Culture=neutral";
            }
        }
    }
}
