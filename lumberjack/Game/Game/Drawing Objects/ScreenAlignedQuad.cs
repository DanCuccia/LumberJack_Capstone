using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Drawing_Objects
{
    public class ScreenAlignedQuad
    {
        GraphicsDevice m_device;
        VertexPositionTexture[] m_verts;
        short[] m_indices;

        /// <summary>
        /// Default CTOR
        /// </summary>
        /// <param name="device">device used to create vertex buffer</param>
        public ScreenAlignedQuad(GraphicsDevice device)
        {
            m_device = device;
            m_verts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(1,-1,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,-1,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,1,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(1,1,0),
                                new Vector2(1,0))
                        };
                
            m_indices = new short[] { 0, 1, 2, 2, 3, 0 };
        }

        public void Draw()
        {
            m_device.DrawUserIndexedPrimitives<VertexPositionTexture>(
                            PrimitiveType.TriangleList,
                            m_verts,
                            0,
                            4,
                            m_indices,
                            0,
                            2);
        }

        public short[] IndexArray
        {
            get { return m_indices; }
        }
        public VertexPositionTexture[] VertexArray
        {
            get { return m_verts; }
        }
        
    }
}
