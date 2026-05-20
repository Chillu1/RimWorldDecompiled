using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse;

public abstract class SectionLayer_Things : SectionLayer
{
	private CellRect bounds;

	private List<LayerSubMesh> tmpFormerlyEnabled = new List<LayerSubMesh>();

	protected bool requireAddToMapMesh;

	public SectionLayer_Things(Section section)
		: base(section)
	{
	}

	public override CellRect GetBoundaryRect()
	{
		return bounds;
	}

	public override void DrawLayer()
	{
		if (!DebugViewSettings.drawThingsPrinted)
		{
			return;
		}
		if (WorldComponent_GravshipController.CutsceneInProgress && !WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			tmpFormerlyEnabled.Clear();
			for (int i = 0; i < subMeshes.Count; i++)
			{
				LayerSubMesh layerSubMesh = subMeshes[i];
				if (!layerSubMesh.disabled && layerSubMesh.material.renderQueue == 2950)
				{
					tmpFormerlyEnabled.Add(layerSubMesh);
					layerSubMesh.disabled = true;
				}
			}
			base.DrawLayer();
			for (int j = 0; j < tmpFormerlyEnabled.Count; j++)
			{
				tmpFormerlyEnabled[j].disabled = false;
			}
		}
		else
		{
			base.DrawLayer();
		}
	}

	public override void Regenerate()
	{
		ClearSubMeshes(MeshParts.All);
		bounds = section.CellRect;
		foreach (IntVec3 item in section.CellRect)
		{
			List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Thing thing = list[i];
				if ((thing.def.seeThroughFog || !base.Map.fogGrid.IsFogged(thing.Position)) && thing.def.drawerType != DrawerType.None && (thing.def.drawerType != DrawerType.RealtimeOnly || !requireAddToMapMesh) && (!(thing.def.hideAtSnowOrSandDepth < 1f) || !(Math.Max(base.Map.snowGrid.GetDepth(thing.Position), thing.Position.GetSandDepth(base.Map)) > thing.def.hideAtSnowOrSandDepth)) && (thing.def.plant == null || thing.def.plant.showInFrozenWater || thing.Position.GetTerrain(base.Map) != TerrainDefOf.ThinIce) && thing.Position.x == item.x && thing.Position.z == item.z)
				{
					TakePrintFrom(thing);
					bounds = bounds.Encapsulate(thing.OccupiedDrawRect());
				}
			}
		}
		FinalizeMesh(MeshParts.All);
	}

	protected abstract void TakePrintFrom(Thing t);
}
