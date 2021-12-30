using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Game.Game_Objects;
using Game.Managers;

namespace Game
{

    public struct InfoboxSetup
    {
        public String name;
        public String text;
        public Vector2 position;
        public Vector2 targetPosition;
        public Color backgroundColor;
        public Color foregroundColor;
        public Vector2 dimensions;
        public String fontName;
        public bool isVisible;
        public double startTime;
        public double duration;
    }

    public enum Direction
    {
        DIR_UP,
        DIR_DOWN,
        DIR_LEFT,
        DIR_RIGHT
    }

    public class Infobox
    {
        protected String name;
        protected String text;
        protected Vector2 position;
        protected Vector2 targetPosition;
        protected Color backgroundColor;
        protected Color foregroundColor;
        protected Vector2 dimensions;
        protected double startTime;
        protected double duration;

        protected bool isActive;
        protected bool isVisible;

        protected SpriteFont font;

        public static Texture2D background;
        public static ContentManager content;

        public Infobox()
        {
        }
        public Infobox(InfoboxSetup ibs)
        {
            this.text = ibs.text;
            this.position = ibs.position;
            this.targetPosition = ibs.targetPosition;
            this.backgroundColor = ibs.backgroundColor;
            this.foregroundColor = ibs.foregroundColor;
            this.dimensions = ibs.dimensions;
            this.isVisible = ibs.isVisible;
            this.isActive = true;
            this.font = content.Load<SpriteFont>(ibs.fontName);
            this.startTime = ibs.startTime;
            this.duration = ibs.duration;
            this.name = ibs.name;
            if (background == null)
            {
                background = content.Load<Texture2D>("sprites\\tooltipBack");
            }
        }

        public static InfoboxSetup MakeMessage(String message, String name, double duration, GameTime time, float startHeight = 0, bool urgent = false )
        {
            InfoboxSetup ibs = new InfoboxSetup();

            ibs.backgroundColor = Color.Black;
            ibs.foregroundColor = Color.BlanchedAlmond;
            ibs.fontName = "fonts\\pericles";
            ibs.isVisible = true;
            ibs.dimensions = new Vector2(1000, 24);

            ibs.startTime = time.TotalGameTime.TotalMilliseconds;
            ibs.duration = duration;
            ibs.text = message;
            ibs.position = new Vector2(1500, startHeight);
            ibs.targetPosition = new Vector2(600, startHeight);
            ibs.name = name;

            return ibs;
        }

        public void SetText(String text)
        {
            this.text = text;
        }

        public String GetName()
        {
            return name;
        }

        public bool IsClicked()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Vector2 mp = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                if (mp.X > position.X &&
                    mp.X < position.X + dimensions.X)
                {
                    if (mp.Y > position.Y &&
                        mp.Y < position.Y + dimensions.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void move(Direction d, float speed)
        {
            switch (d)
            {
                case Direction.DIR_UP:
                    if (position.Y > targetPosition.Y)
                    {
                        position.Y -= speed;
                    }
                    break;
                case Direction.DIR_DOWN:
                    if (position.Y < targetPosition.Y)
                    {
                        position.Y += speed;
                    }
                    break;
                case Direction.DIR_LEFT:
                    if (position.X > targetPosition.X)
                    {
                        position.X -= speed;
                    }
                    break;
                case Direction.DIR_RIGHT:
                    if (position.X < targetPosition.X)
                    {
                        position.X += speed;
                    }
                    break;
            }

        }

        public void setPosition(Vector2 position)
        {
            this.position = position;
        }

        public void show()
        {
            isVisible = true;
        }

        public void hide()
        {
            isVisible = false;
        }

        public void activate()
        {
            isActive = true;
        }

        public void kill()
        {
            isActive = false;
        }

        public bool IsActive()
        {
            return isActive;
        }

        public bool IsVisible()
        {
            return isVisible;
        }

        public virtual void update(GameTime time)
        {
            //Default behavior for tooltips
            if (time.TotalGameTime.TotalMilliseconds < (startTime + duration))
            {
                move(Direction.DIR_LEFT, 10.0f);
            }
            else
            {
                kill();
            }
        }

        public void draw(SpriteBatch renderer)
        {
            renderer.Draw(background, new Rectangle((int)position.X - 2, (int)position.Y - 2, (int)dimensions.X, (int)dimensions.Y), backgroundColor);
            renderer.DrawString(font, text, position, foregroundColor);
        }

    }
}
