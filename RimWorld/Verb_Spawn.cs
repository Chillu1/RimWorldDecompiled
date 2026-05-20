using Verse;
using Verse.AI;

namespace RimWorld;

public class Verb_Spawn : Verb_CastBase
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
		base.ReloadableCompSource?.UsedOnce();
		return true;
	}
}
