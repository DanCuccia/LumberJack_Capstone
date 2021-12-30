using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

using Game.Game_Objects;
using Game.Math_Physics;

namespace Game.Managers.Factories
{
    /// <summary>
    /// Used in conjunction with loading from xml
    /// LogicProps are special, and need to save extra parameters which PropXMLStruct2 cannot supply,
    /// do not use the overriden getProp(), use GetCompleteSpecialProp() instead
    /// </summary>
    class LogicPropFactory : PropFactory
    {
        /// <summary>Do Not Call, use GetCompleteSpecialProp()</summary>
        public override Prop getProp(ContentManager content, PropXMLStruct2 xmlInput)
        { throw new NotImplementedException("Do Not Call, use GetCompleteSpecialProp()"); }

        /// <summary>Get a complete Special prop from xml</summary>
        /// <returns>fully constructed special prop</returns>
        public LogicProp GetCompleteSpecialProp(ContentManager content, LogicPropXmlMedium xmlInput)
        {
            LogicProp output = null;

            switch (xmlInput.id)
            {
                case GameIDList.LogicProp_RiverDam:
                    output = new RiverDam(content);
                    break;
                case GameIDList.LogicProp_BrokenHouse:
                    output = new BrokenHouse(content);
                    break;
                case GameIDList.LogicProp_BoatDock:
                    output = new BoatDock(content);
                    break;
                case GameIDList.LogicProp_Fence:
                    output = new Fence(content);
                    break;
                case GameIDList.LogicProp_BoatA:
                    output = new BoatA(content);
                    break;
                case GameIDList.LogicProp_BoatB:
                    output = new BoatB(content);
                    break;
                case GameIDList.LogicProp_Cabbage:
                    output = new Cabbage(content);
                    break;
            }

            if (output != null)
            {
                output.ID = xmlInput.id;
                output.Position = xmlInput.position;
                output.Rotation = xmlInput.rotation;
                output.Scale = xmlInput.scale;
                output.Enabled = xmlInput.enabled;
                output.GetBoundingModel().UpdateBoundingBox();
            }

            return output;
        }
    }
}
