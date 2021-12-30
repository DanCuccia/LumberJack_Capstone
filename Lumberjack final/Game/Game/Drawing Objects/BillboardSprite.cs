/*
 *  Dan Cuccia
 * 
 *  Animating Sprites sheets in 3D space!
 *  
 *  Utilizes "Math" in the vertex shader to lock the billboard's Y-rotation
 *  To the camera's lookat-vector, effectively locking the billboard to
 *  Permanently face the camera in 3D space.
 *  
 *  The vertices do NOT receive a position! they dynamically get moved inside
 *  the vertex shader according to it's frameSize variables. 
 *  
 *  I combine vector2's into vector4's inside the vertex struct, to help manage
 *  HLSL memory -- 
 *      -Vector4: TexCoordPosUV -- float4( vertPos.X, vertPos.Y, texCoord.U, texCoord.V )
 *      -Vector4: SizeScale -- float4( cell.W, cell.H, scale.W, scale.H )
 * 
 *  Evertime the sprite gets updated, it will release it's verts and start anew.
 *   note:This will be cleaner in the next iteration of this class.
 *   
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Drawing_Objects
{
    /// <summary>
    /// The vertex declaration of a billboard sprite
    /// Conatins all info needed to be passed into the shader
    /// </summary>
    public struct BillboardSpriteVertex : IVertexType
    {
        public Vector3 Position;
        public Vector4 TexCoordPosUV;
        public Vector4 SizeScale;

        public static int SizeInBytes = (3 + 4 + 4) * 4;
        public readonly static VertexDeclaration VertexDeclaration
            = new VertexDeclaration(
                 new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                 new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
                 new VertexElement((sizeof(float) * (3 + 4)), VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
             );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    /// <summary>
    /// This class puts an image up in 3D space, the vertex shader we use in
    /// here will lock Y rotation to always face the camera.
    /// </summary>
    public class BillboardSprite
    {
        GraphicsDeviceManager   m_device;

        Vector3                 m_position;
        Vector2                 m_scale             = new Vector2(1f, 1f);
        Texture2D               m_texture;

        bool                    m_isDirty           = true;
        VertexBuffer            m_vertexBuffer;

        bool                    m_isAnimating       = false;
        int                     m_lastFrameTime     = 0;
        int                     m_milliesPerFrame   = 30;
        Point                   m_frameSize;
        Point                   m_currentFrame      = Point.Zero;
        Point                   m_sheetSize         = new Point(1,1);


        #region Initialization

        /// <summary>Default CTOR -- must call Initialize()</summary>
        /// <param name="ID">base class:OnScreenObject ID</param>
        public BillboardSprite(){ }

        /// <summary>
        /// Init all member variables here
        /// </summary>
        /// <param name="graphics">device for vert buffer</param>
        /// <param name="image">image to display</param>
        /// <returns>yay/nay success</returns>
        public bool Initialize(GraphicsDeviceManager graphics, Texture2D image)
        {
            if (graphics == null)
                throw new ArgumentNullException("BillboardSprite::Initialize: param:graphics is null");
            if(image == null)
                throw new ArgumentNullException("BillboardSprite::Initialize: param:image is null");

            m_device = graphics;
            m_texture = image;
            m_frameSize = new Point(image.Width, image.Height);

            CreateVerts();

            return true;
        }

        public bool InitializeAnimation(Point frameSize, Point sheetSize, int frameSpeed = 30)
        {
            m_isAnimating = true;
            m_frameSize = frameSize;
            m_sheetSize = sheetSize;
            m_milliesPerFrame = frameSpeed;

            CreateVerts();
            return true;
        }

        /// <summary>
        /// privately called after there is a change, during the request of the vertBuffer
        /// </summary>
        private void CycleVerts()
        {
            CreateVerts();
        }

        /// <summary>
        /// This will cycle the vertices, which only needs to be done
        /// </summary>
        private void CreateVerts()
        {
            if (m_vertexBuffer != null)
            {
                m_vertexBuffer.Dispose();
                m_vertexBuffer = null;
            }

            BillboardSpriteVertex[] verts = new BillboardSpriteVertex[6];

            short index = 0;
            verts[index++] = GetVert(m_position, new Vector2(0, 0), m_scale);
            verts[index++] = GetVert(m_position, new Vector2(1, 0), m_scale);
            verts[index++] = GetVert(m_position, new Vector2(1, 1), m_scale);

            verts[index++] = GetVert(m_position, new Vector2(0, 0), m_scale);
            verts[index++] = GetVert(m_position, new Vector2(1, 1), m_scale);
            verts[index++] = GetVert(m_position, new Vector2(0, 1), m_scale);

            m_vertexBuffer = new VertexBuffer(
                m_device.GraphicsDevice,
                typeof(BillboardSpriteVertex),
                6,
                BufferUsage.WriteOnly );
            m_vertexBuffer.SetData(verts);

            m_isDirty = false;
        }

        /// <summary>
        /// Used in Create verts to help clean up code
        /// </summary>
        /// <param name="pos">vert position</param>
        /// <param name="tex">tex coordinate, 0-1, we take care of frame offsets in here</param>
        /// <param name="scale">scale x and y</param>
        /// <returns>a newly created vertex</returns>
        private BillboardSpriteVertex GetVert(Vector3 pos, Vector2 tex, Vector2 scale)
        {
            BillboardSpriteVertex t = new BillboardSpriteVertex();
            t.Position = pos;

            Vector2 texCoord = Vector2.Zero;

            float x, y;
            x = ((tex.X * m_frameSize.X) + (m_currentFrame.X * m_frameSize.X));
            y = ((tex.Y * m_frameSize.Y) + (m_currentFrame.Y * m_frameSize.Y));

            //t.TextureCoordinate = tex;
            t.TexCoordPosUV = new Vector4( 
                tex.X,
                tex.Y,
                MathHelper.Lerp(0, 1, x / m_texture.Width), 
                MathHelper.Lerp(0, 1, y / m_texture.Height) 
                );
            t.SizeScale = new Vector4((float)m_frameSize.X, (float)m_frameSize.Y, scale.X, scale.Y);
            return t;
        }

        #endregion Initialization





        /// <summary>Sprite Sheet Cell animation logic in here</summary>
        /// <param name="gameTime">current game time</param>
        public void Animate(GameTime gameTime)
        {
            if (m_isAnimating)
            {
                m_lastFrameTime += gameTime.ElapsedGameTime.Milliseconds;
                if (m_lastFrameTime > m_milliesPerFrame)
                {
                    m_lastFrameTime -= m_milliesPerFrame;
                    m_currentFrame.X++;
                    if (m_currentFrame.X >= m_sheetSize.X)
                    {
                        m_currentFrame.X = 0;
                        m_currentFrame.Y++;
                        if (m_currentFrame.Y >= m_sheetSize.Y)
                            m_currentFrame.Y = 0;
                    }
                    CreateVerts();
                }
            }
        }


        #region Mutators

        public Texture2D Texture
        {
            get { return m_texture; }
            set { m_texture = value; }
        }
        public Vector2 Scale
        {
            get { return m_scale; }
            set 
            {
                m_scale = value; 
                m_isDirty = true;
            }
        }
        public float ScaleX
        {
            get { return m_scale.X; }
            set 
            { 
                m_scale.X = value;
                m_isDirty = true;
            }
        }
        public float ScaleY
        {
            get { return m_scale.Y; }
            set 
            { 
                m_scale.Y = value;
                m_isDirty = true;
            }
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
        public VertexBuffer VertexBuffer
        {
            get 
            {
                if (m_isDirty)
                {
                    CycleVerts();
                    return m_vertexBuffer;
                }else return m_vertexBuffer;
            }
        }

        #endregion Mutators
    }
}
