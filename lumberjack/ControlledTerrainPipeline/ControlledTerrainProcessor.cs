using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace ControlledTerrainPipeline
{
    struct Vertex
    {
        public Vector3 pos;
        public Vector2 tex;
        public Vector3 weight;
    }

    /// <summary>Create a model from a texture2d, using the controlled Texture process
    /// R: path texture
    /// G: grass texture
    /// B: rock texture
    /// A: vertex height value
    /// </summary>
    [ContentProcessor(DisplayName = "ControlledTerrainPipeline.ContentProcessor1")]
    public class ControlledTerrainProcessor : ContentProcessor<Texture2DContent, ModelContent>
    {
        private float terrainScale = 8f;
        [DefaultValue(8f)]
        [DisplayName("X,Z Scale")]
        [Description("Distance between vertices")]
        public float TerrainScale
        {
            get { return terrainScale; }
            set { terrainScale = value; }
        }

        private float heightScale = 64f;
        [DefaultValue(64f)]
        [DisplayName("Y Scale")]
        [Description("Terrain Geometry Height")]
        public float TerrainBumpiness
        {
            get { return heightScale; }
            set { heightScale = value; }
        }

        private float texCoordScale = .1f;
        [DefaultValue(.1f)]
        [DisplayName("TexCoord Scale")]
        [Description("Texture Tiling Density")]
        public float TexCoordScale
        {
            get { return texCoordScale; }
            set { texCoordScale = value; }
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

                    pos.X = (x - (heightmap.Width-1) / 2) * terrainScale;
                    pos.Z = (y - (heightmap.Height-1) / 2) * terrainScale;
                    pos.Y = (heightmap.GetPixel(x, y).W - 1) * heightScale;

                    verts[x + y * heightmap.Width].pos = pos;

                    builder.CreatePosition(pos);
                }


            //calc vert tex coord & weight
            for (int y = 0; y < heightmap.Height; y++)
                for (int x = 0; x < heightmap.Width; x++)
                {
                    verts[x + y * heightmap.Width].tex = new Vector2(x, y) * texCoordScale;
                    verts[x + y * heightmap.Width].weight = GetTexWeight(heightmap, x, y);
                }

            int texCoord = builder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));
            int texWeight = builder.CreateVertexChannel<Vector3>(VertexChannelNames.TextureCoordinate(1));

            //finish data assignment
            for (int y = 0; y < heightmap.Height - 1; y++)
                for (int x = 0; x < heightmap.Width - 1; x++)
                {
                    //v1
                    builder.SetVertexChannelData(texCoord, verts[(x) + (y) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x) + (y) * heightmap.Width].weight);
                    builder.AddTriangleVertex((x) + (y) * heightmap.Width);

                    //v2
                    builder.SetVertexChannelData(texCoord, verts[(x + 1) + (y) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x + 1) + (y) * heightmap.Width].weight);
                    builder.AddTriangleVertex((x + 1) + (y) * heightmap.Width);

                    //v3
                    builder.SetVertexChannelData(texCoord, verts[(x + 1) + (y + 1) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x + 1) + (y + 1) * heightmap.Width].weight);
                    builder.AddTriangleVertex((x + 1) + (y + 1) * heightmap.Width);

                    //v4
                    builder.SetVertexChannelData(texCoord, verts[(x) + (y) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x) + (y) * heightmap.Width].weight);
                    builder.AddTriangleVertex((x) + (y) * heightmap.Width);

                    //v5
                    builder.SetVertexChannelData(texCoord, verts[(x + 1) + (y + 1) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x + 1) + (y + 1) * heightmap.Width].weight);
                    builder.AddTriangleVertex((x + 1) + (y + 1) * heightmap.Width);

                    //v6
                    builder.SetVertexChannelData(texCoord, verts[(x) + (y + 1) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x) + (y + 1) * heightmap.Width].weight);
                    builder.AddTriangleVertex((x) + (y + 1) * heightmap.Width);

                }

            MeshContent terrainMesh = builder.FinishMesh();

            ModelContent model = context.Convert<MeshContent, ModelContent>(terrainMesh, "ModelProcessor");

            model.Tag = new ControlledHeightMapData(terrainMesh, 
                heightmap, 
                terrainScale, 
                heightmap.Width, 
                heightmap.Height, 
                heightScale);

            return model;


        }




        #region privates

        private Vector3 GetTexWeight(PixelBitmapContent<Vector4> heights, int x, int y)
        {
            Vector3 weight = Vector3.Zero;

            weight.X = heights.GetPixel(x, y).X / 255;
            weight.Y = heights.GetPixel(x, y).Y / 255;
            weight.Z = heights.GetPixel(x, y).Z / 255;

            float total = weight.X + weight.Y + weight.Z;
            weight.X /= total;
            weight.Y /= total;
            weight.Z /= total;

            return weight;
        }

        #endregion privates
    }
}