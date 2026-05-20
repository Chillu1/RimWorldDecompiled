using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public static class ShipUtility
{
	private static Dictionary<ThingDef, int> requiredParts;

	private static List<Building> closedSet = new List<Building>();

	private static List<Building> openSet = new List<Building>();

	public static Dictionary<ThingDef, int> RequiredParts()
	{
		if (requiredParts == null)
		{
			requiredParts = new Dictionary<ThingDef, int>();
			requiredParts[ThingDefOf.Ship_CryptosleepCasket] = 1;
			requiredParts[ThingDefOf.Ship_ComputerCore] = 1;
			requiredParts[ThingDefOf.Ship_Reactor] = 1;
			requiredParts[ThingDefOf.Ship_Engine] = 3;
			requiredParts[ThingDefOf.Ship_Beam] = 1;
			requiredParts[ThingDefOf.Ship_SensorCluster] = 1;
		}
		return requiredParts;
	}

	public static IEnumerable<string> LaunchFailReasons(Building rootBuilding)
	{
		List<Building> shipParts = ShipBuildingsAttachedTo(rootBuilding).ToList();
		foreach (KeyValuePair<ThingDef, int> partDef in RequiredParts())
		{
			int num = shipParts.Count((Building pa) => pa.def == partDef.Key);
			if (num < partDef.Value)
			{
				yield return string.Format("{0}: {1}x {2} ({3} {4})", "ShipReportMissingPart".Translate(), partDef.Value - num, partDef.Key.label, "ShipReportMissingPartRequires".Translate(), partDef.Value);
			}
		}
		bool fullPodFound = false;
		foreach (Building item in shipParts)
		{
			if (item.def == ThingDefOf.Ship_CryptosleepCasket && item is Building_CryptosleepCasket { HasAnyContents: not false })
			{
				fullPodFound = true;
				break;
			}
		}
		foreach (Building item2 in shipParts)
		{
			CompHibernatable compHibernatable = item2.TryGetComp<CompHibernatable>();
			if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
			{
				yield return string.Format("{0}: {1}", "ShipReportHibernating".Translate(), item2.LabelCap);
			}
			else if (compHibernatable != null && !compHibernatable.Running)
			{
				yield return string.Format("{0}: {1}", "ShipReportNotReady".Translate(), item2.LabelCap);
			}
		}
		if (!fullPodFound)
		{
			yield return "ShipReportNoFullPods".Translate();
		}
	}

	public static bool HasHibernatingParts(Building rootBuilding)
	{
		foreach (Building item in ShipBuildingsAttachedTo(rootBuilding).ToList())
		{
			CompHibernatable compHibernatable = item.TryGetComp<CompHibernatable>();
			if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
			{
				return true;
			}
		}
		return false;
	}

	public static void StartupHibernatingParts(Building rootBuilding)
	{
		foreach (Building item in ShipBuildingsAttachedTo(rootBuilding).ToList())
		{
			CompHibernatable compHibernatable = item.TryGetComp<CompHibernatable>();
			if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
			{
				compHibernatable.Startup();
			}
		}
		SoundDefOf.ShipReactor_Startup.PlayOneShot(SoundInfo.InMap(rootBuilding));
	}

	public static List<Building> ShipBuildingsAttachedTo(Building root)
	{
		closedSet.Clear();
		if (root == null || root.Destroyed)
		{
			return closedSet;
		}
		openSet.Clear();
		openSet.Add(root);
		while (openSet.Count > 0)
		{
			Building building = openSet[openSet.Count - 1];
			openSet.Remove(building);
			closedSet.Add(building);
			foreach (IntVec3 item in GenAdj.CellsAdjacentCardinal(building))
			{
				Building edifice = item.GetEdifice(building.Map);
				if (edifice != null && edifice.def.building.shipPart && !closedSet.Contains(edifice) && !openSet.Contains(edifice))
				{
					openSet.Add(edifice);
				}
			}
		}
		return closedSet;
	}

	public static IEnumerable<Gizmo> ShipStartupGizmos(Building building)
	{
		if (!HasHibernatingParts(building))
		{
			yield break;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.action = delegate
		{
			string text = "HibernateWarning";
			if (building.Map.info.parent.GetComponent<EscapeShipComp>() == null)
			{
				text += "Standalone";
			}
			if (!Find.Storyteller.difficulty.allowBigThreats)
			{
				text += "Pacifist";
			}
			DiaNode diaNode = new DiaNode(text.Translate());
			DiaOption item = new DiaOption("Confirm".Translate())
			{
				action = delegate
				{
					StartupHibernatingParts(building);
				},
				resolveTree = true
			};
			diaNode.options.Add(item);
			DiaOption item2 = new DiaOption("GoBack".Translate())
			{
				resolveTree = true
			};
			diaNode.options.Add(item2);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
		};
		command_Action.defaultLabel = "CommandShipStartup".Translate();
		command_Action.defaultDesc = "CommandShipStartupDesc".Translate();
		command_Action.hotKey = KeyBindingDefOf.Misc1;
		command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower");
		yield return command_Action;
	}
}
