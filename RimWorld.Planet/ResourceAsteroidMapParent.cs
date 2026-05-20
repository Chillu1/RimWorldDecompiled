using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class ResourceAsteroidMapParent : SpaceMapParent
{
	private Color cachedStuffColor;

	public override Color ExpandingIconColor => cachedStuffColor;

	public override void SpawnSetup()
	{
		base.SpawnSetup();
		ThingDef mineableThing = preciousResource.building.mineableThing;
		cachedStuffColor = mineableThing.stuffProps.color;
	}
}
