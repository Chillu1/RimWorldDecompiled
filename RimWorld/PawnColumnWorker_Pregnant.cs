using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_Pregnant : PawnColumnWorker_Icon
	{
		private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant");

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			if (GetPregnantHediff(pawn) == null)
			{
				return null;
			}
			return Icon;
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return GetTooltipText(pawn);
		}

		public static string GetTooltipText(Pawn pawn)
		{
			float gestationProgress = GetPregnantHediff(pawn).GestationProgress;
			int num = (int)(pawn.RaceProps.gestationPeriodDays * 60000f);
			int numTicks = (int)(gestationProgress * (float)num);
			return "PregnantIconDesc".Translate(numTicks.ToStringTicksToDays("F0"), num.ToStringTicksToDays("F0"));
		}

		private static Hediff_Pregnant GetPregnantHediff(Pawn pawn)
		{
			return (Hediff_Pregnant)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant, mustBeVisible: true);
		}
	}
}
