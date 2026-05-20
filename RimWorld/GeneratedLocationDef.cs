using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GeneratedLocationDef : Def
{
	private List<PlanetLayerDef> layerDefs;

	public int layerMaximum = -1;

	public WorldObjectDef worldObjectDef;

	public List<ThingDef> preciousResources;

	public FloatRange TimeoutRangeDays = new FloatRange(20f, 40f);

	public float weight = 1f;

	private static List<PlanetLayerDef> SurfaceList;

	public List<PlanetLayerDef> LayerDefs
	{
		get
		{
			object obj = layerDefs;
			if (obj == null)
			{
				obj = SurfaceList;
				if (obj == null)
				{
					obj = new List<PlanetLayerDef> { PlanetLayerDefOf.Surface };
					SurfaceList = (List<PlanetLayerDef>)obj;
				}
			}
			return (List<PlanetLayerDef>)obj;
		}
	}
}
