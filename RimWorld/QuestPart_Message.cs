using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_Message : QuestPart
	{
		public string inSignal;

		public string message;

		public MessageTypeDef messageType;

		public LookTargets lookTargets;

		public bool historical = true;

		public bool getLookTargetsFromSignal = true;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				GlobalTargetInfo globalTargetInfo = lookTargets.TryGetPrimaryTarget();
				if (globalTargetInfo.IsValid)
				{
					yield return globalTargetInfo;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				LookTargets lookTargets = this.lookTargets;
				if (getLookTargetsFromSignal && !lookTargets.IsValid())
				{
					SignalArgsUtility.TryGetLookTargets(signal.args, "SUBJECT", out lookTargets);
				}
				TaggedString formattedText = signal.args.GetFormattedText(message);
				if (!formattedText.NullOrEmpty())
				{
					Messages.Message(formattedText, lookTargets, messageType ?? MessageTypeDefOf.NeutralEvent, quest, historical);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref message, "message");
			Scribe_Defs.Look(ref messageType, "messageType");
			Scribe_Deep.Look(ref lookTargets, "lookTargets");
			Scribe_Values.Look(ref historical, "historical", defaultValue: true);
			Scribe_Values.Look(ref getLookTargetsFromSignal, "getLookTargetsFromSignal", defaultValue: true);
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			message = "Dev: Test";
			messageType = MessageTypeDefOf.PositiveEvent;
		}
	}
}
