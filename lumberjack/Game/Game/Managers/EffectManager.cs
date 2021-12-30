using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Managers
{
    /// <summary>
    /// A wrapped effect with an ID (RenderEffect enum), used in the manager
    /// </summary>
    public class BEffect
    {
        RenderEffect m_type;
        Effect m_effect;

        public RenderEffect Name
        {
            get { return m_type; }
            set { m_type = value; }
        }
        public Effect Effect
        {
            get { return m_effect; }
            set { m_effect = value; }
        }
    }

    /// <summary>
    /// Manages all loaded effects, used in conjuction with Renderer
    /// </summary>
    public class EffectManager
    {

        List<BEffect> m_fxList = new List<BEffect>();

        /// <summary>
        /// null CTOR
        /// </summary>
        public EffectManager() { }

        /// <summary>
        /// Get a loaded Effect from the list
        /// </summary>
        /// <param name="fx">which effect your looking for</param>
        /// <returns>the loaded ready-to-use effect</returns>
        public Effect GetEffect(RenderEffect fx)
        {
            foreach (BEffect effect in m_fxList)
            {
                if (effect.Name == fx)
                    return effect.Effect;
            }
            return null;
        }

        /// <summary>
        /// An an effect to the list
        /// </summary>
        /// <param name="loadedEffect">the loaded effect from content</param>
        /// <param name="type">the renderType enum </param>
        public void AddEffect(Effect loadedEffect, RenderEffect type)
        {
            if (loadedEffect == null)
                throw new NullReferenceException("Manager_Effect::AddEffect: loadedEffect is null");
            BEffect t = new BEffect();
            t.Effect = loadedEffect;
            t.Name = type;
            m_fxList.Add(t);
        }

        /// <summary>
        /// Purge all loaded effects
        /// </summary>
        public void ClearAll()
        {
            m_fxList.Clear();
        }

    }

}
