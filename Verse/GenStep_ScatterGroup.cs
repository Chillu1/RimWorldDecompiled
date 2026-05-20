using System.Collections.Generic;
using System.Xml;
using RimWorld;
using UnityEngine;

namespace Verse;

public class GenStep_ScatterGroup : GenStep_Scatterer
{
	private struct ThingSpawn
	{
		public IntVec3 pos;

		public Rot4 rotation;

		public CellRect occupiedRect;

		public ThingDef def;

		public int stackSize;

		public bool spawnForbidden;

		public ScatterGroup sourceGroup;
	}

	public class ThingWeight
	{
		public ThingDef thing;

		public float weight;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "thing", "weight");
		}
	}

	public class ScatterGroup
	{
		public List<ThingWeight> things;

		public IntRange stackSizeRange = IntRange.One;

		public bool spawnForbidden;

		public float weight = 1f;

		public ThingDef spawnAtCenter;

		public ThingDef spawnAtCenterFilthDef;

		public bool indoorRuin;

		public FloatRange coveredCellsPer10Cells;

		public ThingDef filthDef;

		public int filthExpandBy;

		public float filthChance = 0.5f;

		public IntRange clusterRectRadius;
	}

	public List<ScatterGroup> groups;

	[Unsaved(false)]
	private bool dontChooseIndoor;

	private static List<ThingSpawn> tmpSpawns = new List<ThingSpawn>();

	private static List<ScatterGroup> tmpScatterGroups = new List<ScatterGroup>();

	private static List<ThingWeight> tmpWeightedThings = new List<ThingWeight>();

	private static Dictionary<ThingWeight, float> tmpWeightedThingsRandom = new Dictionary<ThingWeight, float>();

	private static List<IntVec3> tmpCellsRandomOrderWorkingList = new List<IntVec3>();

	private static List<Rot4> tmpPossibleRotations = new List<Rot4>();

	public override int SeedPart => 1237834582;

	public override void Generate(Map map, GenStepParams parms)
	{
		try
		{
			base.Generate(map, parms);
		}
		finally
		{
			dontChooseIndoor = false;
		}
	}

	private bool IndoorRuinSpot(CellRect rect, Map map)
	{
		float sqrMagnitude = ((new Vector2(rect.minX, rect.minZ) - new Vector2(rect.maxX, rect.maxZ)) * 2f).sqrMagnitude;
		if (rect.CenterCell.GetTerrain(map).BuildableByPlayer)
		{
			foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.Wall))
			{
				if ((float)item.Position.DistanceToSquared(rect.CenterCell) < sqrMagnitude)
				{
					return true;
				}
			}
		}
		return false;
	}

	private ScatterGroup GetGroup(Map map)
	{
		tmpScatterGroups.Clear();
		foreach (ScatterGroup group in groups)
		{
			if (!dontChooseIndoor || !group.indoorRuin)
			{
				tmpScatterGroups.Add(group);
			}
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
			if (GetGroup(map).indoorRuin)
			{
				if (base.TryFindScatterCell(map, out result))
				{
					return true;
				}
				dontChooseIndoor = true;
			}
			if (GetGroup(map) != null && base.TryFindScatterCell(map, out result))
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
			Log.Warning("Scatterer " + ToString() + " from def " + def.defName + " could not find cell to generate at.");
		}
		result = default(IntVec3);
		return false;
	}

	private bool CalculateScatterInformation(IntVec3 loc, Map map, out CellRect rect, List<ThingSpawn> outThingSpawns)
	{
		tmpWeightedThings.Clear();
		ScatterGroup scatterGroup = GetGroup(map);
		rect = CellRect.CenteredOn(loc, scatterGroup.clusterRectRadius.RandomInRange);
		CellRect localRect = rect;
		if (!rect.InBounds(map))
		{
			return false;
		}
		if (scatterGroup.indoorRuin != IndoorRuinSpot(rect, map))
		{
			return false;
		}
		if (!scatterGroup.things.NullOrEmpty())
		{
			tmpWeightedThings.AddRange(scatterGroup.things);
		}
		if (scatterGroup.spawnAtCenter != null)
		{
			if (!CanSpawn(scatterGroup.spawnAtCenter, rect.CenterCell, Rot4.North, GenAdj.OccupiedRect(rect.CenterCell, Rot4.North, scatterGroup.spawnAtCenter.size)))
			{
				return false;
			}
			outThingSpawns.Add(new ThingSpawn
			{
				def = scatterGroup.spawnAtCenter,
				occupiedRect = GenAdj.OccupiedRect(rect.CenterCell, Rot4.North, scatterGroup.spawnAtCenter.size),
				pos = rect.CenterCell,
				rotation = Rot4.North,
				sourceGroup = scatterGroup,
				stackSize = 1,
				spawnForbidden = false
			});
			if (scatterGroup.spawnAtCenterFilthDef != null)
			{
				outThingSpawns.Add(new ThingSpawn
				{
					def = scatterGroup.spawnAtCenterFilthDef,
					occupiedRect = GenAdj.OccupiedRect(rect.CenterCell, Rot4.North, scatterGroup.spawnAtCenterFilthDef.size),
					pos = rect.CenterCell,
					rotation = Rot4.North,
					sourceGroup = scatterGroup
				});
			}
		}
		int num = 0;
		foreach (IntVec3 item in rect)
		{
			bool flag = false;
			foreach (ThingWeight tmpWeightedThing in tmpWeightedThings)
			{
				WritePossibleRotations(tmpWeightedThing.thing);
				foreach (Rot4 tmpPossibleRotation in tmpPossibleRotations)
				{
					if (CanSpawn(tmpWeightedThing.thing, item, tmpPossibleRotation, GenAdj.OccupiedRect(item, tmpPossibleRotation, tmpWeightedThing.thing.size)))
					{
						num++;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		FloatRange coveredCellsPer10Cells = scatterGroup.coveredCellsPer10Cells;
		int num2 = (int)((float)num / 10f * coveredCellsPer10Cells.TrueMin);
		int num3 = (int)((float)num / 10f * coveredCellsPer10Cells.TrueMax);
		int num4 = (int)((float)num / 10f * coveredCellsPer10Cells.RandomInRange);
		int num5 = 0;
		while (num5 < num4)
		{
			tmpWeightedThingsRandom.Clear();
			foreach (ThingWeight tmpWeightedThing2 in tmpWeightedThings)
			{
				tmpWeightedThingsRandom.Add(tmpWeightedThing2, tmpWeightedThing2.weight * Rand.Value);
			}
			tmpWeightedThings.SortByDescending((ThingWeight e) => tmpWeightedThingsRandom[e]);
			bool flag2 = false;
			foreach (ThingWeight tmpWeightedThing3 in tmpWeightedThings)
			{
				if (num5 + tmpWeightedThing3.thing.size.Area > num3)
				{
					foreach (ThingWeight tmpWeightedThing4 in tmpWeightedThings)
					{
						if (tmpWeightedThing4 != tmpWeightedThing3 && num5 + tmpWeightedThing4.thing.size.Area <= num3)
						{
							break;
						}
					}
				}
				WritePossibleRotations(tmpWeightedThing3.thing);
				foreach (IntVec3 item2 in rect.Cells.InRandomOrder(tmpCellsRandomOrderWorkingList))
				{
					if (num5 + tmpWeightedThing3.thing.size.Area > num3)
					{
						break;
					}
					foreach (Rot4 tmpPossibleRotation2 in tmpPossibleRotations)
					{
						CellRect occupiedRect = GenAdj.OccupiedRect(item2, tmpPossibleRotation2, tmpWeightedThing3.thing.size);
						if (CanSpawn(tmpWeightedThing3.thing, item2, tmpPossibleRotation2, occupiedRect))
						{
							outThingSpawns.Add(new ThingSpawn
							{
								def = tmpWeightedThing3.thing,
								occupiedRect = occupiedRect,
								pos = item2,
								rotation = tmpPossibleRotation2,
								sourceGroup = scatterGroup,
								stackSize = scatterGroup.stackSizeRange.RandomInRange,
								spawnForbidden = scatterGroup.spawnForbidden
							});
							num5 += occupiedRect.Area;
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
				if (flag2)
				{
					break;
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		return num5 >= num2;
		bool CanSpawn(ThingDef def, IntVec3 cell, Rot4 rot, CellRect other)
		{
			if (!other.InBounds(map) || !other.FullyContainedWithin(localRect))
			{
				return false;
			}
			foreach (ThingSpawn outThingSpawn in outThingSpawns)
			{
				if (outThingSpawn.occupiedRect.Overlaps(other))
				{
					return false;
				}
			}
			if (GenSpawn.WouldWipeAnythingWith(cell, rot, def, map, (Thing x) => x.def == def || (x.def.category != ThingCategory.Plant && x.def.category != ThingCategory.Filth)))
			{
				return false;
			}
			foreach (IntVec3 item3 in other)
			{
				if (useFallback)
				{
					if (fallbackValidators != null)
					{
						for (int num6 = 0; num6 < fallbackValidators.Count; num6++)
						{
							if (!fallbackValidators[num6].Allows(loc, map))
							{
								return false;
							}
						}
					}
				}
				else if (validators != null)
				{
					for (int num7 = 0; num7 < validators.Count; num7++)
					{
						if (!validators[num7].Allows(loc, map))
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
		static void WritePossibleRotations(ThingDef thing)
		{
			tmpPossibleRotations.Clear();
			if (thing.rotatable)
			{
				tmpPossibleRotations.Add(Rot4.North);
				tmpPossibleRotations.Add(Rot4.East);
				tmpPossibleRotations.Add(Rot4.South);
				tmpPossibleRotations.Add(Rot4.West);
			}
			else
			{
				tmpPossibleRotations.Add(Rot4.North);
			}
			tmpPossibleRotations.Shuffle();
		}
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		using (Rand.Block(map.NextGenSeed))
		{
			try
			{
				if (!CalculateScatterInformation(loc, map, out var _, tmpSpawns) && warnOnFail)
				{
					Log.Warning($"Tried scattering group from def {def.defName} at {loc} on map {map} which is an invalid location!");
					return;
				}
				foreach (ThingSpawn tmpSpawn in tmpSpawns)
				{
					if (tmpSpawn.def.IsFilth)
					{
						FilthMaker.TryMakeFilth(tmpSpawn.pos, map, tmpSpawn.def);
					}
					else if (GenSpawn.CanSpawnAt(tmpSpawn.def, tmpSpawn.pos, map, tmpSpawn.rotation))
					{
						Thing thing = ThingMaker.MakeThing(tmpSpawn.def);
						thing.stackCount = tmpSpawn.stackSize;
						GenSpawn.Spawn(thing, tmpSpawn.pos, map, tmpSpawn.rotation);
						if (tmpSpawn.spawnForbidden && thing.HasComp<CompForbiddable>())
						{
							thing.SetForbidden(value: true);
						}
					}
					if (tmpSpawn.sourceGroup.filthDef == null || tmpSpawn.def.IsFilth)
					{
						continue;
					}
					foreach (IntVec3 item in tmpSpawn.occupiedRect.ExpandedBy(tmpSpawn.sourceGroup.filthExpandBy))
					{
						if (Rand.Chance(tmpSpawn.sourceGroup.filthChance) && item.InBounds(map))
						{
							FilthMaker.TryMakeFilth(item, map, tmpSpawn.sourceGroup.filthDef);
						}
					}
				}
			}
			finally
			{
				tmpSpawns.Clear();
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
			try
			{
				CellRect rect;
				return CalculateScatterInformation(loc, map, out rect, tmpSpawns);
			}
			finally
			{
				tmpSpawns.Clear();
			}
		}
	}
}
