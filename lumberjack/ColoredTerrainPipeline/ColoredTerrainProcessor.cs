using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace ColoredTerrainPipeline
{

    struct Vertex
    {
        public Vector3 pos;
        public Vector4 color;
    }

    /// <summary>create a model from a texture2d, that using vertex coloring</summary>
    [ContentProcessor(DisplayName = "ColoredTerrainPipeline.ContentProcessor1")]
    public class ColoredTerrainProcessor : ContentProcessor<Texture2DContent, ModelContent>
    {

        private float terrainScale = 8f;
        [DefaultValue(8f)]
        [DisplayName("Terrain Scale")]
        [Description("Distance between vertices")]
        public float TerrainScale
        {
            get { return terrainScale; }
            set { terrainScale = value; }
        }

        private float heightScale = 64f;
        [DefaultValue(64f)]
        [DisplayName("Terrain Height Scale")]
        [Description("Terrain Geometry Height")]
        public float TerrainBumpiness
        {
            get { return heightScale; }
            set { heightScale = value; }
        }


        public override ModelContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            MeshBuilder builder = MeshBuilder.StartMesh("terrain");

            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));
            PixelBitmapContent<Vector4> heightmap;
            heightmap = (PixelBitmapContent<Vector4>)input.Mipmaps[0];

            Vertex[] verts;
            verts = new Vertex[heightmap.Width * heightmap.Height];

            //calc vert positions
            for (int y = 0; y < heightmap.Height; y++)
                for (int x = 0; x < heightmap.Width; x++)
                {
                    Vector3 pos;

                    pos.X = (x - heightmap.Width / 2) * terrainScale;
                    pos.Z = (y - heightmap.Height / 2) * terrainScale;
                    pos.Y = (heightmap.GetPixel(x, y).W - 1) * heightScale;

                    verts[x + y * heightmap.Width].pos = pos;

                    builder.CreatePosition(pos);
                }

            int colorHandle = builder.CreateVertexChannel<Vector4>(VertexChannelNames.TextureCoordinate(0));

            //finish data assignment
            for (int y = 0; y < heightmap.Height - 1; y++)
                for (int x = 0; x < heightmap.Width - 1; x++)
                {
                    //v1
                    builder.SetVertexChannelData(colorHandle, GetColor(heightmap, x, y));
                    builder.AddTriangleVertex((x) + (y) * heightmap.Width);

                    //v2
                    builder.SetVertexChannelData(colorHandle, GetColor(heightmap, x, y));
                    builder.AddTriangleVertex((x + 1) + (y) * heightmap.Width);

                    //v3
                    builder.SetVertexChannelData(colorHandle, GetColor(heightmap, x, y));
                    builder.AddTriangleVertex((x + 1) + (y + 1) * heightmap.Width);

                    //v4
                    builder.SetVertexChannelData(colorHandle, GetColor(heightmap, x, y));
                    builder.AddTriangleVertex((x) + (y) * heightmap.Width);

                    //v5
                    builder.SetVertexChannelData(colorHandle, GetColor(heightmap, x, y));
                    builder.AddTriangleVertex((x + 1) + (y + 1) * heightmap.Width);

                    //v6
                    builder.SetVertexChannelData(colorHandle, GetColor(heightmap, x, y));
                    builder.AddTriangleVertex((x) + (y + 1) * heightmap.Width);

                }

            MeshContent terrainMesh = builder.FinishMesh();

            ModelContent model = context.Convert<MeshContent, ModelContent>(terrainMesh, "ModelProcessor");

            model.Tag = new ColoredHeightMapData(heightmap, terrainScale, TerrainBumpiness);

            return model;
        }

        private Vector4 GetColor(PixelBitmapContent<Vector4> heights, int x, int y)
        {
            Vector4 color = Vector4.Zero;

            color.X = heights.GetPixel(x, y).X / 255;
            color.Y = heights.GetPixel(x, y).Y / 255;
            color.Z = heights.GetPixel(x, y).Z / 255;
            color.W = 1;

            float total = color.X + color.Y + color.Z;
            color.X /= total;
            color.Y /= total;
            color.Z /= total;

            return color;
        }
    }
}