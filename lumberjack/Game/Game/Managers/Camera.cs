using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game.Managers
{
    /// <summary>Defines what type of camera mode to upate() as</summary>
    public enum CameraType
    {
        CAM_CHASE,
        CAM_FREE,
        CAM_STATIONARY
    }

    /// <summary>Holds all camera type information
    /// Yaw:X Pitch:Y Roll:Z</summary>
    public class Camera
    {

        #region Member Variables

        CameraType      m_camType                   = CameraType.CAM_FREE;

        Matrix          m_world                     = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
        Matrix          m_view                      = Matrix.Identity;
        Matrix          m_projection;
        
        Viewport        m_viewport;

        Vector3         m_position;
        Vector3         m_lookAtTarget;
        Matrix          m_camRotation               = Matrix.Identity;
        float           m_yaw, m_pitch, m_roll      = 0f;

        float           m_aspectRatio;
        float           m_nearPlaneDist             = 1f;
        float           m_farPlaneDist              = 100000f;

        const float     m_speed                     = 25f;
        const float     m_rotSpeed                  = .02f;

        Vector3         m_cameraOffset;
        Vector3         m_targetOffset;
        Vector3         m_gotoPosition;
        float           m_stepAmount                = .15f;

        int             m_switchTime                = 0;
        const int       m_switchWaitTime            = 500;
        bool            m_canSwitch = true;


        #endregion Member Variables


        #region Initialization & RunTime

        /// <summary>Default Contructor</summary>
        public Camera(Viewport viewPort)
        {
            m_viewport = viewPort;

            m_lookAtTarget = new Vector3(0.0f, 0.0f, 0.0f);
            m_position = new Vector3(0.0f, 40.0f, -100.0f);
            m_cameraOffset = new Vector3(0.0f, 70.0f, 100.0f);
            m_targetOffset = new Vector3(20.0f, 50.0f, 0.0f);
            m_gotoPosition = m_position;
            Mouse.SetPosition(m_viewport.Width / 2, m_viewport.Height / 2);
        }

        /// <summary>You must call this function, standard initialization of user parameters</summary>
        /// <param name="camMode"> what type of camera this is</param>
        /// <param name="aspectRatio"> the screen aspect ratio (get from device) </param>
        public void Initialize(CameraType camMode, float aspectRatio)
        {
            m_camType = camMode;
            m_aspectRatio = aspectRatio;
            m_projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, m_aspectRatio, m_nearPlaneDist, m_farPlaneDist);
        }

        /// <summary>The main update function to be used once before drawing a frame
        /// Updates the view and projection matrices, will update according
        /// CameraType enum variable</summary>
        /// <param name="chasedObjectPos">the world position of the chased object</param>
        public void Update(Vector3 chasedObjectPos)
        {
            switch (m_camType)
            {
                case CameraType.CAM_FREE:

                    m_camRotation.Forward.Normalize();
                    m_camRotation.Up.Normalize();
                    m_camRotation.Right.Normalize();

                    m_camRotation = Matrix.CreateFromYawPitchRoll(m_yaw, m_pitch, m_roll);

                    m_lookAtTarget = m_position + m_camRotation.Forward;

                    break;

                case CameraType.CAM_CHASE:

                    m_camRotation.Forward.Normalize();

                    m_camRotation = Matrix.CreateFromAxisAngle(m_camRotation.Forward, m_roll);

                    m_lookAtTarget = chasedObjectPos + m_targetOffset;

                    m_gotoPosition = Vector3.Transform(m_cameraOffset, Matrix.CreateTranslation(chasedObjectPos));
                    m_position = Vector3.SmoothStep(m_position, m_gotoPosition, m_stepAmount);

                    m_yaw = MathHelper.SmoothStep(m_yaw, 0f, 0.1f);
                    m_pitch = MathHelper.SmoothStep(m_pitch, 0f, 0.1f);

                    break;
            }

            m_view = Matrix.CreateLookAt(m_position, m_lookAtTarget, Vector3.Up);
        }

        /// <summary>Reset Camera's rotation values, and rotation matrix</summary>
        public void ResetCamera()
        {
            m_yaw = m_pitch = m_roll = 0.0f;
            m_camRotation = Matrix.Identity;
            m_position = Vector3.Zero;
        }

        #endregion



        #region Input-Movement

        /// <summary>Switch the Camera during Free cam to Stationary and vise versa</summary>
        /// <param name="time">current time</param>
        /// <param name="kb">current keyboard state</param>
        private void SwitchCamTest(GameTime time, KeyboardState kb)
        {
            //hold switching for .5 seconds after switched previously
            if (m_canSwitch == false)
            {
                m_switchTime += time.ElapsedGameTime.Milliseconds;
                if (m_switchTime > 500)
                {
                    m_switchTime = 0;
                    m_canSwitch = true;
                }
            }

            //switch with Z button from stationary to free
            if (kb.IsKeyDown(Keys.Z) && m_canSwitch)
                if (m_camType == CameraType.CAM_STATIONARY)
                {
                    Mouse.SetPosition(m_viewport.Width / 2, m_viewport.Height / 2);
                    m_camType = CameraType.CAM_FREE;
                    m_canSwitch = false;
                }

                else if (m_camType == CameraType.CAM_FREE)
                {
                    m_camType = CameraType.CAM_STATIONARY;
                    m_canSwitch = false;
                }
        }


        /// <summary>Encapsulates any movement based from user input</summary>
        /// <param name="kb">current keyboard state</param>
        /// <param name="ms">current mouse state</param>
        /// <param name="gp">current gamepad state</param>
        public void FreeCamInput(GameTime time, KeyboardState kb, MouseState ms, GamePadState gp)
        {
            SwitchCamTest(time, kb);

            //quit out now if stationary
            if (m_camType == CameraType.CAM_STATIONARY)
                return;

            if (kb.IsKeyDown(Keys.F10))
                ResetCamera();
            
            //if (kb.IsKeyDown(Keys.Left))
            //    m_yaw += m_rotSpeed;
            if (gp.ThumbSticks.Left.X < 0)
                m_yaw += m_rotSpeed;

            //if (kb.IsKeyDown(Keys.Right))
            //    m_yaw += -m_rotSpeed;
            if (gp.ThumbSticks.Left.X > 0)
                m_yaw += -m_rotSpeed;

            //if (kb.IsKeyDown(Keys.Up))
            //    m_pitch += -m_rotSpeed;
            if (gp.ThumbSticks.Left.Y < 0)
                m_pitch += -m_rotSpeed;

            //if (kb.IsKeyDown(Keys.Down))
            //    m_pitch += m_rotSpeed;
            if(gp.ThumbSticks.Left.Y > 0)
                m_pitch += m_rotSpeed;

            m_pitch += -(ms.Y - m_viewport.Height / 2) * .003f;
            m_yaw += -(ms.X - m_viewport.Width / 2) * .003f;
            Mouse.SetPosition(m_viewport.Width / 2, m_viewport.Height / 2);

            if (m_camType == CameraType.CAM_FREE)
            {
                if (kb.IsKeyDown(Keys.W) || (gp.ThumbSticks.Left.Y > 0))
                {
                    if (kb.IsKeyDown(Keys.Space))
                        MoveCamera(m_camRotation.Forward);
                    MoveCamera(m_camRotation.Forward);
                }

                if (kb.IsKeyDown(Keys.S) || (gp.ThumbSticks.Left.Y < 0))
                    MoveCamera(-m_camRotation.Forward);

                if (kb.IsKeyDown(Keys.A) || (gp.ThumbSticks.Left.X > 0))
                    MoveCamera(-m_camRotation.Right);

                if (kb.IsKeyDown(Keys.D) || (gp.ThumbSticks.Left.X < 0))
                    MoveCamera(m_camRotation.Right);

                if (kb.IsKeyDown(Keys.E))
                    MoveCamera(m_camRotation.Up);

                if (kb.IsKeyDown(Keys.Q))
                    MoveCamera(-m_camRotation.Up);
            }
            
        }

        /// <summary> Moves the position of the camera, by a varible m_speed</summary>
        /// <param name="vector">the direction in which to move</param>
        private void MoveCamera(Vector3 vector)
        {
            m_position += m_speed * vector;
        }


        #endregion input-movement



        #region Mutators

        public Vector3 Position
        {
            set { m_position = value; }
            get { return m_position; }
        }
        public Vector4 PositionDot4
        {
            get { return new Vector4(m_position.X, m_position.Y, m_position.Z, 1f); }
        }
        public Vector3 LookAtTarget
        {
            set { m_lookAtTarget = value; }
            get { return m_lookAtTarget; }
        }
        public float AspectRatio
        {
            get { return m_aspectRatio; }
            set
            {
                m_aspectRatio = value;
                m_projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.PiOver4, 
                    m_aspectRatio,
                    m_nearPlaneDist,
                    m_farPlaneDist);

            }
        }
        public float NearPlaneDist
        {
            get { return m_nearPlaneDist; }
            set 
            { 
                m_nearPlaneDist = value;
                m_projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    m_aspectRatio, 
                    m_nearPlaneDist, 
                    m_farPlaneDist);
            }
        }
        public float FarPlaneDist
        {
            get { return m_farPlaneDist; }
            set 
            { 
                m_farPlaneDist = value;
                m_projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 
                    m_aspectRatio, 
                    m_nearPlaneDist, 
                    m_farPlaneDist);
            }
        }
        public Matrix ViewMatrix
        {
            get { return m_view; }
        }
        public Matrix ProjectionMatrix
        {
            get { return m_projection; }
        }
        public Matrix WorldMatrix
        {
            get { return m_world; }
        }
        public Matrix RotationMatrix
        {
            get { return m_camRotation; }
        }
        public float Speed
        {
            get { return m_speed; }
        }
        public CameraType CameraType
        {
            get { return m_camType; }
            set { m_camType = value; }
        }
        public float Yaw
        {
            get { return m_yaw; }
            set { m_yaw = value; }
        }
        public float Pitch
        {
            get { return m_pitch; }
            set { m_pitch = value; }
        }


        #endregion//mutators
    }
}
