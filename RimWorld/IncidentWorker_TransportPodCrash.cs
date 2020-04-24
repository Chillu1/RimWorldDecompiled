using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_TransportPodCrash : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<Thing> things = ThingSetMakerDefOf.RefugeePod.root.Generate();
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			Pawn pawn = ThingUtility.FindPawn(things);
			pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;
			TaggedString title = "LetterLabelRefugeePodCrash".Translate();
			TaggedString text = "RefugeePodCrash".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
			text += "\n\n";
			if (pawn.Faction == null)
			{
				text += "RefugeePodCrash_Factionless".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
			}
			else if (pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				text += "RefugeePodCrash_Hostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
			}
			else
			{
				text += "RefugeePodCrash_NonHostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
			}
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
			SendStandardLetter(title, text, LetterDefOf.NeutralEvent, parms, new TargetInfo(intVec, map));
			ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
			activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things);
			activeDropPodInfo.openDelay = 180;
			activeDropPodInfo.leaveSlag = true;
			DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo);
			return true;
		}
	}
}
