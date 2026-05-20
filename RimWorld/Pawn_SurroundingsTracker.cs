using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Pawn_SurroundingsTracker : IExposable
{
	public Pawn pawn;

	public List<TreeSighting> miniTreeSightings = new List<TreeSighting>();

	public List<TreeSighting> fullTreeSightings = new List<TreeSighting>();

	public List<TreeSighting> superTreeSightings = new List<TreeSighting>();

	public List<SkullspikeSighting> skullspikeSightings = new List<SkullspikeSighting>();

	private const int MaxSightingsPerCategory = 15;

	private const int TreeCheckInterval = 2500;

	public Pawn_SurroundingsTracker()
	{
	}

	public Pawn_SurroundingsTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	private int NumSightingsInRange(ref List<TreeSighting> sightings, int ticks)
	{
		int num = 0;
		for (int i = 0; i < sightings.Count; i++)
		{
			if (sightings[i].TicksSinceSighting <= ticks)
			{
				num++;
			}
		}
		return num;
	}

	public int NumSightingsInRange(TreeCategory treeCategory, int ticks)
	{
		return treeCategory switch
		{
			TreeCategory.Mini => NumSightingsInRange(ref miniTreeSightings, ticks), 
			TreeCategory.Full => NumSightingsInRange(ref fullTreeSightings, ticks), 
			TreeCategory.Super => NumSightingsInRange(ref superTreeSightings, ticks), 
			_ => 0, 
		};
	}

	public int NumSkullspikeSightings()
	{
		return skullspikeSightings.Count;
	}

	public void SurroundingsTrackerTickInterval(int delta)
	{
		if (pawn.IsHashIntervalTick(2500, delta) && ModsConfig.IdeologyActive && pawn.Ideo != null && pawn.Ideo.cachedPossibleSituationalThoughts.Contains(ThoughtDefOf.TreesDesired) && pawn.Awake())
		{
			if (pawn.Spawned)
			{
				GetSpawnedTreeSightings();
			}
			else
			{
				GetCaravanTreeSightings();
			}
		}
		if (!pawn.IsHashIntervalTick(60, delta) || !ModsConfig.IdeologyActive || pawn.Ideo == null || !pawn.Spawned || !pawn.Awake() || (!pawn.Ideo.cachedPossibleSituationalThoughts.Contains(ThoughtDefOf.Skullspike_Desired) && !pawn.Ideo.cachedPossibleSituationalThoughts.Contains(ThoughtDefOf.Skullspike_Disapproved)) || PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn))
		{
			return;
		}
		for (int num = skullspikeSightings.Count - 1; num >= 0; num--)
		{
			if (skullspikeSightings[num].TicksSinceSighting > 1800)
			{
				skullspikeSightings.RemoveAt(num);
			}
		}
		if (pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Skullspike).Count == 0)
		{
			return;
		}
		IntVec3 positionHeld = pawn.PositionHeld;
		int num2 = GenRadial.NumCellsInRadius(10f);
		for (int i = 0; i < num2; i++)
		{
			IntVec3 c = positionHeld + GenRadial.RadialPattern[i];
			if (!c.InBounds(pawn.Map) || c.Fogged(pawn.Map))
			{
				continue;
			}
			Thing firstThing = c.GetFirstThing(pawn.Map, ThingDefOf.Skullspike);
			if (firstThing == null || !GenSight.LineOfSight(pawn.PositionHeld, firstThing.PositionHeld, firstThing.Map, skipFirstCell: true))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < skullspikeSightings.Count; j++)
			{
				if (skullspikeSightings[j].skullspike == firstThing)
				{
					SkullspikeSighting value = skullspikeSightings[j];
					value.tickSighted = Find.TickManager.TicksGame;
					skullspikeSightings[j] = value;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				skullspikeSightings.Add(new SkullspikeSighting
				{
					skullspike = firstThing,
					tickSighted = Find.TickManager.TicksGame
				});
			}
		}
	}

	private void GetSpawnedTreeSightings()
	{
		foreach (TreeSighting item in IdeoUtility.TreeSightingsNearPawn(pawn.Position, pawn.Map, pawn.Ideo))
		{
			if (item.Tree != null)
			{
				_ = item.Tree.def.plant.treeCategory;
			}
			switch (item.Tree.def.plant.treeCategory)
			{
			case TreeCategory.Mini:
				AddSighting(ref miniTreeSightings, item);
				break;
			case TreeCategory.Full:
				AddSighting(ref fullTreeSightings, item);
				break;
			case TreeCategory.Super:
				AddSighting(ref superTreeSightings, item);
				break;
			}
		}
	}

	private void GetCaravanTreeSightings()
	{
		Caravan caravan = pawn.GetCaravan();
		if (caravan != null)
		{
			int treeSightingsPerHourFromCaravan = caravan.Biome.TreeSightingsPerHourFromCaravan;
			for (int i = 0; i < treeSightingsPerHourFromCaravan; i++)
			{
				AddSighting(ref fullTreeSightings, new TreeSighting(null, Find.TickManager.TicksGame));
			}
		}
	}

	private void AddSighting(ref List<TreeSighting> list, TreeSighting newSighting)
	{
		if (newSighting.tree != null)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].tree != null && list[num].tree == newSighting.tree)
				{
					list.RemoveAt(num);
				}
			}
		}
		list.Add(newSighting);
		for (int i = 0; i < list.Count - 15; i++)
		{
			list.RemoveAt(0);
		}
	}

	public void Clear()
	{
		miniTreeSightings.Clear();
		fullTreeSightings.Clear();
		superTreeSightings.Clear();
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref miniTreeSightings, "miniTreeSightings", LookMode.Deep);
		Scribe_Collections.Look(ref fullTreeSightings, "fullTreeSightings", LookMode.Deep);
		Scribe_Collections.Look(ref superTreeSightings, "superTreeSightings", LookMode.Deep);
	}
}
