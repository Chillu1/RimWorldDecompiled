using Verse;

namespace RimWorld;

public class SignalAction_Delay : SignalAction
{
	public int delayTicks;

	public string completedSignalTag;

	private bool activated;

	public bool Activated => activated;

	public virtual Alert_ActionDelay Alert => null;

	public virtual bool ShouldRemoveNow => false;

	public override void Notify_SignalReceived(Signal signal)
	{
		if (signal.tag == signalTag)
		{
			DoAction(signal.args);
		}
	}

	protected override void DoAction(SignalArgs args)
	{
		if (delayTicks <= 0)
		{
			CompleteInt();
		}
		else
		{
			activated = true;
		}
	}

	protected virtual void Complete()
	{
		if (!completedSignalTag.NullOrEmpty())
		{
			Find.SignalManager.SendSignal(new Signal(completedSignalTag));
		}
	}

	private void CompleteInt()
	{
		Complete();
		if (!base.Destroyed)
		{
			Destroy();
		}
	}

	protected override void Tick()
	{
		if (activated)
		{
			if (delayTicks <= 0)
			{
				CompleteInt();
			}
			else if (ShouldRemoveNow)
			{
				Destroy();
			}
			else
			{
				delayTicks--;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref activated, "activated", defaultValue: false);
		Scribe_Values.Look(ref delayTicks, "delayTicks", 0);
		Scribe_Values.Look(ref completedSignalTag, "completedSignalTag");
	}
}
