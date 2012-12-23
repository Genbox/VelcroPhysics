using System;
using System.Collections.Generic;

namespace FarseerPhysics.Fluids
{
    public class FluidSystem2
    {
        public const int MaxNeighbors = 25;
        public const int CellSize = 1;

        // Most of these can be tuned at runtime with F1-F9 and keys 1-9 (no numpad)
        public const float InfluenceRadius = 20.0f;
        public const float InfluenceRadiusSquared = InfluenceRadius * InfluenceRadius;
        public const float Stiffness = 0.504f;
        public const float StiffnessFarNearRatio = 10.0f;
        public const float StiffnessNear = Stiffness * StiffnessFarNearRatio;
        public const float ViscositySigma = 0.0f;
        public const float ViscosityBeta = 0.3f;
        public const float DensityRest = 10.0f;
        public const float KSpring = 0.3f;
        public const float RestLength = 5.0f;
        public const float RestLengthSquared = RestLength * RestLength;
        public const float YieldRatioStretch = 0.5f;
        public const float YieldRatioCompress = 0.5f;
        public const float Plasticity = 0.5f;
        public const int VelocityCap = 150;
        public const float DeformationFactor = 0f;
        public const float CollisionForce = 0.3f;

        private bool _isElasticityInitialized;
        private bool _elasticityEnabled;
        private bool _isPlasticityInitialized;
        private bool _plasticityEnabled;

        private float _deltaTime2;
        private Vec2 _dx = new Vec2(0.0f, 0.0f);
        private const int Wpadding = 20;
        private const int Hpadding = 20;

        public SpatialTable Particles;

        // Temp variables
        private Vec2 _rij = new Vec2(0.0f, 0.0f);
        private Vec2 _tempVect = new Vec2(0.0f, 0.0f);

        private Dictionary<int, List<int>> _springPresenceTable;
        private List<Spring2> _springs;
        private List<Particle> _tempParticles;

        private int _worldWidth;
        private int _worldHeight;

        public FluidSystem2(Vec2 gravity, int maxParticleLimit, int worldWidth, int worldHeight)
        {
            _worldHeight = worldHeight;
            _worldWidth = worldWidth;
            Particles = new SpatialTable(worldWidth, worldHeight, CellSize);
            MaxParticleLimit = maxParticleLimit;
            Gravity = gravity;
            Particles.Initialize();
        }

        public Vec2 Gravity { get; set; }
        public int MaxParticleLimit { get; private set; }

        public bool ElasticityEnabled
        {
            get { return _elasticityEnabled; }
            set
            {
                if (!_isElasticityInitialized)
                    InitializeElasticity();

                _elasticityEnabled = value;
            }
        }

        public bool PlasticityEnabled
        {
            get { return _plasticityEnabled; }
            set
            {
                if (!_isPlasticityInitialized)
                    InitializePlasticity();

                _plasticityEnabled = value;
            }
        }

        private void UpdateParticleVelocity(float deltaTime)
        {
            foreach (Particle particle in Particles)
            {
                particle.PreviousPosition.Set(particle.Position);
                particle.Position.Set(particle.Position.X + (deltaTime * particle.Velocity.X), particle.Position.Y + (deltaTime * particle.Velocity.Y));
            }
        }

        private void WallCollision(Particle pi)
        {
            float x = 0;
            float y = 0;

            if (pi.Position.X > (_worldWidth / 2 - Wpadding))
                x -= (pi.Position.X - (_worldWidth / 2 - Wpadding)) / CollisionForce;
            else if (pi.Position.X < (-_worldWidth / 2 + Wpadding))
                x += ((-_worldWidth / 2 + Wpadding) - pi.Position.X) / CollisionForce;

            if (pi.Position.Y > (_worldHeight - Hpadding))
                y -= (pi.Position.Y - (_worldHeight - Hpadding)) / CollisionForce;
            else if (pi.Position.Y < Hpadding)
                y += (Hpadding - pi.Position.Y) / CollisionForce;

            pi.Velocity.X += x;
            pi.Velocity.Y += y;
        }

        private void CapVelocity(Vec2 v)
        {
            if (v.X > VelocityCap)
                v.X = VelocityCap;
            else if (v.X < -VelocityCap)
                v.X = -VelocityCap;

            if (v.Y > VelocityCap)
                v.Y = VelocityCap;
            else if (v.Y < -VelocityCap)
                v.Y = -VelocityCap;
        }

        private void InitializePlasticity()
        {
            _isPlasticityInitialized = true;

            _springs.Clear();
            foreach (Particle pa in Particles)
            {
                foreach (Particle pb in Particles)
                {
                    if (pa.GetHashCode() == pb.GetHashCode())
                        continue;

                    float q = pa.Position.dst(pb.Position);
                    _rij.Set(pb.Position);
                    _rij.Sub(pa.Position);
                    _rij.mul(1 / q);

                    if (q < RestLength)
                    {
                        _springs.Add(new Spring2(pa, pb, q));
                    }
                }
                pa.Velocity.Set(0, 0);
            }
        }

        private void CalculatePlasticity(float deltaTime)
        {
            foreach (Spring2 spring in _springs)
            {
                spring.Update();

                if (spring.CurrentDistance == 0)
                    continue;

                _rij.Set(spring.PB.Position);
                _rij.Sub(spring.PA.Position);
                _rij.mul(1 / spring.CurrentDistance);
                float D = deltaTime * KSpring * (spring.RestLength - spring.CurrentDistance);
                _rij.mul(D * 0.5f);
                spring.PA.Position.Set(spring.PA.Position.X - _rij.X, spring.PA.Position.Y - _rij.Y);
                spring.PB.Position.Set(spring.PB.Position.X + _rij.X, spring.PB.Position.Y + _rij.Y);
            }
        }

        private void InitializeElasticity()
        {
            _isElasticityInitialized = true;

            foreach (Particle particle in Particles)
            {
                _springPresenceTable.Add(particle.GetHashCode(), new List<int>(MaxParticleLimit));
                particle.Velocity.Set(0, 0);
            }
        }

        private void CalculateElasticity(float deltaTime)
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                Particle pa = Particles[i];

                if (Particles.CountNearBy(pa) <= 1)
                    continue;

                _tempParticles = Particles.GetNearby(pa);
                int len2 = _tempParticles.Count;

                if (len2 > MaxNeighbors)
                    len2 = MaxNeighbors;

                for (int j = 0; j < len2; j++)
                {
                    Particle pb = Particles[j];

                    if (pa.GetHashCode() == pb.GetHashCode() || pa.Position.dst2(pb.Position) > RestLengthSquared)
                        continue;
                    if (!_springPresenceTable[pa.GetHashCode()].Contains(pb.GetHashCode()))
                    {
                        _springs.Add(new Spring2(pa, pb, RestLength));
                        _springPresenceTable[pa.GetHashCode()].Add(pb.GetHashCode());
                    }
                }
            }

            for (int i = _springs.Count - 1; i >= 0; i--)
            {
                Spring2 spring = _springs[i];
                spring.Update();

                // Stretch
                if (spring.CurrentDistance > (spring.RestLength + DeformationFactor))
                {
                    spring.RestLength += deltaTime * Plasticity * (spring.CurrentDistance - spring.RestLength - (YieldRatioStretch * spring.RestLength));
                }
                // Compress
                else if (spring.CurrentDistance < (spring.RestLength - DeformationFactor))
                {
                    spring.RestLength -= deltaTime * Plasticity * (spring.RestLength - (YieldRatioCompress * spring.RestLength) - spring.CurrentDistance);
                }
                // Remove springs with restLength longer than REST_LENGTH
                if (spring.RestLength > RestLength)
                {
                    _springs.RemoveAt(i);
                    _springPresenceTable[spring.PA.GetHashCode()].Remove(spring.PB.GetHashCode());
                }
                else
                {
                    if (spring.CurrentDistance == 0)
                        continue;

                    _rij.Set(spring.PB.Position);
                    _rij.Sub(spring.PA.Position);
                    _rij.mul(1 / spring.CurrentDistance);
                    float D = deltaTime * KSpring * (spring.RestLength - spring.CurrentDistance);
                    _rij.mul(D * 0.5f);
                    spring.PA.Position.Set(spring.PA.Position.X - _rij.X, spring.PA.Position.Y - _rij.Y);
                    spring.PB.Position.Set(spring.PB.Position.X + _rij.X, spring.PB.Position.Y + _rij.Y);
                }
            }
        }

        private void ApplyGravity(Particle particle)
        {
            particle.Velocity.Set(particle.Velocity.X + Gravity.X, particle.Velocity.Y + Gravity.Y);
        }

        private void ApplyViscosity(float deltaTime)
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                Particle particle = Particles[i];

                _tempParticles = Particles.GetNearby(particle);
               
                int len2 = _tempParticles.Count;
                if (len2 > MaxNeighbors)
                    len2 = MaxNeighbors;

                for (int j = 0; j < len2; j++)
                {
                    Particle tempParticle = _tempParticles[j];

                    float q = particle.Position.dst2(tempParticle.Position);
                    if ((q < InfluenceRadiusSquared) && (q != 0))
                    {
                        q = (float)Math.Sqrt(q);
                        _rij.Set(tempParticle.Position);
                        _rij.Sub(particle.Position);
                        _rij.mul(1 / q);
                        q /= InfluenceRadius;

                        _tempVect.Set(particle.Velocity);
                        _tempVect.Sub(tempParticle.Velocity);
                        float u = _tempVect.dot(_rij);
                        if (u <= 0.0f)
                            continue;
                        float I = (deltaTime * (1 - q) * (ViscositySigma * u + ViscosityBeta * u * u));
                        _rij.mul(I * 0.5f);
                        _tempVect.Set(particle.Velocity);
                        _tempVect.Sub(_rij);
                        particle.Velocity.Set(_tempVect);
                        _tempVect.Set(tempParticle.Velocity);
                        _tempVect.add(_rij);
                        tempParticle.Velocity.Set(_tempVect);
                    }
                }
            }
        }

        private void DoubleDensityRelaxation()
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                Particle particle = Particles[i];
                particle.Density = 0;
                particle.NearDensity = 0;

                _tempParticles = Particles.GetNearby(particle);
              
                int len2 = _tempParticles.Count;
                if (len2 > MaxNeighbors)
                    len2 = MaxNeighbors;

                for (int j = 0; j < len2; j++)
                {
                    Particle tempParticle = _tempParticles[j];

                    float q = particle.Position.dst2(tempParticle.Position);
                    if (q < InfluenceRadiusSquared && q != 0)
                    {
                        q = (float)Math.Sqrt(q);
                        q /= InfluenceRadius;
                        float qq = ((1 - q) * (1 - q));
                        particle.Density += qq;
                        particle.NearDensity += qq * (1 - q);
                    }
                }

                particle.Pressure = (Stiffness * (particle.Density - DensityRest));
                particle.NearPressure = (StiffnessNear * particle.NearDensity);
                _dx.Set(0.0f, 0.0f);

                for (int j = 0; j < len2; j++)
                {
                    Particle tempParticle = _tempParticles[j];

                    float q = particle.Position.dst2(tempParticle.Position);
                    if ((q < InfluenceRadiusSquared) && (q != 0))
                    {
                        q = (float)Math.Sqrt(q);
                        _rij.Set(tempParticle.Position);
                        _rij.Sub(particle.Position);
                        _rij.mul(1 / q);
                        q /= InfluenceRadius;

                        float D = (_deltaTime2 * (particle.Pressure * (1 - q) + particle.NearPressure * (1 - q) * (1 - q)));
                        _rij.mul(D * 0.5f);
                        tempParticle.Position.Set(tempParticle.Position.X + _rij.X, tempParticle.Position.Y + _rij.Y);
                        _dx.Sub(_rij);
                    }
                }
                particle.Position.Set(particle.Position.add(_dx));
            }
        }

        public void Update(float deltaTime)
        {
            _deltaTime2 = deltaTime * deltaTime;

            ApplyViscosity(deltaTime);

            //Update velocity
            UpdateParticleVelocity(deltaTime);

            Particles.Rehash();

            if (_elasticityEnabled)
                CalculateElasticity(deltaTime);

            if (_plasticityEnabled)
                CalculatePlasticity(deltaTime);

            DoubleDensityRelaxation();

            foreach (Particle particle in Particles)
            {
                particle.Velocity.Set((particle.Position.X - particle.PreviousPosition.X) / deltaTime, (particle.Position.Y - particle.PreviousPosition.Y) / deltaTime);
                ApplyGravity(particle);
                WallCollision(particle);
                CapVelocity(particle.Velocity);
            }
        }

        public void AddParticle(Vec2 vec2)
        {
            Particles.Add(new Particle(vec2.X, vec2.Y));
        }
    }

    public class Particle
    {
        public float Density;
        public float NearDensity;
        public float NearPressure;
        public Vec2 Position = new Vec2(0, 0);
        public float Pressure;
        public Vec2 PreviousPosition = new Vec2(0, 0);
        public Vec2 Velocity = new Vec2(0, 0);

        public Particle(float posX, float posY)
        {
            Position = new Vec2(posX, posY);
        }
    }

    public class Spring2
    {
        public float CurrentDistance;
        public Particle PA;
        public Particle PB;
        public float RestLength;

        public Spring2(Particle pa, Particle pb, float restLength)
        {
            PA = pa;
            PB = pb;
            RestLength = restLength;
        }

        public void Update()
        {
            CurrentDistance = PA.Position.dst(PB.Position);
        }

        public bool Contains(Particle p)
        {
            return (PA.GetHashCode() == p.GetHashCode() || PB.GetHashCode() == p.GetHashCode());
        }
    }

    public class Vec2
    {
        public float X;
        public float Y;

        public Vec2(float posX, float posY)
        {
            X = posX;
            Y = posY;
        }

        public void Set(Vec2 v)
        {
            X = v.X;
            Y = v.Y;
        }

        public void Set(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Sub(Vec2 v)
        {
            X -= v.X;
            Y -= v.Y;
        }

        public float dst(Vec2 v)
        {
            float x_d = v.X - X;
            float y_d = v.Y - Y;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }

        public Vec2 add(Vec2 v)
        {
            X += v.X;
            Y += v.Y;
            return this;
        }

        public void mul(float scalar)
        {
            X *= scalar;
            Y *= scalar;
        }

        public float dst2(Vec2 v)
        {
            return (v.X - X) * (v.X - X) + (v.Y - Y) * (v.Y - Y);
        }

        public float dot(Vec2 v)
        {
            return X * v.X + Y * v.Y;
        }

        public void Sub(float x, float y)
        {
            X -= x;
            Y -= y;
        }

        public void add(float x, float y)
        {
            X += x;
            Y += y;
        }
    }
}