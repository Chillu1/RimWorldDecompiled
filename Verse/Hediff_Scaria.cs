using System.Text;
using RimWorld;

namespace Verse;

public class Hediff_Scaria : Hediff
{
	public const float ScariaManhunterChanceOffset = 0.5f;

	private const int KillAfterDaysManhunter = 5;

	private int manhunterTick = -999999;

	public override bool ShouldRemove => false;

	public override string TipStringExtra
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (TicksLeft >= 0)
			{
				stringBuilder.Append("DeathIn".Translate(TicksLeft.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor)).Resolve().CapitalizeFirst());
			}
			string tipStringExtra = base.TipStringExtra;
			if (!tipStringExtra.NullOrEmpty())
			{
				stringBuilder.AppendLineIfNotEmpty();
				stringBuilder.AppendLineIfNotEmpty();
				stringBuilder.Append(tipStringExtra);
			}
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.Append("  - " + "HarmedRevengeChance".Translate() + ": " + 0.5f.ToStringPercentSigned());
			return stringBuilder.ToString();
		}
	}

	private int TicksLeft => 300000 - (Find.TickManager.TicksGame - manhunterTick);

	private bool IsBerserk
	{
		get
		{
			if (!pawn.RaceProps.Humanlike || pawn.mindState.mentalStateHandler.CurStateDef != MentalStateDefOf.BerserkPermanent)
			{
				if (pawn.RaceProps.Animal)
				{
					return pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.ManhunterPermanent;
				}
				return false;
			}
			return true;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref manhunterTick, "manhunterTick", 0);
	}

	public override void TickInterval(int delta)
	{
		if (pawn.RaceProps.Humanlike && !pawn.mindState.mentalStateHandler.InMentalState && !pawn.Downed)
		{
			StartMentalState();
		}
		if (pawn.IsAnimal && pawn.Faction != null)
		{
			StartMentalState();
		}
		if (manhunterTick < 0 && IsBerserk)
		{
			manhunterTick = Find.TickManager.TicksGame;
		}
		if (manhunterTick > 0 && !IsBerserk)
		{
			manhunterTick = -999999;
		}
		if (manhunterTick > 0 && Find.TickManager.TicksGame - manhunterTick > 300000)
		{
			pawn.Kill(null, this);
		}
	}

	private void StartMentalState()
	{
		pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.BerserkPermanent, "CausedByHediff".Translate(def.LabelCap), forced: true, forceWake: true, causedByMood: false, null, transitionSilently: true);
	}

	public override void PostRemoved()
	{
		if (IsBerserk && !pawn.mindState.mentalStateHandler.CurState.causedByMood)
		{
			pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
		}
	}

	public override void Notify_PawnDamagedThing(Thing thing, DamageInfo dinfo, DamageWorker.DamageResult result)
	{
		if (result.hediffs.NullOrEmpty())
		{
			return;
		}
		foreach (Hediff hediff in result.hediffs)
		{
			if (dinfo.Def == DamageDefOf.Bite || dinfo.Def == DamageDefOf.Scratch || dinfo.Def == DamageDefOf.ScratchToxic)
			{
				HediffComp_Infecter hediffComp_Infecter = hediff.TryGetComp<HediffComp_Infecter>();
				if (hediffComp_Infecter != null)
				{
					hediffComp_Infecter.fromScaria = true;
				}
			}
		}
	}
}
