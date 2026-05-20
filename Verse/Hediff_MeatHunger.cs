using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class Hediff_MeatHunger : Hediff
{
	private static readonly Dictionary<int, int> StageIndexToBetrayalMTBHours = new Dictionary<int, int>
	{
		{ 0, 0 },
		{ 1, 0 },
		{ 2, 8 },
		{ 3, 2 }
	};

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Meat hunger"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void TickInterval(int delta)
	{
		int num = StageIndexToBetrayalMTBHours[CurStageIndex];
		if (num > 0 && Rand.MTBEventOccurs(num, 2500f, 1f) && pawn.IsColonySubhuman)
		{
			pawn.SetFaction(Faction.OfEntities);
			Find.LetterStack.ReceiveLetter("GhoulBetrayalLabel".Translate(), "GhoulBetrayalText".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatBig, pawn);
		}
	}
}
