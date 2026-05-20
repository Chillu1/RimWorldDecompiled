using UnityEngine;

namespace Verse;

public abstract class SubEffecter_DrifterEmote : SubEffecter
{
	public SubEffecter_DrifterEmote(SubEffecterDef def, Effecter parent)
		: base(def, parent)
	{
	}

	protected void MakeMote(TargetInfo A, int overrideSpawnTick = -1)
	{
		Vector3 vector = ((A.HasThing && A.Thing.DrawPosHeld.HasValue) ? A.Thing.DrawPosHeld.Value : A.Cell.ToVector3Shifted());
		if (!vector.ShouldSpawnMotesAt(A.Map))
		{
			return;
		}
		int randomInRange = def.burstCount.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			Mote mote = (Mote)ThingMaker.MakeThing(def.moteDef);
			mote.Scale = def.scale.RandomInRange;
			mote.exactPosition = vector + base.EffectiveOffset + Gen.RandomHorizontalVector(def.positionRadius);
			mote.rotationRate = def.rotationRate.RandomInRange;
			mote.exactRotation = def.rotation.RandomInRange;
			if (overrideSpawnTick != -1)
			{
				mote.ForceSpawnTick(overrideSpawnTick);
			}
			if (mote is MoteThrown moteThrown)
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
