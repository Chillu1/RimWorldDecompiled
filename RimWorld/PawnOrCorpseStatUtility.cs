using System;
using Verse;

namespace RimWorld;

public static class PawnOrCorpseStatUtility
{
	public static bool TryGetPawnOrCorpseStat(StatRequest req, Func<Pawn, float> pawnStatGetter, Func<ThingDef, float> pawnDefStatGetter, out float stat)
	{
		if (req.HasThing)
		{
			if (req.Thing is Pawn arg)
			{
				stat = pawnStatGetter(arg);
				return true;
			}
			if (req.Thing is Corpse corpse)
			{
				stat = pawnStatGetter(corpse.InnerPawn);
				return true;
			}
		}
		else if (req.Def is ThingDef thingDef)
		{
			if (thingDef.category == ThingCategory.Pawn)
			{
				stat = pawnDefStatGetter(thingDef);
				return true;
			}
			if (thingDef.IsCorpse)
			{
				stat = pawnDefStatGetter(thingDef.ingestible.sourceDef);
				return true;
			}
		}
		stat = 0f;
		return false;
	}
}
