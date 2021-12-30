using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Managers;
using Game.Math_Physics;

namespace Game.Drawing_Objects
{
    /// <summary>This class handles all terrain generation based off of a height map.</summary>
    public class GeneratedTerrain
    {
        Model               m_terrainModel;
        HeightMapData       m_heightData;

        RenderTechnique     m_technique = RenderTechnique.RT_TOON;

        Effect              m_effect;
        Camera              m_camera;

        Vector3             m_position = Vector3.Zero;
        float               m_scale = 1f;

        bool                m_isDirty = false;
        Matrix              m_objectMatrix = Matrix.Identity;

        #region Initialization

        /// <summary>Default CTOR, init is all done in Initialize</summary>
        public GeneratedTerrain(){ }

        /// <summary>The main initialization m_heightData for easy functionality 
        /// of terrain heights info</summary>
        /// <param name="content">loader</param>
        /// <param name="heightmapFilename">which heightmap to load as a mesh</param>
        /// <param name="texturePack">which texture pack to load </param>
        public void Initialize(ContentManager content, Camera camera, string heightmapFilename)
        {
            m_terrainModel = content.Load<Model>(heightmapFilename);

            Renderer render = Renderer.getInstance();
            m_effect = render.EffectManager.GetEffect(RenderEffect.RFX_TOON);

            m_camera = camera;

            m_heightData = m_terrainModel.Tag as HeightMapData;
            if (m_heightData == null)
                throw new ArgumentNullException("GeneratedTerrain::Initialize: terrainModel has no heightMapData");

            foreach (ModelMesh mesh in m_terrainModel.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = m_effect;
        }

        #endregion Initialization

        /// <summary>Draw's the Terrain model</summary>
        public void Draw()
        {
            m_effect.Parameters["World"].SetValue(this.ObjectMatrix);

            if (Renderer.m_renderPhase == RenderPhase.PHASE_DEPTH)
            {
                FeedMesh();
                return;
            }

            if (m_technique == RenderTechnique.RT_COLOR)
            {
                m_effect.CurrentTechnique = m_effect.Techniques["TerrainColoredToon"];
                m_effect.CurrentTechnique.Passes[0].Apply();

                FeedMesh();
                return;
            }

            m_effect.CurrentTechnique = m_effect.Techniques["TerrainToon"];
            m_effect.CurrentTechnique.Passes[0].Apply();
            FeedMesh();

        }

        private void FeedMesh()
        {
            foreach (ModelMesh mesh in m_terrainModel.Meshes)
                mesh.Draw();
        }


        private void UpdateMatrix()
        {
            m_objectMatrix = Matrix.CreateScale(m_scale) * 
                Matrix.CreateTranslation(m_position);
            m_isDirty = false;
        }


        #region Mutators

        public Model Model
        {
            get { return m_terrainModel; }
        }
        public float MatScale
        {
            get { return m_scale; }
            set { 
                m_scale = value;
                m_isDirty = true;
            }
        }
        public Vector3 Position
        {
            set { 
                m_position = value;
                m_isDirty = true;
            }
            get { return m_position; }
        }
        public float PositionX
        {
            set { 
                m_position.X = value;
                m_isDirty = true;
            }
            get { return m_position.X; }
        }
        public float PositionY
        {
            set { 
                m_position.Y = value;
                m_isDirty = true;
            }
            get { return m_position.Y; }
        }
        public float PositionZ
        {
            set { 
                m_position.Z = value;
                m_isDirty = true;
            }
            get { return m_position.Z; }
        }
        public float Scale
        {
            get { return m_heightData.Scale; }
        }
        public float GetTerrainY(Vector3 position)
        {
            return m_heightData.GetHeight(position);
        }
        public HeightMapData HeightData
        {
            get { return m_heightData; }
        }
        public Matrix ObjectMatrix
        {
            get
            {
                if (m_isDirty)
                {
                    UpdateMatrix();
                    return m_objectMatrix;
                }
                else
                    return m_objectMatrix;
            }
        }

        #endregion Mutators

    }
}
