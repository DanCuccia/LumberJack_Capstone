using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Game.Drawing_Objects;
using Game.Game_Objects;

namespace Game.Managers.Factories
{
    class WaterFactory : PropFactory
    {
        public WaterFactory() { }

        /// <summary>This Factory is used by WorldManager,
        /// when loading the WaterList from xml</summary>
        /// <param name="input">intermediate structure from xml</param>
        /// <returns>completed water object</returns>
        public WaterVolume GetCompleteWaterVolume(ContentManager content, GraphicsDevice device, WaterXMLStruct input)
        {
            WaterVolume water = new WaterVolume(device);
            water.Initialize(content, input.vertexDistanceX, input.vertexdistanceZ);

            water.Parameters.TextureScale = input.textureScale;
            water.Parameters.BumpSpeed = input.bumpSpeed;
            water.Parameters.DeepColor = input.deepColor;
            water.Parameters.ShallowColor = input.shallowColor;
            water.Position = input.position;
            water.Rotation = input.rotation;

            return water;
        }

        /// <summary>WaterFactory is special, Water does not extend Prop,
        /// Call "GetCompleteWaterVolume()" instead</summary>
        public override Prop getProp(ContentManager content, PropXMLStruct xmlInput)
        {throw new NotImplementedException();}
    }
}
