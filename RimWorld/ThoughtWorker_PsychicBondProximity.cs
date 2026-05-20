using System.Linq;
using Verse;

namespace RimWorld;

public class ThoughtWorker_PsychicBondProximity : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		Hediff_PsychicBond hediff_PsychicBond = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) as Hediff_PsychicBond;
		if (hediff_PsychicBond?.target == null)
		{
			return ThoughtState.Inactive;
		}
		if (NearPsychicBondedPerson(p, hediff_PsychicBond))
		{
			return ThoughtState.ActiveAtStage(0);
		}
		return ThoughtState.ActiveAtStage(1);
	}

	public static bool NearPsychicBondedPerson(Pawn pawn, Hediff_PsychicBond bondHediff)
	{
		Thing thing = bondHediff?.target;
		Pawn bondedPawn = thing as Pawn;
		if (bondedPawn == null)
		{
			return false;
		}
		IThingHolder parentHolder = pawn.ParentHolder;
		IThingHolder parentHolder2 = bondedPawn.ParentHolder;
		if ((parentHolder != null && ThingOwnerUtility.ContentsSuspended(parentHolder)) || (parentHolder2 != null && ThingOwnerUtility.ContentsSuspended(parentHolder2)))
		{
			return false;
		}
		if (parentHolder != null && parentHolder == parentHolder2)
		{
			return true;
		}
		if (QuestUtility.GetAllQuestPartsOfType<QuestPart_LendColonistsToFaction>().FirstOrDefault((QuestPart_LendColonistsToFaction p) => p.LentColonistsListForReading.Contains(pawn) && p.LentColonistsListForReading.Contains(bondedPawn)) != null)
		{
			return true;
		}
		return pawn.MapHeld == bondedPawn.MapHeld;
	}
}
