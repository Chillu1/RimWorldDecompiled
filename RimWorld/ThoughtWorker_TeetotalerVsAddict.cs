using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_TeetotalerVsAddict : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			if (!p.RaceProps.Humanlike)
			{
				return false;
			}
			if (!p.IsTeetotaler())
			{
				return false;
			}
			if (!other.RaceProps.Humanlike)
			{
				return false;
			}
			if (!RelationsUtility.PawnsKnowEachOther(p, other))
			{
				return false;
			}
			List<Hediff> hediffs = other.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.IsAddiction)
				{
					return true;
				}
			}
			return false;
		}
	}
}
