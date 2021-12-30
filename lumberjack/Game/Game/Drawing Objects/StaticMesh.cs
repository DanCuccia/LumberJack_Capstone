using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Managers;

namespace Game.Drawing_Objects
{
    public enum RenderTechnique
    {
        RT_TOON,
        RT_WOOD,
        RT_DARKWOOD,
        RT_COLOR
    }

    /// <summary>A class that wraps the standard Framwork.Graphics Model class with
    /// any other data neccessary (position, rotation, scaling etc...)</summary>
    public class StaticMesh
    {
        Model                   m_model;
        Vector3                 m_position      = Vector3.Zero;
        Vector3                 m_rotation      = Vector3.Zero;
        float                   m_scale         = 1f;

        bool                    m_isDirty       = true;
        Matrix                  m_modelMatrix   = Matrix.Identity;

        MeshTextureManager    m_textures      = new MeshTextureManager();
        Effect                  m_effect;
        RenderTechnique         m_technique     = RenderTechnique.RT_TOON;

        Vector4                 m_color         = new Vector4(0.929411f, 0.942549f, 1f, 1f);

        #region Initialization


        /// <summary>Default CTOR</summary>
        public StaticMesh() { }

        /// <summary>Call this to load additional assets</summary>
        /// <param name="model">the loaded model from content</param>
        /// <returns>yay/nay success</returns>
        public void Initialize(Model model)
        {
            if (model == null)
                throw new NullReferenceException("StaticMesh::Initialize: input model is null");
            m_model = model;
            UpdateMatrix();
        }

        /// <summary>Init the staticmesh to render with a cartoon texturing shader technique</summary>
        /// <param name="modelFilepath">what model file to load</param>
        /// <param name="diffuseFilepath">wha texture file to load</param>
        public void Initialize(ContentManager content, string modelFilepath, string diffuseFilepath)
        {
            m_model = content.Load<Model>(modelFilepath);
            if (m_model == null)
                throw new Exception("StaticMesh::Initialize m_model loaded null");

            if (diffuseFilepath != null ||
                diffuseFilepath != "" )
                loadTexture(content, diffuseFilepath);

            UpdateMatrix();

            Renderer render = Renderer.getInstance();
            m_effect = render.EffectManager.GetEffect(RenderEffect.RFX_TOON);

            foreach (ModelMesh mesh in m_model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = m_effect;
        }

        /// <summary>Initiailize the staticmesh under a specific toon shader technique</summary>
        /// <param name="modelFilepath">what model file to load</param>
        /// <param name="technique">the rendering technique to use</param>
        public void Initialize(ContentManager content, string modelFilepath, RenderTechnique technique)
        {
            m_model = content.Load<Model>(modelFilepath);
            if (m_model == null)
                throw new Exception("StaticMesh::Initialize m_model loaded null");

            UpdateMatrix();

            Renderer render = Renderer.getInstance();
            m_effect = render.EffectManager.GetEffect(RenderEffect.RFX_TOON);

            foreach (ModelMesh mesh in m_model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = m_effect;

            m_technique = technique;
        }

        /// <summary>Init the StaticMesh under vertex coloring shader technique</summary>
        /// <param name="modelFilepath">what model file to load</param>
        /// <param name="color">what color to render the object as</param>
        public void Initialize(ContentManager content, string modelFilepath, Vector4 color)
        {
            m_color = color;
            this.Initialize(content, modelFilepath, RenderTechnique.RT_COLOR);
        }

        /// <summary>add a pre-loaded texture here</summary>
        /// <param name="tex">loaded texture</param>
        /// <param name="type">type of texture</param>
        public void loadTexture(Texture2D tex, TextureType type)
        {
            m_textures.AddTexture(tex, type);
        }

        /// <summary>load a texture using a string</summary>
        /// <param name="content">loading with</param>
        /// <param name="filename">file path</param>
        public void loadTexture(ContentManager content, string filepath)
        {
            this.loadTexture(content.Load<Texture2D>(filepath), TextureType.TEX_DIFFUSE);
        }

        

        #endregion Initialization



        #region Run-Time

        /// <summary>Set object matrices, switch shader technique depending on the enum,
        /// then draw. Don't switch techniques if we're in middle of drawing depth texture</summary>
        public void Draw()
        {
            Matrix[] transforms = new Matrix[m_model.Bones.Count];
            m_model.CopyAbsoluteBoneTransformsTo(transforms);

            if (Renderer.m_renderPhase == RenderPhase.PHASE_DEPTH)
            {
                FeedMesh(transforms);
                return;
            }

            switch(m_technique)
            {
                case RenderTechnique.RT_TOON:
                    Texture2D tex = m_textures.GetTexture(TextureType.TEX_DIFFUSE);
                    m_effect.CurrentTechnique = m_effect.Techniques["Toon"];
                    if (tex != null)
                    {
                        m_effect.Parameters["Texture"].SetValue(tex);
                        m_effect.Parameters["TextureEnabled"].SetValue(true);
                    }
                    else
                    {
                        m_effect.Parameters["TextureEnabled"].SetValue(false);
                        m_effect.Parameters["ModelColor"].SetValue(m_color);
                    }
                    break;

                case RenderTechnique.RT_WOOD:
                    m_effect.CurrentTechnique = m_effect.Techniques["WoodToon"];
                    break;
                    
                case RenderTechnique.RT_DARKWOOD:
                    m_effect.CurrentTechnique = m_effect.Techniques["DarkWood"];
                    break;

                case RenderTechnique.RT_COLOR:
                    m_effect.CurrentTechnique = m_effect.Techniques["VertexColor"];
                    m_effect.Parameters["ModelColor"].SetValue(m_color);
                    break;
            }

            FeedMesh(transforms);

        }

        /// <summary>Used in main Draw() -- assign the world matrix, draw the verts.</summary>
        private void FeedMesh(Matrix[] trans)
        {
            foreach (ModelMesh mesh in m_model.Meshes)
            {
                m_effect.Parameters["World"].SetValue( trans[mesh.ParentBone.Index] *
                    this.WorldMatrix);
                mesh.Draw();
            }
        }

        /// <summary>Update the model matrix, containing scale, rotation and position,
        /// only called </summary>
        private void UpdateMatrix()
        {
            m_modelMatrix = Matrix.CreateScale(m_scale) *
                Matrix.CreateRotationX(MathHelper.ToRadians(m_rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(m_rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(m_rotation.Z)) *
                Matrix.CreateTranslation(m_position);
            m_isDirty = false;
        }

        #endregion Run-Time



        #region Mutators

        public Model Model
        {
            get { return m_model; }
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
        public float Scale
        {
            get { return m_scale; }
            set 
            { 
                m_scale = value;
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
        public Matrix WorldMatrix
        {
            get 
            {
                if (m_isDirty == false)
                    return m_modelMatrix;
                else
                {
                    UpdateMatrix();
                    return m_modelMatrix;
                }
            }
        }
        public MeshTextureManager Textures
        {
            get { return m_textures; }
        }
       

        #endregion Mutators
    }
}
