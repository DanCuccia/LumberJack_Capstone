using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Game.Managers;
using Game.Math_Physics;

namespace Game.Drawing_Objects
{
    public class Line
    {
        #region Member Variables

        GraphicsDevice          device;//ref
        Renderer                render;//ref

        Matrix                  worldMatrix;

        Effect                  effect;//ref
        VertexPositionColor[]   PointList;
        VertexBuffer            vertexBuffer;

        int                     points              = 2;
        short[]                 lineListIndices;

        #endregion Member Varialbes

        #region Initialization

        public Line(GraphicsDevice graphicsdevice)
        {
            device = graphicsdevice;
            worldMatrix = Matrix.Identity;
            Initialize();
        }

        public void Initialize()
        {
            render = Renderer.getInstance();
            effect = render.EffectManager.GetEffect(RenderEffect.RFX_TOON);
            
            SetPointPosition(new Vector3(0, 0,0), new Vector3(1, 1,1), Color.PaleVioletRed, Color.Red);
            
            InitializeLineList();
        }

        private void SetPointPosition(Vector3 start, Vector3 end, Color color1, Color color2)
        {
            if (PointList == null)
            {
                PointList = new VertexPositionColor[points];
                PointList[0] = new VertexPositionColor( new Vector3(start.X, start.Y, start.Z), color1);
                PointList[1] = new VertexPositionColor( new Vector3(end.X, end.Y, end.Z), color2);
            }
            else
            {
                PointList[0].Position.X = start.X;
                PointList[0].Position.Y = start.Y;
                PointList[0].Position.Z = start.Z;
                PointList[0].Color = color1;

                PointList[1].Position.X = end.X;
                PointList[1].Position.Y = end.Y;
                PointList[1].Position.Z = end.Z;
                PointList[1].Color = color2;
            }

            if (vertexBuffer == null)
            {
                // Initialize the vertex buffer
                vertexBuffer = new VertexBuffer(device,
                    VertexPositionColor.VertexDeclaration,
                    PointList.Length, BufferUsage.None);
                vertexBuffer.SetData<VertexPositionColor>(PointList);
            }
        }

        /// <summary>generate indices</summary>
        private void InitializeLineList()
        {
            lineListIndices = new short[2];
            for (int i = 0; i < 1; i++)
            {
                lineListIndices[i * 2] = (short)(i);
                lineListIndices[(i * 2) + 1] = (short)(i + 1);
            }
        }

        #endregion Initialization

        #region API

        /// <summary>main draw call</summary>
        /// <param name="start">first position</param>
        /// <param name="end">second position</param>
        /// <param name="color"></param>
        public void Draw(Vector3 start, Vector3 end, Vector4 startColor, Vector4 endColor)
        {
            if (Renderer.m_renderPhase == RenderPhase.PHASE_DEPTH)
                return;

            SetPointPosition(start, end, Color.FromNonPremultiplied(startColor), Color.FromNonPremultiplied(endColor));

            effect.CurrentTechnique = effect.Techniques["Line3D"];
            effect.CurrentTechnique.Passes[0].Apply();
            
            FeedMesh();
        }

        /// <summary>rasterize the line</summary>
        private void FeedMesh()
        {
            effect.Parameters["World"].SetValue(worldMatrix);

            render.FlipToWireframe();

            device.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.LineList, PointList, 0, 2,
                lineListIndices, 0, 1);

            render.FlipToSolid();
        }

        #endregion API
    }
}
