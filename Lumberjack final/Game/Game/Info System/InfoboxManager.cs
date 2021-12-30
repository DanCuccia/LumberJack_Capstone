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
    public class InfoboxManager
    {
        List<Infobox> data;

        public InfoboxManager()
        {
            data = new List<Infobox>();
        }

        public void Add(InfoboxSetup setup)
        {
            Infobox temp = new Infobox(setup);

            data.Add(temp);
        }

        public void Add(Infobox i)
        {
            data.Add(i);
        }

        public int Count()
        {
            return data.Count;
        }

        public void Update(GameTime time)
        {
            List<Infobox> remove = new List<Infobox>();

            foreach (Infobox i in data)
            {
                i.update(time);
                if (!i.IsActive())
                {
                    remove.Add(i);
                }
            }
            foreach (Infobox i in remove)
            {
                data.Remove(i);
            }
        }

        public Infobox Get(String name)
        {
            foreach (Infobox i in data)
            {
                if (i.GetName() == name)
                {
                    return i;
                }
            }

            return null;
        }

        public void Draw(SpriteBatch renderer)
        {
            foreach (Infobox i in data)
            {
                if (i.IsVisible())
                {
                    i.draw(renderer);
                }
            }
        }
    }
}
