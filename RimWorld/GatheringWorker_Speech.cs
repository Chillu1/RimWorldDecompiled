using System;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class GatheringWorker_Speech : GatheringWorker
	{
		protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
		{
			return new LordJob_Joinable_Speech(spot, organizer, def);
		}

		public override bool CanExecute(Map map, Pawn organizer = null)
		{
			if (organizer == null)
			{
				return false;
			}
			if (!TryFindGatherSpot(organizer, out var _))
			{
				return false;
			}
			return true;
		}

		protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
		{
			Building_Throne building_Throne = RoyalTitleUtility.FindBestUsableThrone(organizer);
			if (building_Throne != null)
			{
				spot = building_Throne.InteractionCell;
				return true;
			}
			spot = IntVec3.Invalid;
			return false;
		}

		protected override void SendLetter(IntVec3 spot, Pawn organizer)
		{
			Find.LetterStack.ReceiveLetter(def.letterTitle, def.letterText.Formatted(organizer.Named("ORGANIZER")) + "\n\n" + OutcomeBreakdownForPawn(organizer), LetterDefOf.PositiveEvent, new TargetInfo(spot, organizer.Map));
		}

		public static string OutcomeBreakdownForPawn(Pawn organizer)
		{
			return "AbilitySpeechStatInfo".Translate(organizer.Named("ORGANIZER"), StatDefOf.SocialImpact.label) + ": " + organizer.GetStatValue(StatDefOf.SocialImpact).ToStringPercent() + "\n\n" + "AbilitySpeechPossibleOutcomes".Translate() + ":\n" + (from o in LordJob_Joinable_Speech.OutcomeChancesForPawn(organizer).Reverse()
				select o.Item1.stages[0].LabelCap + " " + o.Item2.ToStringPercent()).ToLineList("  - ");
		}
	}
}
