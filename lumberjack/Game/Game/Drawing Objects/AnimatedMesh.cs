using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SkinnedModel;

using Game.Managers;

namespace Game.Drawing_Objects
{
    class ModelBase
    {
        int m_ID;
        Model m_model;

        public ModelBase(int id, Model loadedModel)
        {
            m_ID = id;
            m_model = loadedModel;
        }

        public int ID
        {
            set { m_ID = value; }
            get { return m_ID; }
        }
        public Model Model
        {
            set { m_model = value; }
            get { return m_model; }
        }

    }

    class AnimatedMesh
    {
        AnimationController m_animController;   //animation controls
        int                 m_myModelID;        //the ID to which Model this is animating
        Vector3             m_position;         //world space position
        Vector3             m_rotation;         //world space rotation
        Camera              m_camera;           //reference used in draw()

        public AnimatedMesh()
        {}

        public bool Initialize(SkinningData skinningData, int modelId)
        {
            if (skinningData == null)
                throw new Exception("AnimatedMesh::Initialize() : Cannot find skin data from model.");
            m_animController = new AnimationController(skinningData);
            m_myModelID = modelId;
            return true;
        }


        /// <summary>Move the animationController along in it's current clip</summary>
        /// <param name="gameTime">the current game time</param>
        public void Animate(GameTime gameTime)
        {
            m_animController.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
        }

        public void StartAnimation(string clip)
        {
            throw new NotImplementedException();
        }


        #region MUTATORS
        // Mutators
        public Vector3 Position
        {
            set { m_position = value; }
            get { return m_position; }
        }
        public Vector3 Rotation
        {
            set { m_rotation = value; }
            get { return m_rotation; }
        }
        public Camera Camera
        {
            set { m_camera = value; }
            get { return m_camera; }
        }
        public AnimationController AnimationController
        {
            get { return m_animController; }
            set { }
        }
        public int MyModelID
        {
            get { return m_myModelID; }
            set { m_myModelID = value; }
        }


        #endregion//MUTATORS

    }
}
