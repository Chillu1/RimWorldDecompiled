using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class ArchotechSettlement : Settlement
{
	public List<Building> existingBuildings = new List<Building>();

	private IEnumerable<Building> AllStudiableArchotechBuildings
	{
		get
		{
			foreach (Building item in base.Map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.GrandArchotechStructure))
			{
				yield return item;
			}
			foreach (Building item2 in base.Map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.MajorArchotechStructureStudiable))
			{
				yield return item2;
			}
		}
	}

	public bool AnyArchotechBuildingRequiresStudy
	{
		get
		{
			foreach (Building allStudiableArchotechBuilding in AllStudiableArchotechBuildings)
			{
				CompStudiable compStudiable = allStudiableArchotechBuilding.TryGetComp<CompStudiable>();
				if (compStudiable != null && !compStudiable.Completed)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override void Abandon(bool wasGravshipLaunch)
	{
		AbandonedArchotechStructures abandonedArchotechStructures = (AbandonedArchotechStructures)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.AbandonedArchotechStructures);
		abandonedArchotechStructures.Tile = base.Tile;
		abandonedArchotechStructures.worldObjectDef = def;
		abandonedArchotechStructures.archotechStructures.AddRange(AllStudiableArchotechBuildings);
		foreach (Building archotechStructure in abandonedArchotechStructures.archotechStructures)
		{
			archotechStructure.DeSpawn();
		}
		Find.WorldObjects.Add(abandonedArchotechStructures);
		Find.LetterStack.ReceiveLetter("LetterLabelArchotechStructuresAbandoned".Translate(), "LetterTextArchotechStructuresAbandoned".Translate(this), LetterDefOf.NeutralEvent, abandonedArchotechStructures);
		Destroy();
	}

	public override void PostMapGenerate()
	{
		base.PostMapGenerate();
		foreach (Building existingBuilding in existingBuildings)
		{
			Building building = base.Map.listerBuildings.AllBuildingsNonColonistOfDef(existingBuilding.def).FirstOrDefault((Building b) => !existingBuildings.Contains(b));
			if (building != null)
			{
				IntVec3 position = building.Position;
				building.DeSpawn();
				GenSpawn.Spawn(existingBuilding, position, base.Map);
			}
			else
			{
				existingBuilding.Destroy();
			}
		}
		existingBuildings.Clear();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref existingBuildings, "existingBuildings", LookMode.Deep);
	}
}
