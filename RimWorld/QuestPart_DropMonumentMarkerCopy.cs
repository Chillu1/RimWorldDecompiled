using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_DropMonumentMarkerCopy : QuestPart
	{
		public MapParent mapParent;

		public string inSignal;

		public string outSignalResult;

		private MonumentMarker copy;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				if (mapParent != null)
				{
					yield return mapParent;
				}
				if (copy != null)
				{
					yield return copy;
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
			copy = null;
			MonumentMarker arg = signal.args.GetArg<MonumentMarker>("SUBJECT");
			if (arg != null && mapParent != null && mapParent.HasMap)
			{
				Map map = mapParent.Map;
				IntVec3 dropCenter = DropCellFinder.RandomDropSpot(map);
				copy = (MonumentMarker)ThingMaker.MakeThing(ThingDefOf.MonumentMarker);
				copy.sketch = arg.sketch.DeepCopy();
				if (!arg.questTags.NullOrEmpty())
				{
					copy.questTags = new List<string>();
					copy.questTags.AddRange(arg.questTags);
				}
				DropPodUtility.DropThingsNear(dropCenter, map, Gen.YieldSingle((Thing)copy.MakeMinified()), 110, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: false);
			}
			if (!outSignalResult.NullOrEmpty())
			{
				if (copy != null)
				{
					Find.SignalManager.SendSignal(new Signal(outSignalResult, copy.Named("SUBJECT")));
				}
				else
				{
					Find.SignalManager.SendSignal(new Signal(outSignalResult));
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref outSignalResult, "outSignalResult");
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_References.Look(ref copy, "copy");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			if (Find.AnyPlayerHomeMap != null)
			{
				mapParent = Find.RandomPlayerHomeMap.Parent;
			}
		}
	}
}
