using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class QuestPart_DestroyThingsOrPassToWorld : QuestPart
	{
		public string inSignal;

		public List<Thing> things = new List<Thing>();

		public bool questLookTargets = true;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				if (questLookTargets)
				{
					for (int i = 0; i < things.Count; i++)
					{
						yield return things[i];
					}
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				Destroy(things);
			}
		}

		public static void Destroy(List<Thing> things)
		{
			for (int i = 0; i < things.Count; i++)
			{
				Thing thing = (!(things[i].ParentHolder is MinifiedThing)) ? things[i] : ((Thing)things[i].ParentHolder);
				if (!thing.Destroyed)
				{
					thing.DestroyOrPassToWorld(DestroyMode.QuestLogic);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Collections.Look(ref things, "things", LookMode.Reference);
			Scribe_Values.Look(ref questLookTargets, "questLookTargets", defaultValue: true);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				things.RemoveAll((Thing x) => x == null);
			}
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			if (Find.AnyPlayerHomeMap != null)
			{
				List<Thing> source = Find.RandomPlayerHomeMap.listerThings.ThingsInGroup(ThingRequestGroup.Plant);
				things.Clear();
				things.Add(source.FirstOrDefault());
			}
		}
	}
}
