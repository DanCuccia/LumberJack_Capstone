using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Drawing_Objects
{
    /// <summary> Vertex of the water mesh</summary>
    public struct WaterVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 BiNormal;

        public static int SizeInBytes = (3 + 3 + 2 + 3 + 3) * 4;
        public readonly static  VertexDeclaration VertexDeclaration 
            = new VertexDeclaration(
                 new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
                 new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
                 new VertexElement( sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
                 new VertexElement( sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0 ),
                 new VertexElement( sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0 )
             );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    /// <summary>A wrapper to contain all possilble water settings</summary>
    public class WaterSettings
    {
        public float        BumpHeight          = .5f;
        public Vector2      TextureScale        = new Vector2(4f, 4f);
        public Vector2      BumpSpeed           = new Vector2(0f, .0f);
        public float        FresnelBias         = .025f;
        public float        FresnelPower        = 1f;
        public float        HDRMultiplier       = 1f;
        public Vector4      DeepColor           = new Vector4(0f, .4f, .5f, 1f);
        public Vector4      ShallowColor        = new Vector4(.55f, .75f, .75f, 1f);
        public Vector4      ReflectionColor     = new Vector4(1f, 1f, 1f, 1f);
        public float        ReflectionAmount    = .5f;
        public float        WaterAmount         = .5f;
    }

    /// <summary>Intermediary class between XML and run-time</summary>
    public class WaterXMLStruct
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector2 bumpSpeed;
        public Vector4 deepColor;
        public Vector4 shallowColor;
        public Vector2 textureScale;
        public float vertexDistanceX;
        public float vertexdistanceZ;

        public WaterXMLStruct() { }
    }
    
    /// <summary>The main user-end water class</summary>
    public class WaterVolume
    {

        #region Member Variables

        GraphicsDevice          m_device;   //ref

        WaterVertex[]           m_verts;

        int                     m_width; 
        int                     m_height;
        float                   m_vertDistX;
        float                   m_vertDistZ;

        Vector3                 m_position = Vector3.Zero;
        Vector3                 m_rotation = Vector3.Zero;
        float                   m_scale = 1f;
        bool                    m_isDirty = true;
        Matrix                  m_modelMatrix;

        VertexBuffer            m_vertexBuffer;
        IndexBuffer             m_indexBuffer;

        WaterSettings           m_parameters;

        Effect                  m_effect;

        #endregion Member Variables


        #region Initialization

        /// <summary>Default CTOR</summary>
        /// <param name="ID">base class ID</param>
        public WaterVolume(GraphicsDevice graphics)
        {
            m_device = graphics;
            m_parameters = new WaterSettings();
        }

        /// <summary>all Initialization in here -- must call CenterWithWorld() to complete m_scale & m_position</summary>
        /// <param name="content">Load wave normal maps</param>
        /// <param name="height">how many vertices long</param>
        /// <param name="width">how many vertices wide</param>
        /// <param name="vertDistance">The world unit distance between vertices</param>
        public void Initialize(ContentManager content, float vertDistanceX, float vertDistanceZ) 
        {
            m_width = 2;
            m_height = 2;
            m_vertDistX = vertDistanceX;
            m_vertDistZ = vertDistanceZ;

            Renderer render = Renderer.getInstance();
            m_effect = render.EffectManager.GetEffect(RenderEffect.RFX_TOON);

            LoadMesh();

            SetFXParameters();

            UpdateMatrix();
        }

        public void ChangeVertDistance()
        {
            LoadMesh();
            UpdateMatrix();
        }

        

        /// <summary>Loads the mesh data, builds the vertex/index buffers</summary>
        private void LoadMesh()
        {
            m_verts = new WaterVertex[m_width * m_height];

            CreateVerts();
            CreateTangBiNorm();

            m_vertexBuffer = new VertexBuffer(
                m_device,
                typeof(WaterVertex), 
                m_width * m_height, 
                BufferUsage.WriteOnly );
            m_vertexBuffer.SetData(m_verts);

            CreateIndices();
        }

        private void SetFXParameters()
        {
            m_effect.Parameters["BumpHeight"].SetValue(m_parameters.BumpHeight);
            m_effect.Parameters["TextureScale"].SetValue(m_parameters.TextureScale);
            m_effect.Parameters["BumpSpeed"].SetValue(m_parameters.BumpSpeed);
            m_effect.Parameters["FresnelBias"].SetValue(m_parameters.FresnelBias);
            m_effect.Parameters["FresnelPower"].SetValue(m_parameters.FresnelPower);
            m_effect.Parameters["HDRMultiplier"].SetValue(m_parameters.HDRMultiplier);
            m_effect.Parameters["DeepColor"].SetValue(m_parameters.DeepColor);
            m_effect.Parameters["ShallowColor"].SetValue(m_parameters.ShallowColor);
            m_effect.Parameters["ReflectionColor"].SetValue(m_parameters.ReflectionColor);
            m_effect.Parameters["ReflectionAmount"].SetValue(m_parameters.ReflectionAmount);
            m_effect.Parameters["WaterAmount"].SetValue(m_parameters.WaterAmount);
        }

        /// <summary>sets up the vertecis</summary>
        private void CreateVerts()
        {
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    float aX = (x - m_width/2) * m_vertDistX;
                    float aY = (y - m_height/2) * m_vertDistZ;
                    m_verts[x + y * m_width].Position = new Vector3(aX, 0, aY);
                    m_verts[x + y * m_width].Normal = new Vector3(0, 1, 0);
                    m_verts[x + y * m_width].TextureCoordinate.X = (float)x / 30f;
                    m_verts[x + y * m_width].TextureCoordinate.Y = (float)y / 30f;
                }
            }
        }

        /// <summary>Sets up tangents and bi-normals</summary>
        private void CreateTangBiNorm()
        {
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
                {
                    //Tangents
                    if (x != 0 && x < m_width - 1)
                        m_verts[x + y * m_width].Tangent = m_verts[x - 1 + y * m_width].Position - m_verts[x + 1 + y * m_width].Position;
                    else
                        if (x == 0)
                            m_verts[x + y * m_width].Tangent = m_verts[x + y * m_width].Position - m_verts[x + 1 + y * m_width].Position;
                        else
                            m_verts[x + y * m_width].Tangent = m_verts[x - 1 + y * m_width].Position - m_verts[x + y * m_width].Position;

                    //Bi Normals
                    if (y != 0 && y < m_height - 1)
                        m_verts[x + y * m_width].BiNormal = m_verts[x + (y - 1) * m_width].Position - m_verts[x + (y + 1) * m_width].Position;
                    else
                        if (y == 0)
                            m_verts[x + y * m_width].BiNormal = m_verts[x + y * m_width].Position - m_verts[x + (y + 1) * m_width].Position;
                        else
                            m_verts[x + y * m_width].BiNormal = m_verts[x + (y - 1) * m_width].Position - m_verts[x + y * m_width].Position;
                }
            }
        }

        /// <summary>Sets up the Triangle list</summary>
        private void CreateIndices()
        {
            int[] indicies = new int[(m_width - 1) * (m_height - 1) *6];
            //for (int x = 0; x < m_width - 1; x++)
            //{
            //    for (int y = 0; y < m_height - 1; y++)
            //    {
            //        indicies[(x + y * (m_width - 1)) * 6] = ((x + 1) + (y + 1) * m_width);
            //        indicies[(x + y * (m_width - 1)) * 6 + 1] = ((x + 1) + y * m_width);
            //        indicies[(x + y * (m_width - 1)) * 6 + 2] = (x + y * m_width);

            //        indicies[(x + y * (m_width - 1)) * 6 + 3] = ((x + 1) + (y + 1) * m_width);
            //        indicies[(x + y * (m_width - 1)) * 6 + 4] = (x + y * m_width);
            //        indicies[(x + y * (m_width - 1)) * 6 + 5] = (x + (y + 1) * m_width);
            //    }
            //}
            for (int x = 0; x < m_width - 1; x++)
            {
                for (int y = 0; y < m_height - 1; y++)
                {
                    indicies[(x + y * (m_width - 1)) * 6]     = ((x) + (y+1) * m_width);
                    indicies[(x + y * (m_width - 1)) * 6 + 1] = ((x+1) + (y+1) * m_width);
                    indicies[(x + y * (m_width - 1)) * 6 + 2] = ((x+1) + (y) * m_width);

                    indicies[(x + y * (m_width - 1)) * 6 + 3] = ((x) + (y+1) * m_width);
                    indicies[(x + y * (m_width - 1)) * 6 + 4] = ((x+1) + (y) * m_width);
                    indicies[(x + y * (m_width - 1)) * 6 + 5] = ((x) + (y) * m_width);
                }
            }

            m_indexBuffer = new IndexBuffer(
                m_device,
                typeof(int),
                (m_width - 1) * (m_height - 1) * 6,
                BufferUsage.WriteOnly );
            m_indexBuffer.SetData(indicies);
        }

        #endregion Initialization

        /// <summary> Main Draw Interface</summary>
        public void Draw()
        {
            m_effect.Parameters["World"].SetValue(this.WorldMatrix);
            SetFXParameters();

            if (Renderer.m_renderPhase == RenderPhase.PHASE_DEPTH)
            {
                m_effect.CurrentTechnique.Passes[0].Apply();
                FeedMesh();
                return;
            }

            m_effect.CurrentTechnique = m_effect.Techniques["WaterToon"];
            m_effect.CurrentTechnique.Passes[0].Apply();
            FeedMesh();
        }

        /// <summary>Used Privately in Draw()</summary>
        private void FeedMesh()
        {
            m_device.SetVertexBuffer(m_vertexBuffer);
            m_device.Indices = m_indexBuffer;
            m_device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, 0, 0, m_width * m_height, 0,
                (m_width - 1) * (m_height - 1) * 2);
        }

        /// <summary>If the matrix is dirty and called for, it'll be updated first</summary>
        private void UpdateMatrix()
        {
            m_modelMatrix = Matrix.CreateScale(m_scale) *
                Matrix.CreateRotationX(MathHelper.ToRadians(180 + m_rotation.X) ) *  //HACK todo: fix winding order?
                Matrix.CreateRotationY(MathHelper.ToRadians(m_rotation.Y)) * 
                Matrix.CreateRotationZ(MathHelper.ToRadians(m_rotation.Z)) *
                Matrix.CreateTranslation(m_position);
            m_isDirty = false;
        }

        /// <summary>Retrieve a slim version of the water parameters used for xml IO</summary>
        public WaterXMLStruct GetXmlStruct()
        {
            WaterXMLStruct output = new WaterXMLStruct();

            output.position = this.m_position;
            output.rotation = this.m_rotation;
            output.bumpSpeed = this.Parameters.BumpSpeed;
            output.deepColor = this.Parameters.DeepColor;
            output.shallowColor = this.Parameters.ShallowColor;
            output.textureScale = this.Parameters.TextureScale;
            output.vertexDistanceX = this.m_vertDistX;
            output.vertexdistanceZ = this.m_vertDistZ;

            return output;
        }


        #region Mutators

        public VertexBuffer Vertices
        {
            get { return m_vertexBuffer; }
        }
        public IndexBuffer Indices
        {
            get { return m_indexBuffer; }
        }
        public int Width
        {
            get { return m_width; }
        }
        public int Height
        {
            get { return m_height; }
        }
        public WaterSettings Parameters
        {
            get { return m_parameters; }
        }
        public Vector3 Position
        {
            get { return m_position; }
            set 
            { 
                m_position = value;
                m_isDirty = true;
            }
        }
        public float PositionX
        {
            get { return m_position.X; }
            set
            {
                m_position.X = value;
                m_isDirty = true;
            }
        }
        public float PositionY
        {
            get { return m_position.Y; }
            set
            {
                m_position.Y = value;
                m_isDirty = true;
            }
        }
        public float PositionZ
        {
            get { return m_position.Z; }
            set
            {
                m_position.Z = value;
                m_isDirty = true;
            }
        }
        public Vector3 Rotation
        {
            get { return m_rotation; }
            set
            {
                m_rotation = value;
                m_isDirty = true;
            }
        }
        public float RotationX
        {
            get { return m_rotation.X; }
            set
            {
                m_rotation.X = value;
                m_isDirty = true;
            }
        }
        public float RotationY
        {
            get { return m_rotation.Y; }
            set
            {
                m_rotation.Y = value;
                m_isDirty = true;
            }
        }
        public float RotationZ
        {
            get { return m_rotation.Z; }
            set
            {
                m_rotation.Z = value;
                m_isDirty = true;
            }
        }
        public float Scale
        {
            get { return m_scale; }
            set
            {
                m_scale = value;
                m_isDirty = true;
            }
        }
        public Matrix WorldMatrix
        {
            get
            {
                if (m_isDirty)
                {
                    UpdateMatrix();
                    return m_modelMatrix;
                }
                else
                    return m_modelMatrix;
            }
        }
        public float VertexDistanceX
        {
            get { return m_vertDistX; }
            set
            {
                m_vertDistX = value;
                ChangeVertDistance();
            }
        }
        public float VertexDistanceZ
        {
            get { return m_vertDistZ; }
            set
            {
                m_vertDistZ = value;
                ChangeVertDistance();
            }
        }


        #endregion Mutators
    }

}
