using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldLayer_WorldObjects : WorldLayer
	{
		protected abstract bool ShouldSkip(WorldObject worldObject);

		public override IEnumerable Regenerate()
		{
			foreach (object item in base.Regenerate())
			{
				yield return item;
			}
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (!worldObject.def.useDynamicDrawer && !ShouldSkip(worldObject))
				{
					Material material = worldObject.Material;
					if (material == null)
					{
						Log.ErrorOnce("World object " + worldObject + " returned null material.", Gen.HashCombineInt(1948576891, worldObject.ID));
						continue;
					}
					LayerSubMesh subMesh = GetSubMesh(material);
					Rand.PushState();
					Rand.Seed = worldObject.ID;
					worldObject.Print(subMesh);
					Rand.PopState();
				}
			}
			FinalizeMesh(MeshParts.All);
		}
	}
}
