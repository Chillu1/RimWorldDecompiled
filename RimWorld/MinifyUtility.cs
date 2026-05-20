using Verse;
using Verse.Sound;

namespace RimWorld;

public static class MinifyUtility
{
	public static MinifiedThing MakeMinified(this Thing thing, DestroyMode destroyMode = DestroyMode.Vanish)
	{
		if (!thing.def.Minifiable)
		{
			Log.Warning("Tried to minify " + thing?.ToString() + " which is not minifiable.");
			return null;
		}
		thing.DeSpawnOrDeselect(destroyMode);
		if (thing.holdingOwner != null)
		{
			Log.Warning("Can't minify thing which is in a ThingOwner because we don't know how to handle it. Remove it from the container first. holder=" + thing.ParentHolder);
			return null;
		}
		Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(thing);
		MinifiedThing minifiedThing = (MinifiedThing)ThingMaker.MakeThing(thing.def.minifiedDef);
		minifiedThing.InnerThing = thing;
		blueprint_Install?.SetThingToInstallFromMinified(minifiedThing);
		if (minifiedThing.InnerThing.stackCount > 1)
		{
			Log.Warning("Tried to minify " + thing.LabelCap + " with stack count " + minifiedThing.InnerThing.stackCount + ". Clamped stack count to 1.");
			minifiedThing.InnerThing.stackCount = 1;
		}
		return minifiedThing;
	}

	public static Thing TryMakeMinified(this Thing thing)
	{
		if (thing.def.Minifiable)
		{
			return thing.MakeMinified();
		}
		return thing;
	}

	public static Thing GetInnerIfMinified(this Thing outerThing)
	{
		if (outerThing is MinifiedThing minifiedThing)
		{
			return minifiedThing.InnerThing;
		}
		return outerThing;
	}

	public static MinifiedThing Uninstall(this Thing th)
	{
		if (!th.Spawned)
		{
			Log.Warning("Can't uninstall unspawned thing " + th);
			return null;
		}
		bool num = Find.Selector.IsSelected(th);
		Map map = th.Map;
		MinifiedThing minifiedThing = th.MakeMinified();
		GenPlace.TryPlaceThing(minifiedThing, th.Position, map, ThingPlaceMode.Near);
		SoundDefOf.ThingUninstalled.PlayOneShot(new TargetInfo(th.Position, map));
		if (num)
		{
			Find.Selector.Select(minifiedThing, playSound: false, forceDesignatorDeselect: false);
		}
		return minifiedThing;
	}
}
