using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Math_Physics;
using Game.Drawing_Objects;

namespace Game.Game_Objects.Build_System
{
    [Serializable]
    public class BuildableObjectXmlMedium
    {
        public Vector3 Origin;
        public Vector3 WorldPosition;
        public Vector3 Rotation;
        public string ModelFilepath;
        public int Type;
        
    }

    public enum PrimitiveType
    {
        Block,
        Plank,
        Rod,
        Disk,
    }

    public class BuildableObject : StaticMesh
    {
        public Point3 origin;
        public Point3 rotationValue = new Point3(0,0,0);
        public PrimitiveType type;

        public BuildableObject(Vector3 position, Point3 origin, BuildableObject creator, bool hasTexCoords = true)
            : base()
        {
            base.Initialize(creator.Model, creator.m_modelFilepath, RenderTechnique.RT_WOOD);

            this.origin = origin;

            if (creator != null)
            {
                this.rotationValue = creator.rotationValue;
                this.Rotation = creator.Rotation;
            }

            type = creator.type;

            this.Position = position;
            GenerateBoundingBox(hasTexCoords);
            UpdateBoundingBox();
        }

        public BuildableObject(Model model, string modelFilepath, Vector3 position, Point3 origin, PrimitiveType type, bool hasTexCoords = true)
            : base()
        {
            base.Initialize(model, modelFilepath, RenderTechnique.RT_WOOD);

            this.origin = origin;
            this.rotationValue = new Point3(0, 0, 0);
            this.type = type;
            this.Position = position;
            GenerateBoundingBox(hasTexCoords);
            UpdateBoundingBox();
        }

        public bool isInterceptedBy(BuildableObject other)
        {
            if (this.origin.x == other.origin.x)
            {
                if (this.origin.z == other.origin.z)
                {
                    if (this.origin.y == other.origin.y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Get the serializable class of this completed object</summary>
        /// <returns>fully constructed serializable class</returns>
        public BuildableObjectXmlMedium getXmlMedium()
        {
            BuildableObjectXmlMedium output = new BuildableObjectXmlMedium();

            Vector3 temp = Vector3.Zero;

            temp.X = origin.x;
            temp.Y = origin.y;
            temp.Z = origin.z;
            output.Origin = temp;

            temp.X = rotationValue.x;
            temp.Y = rotationValue.y;
            temp.Z = rotationValue.z;
            output.Rotation = temp;

            output.ModelFilepath = this.m_modelFilepath;
            output.Type = (int)this.type;
            output.WorldPosition = this.Position;

            return output;
        }

    }
}
