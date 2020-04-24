using Verse;

namespace RimWorld
{
	public class GenStep_Ambush_Hidden : GenStep_Ambush
	{
		public override int SeedPart => 921085483;

		protected override RectTrigger MakeRectTrigger()
		{
			RectTrigger rectTrigger = base.MakeRectTrigger();
			rectTrigger.activateOnExplosion = true;
			return rectTrigger;
		}

		protected override SignalAction_Ambush MakeAmbushSignalAction(CellRect rectToDefend, IntVec3 root, GenStepParams parms)
		{
			SignalAction_Ambush signalAction_Ambush = base.MakeAmbushSignalAction(rectToDefend, root, parms);
			if (root.IsValid)
			{
				signalAction_Ambush.spawnNear = root;
			}
			else
			{
				signalAction_Ambush.spawnAround = rectToDefend;
			}
			return signalAction_Ambush;
		}
	}
}
