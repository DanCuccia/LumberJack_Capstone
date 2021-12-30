using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Game.Drawing_Objects;

namespace Game.Game_Objects
{
    /// <summary>These are the values that are stored in xml</summary>
    [Serializable]
    public class TriggerXmlMedium
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public int id;
        public bool repeatable;
        public bool hasTriggered;
    }

    /// <summary>The main Trigger Class</summary>
    public class Trigger
    {
        int id = -1;

        StaticMesh model;

        bool hasTriggered = false;
        bool repeatable = false;

        public delegate void CallBack();
        CallBack triggerCallback;

        /// <summary>Default CTOR</summary>
        public Trigger()
        {
            model = new StaticMesh();
        }

        /// <summary>Initialize member variables</summary>
        public void Initialize(ContentManager content)
        {
            model.Initialize(content, "models\\border", "textures\\trigger");
            model.GenerateBoundingBox();
        }

        /// <summary> Draw the trigger, only if we're drawing in development mode</summary>
        public void Draw()
        {
            if (Game1.drawDevelopment == false)
                return;
            model.Draw();
        }

        /// <summary>Execute the trigger's callback</summary>
        public void Execute()
        {
            if (hasTriggered == false && triggerCallback != null)
            {
                triggerCallback();
                if (repeatable == false)
                    hasTriggered = true;
            }
        }

        /// <summary>create the xml medium class used for serialization</summary>
        /// <returns>a compact class ready for xml writing</returns>
        public TriggerXmlMedium GetXmlMedium()
        {
            TriggerXmlMedium output = new TriggerXmlMedium();

            output.id = this.id;
            output.hasTriggered = this.hasTriggered;
            output.repeatable = this.repeatable;
            output.position = model.Position;
            output.rotation = model.Rotation;
            output.scale = model.Scale;

            return output;
        }

        #region Mutators
        //public BoundingBox BoundingBox
        //{
        //    get 
        //    {
        //        model.UpdateBoundingBox();
        //        return model.BoundingBox; 
        //    }
        //}
        public StaticMesh Model
        {
            get { return this.model; }
        }
        public int ID
        {
            set { id = value; }
            get { return id; }
        }
        public bool HasTriggered
        {
            set { hasTriggered = value; }
            get { return hasTriggered; }
        }
        public bool Repeatable
        {
            set { repeatable = value; }
            get { return repeatable; }
        }
        public CallBack TriggerCallback
        {
            set { triggerCallback = value; }
        }
        #endregion Mutators
    }
}
