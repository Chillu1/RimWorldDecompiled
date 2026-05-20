using System.Collections.Generic;
using System.Xml;
using RimWorld;
using Verse.Utility;

namespace Verse;

public class GenStep_ScatterGroupPrefabs : GenStep_Scatterer
{
	private struct PrefabSpawn
	{
		public IntVec3 pos;

		public Rot4 rotation;

		public CellRect occupiedRect;

		public PrefabDef def;

		public ScatterGroup sourceGroup;
	}

	public class ScatterGroup
	{
		public PrefabDef prefab;

		public float weight = 1f;

		public int max = -1;

		public bool indoorRuin;

		public ThingDef filthDef;

		public int filthExpandBy;

		public float filthChance = 0.5f;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "prefab", "weight");
		}
	}

	public List<ScatterGroup> groups;

	[Unsaved(false)]
	private bool dontChooseIndoor;

	private static readonly List<PrefabSpawn> tmpSpawns = new List<PrefabSpawn>();

	private static readonly List<ScatterGroup> tmpScatterGroups = new List<ScatterGroup>();

	private static readonly Dictionary<ScatterGroup, float> tmpWeightedPrefabsRandom = new Dictionary<ScatterGroup, float>();

	private static readonly List<IntVec3> tmpCellsRandomOrderWorkingList = new List<IntVec3>();

	private static readonly List<Rot4> tmpPossibleRotations = new List<Rot4>();

	public override int SeedPart => 79797732;

	public override void Generate(Map map, GenStepParams parms)
	{
		try
		{
			base.Generate(map, parms);
		}
		finally
		{
			dontChooseIndoor = false;
			tmpSpawns.Clear();
		}
	}

	private ScatterGroup GetGroup()
	{
		tmpScatterGroups.Clear();
		foreach (ScatterGroup group in groups)
		{
			if ((!dontChooseIndoor || !group.indoorRuin) && (group.max <= 0 || tmpSpawns.Count((PrefabSpawn spawn) => spawn.def == group.prefab) < group.max))
			{
				tmpScatterGroups.Add(group);
			}
		}
		if (tmpScatterGroups.Empty())
		{
			return null;
		}
		Rand.PushState(Gen.HashCombineInt(Find.World.info.Seed, usedSpots.Count));
		try
		{
			return tmpScatterGroups.RandomElementByWeight((ScatterGroup g) => g.weight);
		}
		finally
		{
			Rand.PopState();
		}
	}

	protected override bool TryFindScatterCell(Map map, out IntVec3 result)
	{
		bool flag = warnOnFail;
		warnOnFail = false;
		try
		{
			dontChooseIndoor = false;
			ScatterGroup scatterGroup = GetGroup();
			if (scatterGroup == null)
			{
				result = default(IntVec3);
				return false;
			}
			if (scatterGroup.indoorRuin)
			{
				if (base.TryFindScatterCell(map, out result))
				{
					return true;
				}
				dontChooseIndoor = true;
			}
			if (GetGroup() != null && base.TryFindScatterCell(map, out result))
			{
				return true;
			}
		}
		finally
		{
			warnOnFail = flag;
		}
		if (warnOnFail)
		{
			Log.Warning("Scatterer " + ToString() + " from def " + def?.defName + " could not find cell to generate at.");
		}
		result = default(IntVec3);
		return false;
	}

	private bool CalculateScatterInformation(IntVec3 loc, Map map, out PrefabSpawn details)
	{
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		tmpScatterGroups.Clear();
		ScatterGroup scatterGroup = GetGroup();
		if (scatterGroup == null)
		{
			details = default(PrefabSpawn);
			return false;
		}
		bool flag = false;
		foreach (ScatterGroup tmpScatterGroup in tmpScatterGroups)
		{
			WritePossibleRotations(tmpScatterGroup.prefab);
			foreach (Rot4 tmpPossibleRotation in tmpPossibleRotations)
			{
				if (CanSpawn(tmpScatterGroup.prefab, loc, tmpPossibleRotation))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		tmpWeightedPrefabsRandom.Clear();
		foreach (ScatterGroup tmpScatterGroup2 in tmpScatterGroups)
		{
			tmpWeightedPrefabsRandom.Add(tmpScatterGroup2, tmpScatterGroup2.weight * Rand.Value);
		}
		tmpScatterGroups.SortByDescending((ScatterGroup e) => tmpWeightedPrefabsRandom[e]);
		CellRect cellRect = loc.RectAbout(new IntVec2(20, 20)).ClipInsideMap(map);
		foreach (ScatterGroup t in tmpScatterGroups)
		{
			if (t.max > 0 && tmpSpawns.Count((PrefabSpawn spawn) => spawn.def == t.prefab) >= t.max)
			{
				continue;
			}
			WritePossibleRotations(t.prefab);
			foreach (IntVec3 item in cellRect.Cells.InRandomOrder(tmpCellsRandomOrderWorkingList))
			{
				foreach (Rot4 tmpPossibleRotation2 in tmpPossibleRotations)
				{
					CellRect occupiedRect = GenAdj.OccupiedRect(item, tmpPossibleRotation2, t.prefab.size);
					if (CanSpawn(t.prefab, item, tmpPossibleRotation2))
					{
						details = new PrefabSpawn
						{
							def = t.prefab,
							occupiedRect = occupiedRect,
							pos = item,
							rotation = tmpPossibleRotation2,
							sourceGroup = scatterGroup
						};
						return true;
					}
				}
			}
		}
		details = default(PrefabSpawn);
		return false;
		bool CanSpawn(PrefabDef prefab, IntVec3 cell, Rot4 rot)
		{
			CellRect other = GenAdj.OccupiedRect(cell, rot, prefab.size);
			if (!other.InBounds(map))
			{
				return false;
			}
			foreach (PrefabSpawn tmpSpawn in tmpSpawns)
			{
				if (tmpSpawn.occupiedRect.Overlaps(other))
				{
					return false;
				}
			}
			foreach (CellRect item2 in usedRects)
			{
				if (item2.Overlaps(other))
				{
					return false;
				}
			}
			foreach (IntVec3 item3 in other)
			{
				if (!item3.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy))
				{
					return false;
				}
				if (useFallback)
				{
					if (fallbackValidators != null)
					{
						for (int i = 0; i < fallbackValidators.Count; i++)
						{
							if (!fallbackValidators[i].Allows(loc, map))
							{
								return false;
							}
						}
					}
				}
				else if (validators != null)
				{
					for (int j = 0; j < validators.Count; j++)
					{
						if (!validators[j].Allows(loc, map))
						{
							return false;
						}
					}
				}
				foreach (Thing thing in item3.GetThingList(map))
				{
					if (thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Pawn || thing.def.IsEdifice() || !thing.def.destroyable)
					{
						return false;
					}
				}
			}
			return true;
		}
		static void WritePossibleRotations(PrefabDef prefab)
		{
			tmpPossibleRotations.Clear();
			foreach (RotEnum bitFlag in prefab.rotations.GetBitFlags())
			{
				tmpPossibleRotations.Add(bitFlag);
			}
			tmpPossibleRotations.Shuffle();
		}
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		using (Rand.Block(map.NextGenSeed))
		{
			if (!CalculateScatterInformation(loc, map, out var details) && warnOnFail)
			{
				Log.Warning($"Tried scattering group from def {def.defName} at {loc} on map {map} which is an invalid location!");
				return;
			}
			if (PrefabUtility.CanSpawnPrefab(details.def, map, details.pos, details.rotation))
			{
				PrefabUtility.SpawnPrefab(details.def, map, details.pos, details.rotation);
				tmpSpawns.Add(details);
			}
			if (details.sourceGroup.filthDef == null)
			{
				return;
			}
			foreach (IntVec3 item in details.occupiedRect.ExpandedBy(details.sourceGroup.filthExpandBy))
			{
				if (Rand.Chance(details.sourceGroup.filthChance) && item.InBounds(map))
				{
					FilthMaker.TryMakeFilth(item, map, details.sourceGroup.filthDef);
				}
			}
		}
	}

	protected override bool CanScatterAt(IntVec3 loc, Map map)
	{
		if (!base.CanScatterAt(loc, map))
		{
			return false;
		}
		using (Rand.Block(map.NextGenSeed))
		{
			PrefabSpawn details;
			return CalculateScatterInformation(loc, map, out details);
		}
	}
}
