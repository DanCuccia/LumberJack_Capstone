using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using XNAnimation;
using XNAnimation.Controllers;

using Game.Managers;

namespace Game.Drawing_Objects
{
    /// <summary>provides Encapsulated functionality for the XNAnimation interface,
    /// The animation controller interface is left public, since it is the main api anyway,
    /// note: m_drawDumb flag will not use our shader, and will  draw with XNA's skinnedEffect shader instead</summary>
    public class AnimatedMesh
    {
        #region Member Variables

        Vector3             m_position = Vector3.Zero;
        Vector3             m_rotation = Vector3.Zero;
        static float        m_scale = 2.5f;   //.X to .fbx scale fix

        bool                m_isDirty = true;
        Matrix              m_world = Matrix.Identity;

        SkinnedModel        m_model;
        AnimationController m_animController;

        Texture2D           m_diffuseTexture;

        static float        m_animSpeed = .022f;

        Effect              m_effect;    //ref

        public delegate void AnimationCompleteCallback();
        AnimationCompleteCallback m_callback;

        #endregion Member Variables


        #region Initialization

        /// <summary>Default CTOR, call initialize</summary>
        public AnimatedMesh() { }

        /// <summary>Initialize member variables, note: this is setup for .X animation files</summary>
        /// <param name="modelFilepath">filepath to the model file containing skeleton animations</param>
        /// <param name="textureFilepath">filepath to the model's diffuse texture</param>
        public void Initialize(ContentManager content, string modelFilepath, string textureFilepath)
        {
            m_effect = Renderer.getInstance().EffectManager.GetEffect(RenderEffect.RFX_TOON);

            m_model = content.Load<SkinnedModel>(modelFilepath);
            m_animController = new AnimationController(m_model.SkeletonBones);
            m_animController.Speed = m_animSpeed;

            m_diffuseTexture = content.Load<Texture2D>(textureFilepath);
            if (m_diffuseTexture == null)
                throw new ArgumentNullException("AnimatedMesh::Initialize diffuse texture loaded null :: " + textureFilepath);

            UpdateMatrix();

            foreach (ModelMesh mesh in m_model.Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = m_effect;
        }

        #endregion Initialization


        #region Run-Time

        /// <summary>Update the animation controller</summary>
        public void Update(GameTime time)
        {
            m_animController.Update(time.ElapsedGameTime, Matrix.Identity);

            if (m_callback != null && m_animController.IsPlaying == false)
            {
                m_callback();
                m_callback = null;
            }
        }

        /// <summary>Draw the animated mesh, note: checks Renderer for rendering phase</summary>
        public void Draw()
        {
            if (Renderer.m_renderPhase == RenderPhase.PHASE_DEPTH)
                m_effect.CurrentTechnique = m_effect.Techniques["AnimatedMeshDepth"];
            else
                m_effect.CurrentTechnique = m_effect.Techniques["AnimatedMesh"];

            m_effect.Parameters["FinalTransforms"].SetValue(m_animController.SkinnedBoneTransforms);
            m_effect.Parameters["World"].SetValue(this.WorldMatrix);

            if (Renderer.m_renderPhase == RenderPhase.PHASE_DIFFUSE)
            {
                m_effect.Parameters["Texture"].SetValue(m_diffuseTexture);
                m_effect.Parameters["TextureEnabled"].SetValue(true);
            }

            foreach (ModelMesh mesh in m_model.Model.Meshes)
                mesh.Draw();

            if(Renderer.m_renderPhase == RenderPhase.PHASE_DEPTH)
                m_effect.CurrentTechnique = m_effect.Techniques["NormalDepth"];

        }

        /// <summary>
        /// Updates the world model matrix of this animated mesh for drawing
        /// </summary>
        private void UpdateMatrix()
        {
            m_world = Matrix.CreateScale(m_scale) *
                Matrix.CreateRotationX(MathHelper.ToRadians(m_rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(m_rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(m_rotation.Z)) *
                Matrix.CreateTranslation(m_position);
            m_isDirty = false;
        }

        /// <summary>
        /// Begins an animation clip
        /// </summary>
        /// <param name="clip">the name of the clip you want to play</param>
        /// <param name="loopEnable">wether to loop or not</param>
        /// <param name="callback">if not looping, you can set a callback for when the animation completes</param>
        public void BeginAnimation(string clip, bool loopEnable = true, AnimationCompleteCallback callback = null)
        {
            m_animController.StartClip(m_model.AnimationClips[clip]);
            m_animController.LoopEnabled = loopEnable;

            if (loopEnable == false && callback != null)
                m_callback = callback;
        }

        public void SetPlaybackMode(PlaybackMode mode)
        {
            m_animController.PlaybackMode = mode;
        }

        #endregion Run-Time


        #region Mutators

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
        public void AddRotation(Vector3 rotValue)
        {
            m_rotation += rotValue;
            m_isDirty = true;
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
                if (m_isDirty)
                {
                    UpdateMatrix();
                    return m_world;
                }
                else
                    return m_world;
            }
        }
        public AnimationController AnimationController
        {
            get { return m_animController; }
        }
        public SkinnedModel SkinnedModel
        {
            get { return m_model; }
        }

        #endregion Mutators

    }
}
