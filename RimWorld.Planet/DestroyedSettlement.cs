using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class DestroyedSettlement : MapParent
{
	public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
	{
		alsoRemoveWorldObject = false;
		if (base.Map.AnyBuildingBlockingMapRemoval)
		{
			return false;
		}
		if (base.Map.mapPawns.AnyPawnBlockingMapRemoval)
		{
			return false;
		}
		if (TransporterUtility.IncomingTransporterPreventingMapRemoval(base.Map))
		{
			return false;
		}
		alsoRemoveWorldObject = true;
		return true;
	}

	public override IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
	{
		foreach (IncidentTargetTagDef item in base.IncidentTargetTags())
		{
			yield return item;
		}
		yield return IncidentTargetTagDefOf.Map_PlayerHome;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (base.HasMap && Find.WorldSelector.SingleSelectedObject == this)
		{
			yield return SettleInExistingMapUtility.SettleCommand(base.Map, requiresNoEnemies: false);
		}
	}
}
