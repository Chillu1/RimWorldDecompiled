using RimWorld;
using Verse.AI;

namespace Verse;

public class Hediff_Deathrest : HediffWithComps
{
	private Gene_Deathrest cachedGene;

	private int lastPauseCheckTick = -1;

	private bool cachedPaused;

	private const int PauseCheckInterval = 120;

	private Gene_Deathrest DeathrestGene => cachedGene ?? (cachedGene = pawn.genes?.GetFirstGeneOfType<Gene_Deathrest>());

	public bool Paused
	{
		get
		{
			if (lastPauseCheckTick < Find.TickManager.TicksGame + 120)
			{
				lastPauseCheckTick = Find.TickManager.TicksGame;
				cachedPaused = SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(pawn);
			}
			return cachedPaused;
		}
	}

	public override string LabelInBrackets
	{
		get
		{
			if (Paused)
			{
				return base.LabelInBrackets;
			}
			return DeathrestGene.DeathrestPercent.ToStringPercent("F0");
		}
	}

	public override string TipStringExtra
	{
		get
		{
			string text = base.TipStringExtra;
			if (Paused)
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "PawnWillKeepDeathrestingLethalInjuries".Translate(pawn.Named("PAWN")).Colorize(ColorLibrary.RedReadable);
			}
			return text;
		}
	}

	public override bool ShouldRemove
	{
		get
		{
			if (DeathrestGene == null)
			{
				return true;
			}
			return base.ShouldRemove;
		}
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		DeathrestGene?.Notify_DeathrestStarted();
	}

	public override void PostRemoved()
	{
		base.PostRemoved();
		DeathrestGene?.Notify_DeathrestEnded();
		if (pawn.Spawned && pawn.CurJobDef == JobDefOf.Deathrest)
		{
			pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public override void PostTickInterval(int delta)
	{
		base.PostTickInterval(delta);
		bool paused = Paused;
		DeathrestGene?.TickDeathresting(paused, delta);
	}
}
