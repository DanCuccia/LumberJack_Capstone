using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Drawing_Objects
{
    /// <summary>
    /// position & color vertex for standard geometry
    /// </summary>
    public struct GeometryVertex : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;

        public static int SizeInBytes = (3 + 4) * 4;
        public readonly static VertexDeclaration VertexDeclaration
            = new VertexDeclaration(
                 new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                 new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
             );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }


    /// <summary>
    /// A basic Cube to draw in 3D, with some functionality, This
    /// is designed just to help debugging purposes
    /// 
    /// Note: The best way to construct is this process..
    ///     1 . Cube cube = new Cube();
    ///     2 . cube.changeParameters();
    ///     3 . cube.Initialize();
    /// </summary>
    public class Cube
    {

        #region Member Variables

        GraphicsDeviceManager m_device;

        Vector3         m_centerPosition    = Vector3.Zero;
        Color           m_color             = Color.Red;
        float           m_opacity           = 1f;
        Vector3         m_diameters         = new Vector3(10f, 10f, 10f);

        VertexBuffer    m_vertexBuffer;

        #endregion Member Variables


        #region Initialization

        /// <summary> Default CTOR -- must call Initialize</summary>
        /// <param name="ID">base class:OnsScreenObject ID</param>
        public Cube() { }

        /// <summary>Initialize all parameters in here </summary>
        /// <param name="device">device used to make vertex buffer</param>
        /// <param name="diameters">sizes of this cube</param>
        /// <returns>yay/nay success</returns>
        public bool Initialize(GraphicsDeviceManager device, Vector3 diameters)
        {
            if (device == null)
                throw new ArgumentNullException("Cube::Initialize: param=device is null");
            m_device = device;
            m_diameters = diameters;

            CycleVerts();

            return true;
        }

        private void CycleVerts()
        {
            if(m_vertexBuffer != null)
            {
                m_vertexBuffer.Dispose();
                m_vertexBuffer = null;
            }
            GeometryVertex[] verts = new GeometryVertex[36];

            int index = 0;
            //front face
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2),  (m_diameters.Y/2), (m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2), (m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2), -(m_diameters.Y/2), (m_diameters.Z/2) ));

            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2), (m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2), -(m_diameters.Y/2), (m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2), (m_diameters.Z/2) ));

            //back face
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2),  (m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));

            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2), -(m_diameters.Z/2) ));

            //top face
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2),  (m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2),  (m_diameters.Z/2) ));

            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2),  (m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2),  (m_diameters.Y/2),  (m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2),  (m_diameters.Z/2) ));

            //bottom face
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2),  (m_diameters.Z/2) ));

            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X/2), -(m_diameters.Y/2),  (m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2),  (m_diameters.Z/2) ));

            //right face
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2),  (m_diameters.Z/2) ));

            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2), -(m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2), -(m_diameters.Y/2),  (m_diameters.Z/2) ));
            verts[index++] = GetVert(new Vector3( (m_diameters.X/2),  (m_diameters.Y/2),  (m_diameters.Z/2) ));

            //left face
            verts[index++] = GetVert(new Vector3(-(m_diameters.X / 2), -(m_diameters.Y / 2), -(m_diameters.Z / 2)));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X / 2),  (m_diameters.Y / 2), -(m_diameters.Z / 2)));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X / 2),  (m_diameters.Y / 2),  (m_diameters.Z / 2)));

            verts[index++] = GetVert(new Vector3(-(m_diameters.X / 2), -(m_diameters.Y / 2), -(m_diameters.Z / 2)));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X / 2), -(m_diameters.Y / 2), (m_diameters.Z / 2)));
            verts[index++] = GetVert(new Vector3(-(m_diameters.X / 2),  (m_diameters.Y / 2), (m_diameters.Z / 2)));

            m_vertexBuffer = new VertexBuffer(
                m_device.GraphicsDevice, 
                typeof(GeometryVertex), 
                36, 
                BufferUsage.WriteOnly );
            m_vertexBuffer.SetData(verts);

        }

        /// <summary>
        /// privately used in initialize, to help clean up code
        /// </summary>
        /// <param name="pos">vertex position</param>
        /// <returns>a newly created vertex</returns>
        private GeometryVertex GetVert(Vector3 pos)
        {
            GeometryVertex t = new GeometryVertex();
            t.Position = pos;
            t.Color = m_color.ToVector4();
            return t;
        }

        #endregion Initialization


        #region Mutators

        public Vector3 CenterPosition
        {
            get { return m_centerPosition; }
            set { m_centerPosition = value; }
        }
        public float CenterPositionX
        {
            get { return m_centerPosition.X; }
            set { m_centerPosition.X = value; }
        }
        public float CenterPositionY
        {
            get { return m_centerPosition.Y; }
            set { m_centerPosition.Y = value; }
        }
        public float CenterPositionZ
        {
            get { return m_centerPosition.Z; }
            set { m_centerPosition.Z = value; }
        }
        public Color Color
        {
            get { return m_color; }
            set { m_color = value; }
        }
        public float Opacity
        {
            get { return m_opacity; }
            set { m_opacity = value; }
        }
        public Vector3 Diameters
        {
            get { return m_diameters; }
            set { m_diameters = value; }
        }
        public VertexBuffer VertexBuffer
        {
            get { return m_vertexBuffer; }
        }

        #endregion Mutators
    }
}
