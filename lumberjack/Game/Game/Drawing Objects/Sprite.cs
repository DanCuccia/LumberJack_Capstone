using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Drawing_Objects
{
    public class Sprite
    {
        protected Texture2D m_texture;
        protected Vector2 m_pos;
        protected float m_rot = 0f;
        protected float m_scale = 1f;
        protected SpriteEffects m_spriteEffects = SpriteEffects.None;

        protected int m_lastFrameTime = 0;
        protected int m_milliesPerFrame;

        protected Point m_frameSize;
        protected Point m_currentFrame = Point.Zero;
        protected Point m_sheetSize;

        protected Color m_tint = Color.White;

        protected bool m_isAnimating = true;
        protected bool m_isVisible = true;
        protected bool m_isCollidable = true;


        #region Initialization

        /// <summary>Default Constructor</summary>
        /// <param name="id">integer ID for base class OnScreenObject</param>
        public Sprite(){ }

        ~Sprite() { }

        /// <summary>Call this for init a sprite sheet</summary>
        /// <param name="frameSize">the width and height of each frame</param>
        /// <param name="sheetSize">the amount of columns and rows of the sheet</param>
        /// <param name="texture"> the loaded texture from Content</param>
        /// <param name="frameSpeed">how fast you want the frames to flip in millies(default:30)</param>
        public void Initialize(Point frameSize, Point sheetSize, Texture2D texture, int frameSpeed = 30)
        {
            m_frameSize = frameSize;
            m_sheetSize = sheetSize;
            m_currentFrame = Point.Zero;
            m_milliesPerFrame = frameSpeed;
            m_texture = texture;
        }


        #endregion Initialization

        #region API

        /// <summary>
        /// The Main 2d draw function 
        /// </summary>
        /// <param name="spriteBatch">the spritebatch with drawing utilities</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (m_isVisible)
            {
                spriteBatch.Draw(m_texture,
                    m_pos,
                    new Rectangle(
                        m_currentFrame.X * m_frameSize.X,
                        m_currentFrame.Y * m_frameSize.Y,
                        m_frameSize.X,
                        m_frameSize.Y),
                    m_tint,
                    m_rot,
                    new Vector2(
                        (m_frameSize.X / 2) * m_scale,
                        (m_frameSize.Y / 2) * m_scale),
                    m_scale,
                    m_spriteEffects,
                    0);
            }
        }

        /// <summary>
        /// if this sprite is animating, this will increment the frame
        /// </summary>
        /// <param name="gameTime">the current time of the game</param>
        public void Animate(GameTime gameTime)
        {
            //animation
            if (m_isAnimating)
            {
                m_lastFrameTime += gameTime.ElapsedGameTime.Milliseconds;
                if (m_lastFrameTime > m_milliesPerFrame)
                {
                    m_lastFrameTime -= m_milliesPerFrame;
                    ++m_currentFrame.X;
                    if (m_currentFrame.X >= m_sheetSize.X)
                    {
                        m_currentFrame.X = 0;
                        ++m_currentFrame.Y;
                        if (m_currentFrame.Y >= m_sheetSize.Y)
                            m_currentFrame.Y = 0;
                    }
                }
            }
        }

        #endregion API

        #region Functionality

        /// <summary>
        /// gets the bounding box of this sprite
        /// </summary>
        /// <returns> the new rectangle bounding box</returns>
        public Rectangle getBB()
        {
            return new Rectangle((int)m_pos.X, (int)m_pos.Y, m_frameSize.X, m_frameSize.Y);
        }

        /// <summary>
        /// Creates a 2d array of colors
        /// </summary>
        /// <param name="texture">the texture to decode</param>
        /// <returns>2D color array</returns>
        public Color[,] TextureTo2DArray()
        {
            //create array & init
            Color[] colors1D = new Color[m_texture.Width * m_texture.Height];
            Color[,] colors2D = new Color[m_texture.Width, m_texture.Height];
            m_texture.GetData(colors1D);

            //Fill the 2D array with approp colors from 1D array
            for (int x = 0; x < m_texture.Width; x++)
                for (int y = 0; y < m_texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * m_texture.Width];

            return colors2D;
        }


        #endregion Functionality

        #region Mutators
        public Point FrameSize
        {
            get { return m_frameSize; }
            set { m_frameSize = value; }
        }
        public bool Collidable
        {
            get { return m_isCollidable; }
            set { m_isCollidable = value; }
        }
        public SpriteEffects SpriteEffect
        {
            get { return m_spriteEffects; }
            set { m_spriteEffects = value; }
        }
        public bool Visible
        {
            get { return m_isVisible; }
            set { m_isVisible = value; }
        }
        public bool Animating
        {
            get { return m_isAnimating; }
            set { m_isAnimating = value; }
        }
        public Vector2 Position
        {
            get { return m_pos; }
            set { m_pos = value; }
        }
        public float Rotation
        {
            get { return m_rot; }
            set { m_rot = value; }
        }
        public float Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }
        public Texture2D Texture
        {
            get { return m_texture; }
            set { m_texture = value; }
        }
        public Color Color
        {
            get { return m_tint; }
            set { m_tint = value; }
        }
        public void setTint(int r, int g, int b)
        {
            m_tint = new Color(r, g, b);
        }
        #endregion
    }
}