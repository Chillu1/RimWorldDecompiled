using Verse;

namespace RimWorld
{
	public class Verb_Spawn : Verb
	{
		protected override bool TryCastShot()
		{
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			GenSpawn.Spawn(verbProps.spawnDef, currentTarget.Cell, caster.Map);
			if (verbProps.colonyWideTaleDef != null)
			{
				Pawn pawn = caster.Map.mapPawns.FreeColonistsSpawned.RandomElementWithFallback();
				TaleRecorder.RecordTale(verbProps.colonyWideTaleDef, caster, pawn);
			}
			if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
			{
				base.EquipmentSource.Destroy();
			}
			return true;
		}
	}
}
