using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_RequiredShuttleThings : QuestPart
	{
		public Thing shuttle;

		public MapParent mapParent;

		public bool requireAllColonistsOnMap;

		public int requiredColonistCount = -1;

		public string inSignal;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				CompShuttle compShuttle = shuttle.TryGetComp<CompShuttle>();
				if (compShuttle != null)
				{
					compShuttle.requireAllColonistsOnMap = requireAllColonistsOnMap;
					compShuttle.requiredColonistCount = requiredColonistCount;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref requireAllColonistsOnMap, "requireAllColonistsOnMap", defaultValue: false);
			Scribe_Values.Look(ref requiredColonistCount, "requiredColonistCount", -1);
		}
	}
}
