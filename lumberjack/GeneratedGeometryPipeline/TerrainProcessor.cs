using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace GeneratedGeometryPipeline
{
    struct Vertex
    {
        public Vector3 pos;
        public Vector2 tex;
        public Vector4 weight;
        public Vector4 color;
    }

    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    /// </summary>
    [ContentProcessor]
    public class TerrainProcessor : ContentProcessor<Texture2DContent, ModelContent>
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
        [DisplayName("Terrain Bumpiness")]
        [Description("Terrain Geometry Height")]
        public float TerrainBumpiness
        {
            get { return heightScale; }
            set { heightScale = value; }
        }

        private float texCoordScale = .1f;
        [DefaultValue(.1f)]
        [DisplayName("Terrain Coordinate Scale")]
        [Description("Texture Tiling Density")]
        public float TexCoordScale
        {
            get { return texCoordScale; }
            set { texCoordScale = value; }
        }

        public override ModelContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            MeshBuilder builder = MeshBuilder.StartMesh("terrain");

            input.ConvertBitmapType(typeof(PixelBitmapContent<float>));
            PixelBitmapContent<float> heightmap;
            heightmap = (PixelBitmapContent<float>)input.Mipmaps[0];

            Vertex[] verts;
            verts = new Vertex[heightmap.Width * heightmap.Height];

            //calc vert positions
            for(int y = 0; y < heightmap.Height; y ++)
                for (int x = 0; x < heightmap.Width; x++)
                {
                    Vector3 pos;

                    pos.X = (x - heightmap.Width / 2) * terrainScale;
                    pos.Z = (y - heightmap.Height / 2) * terrainScale;
                    pos.Y = (heightmap.GetPixel(x, y) - 1) * heightScale;

                    verts[x+y*heightmap.Width].pos = pos;

                    builder.CreatePosition(pos);
                }


            //calc vert tex coord & weight
            //index = 0;
            for(int y = 0; y < heightmap.Height; y ++)
                for (int x = 0; x < heightmap.Width; x++)
                {
                    verts[x + y * heightmap.Width].tex = new Vector2(x, y) * texCoordScale;
                    verts[x + y * heightmap.Width].weight = GetTexWeight(heightmap, x, y);
                    //index++;
                }
            
            int texCoord = builder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));
            int texWeight = builder.CreateVertexChannel<Vector4>(VertexChannelNames.TextureCoordinate(1));
            int color = builder.CreateVertexChannel<Vector4>(VertexChannelNames.Color(0));

            //finish vertex data assignments
            //index = 0;
            for ( int y = 0; y < heightmap.Height -1; y++ )
                for ( int x = 0; x < heightmap.Width -1; x++ )
                {

                    //v1
                    builder.SetVertexChannelData(texCoord, verts[(x) + (y) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x) + (y) * heightmap.Width].weight);
                    builder.SetVertexChannelData(color, new Vector4(.070588f, .5215686f, .0588235f, 1));
                    builder.AddTriangleVertex((x) + (y) * heightmap.Width);
                    
                    //v2
                    builder.SetVertexChannelData(texCoord, verts[(x+1) + (y) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x + 1) + (y) * heightmap.Width].weight);
                    builder.SetVertexChannelData(color, new Vector4(.070588f, .5215686f, .0588235f, 1));
                    builder.AddTriangleVertex((x + 1) + (y) * heightmap.Width);
                    
                    //v3
                    builder.SetVertexChannelData(texCoord, verts[(x+1) + (y+1) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x + 1) + (y + 1) * heightmap.Width].weight);
                    builder.SetVertexChannelData(color, new Vector4(.070588f, .5215686f, .0588235f, 1));
                    builder.AddTriangleVertex((x + 1) + (y + 1) * heightmap.Width);
                    
                    //v4
                    builder.SetVertexChannelData(texCoord, verts[(x) + (y) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x) + (y) * heightmap.Width].weight);
                    builder.SetVertexChannelData(color, new Vector4(.070588f, .5215686f, .0588235f, 1));
                    builder.AddTriangleVertex((x) + (y) * heightmap.Width);
                    
                    //v5
                    builder.SetVertexChannelData(texCoord, verts[(x + 1) + (y + 1) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x + 1) + (y + 1) * heightmap.Width].weight);
                    builder.SetVertexChannelData(color, new Vector4(.070588f, .5215686f, .0588235f, 1));
                    builder.AddTriangleVertex((x + 1) + (y + 1) * heightmap.Width);
                    
                    //v6
                    builder.SetVertexChannelData(texCoord, verts[(x) + (y+1) * heightmap.Width].tex);
                    builder.SetVertexChannelData(texWeight, verts[(x) + (y + 1) * heightmap.Width].weight);
                    builder.SetVertexChannelData(color, new Vector4(.070588f, .5215686f, .0588235f, 1));
                    builder.AddTriangleVertex((x) + (y + 1) * heightmap.Width);

                }

            MeshContent terrainMesh = builder.FinishMesh();

            ModelContent model =  context.Convert<MeshContent, ModelContent>(terrainMesh, "ModelProcessor");

            model.Tag = new HeightMapInfoContent(heightmap, terrainScale, TerrainBumpiness);

            return model;

        }

        void AddVert(MeshBuilder builder, PixelBitmapContent<float> heights, 
            int texCoord, int texWeight, int w, int x, int y)
        {
            Vector2 tCoord = new Vector2(x, y) * texCoordScale;
            builder.SetVertexChannelData(texCoord, tCoord);

            Vector4 weight = GetTexWeight(heights, x, y);
            builder.SetVertexChannelData(texWeight, weight);

            builder.AddTriangleVertex(x + y * w);
        }


        Vector4 GetTexWeight(PixelBitmapContent<float> heights, int x, int y)
        {
            Vector4 weight = Vector4.Zero;

            #region orig
            //weight.X = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * terrainBumps / 3 - 0) / 8f, 0, 1);
            //weight.Y = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * terrainBumps / 3 - 12) / 6f, 0, 1);
            //weight.Z = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * terrainBumps / 3 - 20) / 6f, 0, 1);
            //weight.W = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * terrainBumps / 3 - 30) / 6f, 0, 1);
            #endregion orig

            weight.X = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * heightScale - 0) / (heightScale / 4), 0, 1);
            weight.Y = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * heightScale - (heightScale / 4)) / (heightScale / 4), 0, 1);
            weight.Z = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * heightScale - (heightScale / 4 * 2)) / (heightScale / 4), 0, 1);
            weight.W = MathHelper.Clamp(1f - Math.Abs(heights.GetPixel(x, y) * heightScale - (heightScale / 4 * 3) - 10) / (heightScale / 4), 0, 1);

            float total = weight.X + weight.Y + weight.Z + weight.W;
            weight.X /= total;
            weight.Y /= total;
            weight.Z /= total;
            weight.W /= total;

            return weight;
        }

        Vector3 GetTriangleNormal(Vector3 vert1, Vector3 vert2, Vector3 vert3)
        {
            Vector3 v1 = vert2 - vert1;
            Vector3 v2 = vert3 - vert1;

            Vector3 normal = Vector3.Cross(v1, v2);
            normal = Vector3.Normalize(normal);

            return normal;
        }


    }
}
