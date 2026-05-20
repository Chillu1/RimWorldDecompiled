using System;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_RefugeePodCrash : QuestNode_Root_WandererJoin
{
	public override Pawn GeneratePawn()
	{
		Pawn pawn = ThingUtility.FindPawn(ThingSetMakerDefOf.RefugeePod.root.Generate());
		pawn.guest.Recruitable = true;
		return pawn;
	}

	[Obsolete]
	public override void SendLetter(Quest quest, Pawn pawn)
	{
		SendLetter_NewTemp(quest, pawn, Find.AnyPlayerHomeMap);
	}

	public override void SendLetter_NewTemp(Quest quest, Pawn pawn, Map map)
	{
		TaggedString title = "LetterLabelRefugeePodCrash".Translate();
		TaggedString letterText = "RefugeePodCrash".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		letterText += "\n\n";
		if (pawn.Faction == null)
		{
			letterText += "RefugeePodCrash_Factionless".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		}
		else if (pawn.Faction.HostileTo(Faction.OfPlayer))
		{
			letterText += "RefugeePodCrash_Hostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		}
		else
		{
			letterText += "RefugeePodCrash_NonHostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
		}
		if (pawn.DevelopmentalStage.Juvenile())
		{
			string arg = (pawn.ageTracker.AgeBiologicalYears * 3600000).ToStringTicksToPeriod();
			letterText += "\n\n" + "RefugeePodCrash_Child".Translate(pawn.Named("PAWN"), arg.Named("AGE"));
		}
		QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(pawn), ref letterText);
		PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
		Find.LetterStack.ReceiveLetter(title, letterText, LetterDefOf.NeutralEvent, new TargetInfo(pawn));
	}
}
