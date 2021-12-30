using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Game.Managers;
using Game.Math_Physics;

namespace Game.Drawing_Objects
{
    /// <summary>Vertex declaration of a billboard sprite
    /// Note: -variables are combined for cleaner hlsl memory allocation
    ///       -it may look ugly, I'm going for performance as much as possible</summary>
    public struct BillboardParticleVertex : IVertexType
    {
        public Vector3  Position;
        public Vector4  TexCoordScale;
        public float    Alpha;

        public static int SizeInBytes = (3 + 2 + 2 + 1) * 4;
        public readonly static VertexDeclaration VertexDeclaration
            = new VertexDeclaration(
                 new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                 new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
                 new VertexElement((sizeof(float) * (3+4)), VertexElementFormat.Single, VertexElementUsage.BlendWeight, 0)
             );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    /// <summary>What Type of Particle Emitter, governs Update</summary>
    public enum ParticleType
    {
        PTL_LEAVES,
        PTL_WATER
    }

    /// <summary>
    /// for xml serialization, we need to know which image to load, 
    /// this determines that.
    /// </summary>
    public enum ParticleImage
    {
        PTL_IMG_LEAF = 0,
        PTL_IMG_WATER = 1,
        BLANK
    }

    /// <summary>Individual Particle, only contains information needed for
    /// the emitter to govern</summary>
    public class Particle
    {
        public Vector3 m_position;
        public Vector3 m_velocity = Vector3.Zero;
        public Vector3 m_forcedVelocity = Vector3.Zero;
        public int m_currentLife = 0;
    }
    

    /// <summary>Encapsulates all settings for a 
    /// billboard particle emitter</summary>
    public class EmitterSettings
    {
        public int      MaxParticles;
        public float    MaxDistance;
        public int      MaxLife;
        public float    Spread;
        public Vector3  SpawnRadius;
        public int      Frequency;
        public float    GrowForMillies;
        public float    ShrinkForMillies;
        public float    ShowForMillies;
        public float    FadeForMillies;
        public Vector3  Speed;
        public float    SpeedVariance;

        /// <summary>Default Emitter Settings</summary>
        public EmitterSettings()
        {
            MaxParticles = 500;
            MaxDistance = 1000;
            MaxLife = 5000;
            Spread = .01f;
            SpawnRadius = new Vector3(20, 20, 20);
            Frequency = 25;
            GrowForMillies = 10;
            ShrinkForMillies = 10;
            ShowForMillies = 0;
            FadeForMillies = 0;
            Speed = new Vector3(0f, -1.5f, 0f);
            SpeedVariance = .5f;
        }
    }

    /// <summary>The force class takes care of "wind" effects</summary>
    public class Force
    {
        public Vector3 m_position;
        public Vector3 m_velocity = Vector3.Zero;
        public Vector3 m_diameters = new Vector3(10, 10, 10);
    }

    /// <summary>this class gets serialized</summary>
    public class ParticleEmitterXmlMedium
    {
        public int          TexID;
        public Vector3      Position;
        public int          MaxParticles;
        public float        MaxDistance;
        public int          MaxLife;
        public float        Spread;
        public Vector3      SpawnRadius;
        public int          Frequency;
        public float        GrowForMillies;
        public float        ShrinkForMillies;
        public float        ShowForMillies;
        public float        FadeForMillies;
        public Vector3      Speed;
        public float        SpeedVariance;
    }

    /// <summary>The main user-end Emitter class</summary>
    public class BillboardParticleEmitter
    {
        #region Member Variables

        GraphicsDevice          m_device;   //ref
        Random                  m_random;   //ref

        Vector3                 m_emissionPosition      = Vector3.Zero;
        List<Particle>          m_particles             = new List<Particle>();
        List<Force>             m_forces                = new List<Force>();

        EmitterSettings         m_parameters            = new EmitterSettings();

        int                     m_currentSpawnTime      = 0;

        ParticleImage           m_textureValue          = ParticleImage.BLANK;
        OrientedBoundingBox     m_obb;
        BoundingBoxModel        m_obbModel;

        StaticMesh              particleModel;

        #endregion Member Variables


        #region Initialization


        /// <summary>Default CTOR -- must call initialize()</summary>
        /// <param name="device">ref to the game's grphics device manager, 
        /// used for vertBuffer and indexBuffer</param>
        public BillboardParticleEmitter(GraphicsDevice device)
        {
            m_random = Game1.random;
            m_device = device;
            m_obb = new OrientedBoundingBox(
                new Vector3(-m_parameters.SpawnRadius.X, -m_parameters.SpawnRadius.Y, -m_parameters.SpawnRadius.Z),
                new Vector3(m_parameters.SpawnRadius.X, m_parameters.SpawnRadius.Y, m_parameters.SpawnRadius.Z) );
            m_obbModel = new BoundingBoxModel(device);
        }

        /// <summary>the spawning box can be drawn with an obb (only used for visuals, no physics)</summary>
        public void UpdateOBB()
        {
            m_obb = new OrientedBoundingBox(
                new Vector3(-m_parameters.SpawnRadius.X, -m_parameters.SpawnRadius.Y, -m_parameters.SpawnRadius.Z),
                new Vector3(m_parameters.SpawnRadius.X, m_parameters.SpawnRadius.Y, m_parameters.SpawnRadius.Z));
            m_obb.World = Matrix.Identity * Matrix.CreateTranslation(m_emissionPosition);
        }

        /// <summary>Converted from billboards to models, init all member variables for model particle emitter</summary>
        public void Initialize(ContentManager content, ParticleImage imageValue)
        {
            switch (imageValue)
            {
                case ParticleImage.PTL_IMG_LEAF:
                    particleModel = new StaticMesh();
                    particleModel.Initialize(content, "models\\leaf", MyColors.TreeTopGreen);
                    m_textureValue = ParticleImage.PTL_IMG_LEAF;
                    break;
                case ParticleImage.PTL_IMG_WATER:
                    particleModel = new StaticMesh();
                    particleModel.Initialize(content, "models\\waterSpray", MyColors.WaterSprayBlue);
                    m_textureValue = ParticleImage.PTL_IMG_WATER;
                    break;
            }

            SpawnParticle();
        }

        /// <summary>Use this to add a possible force into the list,
        /// the particle must be within it's diameters for the force to effect it</summary>
        /// <param name="force">the force to add</param>
        public void AddPossibleForce(Force force)
        {
            m_forces.Add(force);
        }

        /// <summary>Load an entire settings set through here</summary>
        /// <param name="settings">the fully made settings class</param>
        public void ReceieveSettings(EmitterSettings settings)
        {
            m_parameters = settings;
        }

        #endregion Initialization


        #region API

        /// <summary> All movement logic in here</summary>
        /// <param name="gameTime">current game time data</param>
        public void Animate(GameTime gameTime)
        {
            foreach (Particle ptl in m_particles)
            {
                ptl.m_position += ptl.m_velocity;
                ptl.m_position += ptl.m_forcedVelocity;

                if (m_forces.Count != 0)
                {
                    foreach (Force force in m_forces)
                    {
                        if (PtlWithinForce(ptl.m_position, force))
                            ptl.m_forcedVelocity += force.m_velocity;
                        else
                        {
                            if (ptl.m_forcedVelocity.X != 0)
                                ptl.m_forcedVelocity.X -= 0.001f;
                            else ptl.m_forcedVelocity.X = 0f;
                            if (ptl.m_forcedVelocity.Y != 0)
                                ptl.m_forcedVelocity.Y -= 0.001f;
                            else ptl.m_forcedVelocity.Y = 0f;
                            if (ptl.m_forcedVelocity.Z != 0)
                                ptl.m_forcedVelocity.Z -= 0.001f;
                            else ptl.m_forcedVelocity.Z = 0f;
                        }
                    }
                }
            }
        }

        /// <summary> Any non-movement logic in here</summary>
        /// <param name="gameTime">current game time data</param>
        public void Update(GameTime gameTime)
        {
            foreach (Particle ptl in m_particles)
                ptl.m_currentLife += gameTime.ElapsedGameTime.Milliseconds;

            UpdateNewAndOld(gameTime);

            //must be called last!
            //if(m_particles.Count != 0)
            //CycleBillboards();
        }

        /// <summary>Main draw call</summary>
        public void Draw()
        {
            if (Renderer.m_renderPhase == RenderPhase.PHASE_DEPTH && 
                this.m_textureValue == ParticleImage.PTL_IMG_WATER)
                return;

            foreach (Particle ptl in m_particles)
            {
                particleModel.Position = ptl.m_position;
                particleModel.RotationX = ptl.m_currentLife * ptl.m_velocity.Y;
                particleModel.RotationY = ptl.m_currentLife * ptl.m_velocity.Y;
                particleModel.Scale = new Vector3(this.DetermineScale(ptl));
                particleModel.Draw();
            }
        }

        public void DrawOBB()
        {
            if (Game1.drawDevelopment)
            {
                m_obbModel.Draw(m_obb, MyColors.LightBlue, MyColors.LightBlue);
            }
        }
        
        /// <summary>rasterize the verts</summary>
        //private void FeedMesh()
        //{
        //    m_device.SetVertexBuffer(m_vertexBuffer);
        //    m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, m_vertexBuffer.VertexCount / 3);
            
        //    m_device.BlendState = BlendState.Opaque;
        //    m_device.DepthStencilState = DepthStencilState.Default;
        //}

        public ParticleEmitterXmlMedium GetXmlMedium()
        {
            ParticleEmitterXmlMedium output = new ParticleEmitterXmlMedium();

            output.TexID            = (int)this.m_textureValue;
            output.Position         = this.Position;
            output.FadeForMillies   = this.Parameters.FadeForMillies;
            output.Frequency        = this.Parameters.Frequency;
            output.GrowForMillies   = this.Parameters.GrowForMillies;
            output.MaxDistance      = this.Parameters.MaxDistance;
            output.MaxLife          = this.Parameters.MaxLife;
            output.MaxParticles     = this.Parameters.MaxParticles;
            output.ShowForMillies   = this.Parameters.ShowForMillies;
            output.ShrinkForMillies = this.Parameters.ShrinkForMillies;
            output.SpawnRadius      = this.Parameters.SpawnRadius;
            output.Speed            = this.Parameters.Speed;
            output.SpeedVariance    = this.Parameters.SpeedVariance;
            output.Spread           = this.Parameters.Spread;

            return output;
        }

        #endregion API


        #region Private Parts

        /// <summary>Removal and Creation logic in here</summary>
        private void UpdateNewAndOld(GameTime gameTime)
        {
            //spawn new ones
            m_currentSpawnTime += gameTime.ElapsedGameTime.Milliseconds;
            if (m_currentSpawnTime > m_parameters.Frequency)
            {
                m_currentSpawnTime -= m_currentSpawnTime;
                SpawnParticle();
            }

            //kill old ones
            List<int> remInd = new List<int>();
            int index = 0;
            foreach (Particle ptl in m_particles)
            {
                //check distance
                if (Vector3.Distance(ptl.m_position, m_emissionPosition) > m_parameters.MaxDistance)
                    remInd.Add(index);
                //check life
                if (ptl.m_currentLife > m_parameters.MaxLife)
                    remInd.Add(index);

                index++;
            }
            for (int i = remInd.Count - 1; i >= 0; i--)
                m_particles.RemoveAt(remInd[i]);
            //foreach (int i in remInd)
            //    m_particles.RemoveAt(i);
        }

        /// <summary>Spawns a new particle within the diameter of the emitter,
        /// position and velocity is randomized within the given parameters</summary>
        private void SpawnParticle()
        {
            if (m_particles.Count >= m_parameters.MaxParticles)
                return;

            Vector3 pos = new Vector3(
                (float)MathHelper.Lerp(m_emissionPosition.X - m_parameters.SpawnRadius.X, m_emissionPosition.X + m_parameters.SpawnRadius.X, (float)m_random.NextDouble()),
                (float)MathHelper.Lerp(m_emissionPosition.Y - m_parameters.SpawnRadius.Y, m_emissionPosition.Y + m_parameters.SpawnRadius.Y, (float)m_random.NextDouble()),
                (float)MathHelper.Lerp(m_emissionPosition.Z - m_parameters.SpawnRadius.Z, m_emissionPosition.Z + m_parameters.SpawnRadius.Z, (float)m_random.NextDouble())
                );

            Vector3 vel = new Vector3(
                (m_parameters.Speed.X + (float)MathHelper.Lerp(0f - m_parameters.Spread, 0f + m_parameters.Spread, (float)m_random.NextDouble())) * (float)MathHelper.Lerp(0f, m_parameters.SpeedVariance, (float)m_random.NextDouble()),
                m_parameters.Speed.Y * (float)MathHelper.Lerp(0f, m_parameters.SpeedVariance, (float)m_random.NextDouble()),
                (m_parameters.Speed.Z + (float)MathHelper.Lerp(0f - m_parameters.Spread, 0f + m_parameters.Spread, (float)m_random.NextDouble())) * (float)MathHelper.Lerp(0f, m_parameters.SpeedVariance, (float)m_random.NextDouble())
                );

            Particle ptl = new Particle();
            ptl.m_position = pos;
            ptl.m_velocity = vel;

            m_particles.Add(ptl);
        }

        /// <summary>This gets called everyframe before draw,
        /// It will create all the billboards necessary for the particle list</summary>
        //private void CycleBillboards()
        //{
        //    ReleaseVerts();
        //    CreateVerts();
        //}

        /// <summary>Wrapper to create the vertices needed for all current particles</summary>
        //private void CreateVerts()
        //{
        //    if (m_particles.Count < 1)
        //        return;

        //    BillboardParticleVertex[] verts = new BillboardParticleVertex[m_particles.Count * 6];

        //    int index = 0;
        //    foreach (Particle ptl in m_particles)
        //    {
        //        float scale = DetermineScale(ptl);
        //        float alpha = DetermineAlpha(ptl);

        //        verts[index++] = GetVert(ptl.m_position, new Vector2(0, 0), new Vector2(scale, scale), alpha);
        //        verts[index++] = GetVert(ptl.m_position, new Vector2(1, 0), new Vector2(scale, scale), alpha);
        //        verts[index++] = GetVert(ptl.m_position, new Vector2(1, 1), new Vector2(scale, scale), alpha);

        //        verts[index++] = GetVert(ptl.m_position, new Vector2(0, 0), new Vector2(scale, scale), alpha);
        //        verts[index++] = GetVert(ptl.m_position, new Vector2(1, 1), new Vector2(scale, scale), alpha);
        //        verts[index++] = GetVert(ptl.m_position, new Vector2(0, 1), new Vector2(scale, scale), alpha);
        //    }

        //    m_vertexBuffer = new VertexBuffer(
        //        m_device,
        //        typeof(BillboardParticleVertex),
        //        m_particles.Count * 6,
        //        BufferUsage.WriteOnly);
        //    m_vertexBuffer.SetData(verts);
        //}

        /// <summary>used in CreateVerts</summary>
        /// <param name="pos">dot3 world position</param>
        /// <param name="tex">dot2 tex coordinates</param>
        /// <returns>a new billboardVert for the buffer</returns>
        //private BillboardParticleVertex GetVert(Vector3 pos, Vector2 tex, Vector2 scale, float blend)
        //{
        //    BillboardParticleVertex t = new BillboardParticleVertex();
        //    t.Position = pos;
        //    t.TexCoordScale = new Vector4(tex.X, tex.Y, scale.X, scale.Y);
        //    t.Alpha = blend;
        //    return t;
        //}

        /// <summary>used in CreateVerts</summary>
        /// <param name="ptl">the particle to test</param>
        /// <returns>the proper alpha scalar</returns>
        private float DetermineAlpha(Particle ptl)
        {
            float alpha = 1f;
            if (ptl.m_currentLife <= m_parameters.ShowForMillies && m_parameters.ShowForMillies != 0f)
            {
                alpha = (ptl.m_currentLife / m_parameters.ShowForMillies);
            }
            else if (ptl.m_currentLife >= (m_parameters.MaxLife - m_parameters.FadeForMillies) &&
                m_parameters.FadeForMillies != 0f)
            {
                float deltaLife = ptl.m_currentLife - (m_parameters.MaxLife - m_parameters.FadeForMillies);
                alpha = 1f - (deltaLife / m_parameters.FadeForMillies);
            }
            return alpha;
        }

        /// <summary>used in CreateVerts, only to help cleanup code</summary>
        /// <param name="ptl">the particle to test</param>
        /// <returns>the proper scaling scalar</returns>
        private float DetermineScale(Particle ptl)
        {
            float scale = 1f;
            if (ptl.m_currentLife <= m_parameters.GrowForMillies && m_parameters.GrowForMillies != 0f)
            {
                scale = ptl.m_currentLife / m_parameters.GrowForMillies;
            }
            else if (ptl.m_currentLife >= (m_parameters.MaxLife - m_parameters.ShrinkForMillies) &&
                m_parameters.ShrinkForMillies != 0f)
            {
                float deltaLife = ptl.m_currentLife - (m_parameters.MaxLife - m_parameters.ShrinkForMillies);
                scale = 1f - (deltaLife / m_parameters.ShrinkForMillies);
            }
            return scale;
        }

        /// <summary> I expect this to completely clear out any data used in the vertex buffer,
        /// I'm hoping garbage collector handles this at the end of this scope (it seems to)</summary>
        //private void ReleaseVerts()
        //{
        //    if (m_vertexBuffer != null)
        //    {
        //        m_vertexBuffer.Dispose();
        //        m_vertexBuffer = null;
        //    }
        //}

        /// <summary> Determines whether the incoming vector3 is within the incoming force</summary>
        /// <param name="pos">positin to test</param>
        /// <param name="box">the force to test against</param>
        /// <returns>true for inside the box</returns>
        private bool PtlWithinForce(Vector3 pos, Force force)
        {
            if (pos.X < force.m_position.X - (force.m_diameters.X / 2) ||
                pos.X > force.m_position.X + (force.m_diameters.X / 2))
                return false;
            if (pos.Y < force.m_position.Y - (force.m_diameters.Y / 2) ||
                pos.Y > force.m_position.Y + (force.m_diameters.Y / 2))
                return false;
            if (pos.Z < force.m_position.Z - (force.m_diameters.Z / 2) ||
                pos.Z > force.m_position.Z + (force.m_diameters.Z / 2))
                return false;
            return true;
        }

        #endregion Private Parts


        #region Mutators

        public Vector3 Position
        {
            get { return m_emissionPosition; }
            set { m_emissionPosition = value; }
        }
        public float PositionX
        {
            get { return m_emissionPosition.X; }
            set { m_emissionPosition.X = value; }
        }
        public float PositionY
        {
            get { return m_emissionPosition.Y; }
            set { m_emissionPosition.Y = value; }
        }
        public float PositionZ
        {
            get { return m_emissionPosition.Z; }
            set { m_emissionPosition.Z = value; }
        }
        public int ParticleCount
        {
            get { return m_particles.Count; }
        }
        public EmitterSettings Parameters
        {
            get { return m_parameters; }
        }

        #endregion Mutators
    }
}
