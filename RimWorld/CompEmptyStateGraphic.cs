using UnityEngine;
using Verse;

namespace RimWorld;

public class CompEmptyStateGraphic : ThingComp
{
	private CompProperties_EmptyStateGraphic Props => (CompProperties_EmptyStateGraphic)props;

	public bool ParentIsEmpty
	{
		get
		{
			if (parent is IThingHolder thingHolder && thingHolder.GetDirectlyHeldThings().NullOrEmpty())
			{
				return true;
			}
			CompPawnSpawnOnWakeup compPawnSpawnOnWakeup = parent.TryGetComp<CompPawnSpawnOnWakeup>();
			if (compPawnSpawnOnWakeup != null && !compPawnSpawnOnWakeup.CanSpawn)
			{
				return true;
			}
			return false;
		}
	}

	public override bool DontDrawParent()
	{
		if (ParentIsEmpty)
		{
			return !Props.alwaysDrawParent;
		}
		return false;
	}

	public override void PostDraw()
	{
		if (ParentIsEmpty && parent.def.drawerType != DrawerType.MapMeshOnly)
		{
			Mesh mesh = Props.graphicData.Graphic.MeshAt(parent.Rotation);
			Vector3 drawPos = parent.DrawPos;
			Graphics.DrawMesh(mesh, drawPos + Props.graphicData.drawOffset.RotatedBy(parent.Rotation), Quaternion.identity, Props.graphicData.Graphic.MatAt(parent.Rotation), 0);
		}
	}

	public override void PostPrintOnto(SectionLayer layer)
	{
		if (ParentIsEmpty)
		{
			Props.graphicData.Graphic.Print(layer, parent, 0f);
		}
	}
}
