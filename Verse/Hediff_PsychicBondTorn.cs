using RimWorld;

namespace Verse;

public class Hediff_PsychicBondTorn : HediffWithTarget
{
	private int creationTick = -1;

	public override string LabelBase => base.LabelBase + " (" + target?.LabelShortCap + ")";

	public override bool Visible => false;

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		if (Current.ProgramState == ProgramState.Playing)
		{
			creationTick = Find.TickManager.TicksGame;
		}
	}

	public override void Notify_Resurrected()
	{
		bool flag = false;
		if (target is Pawn { Dead: false, Destroyed: false } pawn && creationTick >= 0 && Find.TickManager.TicksGame - creationTick <= ThoughtDefOf.PsychicBondTorn.DurationTicks)
		{
			Gene_PsychicBonding gene_PsychicBonding = base.pawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
			if (gene_PsychicBonding != null)
			{
				gene_PsychicBonding.BondTo(pawn);
			}
			else
			{
				pawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>()?.BondTo(base.pawn);
			}
			flag = true;
		}
		if (!flag)
		{
			base.pawn.health.RemoveHediff(this);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref creationTick, "creationTick", -1);
	}
}
