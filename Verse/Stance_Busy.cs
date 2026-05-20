namespace Verse;

public abstract class Stance_Busy : Stance
{
	public int ticksLeft;

	public int startedTick;

	public Verb verb;

	public LocalTargetInfo focusTarg;

	public bool neverAimWeapon;

	protected float pieSizeFactor = 1f;

	public override bool StanceBusy => true;

	public Stance_Busy()
	{
		SetPieSizeFactor();
		startedTick = Find.TickManager.TicksGame;
	}

	public Stance_Busy(int ticks, LocalTargetInfo focusTarg, Verb verb)
	{
		ticksLeft = ticks;
		startedTick = Find.TickManager.TicksGame;
		this.focusTarg = focusTarg;
		this.verb = verb;
	}

	public Stance_Busy(int ticks)
		: this(ticks, null, null)
	{
	}

	private void SetPieSizeFactor()
	{
		if (ticksLeft < 300)
		{
			pieSizeFactor = 1f;
		}
		else if (ticksLeft < 450)
		{
			pieSizeFactor = 0.75f;
		}
		else
		{
			pieSizeFactor = 0.5f;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
		Scribe_Values.Look(ref startedTick, "startedTick", 0);
		Scribe_TargetInfo.Look(ref focusTarg, "focusTarg");
		Scribe_Values.Look(ref neverAimWeapon, "neverAimWeapon", defaultValue: false);
		Scribe_References.Look(ref verb, "verb");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			SetPieSizeFactor();
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && verb != null && verb.BuggedAfterLoading)
		{
			verb = null;
			Log.Warning(GetType()?.ToString() + " had a bugged verb after loading.");
		}
	}

	public override void StanceTick()
	{
		if (!stanceTracker.stunner.Stunned)
		{
			ticksLeft--;
			if (ticksLeft <= 0)
			{
				Expire();
			}
		}
	}

	protected virtual void Expire()
	{
		if (stanceTracker.curStance == this)
		{
			stanceTracker.SetStance(new Stance_Mobile());
		}
	}
}
