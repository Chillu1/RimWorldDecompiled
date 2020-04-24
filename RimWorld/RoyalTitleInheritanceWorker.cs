using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RoyalTitleInheritanceWorker
	{
		private static List<Pawn> tmpPawns = new List<Pawn>();

		public Pawn FindHeir(Faction faction, Pawn pawn, RoyalTitleDef title)
		{
			List<Pawn> relatedPawns = new List<Pawn>();
			foreach (Pawn relatedPawn in pawn.relations.RelatedPawns)
			{
				if (!relatedPawn.Dead)
				{
					relatedPawns.Add(relatedPawn);
				}
			}
			Pawn pawn2 = GetClosestFamilyPawn(ignoreFaction: false);
			if (pawn2 != null)
			{
				return pawn2;
			}
			Pawn pawn3 = PawnsFinder.AllMapsAndWorld_Alive.Where((Pawn p) => p != pawn && p.Faction == pawn.Faction && p.RaceProps.Humanlike).MaxByWithFallback((Pawn p) => pawn.relations.OpinionOf(p));
			if (pawn3 != null)
			{
				return pawn3;
			}
			pawn2 = GetClosestFamilyPawn(ignoreFaction: true);
			if (pawn2 != null)
			{
				return pawn2;
			}
			return null;
			Pawn GetClosestFamilyPawn(bool ignoreFaction)
			{
				foreach (PawnRelationDef royalTitleInheritanceRelation in faction.def.royalTitleInheritanceRelations)
				{
					try
					{
						foreach (Pawn item in relatedPawns)
						{
							if (royalTitleInheritanceRelation.Worker.InRelation(pawn, item) && (ignoreFaction || item.Faction == pawn.Faction))
							{
								tmpPawns.Add(item);
							}
						}
						if (tmpPawns.Count != 0)
						{
							tmpPawns.Sort((Pawn p1, Pawn p2) => p2.ageTracker.AgeBiologicalYears.CompareTo(p1.ageTracker.AgeBiologicalYears));
							Pawn pawn4 = tmpPawns.Where((Pawn p) => p.royalty != null && p.royalty.GetCurrentTitle(faction) == null).FirstOrDefault();
							if (pawn4 != null)
							{
								return pawn4;
							}
							return tmpPawns[0];
						}
					}
					finally
					{
						tmpPawns.Clear();
					}
				}
				return null;
			}
		}
	}
}
