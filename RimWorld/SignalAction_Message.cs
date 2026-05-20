using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class SignalAction_Message : SignalAction
{
	public string message;

	public bool historical = true;

	public MessageTypeDef messageType;

	public LookTargets lookTargets;

	public bool allMustBeAsleep;

	public override void Notify_SignalReceived(Signal signal)
	{
		if (signal.tag != signalTag)
		{
			return;
		}
		if (allMustBeAsleep)
		{
			foreach (GlobalTargetInfo target in lookTargets.targets)
			{
				if (!target.Pawn.DestroyedOrNull() && target.Pawn.Spawned && !target.Pawn.Dead && target.Pawn.Awake())
				{
					return;
				}
			}
		}
		base.Notify_SignalReceived(signal);
	}

	protected override void DoAction(SignalArgs args)
	{
		Messages.Message(message, lookTargets, messageType ?? MessageTypeDefOf.NeutralEvent, historical);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref message, "message");
		Scribe_Values.Look(ref historical, "historical", defaultValue: false);
		Scribe_Values.Look(ref allMustBeAsleep, "allMustBeAsleep", defaultValue: false);
		Scribe_Deep.Look(ref lookTargets, "lookTargets");
		Scribe_Defs.Look(ref messageType, "messageType");
	}
}
