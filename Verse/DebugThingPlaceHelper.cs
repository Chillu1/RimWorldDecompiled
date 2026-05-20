using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class DebugThingPlaceHelper
{
	public static bool IsDebugSpawnable(ThingDef def, bool allowPlayerBuildable = false)
	{
		if (def.forceDebugSpawnable)
		{
			return true;
		}
		if (typeof(Corpse).IsAssignableFrom(def.thingClass) || def.IsBlueprint || def.IsFrame || def == ThingDefOf.ActiveDropPod || def.thingClass == typeof(MinifiedThing) || def.thingClass == typeof(MinifiedTree) || def.thingClass == typeof(UnfinishedThing) || def.thingClass.IsSubclassOf(typeof(SignalAction)) || def.destroyOnDrop)
		{
			return false;
		}
		if (def.category == ThingCategory.Filth || def.category == ThingCategory.Item || def.category == ThingCategory.Plant || def.category == ThingCategory.Ethereal)
		{
			return true;
		}
		if (def.category == ThingCategory.Building && def.building.isNaturalRock)
		{
			return true;
		}
		if (def.category == ThingCategory.Building && !def.BuildableByPlayer)
		{
			return true;
		}
		if (def.category == ThingCategory.Building && def.BuildableByPlayer && allowPlayerBuildable)
		{
			return true;
		}
		return false;
	}

	public static void DebugSpawn(ThingDef def, IntVec3 c, int stackCount = -1, bool direct = false, ThingStyleDef thingStyleDef = null, bool canBeMinified = true, WipeMode? wipeMode = null)
	{
		if (def == ThingDefOf.Fire)
		{
			Fire firstThing = c.GetFirstThing<Fire>(Find.CurrentMap);
			if (firstThing != null)
			{
				firstThing.fireSize = Mathf.Min(firstThing.fireSize + 0.1f, 1.75f);
				return;
			}
		}
		if (stackCount <= 0)
		{
			stackCount = def.stackLimit;
		}
		ThingDef stuff = GenStuff.RandomStuffFor(def);
		Thing thing = ThingMaker.MakeThing(def, stuff);
		if (thingStyleDef != null)
		{
			thing.StyleDef = thingStyleDef;
		}
		thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
		if (thing.def.Minifiable && canBeMinified)
		{
			thing = thing.MakeMinified();
		}
		if (thing.def.CanHaveFaction)
		{
			if (thing.def.building != null && thing.def.building.isInsectCocoon)
			{
				thing.SetFaction(Faction.OfInsects);
			}
			else
			{
				thing.SetFaction(Faction.OfPlayerSilentFail);
			}
		}
		thing.stackCount = stackCount;
		if (wipeMode.HasValue)
		{
			GenSpawn.Spawn(def, c, Find.CurrentMap, wipeMode.Value);
		}
		else
		{
			GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, (!direct) ? ThingPlaceMode.Near : ThingPlaceMode.Direct);
		}
		thing.Notify_DebugSpawned();
	}

	public static List<DebugActionNode> TryPlaceOptionsForStackCount(int stackCount, bool direct)
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => IsDebugSpawnable(def) && (stackCount < 0 || def.stackLimit >= stackCount)))
		{
			ThingDef localDef = item;
			list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMap, delegate
			{
				DebugSpawn(localDef, UI.MouseCell(), stackCount, direct, null, canBeMinified: false);
			}));
		}
		if (stackCount == 1)
		{
			foreach (ThingDef item2 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.Minifiable))
			{
				ThingDef localDef2 = item2;
				list.Add(new DebugActionNode(localDef2.defName + " (minified)", DebugActionType.ToolMap, delegate
				{
					DebugSpawn(localDef2, UI.MouseCell(), stackCount, direct);
				}));
			}
		}
		return list;
	}

	public static List<DebugActionNode> TryPlaceOptionsUnminified()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.Minifiable))
		{
			ThingDef localDef = item;
			list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMap, delegate
			{
				DebugSpawn(localDef, UI.MouseCell(), 1, direct: false, null, canBeMinified: false);
			}));
		}
		return list;
	}

	public static List<DebugActionNode> TryPlaceOptionsForBaseMarketValue(float marketValue, bool direct)
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => IsDebugSpawnable(def) && def.stackLimit > 1))
		{
			ThingDef localDef = item;
			int stackCount = (int)(marketValue / localDef.BaseMarketValue);
			list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMap, delegate
			{
				DebugSpawn(localDef, UI.MouseCell(), stackCount, direct);
			}));
		}
		return list;
	}

	public static List<DebugActionNode> SpawnOptions(WipeMode wipeMode)
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => IsDebugSpawnable(def, allowPlayerBuildable: true)))
		{
			ThingDef localDef = item;
			list.Add(new DebugActionNode(localDef.defName, DebugActionType.ToolMap, delegate
			{
				DebugSpawn(localDef, UI.MouseCell(), 1, direct: true, null, canBeMinified: true, wipeMode);
			}));
		}
		return list;
	}

	public static List<DebugActionNode> TryAbandonOptionsForStackCount()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => IsDebugSpawnable(def)))
		{
			ThingDef localDef = item;
			DebugActionNode debugActionNode = new DebugActionNode();
			debugActionNode.label = localDef.defName;
			debugActionNode.actionType = DebugActionType.Action;
			debugActionNode.AddChild(new DebugActionNode(localDef.defName + " x1", DebugActionType.ToolWorld, delegate
			{
				DebugAbandon(localDef, GenWorld.MouseTile(), 1);
			}));
			debugActionNode.AddChild(new DebugActionNode(localDef.defName + " full stack", DebugActionType.ToolWorld, delegate
			{
				DebugAbandon(localDef, GenWorld.MouseTile(), localDef.stackLimit);
			}));
			for (int num = 50; num <= 1000; num += 50)
			{
				int localCount = num;
				debugActionNode.AddChild(new DebugActionNode(localDef.defName + " x" + num, DebugActionType.ToolWorld, delegate
				{
					DebugAbandon(localDef, GenWorld.MouseTile(), localCount);
				}));
			}
			list.Add(debugActionNode);
		}
		return list;
		static void DebugAbandon(ThingDef def, PlanetTile tile, int count)
		{
			while (count > 0)
			{
				Thing thing = ThingMaker.MakeThing(def);
				thing.stackCount = Mathf.Max(1, Mathf.Min(count, def.stackLimit));
				count -= thing.stackCount;
				thing.Notify_AbandonedAtTile(tile);
				thing.Destroy();
			}
		}
	}
}
