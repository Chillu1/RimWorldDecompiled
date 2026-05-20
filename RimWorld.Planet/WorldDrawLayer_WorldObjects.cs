using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public abstract class WorldDrawLayer_WorldObjects : WorldDrawLayer
{
	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;

	public override bool Visible => DebugViewSettings.drawWorldObjects;

	protected abstract bool ShouldSkip(WorldObject worldObject);

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		foreach (WorldObject item2 in Find.WorldObjects.AllWorldObjectsOnLayer(planetLayer))
		{
			if (!item2.def.useDynamicDrawer && !ShouldSkip(item2))
			{
				Material material = item2.Material;
				if (material == null)
				{
					Log.ErrorOnce($"World object {item2} returned null material.", Gen.HashCombineInt(1948576891, item2.ID));
					continue;
				}
				LayerSubMesh subMesh = GetSubMesh(material);
				Rand.PushState();
				Rand.Seed = item2.ID;
				item2.Print(subMesh);
				Rand.PopState();
			}
		}
		FinalizeMesh(MeshParts.All);
	}
}
