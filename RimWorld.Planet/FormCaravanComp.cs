using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public class FormCaravanComp : WorldObjectComp
{
	public CellRect? foggedRoomsCheckRect;

	public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan");

	private bool anyActiveThreatLastTick;

	public WorldObjectCompProperties_FormCaravan Props => (WorldObjectCompProperties_FormCaravan)props;

	private MapParent MapParent => (MapParent)parent;

	public bool Reform
	{
		get
		{
			if (MapParent.HasMap)
			{
				return !MapParent.Map.IsPlayerHome;
			}
			return true;
		}
	}

	public bool CanFormOrReformCaravanNow
	{
		get
		{
			MapParent mapParent = MapParent;
			if (!mapParent.HasMap)
			{
				return false;
			}
			if (Reform)
			{
				if (AnyActiveThreatNow)
				{
					return false;
				}
				List<Pawn> freeColonistsSpawned = mapParent.Map.mapPawns.FreeColonistsSpawned;
				for (int i = 0; i < freeColonistsSpawned.Count; i++)
				{
					MentalState mentalState = freeColonistsSpawned[i].MentalState;
					if (mentalState != null && mentalState.def.category == MentalStateCategory.Aggro)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool AnyActiveThreatNow
	{
		get
		{
			if (MapParent.HasMap)
			{
				return GenHostility.AnyHostileActiveThreatToPlayer(MapParent.Map, countDormantPawnsAsHostile: true, !parent.CanReformFoggedEnemies);
			}
			return false;
		}
	}

	public bool AnyUnexploredFoggedRooms
	{
		get
		{
			if (!foggedRoomsCheckRect.HasValue)
			{
				return false;
			}
			MapParent mapParent = MapParent;
			if (!mapParent.HasMap)
			{
				return false;
			}
			IReadOnlyList<Room> allRooms = mapParent.Map.regionGrid.AllRooms;
			CellRect value = foggedRoomsCheckRect.Value;
			for (int i = 0; i < allRooms.Count; i++)
			{
				if (!allRooms[i].Fogged || !allRooms[i].ProperRoom)
				{
					continue;
				}
				foreach (IntVec3 cell in allRooms[i].Cells)
				{
					if (value.Contains(cell))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public override void CompTickInterval(int delta)
	{
		base.CompTickInterval(delta);
		bool anyActiveThreatNow = AnyActiveThreatNow;
		if (!anyActiveThreatNow && anyActiveThreatLastTick && Reform && CanFormOrReformCaravanNow)
		{
			if (AnyUnexploredFoggedRooms)
			{
				Messages.Message("MessageCanReformCaravanNowNoMoreEnemiesButUnexploredAreas".Translate(), new LookTargets(parent), MessageTypeDefOf.SituationResolved, historical: false);
			}
			else
			{
				Messages.Message("MessageCanReformCaravanNowNoMoreEnemies".Translate(), new LookTargets(parent), MessageTypeDefOf.NeutralEvent, historical: false);
			}
		}
		anyActiveThreatLastTick = anyActiveThreatNow;
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref anyActiveThreatLastTick, "anyActiveThreatLastTick", defaultValue: false);
		Scribe_Values.Look(ref foggedRoomsCheckRect, "foggedRoomsCheckRect");
	}

	public bool CanReformNow()
	{
		MapParent mapParent = (MapParent)parent;
		if (MapParent.HasMap && Reform && CanFormOrReformCaravanNow)
		{
			return mapParent.Map.mapPawns.FreeColonistsSpawnedCount != 0;
		}
		return false;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		MapParent mapParent = (MapParent)parent;
		if (!mapParent.HasMap)
		{
			yield break;
		}
		if (Reform && CanFormOrReformCaravanNow && mapParent.Map.mapPawns.FreeColonistsSpawnedCount != 0)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandReformCaravan".Translate();
			command_Action.defaultDesc = "CommandReformCaravanDesc".Translate();
			command_Action.icon = FormCaravanCommand;
			command_Action.hotKey = KeyBindingDefOf.Misc2;
			command_Action.tutorTag = "ReformCaravan";
			command_Action.action = delegate
			{
				if (ModsConfig.OdysseyActive && mapParent.Map.listerThings.ThingsOfDef(ThingDefOf.GravEngine).Any())
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmLoseGravship".Translate(), Form));
				}
				else if (ModsConfig.OdysseyActive && mapParent.Map.listerThings.ThingsInGroup(ThingRequestGroup.PassengerShuttle).Any())
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmLoseShuttle".Translate(), Form));
				}
				else
				{
					Form();
				}
			};
			if (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map, countDormantPawnsAsHostile: true))
			{
				command_Action.Disable("CommandReformCaravanFailHostilePawns".Translate());
			}
			yield return command_Action;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		Command_Action command_Action2 = new Command_Action();
		command_Action2.defaultLabel = "DEV: Show available exits";
		command_Action2.action = delegate
		{
			foreach (PlanetTile item in CaravanExitMapUtility.AvailableExitTilesAt(mapParent.Map))
			{
				Find.WorldDebugDrawer.FlashTile(item, 0f, null, 10);
			}
		};
		yield return command_Action2;
		void Form()
		{
			Find.WindowStack.Add(new Dialog_FormCaravan(mapParent.Map, reform: true));
		}
	}
}
