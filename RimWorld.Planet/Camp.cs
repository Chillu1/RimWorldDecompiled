using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class Camp : MapParent
{
	private Material cachedMat;

	private static readonly StringBuilder sb = new StringBuilder();

	public override bool CanReformFoggedEnemies => true;

	public override Material Material
	{
		get
		{
			if (cachedMat != null)
			{
				return cachedMat;
			}
			Color color = base.Faction?.Color ?? Color.white;
			cachedMat = MaterialPool.MatFrom(def.texture, ShaderDatabase.WorldOverlayTransparentLit, color, 3550);
			return cachedMat;
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (Find.WorldSelector.SingleSelectedObject == this)
		{
			Command command = SettleInExistingMapUtility.SettleCommand(base.Map, requiresNoEnemies: true);
			if (!TileFinder.IsValidTileForNewSettlement(base.Tile, sb))
			{
				command.Disable(sb.ToString());
			}
			yield return command;
		}
		sb.Clear();
	}

	public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
	{
		alsoRemoveWorldObject = false;
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

	public override void Notify_MyMapRemoved(Map map)
	{
		base.Notify_MyMapRemoved(map);
		if (ModsConfig.OdysseyActive && map.TileInfo.Landmark != null)
		{
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedLandmark);
			worldObject.Tile = base.Tile;
			worldObject.SetFaction(base.Faction);
			Find.WorldObjects.Add(worldObject);
		}
		else
		{
			WorldObject worldObject2 = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedCamp);
			worldObject2.Tile = base.Tile;
			worldObject2.SetFaction(base.Faction);
			worldObject2.GetComponent<TimeoutComp>().StartTimeout(1800000);
			Find.WorldObjects.Add(worldObject2);
		}
	}
}
