using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Game.Managers;

namespace Game.Game_Objects.Build_System
{
    [Serializable]
    public class CompleteObjXmlMedium
    {
        public Vector3      Position;
        public Vector3      Stride;
        public List<BuildableObjectXmlMedium> BuildableObjectList;
        public int          Blocks;
        public int          Planks;
        public int          Rods;
        public int          Disks;
        public Vector3      MajorScale;
        public string       Name;
        public int          AvailableInstances;
        public Vector3      Rotation;
    }

    public class CompleteObject
    {
        public List<BuildableObject> objects;
        public List<BuildableObject> PiecesList
        {
            get { return objects; }
        }
        public Vector3 position = Vector3.Zero;
        public Vector3 stride = Vector3.Zero;
        public int availableInstances = 0;
        public String name = "<<Empty>>";
        public int blocks;
        public int planks;
        public int rods;
        public int disks;
        public Vector3 majorScale = Vector3.One;
        public Vector3 Rotation { get; set; }
        

        public CompleteObject(CompleteObject other)
        {
            objects = new List<BuildableObject>();
            Rotation = other.Rotation;
            this.name = other.name;

            foreach (BuildableObject b in other.objects)
            {
                bool hasTexCoords = true;
                hasTexCoords = (b.type != PrimitiveType.Rod);
                
                BuildableObject temp = new BuildableObject(b.Model, b.m_modelFilepath, b.Position, b.origin, b.type, hasTexCoords);
                temp.Position = b.Position;
                temp.Rotation = b.Rotation;
                objects.Add(temp);
            }
        }

        public CompleteObject()
        {
            objects = new List<BuildableObject>();
            Rotation = Vector3.Zero;
        }

        public CompleteObject(Vector3 stride)
        {
            objects = new List<BuildableObject>();
            this.stride = stride;
            Rotation = Vector3.Zero;
        }

        public void ChangeMajorScale(Vector3 scale)
        {
            majorScale = scale;
            stride *= majorScale;

            foreach (BuildableObject b in objects)
            {
                b.Scale *= majorScale;
            }
        }

        public void addObject(BuildableObject obj)
        {
            objects.Add(obj);
        }

        public void Move(Vector3 distance)
        {
            position += distance;
        }

        public void AddRotation(Vector3 amount)
        {
            Rotation += amount;
            foreach (BuildableObject b in objects)
            {
                Vector3 bPos = stride;

                bPos.X *= b.origin.x;
                bPos.Y *= 0f;
                bPos.Z *= b.origin.z;

                float distance = Vector3.Distance(bPos, Vector3.Zero);

                Vector3 newPosition = Vector3.Zero;

                newPosition.X = distance * (float)Math.Cos(MathHelper.ToRadians(Rotation.Y) + Math.Atan2(b.origin.z, b.origin.x));
                newPosition.Z = distance * (float)Math.Sin(MathHelper.ToRadians(Rotation.Y) + Math.Atan2(b.origin.z, b.origin.x));
                newPosition.Y = b.origin.y * stride.Y;

                b.Position = newPosition + position;
                b.AddRotation = -amount;
            }
        }

        public void SetRotation(Vector3 amount)
        {
            foreach (BuildableObject b in objects)
            {
                b.Rotation = amount;
            }
        }

        public void Recycle(Player p)
        {
            //Return the value of all of the objects
            // that compose this completeobject to the players
            // lumber count
            p.Inventory.blocks += blocks;
            p.Inventory.planks += planks;
            p.Inventory.rods += rods;
            p.Inventory.disks += disks;
        }

        public void update(GameTime time)
        {
            foreach (BuildableObject o in objects)
            {
                //o.update(time);

                //Vector3 temp = new Vector3(o.origin.x, o.origin.y, o.origin.z);

                //o.Position = this.position + (temp * stride);
            }
        }

        public void render()
        {
            foreach (BuildableObject o in objects)
            {
                o.Draw();
            }
        }

        public void drawOBB()
        {
            foreach (BuildableObject o in objects)
                o.DrawOBB();
        }

        /// <summary>Get the serializable class of this completed object</summary>
        /// <returns>fully contructed serializable completed object</returns>
        public CompleteObjXmlMedium getXmlMedium()
        {
            CompleteObjXmlMedium output = new CompleteObjXmlMedium();

            output.Position =   this.position;
            output.Stride =     this.stride;
            output.AvailableInstances = this.availableInstances;
            output.Blocks =     this.blocks;
            output.Disks =      this.disks;
            output.MajorScale = this.majorScale;
            output.Name =       this.name;
            output.Planks =     this.planks;
            output.Rods =       this.rods;
            output.Rotation =   this.Rotation;

            List<BuildableObjectXmlMedium> buildObjList = new List<BuildableObjectXmlMedium>();
            foreach (BuildableObject obj in objects)
            {
                buildObjList.Add(obj.getXmlMedium());
            }
            output.BuildableObjectList = buildObjList;

            return output;
        }

    }
}
