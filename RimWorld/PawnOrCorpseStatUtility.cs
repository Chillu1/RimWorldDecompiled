using System;
using Verse;

namespace RimWorld
{
	public static class PawnOrCorpseStatUtility
	{
		public static bool TryGetPawnOrCorpseStat(StatRequest req, Func<Pawn, float> pawnStatGetter, Func<ThingDef, float> pawnDefStatGetter, out float stat)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null)
				{
					stat = pawnStatGetter(pawn);
					return true;
				}
				Corpse corpse = req.Thing as Corpse;
				if (corpse != null)
				{
					stat = pawnStatGetter(corpse.InnerPawn);
					return true;
				}
			}
			else
			{
				ThingDef thingDef = req.Def as ThingDef;
				if (thingDef != null)
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
			}
			stat = 0f;
			return false;
		}
	}
}
