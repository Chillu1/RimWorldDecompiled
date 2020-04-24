using UnityEngine;

namespace Verse
{
	public abstract class SubEffecter_Sprayer : SubEffecter
	{
		public SubEffecter_Sprayer(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		protected void MakeMote(TargetInfo A, TargetInfo B)
		{
			Vector3 vector = Vector3.zero;
			switch (def.spawnLocType)
			{
			case MoteSpawnLocType.OnSource:
				vector = A.CenterVector3;
				break;
			case MoteSpawnLocType.BetweenPositions:
			{
				Vector3 vector2 = A.HasThing ? A.Thing.DrawPos : A.Cell.ToVector3Shifted();
				Vector3 vector3 = B.HasThing ? B.Thing.DrawPos : B.Cell.ToVector3Shifted();
				vector = ((A.HasThing && !A.Thing.Spawned) ? vector3 : ((!B.HasThing || B.Thing.Spawned) ? (vector2 * def.positionLerpFactor + vector3 * (1f - def.positionLerpFactor)) : vector2));
				break;
			}
			case MoteSpawnLocType.RandomCellOnTarget:
				vector = ((!B.HasThing) ? CellRect.CenteredOn(B.Cell, 0) : B.Thing.OccupiedRect()).RandomCell.ToVector3Shifted();
				break;
			case MoteSpawnLocType.BetweenTouchingCells:
				vector = A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f;
				break;
			}
			if (parent != null)
			{
				Rand.PushState(parent.GetHashCode());
				if (A.CenterVector3 != B.CenterVector3)
				{
					vector += (B.CenterVector3 - A.CenterVector3).normalized * parent.def.offsetTowardsTarget.RandomInRange;
				}
				vector += Gen.RandomHorizontalVector(parent.def.positionRadius);
				Rand.PopState();
			}
			Map map = A.Map ?? B.Map;
			float num = def.absoluteAngle ? 0f : (B.Cell - A.Cell).AngleFlat;
			if (map == null || !vector.ShouldSpawnMotesAt(map))
			{
				return;
			}
			int randomInRange = def.burstCount.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				Mote obj = (Mote)ThingMaker.MakeThing(def.moteDef);
				GenSpawn.Spawn(obj, vector.ToIntVec3(), map);
				obj.Scale = def.scale.RandomInRange;
				obj.exactPosition = vector + Gen.RandomHorizontalVector(def.positionRadius);
				obj.rotationRate = def.rotationRate.RandomInRange;
				obj.exactRotation = def.rotation.RandomInRange + num;
				obj.instanceColor = def.color;
				MoteThrown moteThrown = obj as MoteThrown;
				if (moteThrown != null)
				{
					moteThrown.airTimeLeft = def.airTime.RandomInRange;
					moteThrown.SetVelocity(def.angle.RandomInRange + num, def.speed.RandomInRange);
				}
			}
		}
	}
}
