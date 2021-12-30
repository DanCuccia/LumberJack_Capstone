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
        CAM_STATIONARY,
        CAM_AUTOMATED
    }

    /// <summary>Holds all camera type information
    /// Yaw:X Pitch:Y Roll:Z</summary>
    public class Camera
    {

        #region Member Variables

        WorldManager    m_world;   //ref
        CameraType      m_camType                   = CameraType.CAM_FREE;

        Matrix          m_view                      = Matrix.Identity;
        Matrix          m_projection;
        
        Viewport        m_viewport;

        Vector3         m_position                  = Vector3.Zero;
        Vector3         m_lookAtTarget              = Vector3.Zero;

        Matrix          m_camRotation               = Matrix.Identity;
        float           m_yaw, m_pitch, m_roll      = 0f;

        float           m_aspectRatio;
        float           m_nearPlaneDist             = 1f;
        float           m_farPlaneDist              = 100000f;

        float           m_stepSpeed;
        Vector3         m_targetGoto;
        Vector3         m_positionGoto;
        public delegate void AutomationCompleteCallback();
        AutomationCompleteCallback m_callback;
        float           m_automationCompleteThreshold = .1f;

        const float     m_speed                     = 20f;
        const float     m_rotSpeed                  = 0.02f;

        float           m_distanceOffset            = 450;
        Vector2         m_distanceClamps            = new Vector2(350, 1000);
        float           m_playerYOffset             = 150;
        Vector2         m_yOffsetClamps             = new Vector2(10, 500);
        float           m_chaseAngle                = 0f;
        Vector3         m_targetOffset              = new Vector3(20, 70, 0);
        int             m_wheelValue;

        Point           m_lastMSPosition;

        Point           m_worldIndices              = new Point(-1, -1);


        #endregion Member Variables


        #region Initialization & RunTime

        /// <summary>Default Contructor</summary>
        public Camera(Viewport viewPort)
        {
            m_viewport = viewPort;
            Mouse.SetPosition(m_viewport.Width / 2, m_viewport.Height / 2);
            m_wheelValue = Mouse.GetState().ScrollWheelValue;
        }

        /// <summary>You must call this function, standard initialization of user parameters</summary>
        /// <param name="camMode"> what type of camera this is</param>
        /// <param name="aspectRatio"> the screen aspect ratio (get from device) </param>
        public void Initialize(CameraType camMode, float aspectRatio)
        {
            m_camType = camMode;
            m_aspectRatio = aspectRatio;
            m_projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, m_aspectRatio, m_nearPlaneDist, m_farPlaneDist);

            m_world = WorldManager.getInstance();
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
                    
                    m_lookAtTarget = chasedObjectPos + m_targetOffset;

                    m_position.X = chasedObjectPos.X + (m_distanceOffset * (float)Math.Sin(MathHelper.ToRadians(m_chaseAngle)));
                    m_position.Z = chasedObjectPos.Z + (m_distanceOffset * (float)Math.Cos(MathHelper.ToRadians(m_chaseAngle)));

                    //float terrainHeight;
                    //m_worldIndices = m_world.getTerrainHeight(m_position, out terrainHeight);
                    //if (m_position.Y + m_playerYOffset <= terrainHeight + 30)
                    //{
                    //    m_playerYOffset = m_position.Y - chasedObjectPos.Y + 30;
                    //}
                    //else
                    //{
                        m_position.Y = chasedObjectPos.Y + m_playerYOffset;
                    //}

                    m_lastMSPosition = new Point(Mouse.GetState().X, Mouse.GetState().Y);

                    break;

                case CameraType.CAM_AUTOMATED:

                    m_camRotation.Forward.Normalize();
                    m_camRotation = Matrix.CreateFromAxisAngle(m_camRotation.Forward, m_roll);

                    m_position = Vector3.SmoothStep(m_position, m_positionGoto, m_stepSpeed);
                    m_lookAtTarget = Vector3.SmoothStep(m_lookAtTarget, m_targetGoto, m_stepSpeed);

                    m_yaw = MathHelper.SmoothStep(m_yaw, 0f, 0.1f);
                    m_pitch = MathHelper.SmoothStep(m_pitch, 0f, 0.1f);

                    if(Vector3.Distance(m_position, m_positionGoto) <= m_automationCompleteThreshold && 
                        Vector3.Distance(m_lookAtTarget, m_targetGoto) <= m_automationCompleteThreshold)
                    //if ((m_position.X - m_positionGoto.X) < m_automationCompleteThreshold &&
                    //    (m_position.Z - m_positionGoto.Z) < m_automationCompleteThreshold &&
                    //    (m_position.Y - m_positionGoto.Y) < m_automationCompleteThreshold)
                    {
                        m_camType = CameraType.CAM_STATIONARY;
                        m_position = m_positionGoto;
                        m_lookAtTarget = m_targetGoto;
                        if (m_callback != null)
                        {
                            m_callback();
                        }
                    }

                    break;

                case CameraType.CAM_STATIONARY:break;
            }

            m_view = Matrix.CreateLookAt(m_position, m_lookAtTarget, Vector3.Up);
        }

        /// <summary>Reset Camera's rotation values, and rotation matrix, and move to world(0,0,0)</summary>
        public void ResetCamera()
        {
            m_yaw = m_pitch = m_roll = 0.0f;
            m_camRotation = Matrix.Identity;
            m_position = Vector3.Zero;
        }


        #endregion


        #region Input-Movement

        public void Input(KeyboardState kb, MouseState ms)
        {
            switch (m_camType)
            {
                case Managers.CameraType.CAM_FREE:
                    FreeCamInput(kb, ms);
                    break;

                case Managers.CameraType.CAM_CHASE:
                    ChaseCamInput(kb, ms);
                    break;

                default: break;

            }
        }

        /// <summary>Encapsulates any movement based from user input</summary>
        /// <param name="kb">current keyboard state</param>
        /// <param name="ms">current mouse state</param>
        private void FreeCamInput(KeyboardState kb, MouseState ms)
        {
            if (m_camType != CameraType.CAM_FREE)
                return;

            m_pitch += -(ms.Y - m_viewport.Height / 2) * .003f;
            m_yaw += -(ms.X - m_viewport.Width / 2) * .003f;
            Mouse.SetPosition(m_viewport.Width / 2, m_viewport.Height / 2);

            if (kb.IsKeyDown(Keys.W))
            {
                if (kb.IsKeyDown(Keys.Space))
                    MoveCamera(m_camRotation.Forward);
                MoveCamera(m_camRotation.Forward);
            }

            if (kb.IsKeyDown(Keys.S))
                MoveCamera(-m_camRotation.Forward);
            if (kb.IsKeyDown(Keys.A))
                MoveCamera(-m_camRotation.Right);
            if (kb.IsKeyDown(Keys.D))
                MoveCamera(m_camRotation.Right);
            if (kb.IsKeyDown(Keys.E))
                MoveCamera(m_camRotation.Up);
            if (kb.IsKeyDown(Keys.Q))
                MoveCamera(-m_camRotation.Up);

        }

        /// <summary>any user-input camera-movement logic in here</summary>
        /// <param name="kb">current keyboard state</param>
        /// <param name="ms">current mouse state</param>
        private void ChaseCamInput(KeyboardState kb, MouseState ms)
        {
            if (m_camType != CameraType.CAM_CHASE)
                return;

            if (kb.IsKeyDown(Keys.OemPeriod) || kb.IsKeyDown(Keys.NumPad6))
                m_chaseAngle += 2f;
            if (kb.IsKeyDown(Keys.OemComma) || kb.IsKeyDown(Keys.NumPad4))
                m_chaseAngle -= 2f;

            if (kb.IsKeyDown(Keys.I) || kb.IsKeyDown(Keys.NumPad8))
            {
                m_playerYOffset += 2f;
                m_playerYOffset = MathHelper.Clamp(m_playerYOffset, m_yOffsetClamps.X, m_yOffsetClamps.Y);
            }
            if (kb.IsKeyDown(Keys.K) || kb.IsKeyDown(Keys.NumPad2))
            {
                m_playerYOffset -= 2f;
                m_playerYOffset = MathHelper.Clamp(m_playerYOffset, m_yOffsetClamps.X, m_yOffsetClamps.Y);
            }

            if (ms.MiddleButton == ButtonState.Pressed)
            {
                m_chaseAngle += -(ms.X - m_lastMSPosition.X) * .3f;
                m_playerYOffset += -(ms.Y - m_lastMSPosition.Y) * .7f;

                m_playerYOffset = MathHelper.Clamp(m_playerYOffset, m_yOffsetClamps.X, m_yOffsetClamps.Y);

                m_lastMSPosition = new Point(ms.X, ms.Y);
                Mouse.SetPosition(m_lastMSPosition.X, m_lastMSPosition.Y);
            }

            if (ms.ScrollWheelValue > m_wheelValue)
            {
                m_distanceOffset -= 20f;
                m_distanceOffset = MathHelper.Clamp(m_distanceOffset, m_distanceClamps.X, m_distanceClamps.Y);
                m_wheelValue = ms.ScrollWheelValue;
            }
            if (ms.ScrollWheelValue < m_wheelValue)
            {
                m_distanceOffset += 20f;
                m_distanceOffset = MathHelper.Clamp(m_distanceOffset, m_distanceClamps.X, m_distanceClamps.Y);
                m_wheelValue = ms.ScrollWheelValue;
            }

        }

        /// <summary> Moves the position of the camera, by a varible m_speed</summary>
        /// <param name="vector">the direction in which to move</param>
        private void MoveCamera(Vector3 vector)
        {
            m_position += m_speed * vector;
        }

        /// <summary>automate the camera to a position</summary>
        /// <param name="targetPosition">goto position of the camera target</param>
        /// <param name="cameraPosition">goto position of the camera</param>
        /// <param name="callback">call this when the automation is complete</param>
        /// <param name="stepSpeed">camera speed to new location</param>
        public void SmoothStepTo(Vector3 targetPosition, Vector3 cameraPosition, AutomationCompleteCallback callback, float stepSpeed = 0.15f, float threshold = 0.1f)
        {
            m_stepSpeed = stepSpeed;
            m_positionGoto = cameraPosition;
            m_targetGoto = targetPosition;
            m_automationCompleteThreshold = threshold;
            m_camType = CameraType.CAM_AUTOMATED;
            m_callback = callback;
        }


        #endregion input-movement



        #region Mutators

        public float ChaseAngle
        {
            get { return m_chaseAngle; }
            set { m_chaseAngle = value; }
        }
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
            set 
            { 
                m_camType = value;
                Mouse.SetPosition(m_viewport.Width / 2, m_viewport.Height / 2);
            }
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
