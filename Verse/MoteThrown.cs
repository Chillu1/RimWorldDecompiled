using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class MoteThrown : Mote
	{
		public float airTimeLeft = 999999f;

		protected Vector3 velocity = Vector3.zero;

		protected bool Flying => airTimeLeft > 0f;

		protected bool Skidding
		{
			get
			{
				if (!Flying)
				{
					return Speed > 0.01f;
				}
				return false;
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return velocity;
			}
			set
			{
				velocity = value;
			}
		}

		public float MoveAngle
		{
			get
			{
				return velocity.AngleFlat();
			}
			set
			{
				SetVelocity(value, Speed);
			}
		}

		public float Speed
		{
			get
			{
				return velocity.MagnitudeHorizontal();
			}
			set
			{
				if (value == 0f)
				{
					velocity = Vector3.zero;
				}
				else if (velocity == Vector3.zero)
				{
					velocity = new Vector3(value, 0f, 0f);
				}
				else
				{
					velocity = velocity.normalized * value;
				}
			}
		}

		protected override void TimeInterval(float deltaTime)
		{
			base.TimeInterval(deltaTime);
			if (base.Destroyed || (!Flying && !Skidding))
			{
				return;
			}
			Vector3 vector = NextExactPosition(deltaTime);
			IntVec3 intVec = new IntVec3(vector);
			if (intVec != base.Position)
			{
				if (!intVec.InBounds(base.Map))
				{
					Destroy();
					return;
				}
				if (def.mote.collide && intVec.Filled(base.Map))
				{
					WallHit();
					return;
				}
			}
			base.Position = intVec;
			exactPosition = vector;
			if (def.mote.rotateTowardsMoveDirection && velocity != default(Vector3))
			{
				exactRotation = velocity.AngleFlat();
			}
			else
			{
				exactRotation += rotationRate * deltaTime;
			}
			velocity += def.mote.acceleration * deltaTime;
			if (def.mote.speedPerTime != 0f)
			{
				Speed = Mathf.Max(Speed + def.mote.speedPerTime * deltaTime, 0f);
			}
			if (airTimeLeft > 0f)
			{
				airTimeLeft -= deltaTime;
				if (airTimeLeft < 0f)
				{
					airTimeLeft = 0f;
				}
				if (airTimeLeft <= 0f && !def.mote.landSound.NullOrUndefined())
				{
					def.mote.landSound.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
			}
			if (Skidding)
			{
				Speed *= skidSpeedMultiplierPerTick;
				rotationRate *= skidSpeedMultiplierPerTick;
				if (Speed < 0.02f)
				{
					Speed = 0f;
				}
			}
		}

		protected virtual Vector3 NextExactPosition(float deltaTime)
		{
			return exactPosition + velocity * deltaTime;
		}

		public void SetVelocity(float angle, float speed)
		{
			velocity = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * speed;
		}

		protected virtual void WallHit()
		{
			airTimeLeft = 0f;
			Speed = 0f;
			rotationRate = 0f;
		}
	}
}
