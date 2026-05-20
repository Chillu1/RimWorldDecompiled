using System;
using System.Linq;
using Verse;

namespace RimWorld;

public class InspirationHandler : IExposable
{
	public Pawn pawn;

	private Inspiration curState;

	private const int CheckStartInspirationIntervalTicks = 100;

	private const float StartInspirationMTBDaysAtMaxMood = 10f;

	private const float MinMood = 0.5f;

	public bool Inspired => curState != null;

	public Inspiration CurState => curState;

	public InspirationDef CurStateDef => curState?.def;

	private float StartInspirationMTBDays
	{
		get
		{
			if (pawn.needs.mood == null)
			{
				return -1f;
			}
			float curLevel = pawn.needs.mood.CurLevel;
			if (curLevel < 0.5f)
			{
				return -1f;
			}
			return GenMath.LerpDouble(0.5f, 1f, 210f, 10f, curLevel);
		}
	}

	public InspirationHandler(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref curState, "curState");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && curState != null)
		{
			curState.pawn = pawn;
		}
	}

	public void InspirationHandlerTickInterval(int delta)
	{
		if (curState != null)
		{
			curState.InspirationTick(delta);
		}
		if (pawn.IsHashIntervalTick(100, delta))
		{
			CheckStartRandomInspiration();
		}
	}

	public bool TryStartInspiration(InspirationDef def, string reason = null, bool sendLetter = true)
	{
		if (Inspired)
		{
			return false;
		}
		if (BlockedByHediff())
		{
			return false;
		}
		if (!def.Worker.InspirationCanOccur(pawn))
		{
			return false;
		}
		curState = (Inspiration)Activator.CreateInstance(def.inspirationClass);
		curState.def = def;
		curState.pawn = pawn;
		curState.reason = reason;
		curState.PostStart(sendLetter);
		return true;
	}

	public void EndInspiration(Inspiration inspiration)
	{
		if (inspiration != null)
		{
			if (curState != inspiration)
			{
				Log.Error("Tried to end inspiration " + inspiration.ToStringSafe() + " but current inspiration is " + curState.ToStringSafe());
				return;
			}
			curState = null;
			inspiration.PostEnd();
		}
	}

	public void EndInspiration(InspirationDef inspirationDef)
	{
		if (curState != null && curState.def == inspirationDef)
		{
			EndInspiration(curState);
		}
	}

	public void Reset()
	{
		curState = null;
	}

	private void CheckStartRandomInspiration()
	{
		if (Inspired || !pawn.health.capacities.CanBeAwake)
		{
			return;
		}
		float startInspirationMTBDays = StartInspirationMTBDays;
		if (!(startInspirationMTBDays < 0f) && Rand.MTBEventOccurs(startInspirationMTBDays, 60000f, 100f))
		{
			InspirationDef randomAvailableInspirationDef = GetRandomAvailableInspirationDef();
			if (randomAvailableInspirationDef != null)
			{
				TryStartInspiration(randomAvailableInspirationDef, "LetterInspirationBeginThanksToHighMoodPart".Translate());
			}
		}
	}

	public InspirationDef GetRandomAvailableInspirationDef()
	{
		return DefDatabase<InspirationDef>.AllDefsListForReading.Where((InspirationDef x) => x.Worker.InspirationCanOccur(pawn)).RandomElementByWeightWithFallback((InspirationDef x) => x.Worker.CommonalityFor(pawn));
	}

	private bool BlockedByHediff()
	{
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			if (hediff.CurStage != null && hediff.CurStage.blocksInspirations)
			{
				return true;
			}
		}
		return false;
	}
}
