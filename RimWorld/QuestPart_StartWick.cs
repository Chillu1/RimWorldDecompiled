using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_StartWick : QuestPart
	{
		public string inSignal;

		public Thing explosiveThing;

		public Thing initiator;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				yield return explosiveThing;
				if (initiator != null)
				{
					yield return initiator;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal && explosiveThing != null)
			{
				explosiveThing.TryGetComp<CompExplosive>()?.StartWick(initiator);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref explosiveThing, "explosiveThing");
			Scribe_References.Look(ref initiator, "initiator");
		}
	}
}
