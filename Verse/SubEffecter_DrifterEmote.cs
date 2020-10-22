using UnityEngine;

namespace Verse
{
	public abstract class SubEffecter_DrifterEmote : SubEffecter
	{
		public SubEffecter_DrifterEmote(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		protected void MakeMote(TargetInfo A)
		{
			Vector3 vector = (A.HasThing ? A.Thing.DrawPos : A.Cell.ToVector3Shifted());
			if (!vector.ShouldSpawnMotesAt(A.Map))
			{
				return;
			}
			int randomInRange = def.burstCount.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				Mote mote = (Mote)ThingMaker.MakeThing(def.moteDef);
				mote.Scale = def.scale.RandomInRange;
				mote.exactPosition = vector + def.positionOffset + Gen.RandomHorizontalVector(def.positionRadius);
				mote.rotationRate = def.rotationRate.RandomInRange;
				mote.exactRotation = def.rotation.RandomInRange;
				MoteThrown moteThrown = mote as MoteThrown;
				if (moteThrown != null)
				{
					moteThrown.airTimeLeft = def.airTime.RandomInRange;
					moteThrown.SetVelocity(def.angle.RandomInRange, def.speed.RandomInRange);
				}
				if (A.HasThing)
				{
					mote.Attach(A);
				}
				GenSpawn.Spawn(mote, vector.ToIntVec3(), A.Map);
			}
		}
	}
}
