using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_SpawnWorldObject : QuestPart
{
	public string inSignal;

	public WorldObject worldObject;

	public List<ThingDef> defsToExcludeFromHyperlinks;

	private bool spawned;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (worldObject != null)
			{
				yield return worldObject;
			}
		}
	}

	public override bool IncreasesPopulation
	{
		get
		{
			if (worldObject is Site { IncreasesPopulation: not false })
			{
				return true;
			}
			return false;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal) || spawned)
		{
			return;
		}
		PlanetTile result = worldObject.Tile;
		if (!result.Valid)
		{
			if (!TileFinder.TryFindNewSiteTile(out result))
			{
				result = PlanetTile.Invalid;
			}
		}
		else if (Find.WorldObjects.AnyWorldObjectAt(result) && !TileFinder.TryFindPassableTileWithTraversalDistance(result, 1, 50, out result, (PlanetTile x) => !Find.WorldObjects.AnyWorldObjectAt(x), ignoreFirstTilePassability: false, TileFinderMode.Near))
		{
			result = PlanetTile.Invalid;
		}
		if (result.Valid)
		{
			worldObject.Tile = result;
			Find.WorldObjects.Add(worldObject);
			spawned = true;
		}
	}

	public override void PostQuestAdded()
	{
		base.PostQuestAdded();
		if (!(worldObject is Site site))
		{
			return;
		}
		for (int i = 0; i < site.parts.Count; i++)
		{
			if (site.parts[i].things == null)
			{
				continue;
			}
			for (int j = 0; j < site.parts[i].things.Count; j++)
			{
				if (site.parts[i].things[j].def == ThingDefOf.PsychicAmplifier)
				{
					Find.History.Notify_PsylinkAvailable();
					return;
				}
			}
		}
	}

	public override void Cleanup()
	{
		if (!spawned)
		{
			worldObject.Destroy();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving && spawned && worldObject != null && (worldObject.Destroyed || !worldObject.Spawned))
		{
			worldObject = null;
		}
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref spawned, "spawned", defaultValue: false);
		Scribe_Collections.Look(ref defsToExcludeFromHyperlinks, "defsToExcludeFromHyperlinks", LookMode.Def);
		if (spawned)
		{
			Scribe_References.Look(ref worldObject, "worldObject");
		}
		else
		{
			Scribe_Deep.Look(ref worldObject, "worldObject");
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (TileFinder.TryFindNewSiteTile(out var tile))
		{
			worldObject = SiteMaker.MakeSite((SitePartDef)null, tile, (Faction)null, ifHostileThenMustRemainHostile: true, (float?)null, (WorldObjectDef)null);
		}
	}
}
