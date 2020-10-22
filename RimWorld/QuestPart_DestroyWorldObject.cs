using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_DestroyWorldObject : QuestPart
	{
		public string inSignal;

		public WorldObject worldObject;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				if (worldObject != null)
				{
					yield return worldObject;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				TryRemove(worldObject);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref worldObject, "worldObject");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			if (TileFinder.TryFindNewSiteTile(out var tile))
			{
				worldObject = SiteMaker.MakeSite((SitePartDef)null, tile, (Faction)null, ifHostileThenMustRemainHostile: true, (float?)null);
			}
		}

		public static void TryRemove(WorldObject worldObject)
		{
			if (worldObject != null && worldObject.Spawned)
			{
				MapParent mapParent = worldObject as MapParent;
				if (mapParent != null && mapParent.HasMap)
				{
					mapParent.forceRemoveWorldObjectWhenMapRemoved = true;
				}
				else
				{
					worldObject.Destroy();
				}
			}
		}
	}
}
