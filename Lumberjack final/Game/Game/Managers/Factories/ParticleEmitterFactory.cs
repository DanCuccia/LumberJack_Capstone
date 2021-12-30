using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Game.Drawing_Objects;
using Game.Game_Objects;

namespace Game.Managers.Factories
{
    class ParticleEmitterFactory : PropFactory
    {
        GraphicsDevice device;

        /// <summary>Default CTOR</summary>
        public ParticleEmitterFactory(GraphicsDevice device)
        {
            this.device = device;
        }

        /// <summary>ParticleEmitterFactory is a special factory that makes objects that doesn't extend Prop,
        /// DO NOT USE, call "GetCompleteEmitter()" instead</summary>
        public override Prop getProp(ContentManager content, PropXMLStruct2 xmlInput)
        {throw new NotImplementedException();}

        public BillboardParticleEmitter GetCompleteEmitter(ContentManager content, ParticleEmitterXmlMedium xmlInput)
        {
            BillboardParticleEmitter emitter = new BillboardParticleEmitter(device);

            //emitter.Initialize(tex, ParticleType.PTL_LEAVES, ParticleImage.PTL_IMG_LEAF);
            //emitter.Initialize(content, ParticleImage.PTL_IMG_LEAF);
            emitter.Initialize(content, (ParticleImage)xmlInput.TexID);
            emitter.Position = xmlInput.Position;

            emitter.Parameters.FadeForMillies = xmlInput.FadeForMillies;
            emitter.Parameters.Frequency = xmlInput.Frequency;
            emitter.Parameters.GrowForMillies = xmlInput.GrowForMillies;
            emitter.Parameters.MaxDistance = xmlInput.MaxDistance;
            emitter.Parameters.MaxLife = xmlInput.MaxLife;
            emitter.Parameters.MaxParticles = xmlInput.MaxParticles;
            emitter.Parameters.ShowForMillies = xmlInput.ShowForMillies;
            emitter.Parameters.ShrinkForMillies = xmlInput.ShrinkForMillies;
            emitter.Parameters.SpawnRadius = xmlInput.SpawnRadius;
            emitter.Parameters.Speed = xmlInput.Speed;
            emitter.Parameters.SpeedVariance = xmlInput.SpeedVariance;
            emitter.Parameters.Spread = xmlInput.Spread;

            return emitter;
        }
    }
}
