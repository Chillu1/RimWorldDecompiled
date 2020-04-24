using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_Dialog : QuestPart
	{
		public class Option : IExposable
		{
			public string text;

			public string outSignal;

			public void ExposeData()
			{
				Scribe_Values.Look(ref text, "text");
				Scribe_Values.Look(ref outSignal, "outSignal");
			}
		}

		public string inSignal;

		public string text;

		public string title;

		public List<Option> options = new List<Option>();

		public Faction relatedFaction;

		public bool addToArchive = true;

		public bool radioMode;

		public bool getLookTargetsFromSignal;

		public LookTargets lookTargets;

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

		public override IEnumerable<Faction> InvolvedFactions
		{
			get
			{
				foreach (Faction involvedFaction in base.InvolvedFactions)
				{
					yield return involvedFaction;
				}
				if (relatedFaction != null)
				{
					yield return relatedFaction;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal))
			{
				return;
			}
			DiaNode diaNode = new DiaNode(signal.args.GetFormattedText(text));
			LookTargets resolvedLookTargets = lookTargets;
			if (getLookTargetsFromSignal && !resolvedLookTargets.IsValid())
			{
				SignalArgsUtility.TryGetLookTargets(signal.args, "SUBJECT", out resolvedLookTargets);
			}
			if (resolvedLookTargets.IsValid())
			{
				DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
				diaOption.action = delegate
				{
					CameraJumper.TryJumpAndSelect(resolvedLookTargets.TryGetPrimaryTarget());
				};
				diaOption.resolveTree = true;
				diaNode.options.Add(diaOption);
			}
			if (options.Any())
			{
				for (int i = 0; i < options.Count; i++)
				{
					int localIndex = i;
					DiaOption diaOption2 = new DiaOption(signal.args.GetFormattedText(options[i].text));
					diaOption2.action = delegate
					{
						Find.SignalManager.SendSignal(new Signal(options[localIndex].outSignal));
					};
					diaOption2.resolveTree = true;
					diaNode.options.Add(diaOption2);
				}
			}
			else
			{
				DiaOption diaOption3 = new DiaOption("OK".Translate());
				diaOption3.resolveTree = true;
				diaNode.options.Add(diaOption3);
			}
			TaggedString formattedText = signal.args.GetFormattedText(title);
			Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, relatedFaction, delayInteractivity: true, radioMode, formattedText));
			if (addToArchive)
			{
				Find.Archive.Add(new ArchivedDialog(diaNode.text, formattedText, relatedFaction));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref title, "title");
			Scribe_Collections.Look(ref options, "options", LookMode.Deep);
			Scribe_References.Look(ref relatedFaction, "relatedFaction");
			Scribe_Values.Look(ref addToArchive, "addToArchive", defaultValue: true);
			Scribe_Values.Look(ref radioMode, "radioMode", defaultValue: false);
			Scribe_Values.Look(ref getLookTargetsFromSignal, "getLookTargetsFromSignal", defaultValue: false);
			Scribe_Deep.Look(ref lookTargets, "lookTargets");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			title = "Title";
			text = "Dev: Test";
			relatedFaction = Faction.OfMechanoids;
			addToArchive = false;
			Option option = new Option();
			option.text = "Option 1";
			option.outSignal = "DebugSignal" + Rand.Int;
			options.Add(option);
			Option option2 = new Option();
			option2.text = "Option 2";
			option2.outSignal = "DebugSignal" + Rand.Int;
			options.Add(option2);
		}
	}
}
