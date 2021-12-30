using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Managers;

namespace Game.Drawing_Objects
{
    public class Sprite
    {
        #region Member Variables

        public String               name;
        protected Vector2           position;
        protected String            textureName;
        public Color                tint            = Color.White;
        public int                  opacity         = 255;
        public float                rotation        = 0f;
        public Vector2              scale           = Vector2.One;
        protected Vector2           origin;
        protected TextureManager    textureMan;
        public SpriteManager        spriteMan;

        protected bool              pressed = false;
        protected AudioManager      audio;
        protected bool              visible = true;

        protected bool              clickable = true;
        public delegate void        ClickCallback();
        ClickCallback               callBack = null;

        Texture2D                   texture;

        public enum RotationOrigin
        {
            TOP_LEFT = 0,
            TOP_MIDDLE,
            TOP_RIGHT,
            CENTER_LEFT,
            CENTER,
            CENTER_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_MIDDLE,
            BOTTOM_RIGHT
        };

        #endregion Member Variables


        public Sprite(String name, Vector2 position, String texture, SpriteManager sm, TextureManager tm, ClickCallback cb = null)
        {
            this.name = name;
            this.textureName = texture;
            this.position = position;
            textureMan = tm;
            setOrigin(RotationOrigin.CENTER);
            spriteMan = sm;

            callBack = cb;

            audio = AudioManager.getInstance();

            this.texture = tm.getTexture(texture);
        }        
        
        public void followMouse()
        {
            setPosition(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
        }

        //Speed = Up,Down,Left,Right
        public void moveOnKeyPress(Vector4 speed, bool WASD = true)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Up) ||
                (Keyboard.GetState().IsKeyDown(Keys.W) && WASD))
            {
                move(new Vector2(0.0f, speed.X));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down) ||
                (Keyboard.GetState().IsKeyDown(Keys.S) && WASD))
            {
                move(new Vector2(0.0f, speed.Y));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) ||
                (Keyboard.GetState().IsKeyDown(Keys.A) && WASD))
            {
                move(new Vector2(speed.Z, 0.0f));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right) ||
                (Keyboard.GetState().IsKeyDown(Keys.D) && WASD))
            {
                move(new Vector2(speed.W, 0.0f));
            }
        }

        public bool isColliding(String name)
        {
            Sprite other = spriteMan.getSprite(name);
            if (other != null)
            {
                if (Math.Abs(this.position.X - other.position.X) < this.textureMan.getTexture(textureName).Width &&
                    Math.Abs(this.position.Y - other.position.Y) < this.textureMan.getTexture(textureName).Height)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public void setPosition(Vector2 position)
        {
            this.position = position;
        }

        public void move(Vector2 speed)
        {
            this.position += speed;
        }

        public void rotate(float amount)
        {
            rotation += amount;
            if (rotation > (2.0f * (float)Math.PI))
            {
                rotation -= (2.0f * (float)Math.PI);
            }
            if (rotation < 0.0f)
            {
                rotation += (2.0f * (float)Math.PI);
            }
        }

        public void setOrigin(RotationOrigin o)
        {
            Texture2D texture = textureMan.getTexture(textureName);
            switch (o)
            {
                case RotationOrigin.TOP_LEFT:
                    origin = new Vector2(0.0f, 0.0f);
                    break;
                case RotationOrigin.TOP_MIDDLE:
                    origin = new Vector2((float)texture.Width / 2.0f, 0.0f);
                    break;
                case RotationOrigin.TOP_RIGHT:
                    origin = new Vector2((float)texture.Width, 0.0f);
                    break;
                case RotationOrigin.CENTER_LEFT:
                    origin = new Vector2(0.0f, (float)texture.Height / 2.0f);
                    break;
                case RotationOrigin.CENTER:
                    origin = new Vector2((float)texture.Width / 2.0f, (float)texture.Height / 2.0f);
                    break;
                case RotationOrigin.CENTER_RIGHT:
                    origin = new Vector2((float)texture.Width, (float)texture.Height / 2.0f);
                    break;
                case RotationOrigin.BOTTOM_LEFT:
                    origin = new Vector2(0.0f, (float)texture.Height);
                    break;
                case RotationOrigin.BOTTOM_MIDDLE:
                    origin = new Vector2((float)texture.Width / 2.0f, (float)texture.Height);
                    break;
                case RotationOrigin.BOTTOM_RIGHT:
                    origin = new Vector2((float)texture.Width, (float)texture.Height);
                    break;
            }
        }

        public bool isClicked()
        {
            if (clickable == false)
                return false;

            Rectangle r = new Rectangle((int)position.X - texture.Width / 2, 
                (int)position.Y - texture.Height / 2, 
                texture.Width, texture.Height);

            bool result = (r.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed);

            if (result && callBack != null)
                callBack();

            return result;
        }

        private void onClick(MouseState ms)
        {
            if (pressed == false)
            {
                Rectangle r = new Rectangle((int)position.X - texture.Width / 2,
                    (int)position.Y - texture.Height / 2,
                    texture.Width, texture.Height);

                bool result = (r.Contains(ms.X, ms.Y) &&
                    ms.LeftButton == ButtonState.Pressed);

                if (result == true)
                {
                    pressed = true;
                    audio.Play2DSound("btn_down", false);
                }
            }
        }

        private void onHover(MouseState ms)
        {
            Rectangle r = new Rectangle((int)position.X - texture.Width / 2,
                    (int)position.Y - texture.Height / 2,
                    texture.Width, texture.Height);

            bool result = r.Contains(ms.X, ms.Y);

            if (result == false && pressed == true)
                pressed = false;
        }

        private void onRelease(MouseState ms)
        {
            if (pressed == true)
            {
                Rectangle r = new Rectangle((int)position.X - texture.Width / 2,
                    (int)position.Y - texture.Height / 2,
                    texture.Width, texture.Height);

                bool result = (r.Contains(ms.X, ms.Y) &&
                    ms.LeftButton == ButtonState.Released);

                if (result && callBack != null)
                {
                    callBack();
                    audio.Play2DSound("btn_up", false);
                    pressed = false;
                }
            }
        }

        public void draw(SpriteBatch sb)
        {
            if (this.visible == false)
                return;

            Texture2D texture = textureMan.getTexture(textureName);
            sb.Draw(texture, position, null, 
                Color.FromNonPremultiplied(tint.R, tint.G, tint.B, opacity), 
                rotation, origin, scale, SpriteEffects.None, 0);
        }

        public virtual void update(GameTime time)
        {
            if (clickable == false)
                return;

            MouseState ms = Mouse.GetState();
            onClick(ms);
            onHover(ms);
            onRelease(ms);
            
        }


        #region Mutators

        public Vector2 Position
        {
            set { position = value; }
            get { return position; }
        }
        public int Opacity
        {
            set { opacity = value; }
            get { return opacity; }
        }
        public Texture2D Texture
        {
            get { return texture; }
        }
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }
        public bool Clickable
        {
            get { return this.clickable; }
            set { this.clickable = value; }
        }
        #endregion Mutators
    }
}
