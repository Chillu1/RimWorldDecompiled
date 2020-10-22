using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class QuestPart_FactionGoodwillChange : QuestPart
	{
		public string inSignal;

		public int change;

		public Faction faction;

		public bool canSendMessage = true;

		public bool canSendHostilityLetter = true;

		public string reason;

		public bool getLookTargetFromSignal = true;

		public GlobalTargetInfo lookTarget;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				yield return lookTarget;
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
				if (faction != null)
				{
					yield return faction;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal && faction != null && faction != Faction.OfPlayer)
			{
				LookTargets lookTargets;
				GlobalTargetInfo value = (lookTarget.IsValid ? lookTarget : ((!getLookTargetFromSignal) ? GlobalTargetInfo.Invalid : ((!SignalArgsUtility.TryGetLookTargets(signal.args, "SUBJECT", out lookTargets)) ? GlobalTargetInfo.Invalid : lookTargets.TryGetPrimaryTarget())));
				FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
				int arg = 0;
				if (!signal.args.TryGetArg("GOODWILL", out arg))
				{
					arg = change;
				}
				faction.TryAffectGoodwillWith(Faction.OfPlayer, arg, canSendMessage, canSendHostilityLetter, signal.args.GetFormattedText(reason), value);
				TaggedString text = "";
				faction.TryAppendRelationKindChangedInfo(ref text, playerRelationKind, faction.PlayerRelationKind);
				if (!text.NullOrEmpty())
				{
					text = "\n\n" + text;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref change, "change", 0);
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref canSendMessage, "canSendMessage", defaultValue: true);
			Scribe_Values.Look(ref canSendHostilityLetter, "canSendHostilityLetter", defaultValue: true);
			Scribe_Values.Look(ref reason, "reason");
			Scribe_Values.Look(ref getLookTargetFromSignal, "getLookTargetFromSignal", defaultValue: true);
			Scribe_TargetInfo.Look(ref lookTarget, "lookTarget");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			change = -15;
			faction = Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
		}
	}
}
