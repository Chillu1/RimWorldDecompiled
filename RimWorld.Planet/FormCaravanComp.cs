using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class FormCaravanComp : WorldObjectComp
	{
		public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan");

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
				if (Reform && (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map, countDormantPawnsAsHostile: true) || mapParent.Map.mapPawns.FreeColonistsSpawnedCount == 0))
				{
					return false;
				}
				return true;
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			MapParent mapParent = (MapParent)parent;
			if (!mapParent.HasMap)
			{
				yield break;
			}
			if (!Reform)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandFormCaravan".Translate();
				command_Action.defaultDesc = "CommandFormCaravanDesc".Translate();
				command_Action.icon = FormCaravanCommand;
				command_Action.hotKey = KeyBindingDefOf.Misc2;
				command_Action.tutorTag = "FormCaravan";
				command_Action.action = delegate
				{
					Find.WindowStack.Add(new Dialog_FormCaravan(mapParent.Map));
				};
				yield return command_Action;
			}
			else if (mapParent.Map.mapPawns.FreeColonistsSpawnedCount != 0)
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "CommandReformCaravan".Translate();
				command_Action2.defaultDesc = "CommandReformCaravanDesc".Translate();
				command_Action2.icon = FormCaravanCommand;
				command_Action2.hotKey = KeyBindingDefOf.Misc2;
				command_Action2.tutorTag = "ReformCaravan";
				command_Action2.action = delegate
				{
					Find.WindowStack.Add(new Dialog_FormCaravan(mapParent.Map, reform: true));
				};
				if (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map, countDormantPawnsAsHostile: true))
				{
					command_Action2.Disable("CommandReformCaravanFailHostilePawns".Translate());
				}
				yield return command_Action2;
			}
			if (!Prefs.DevMode)
			{
				yield break;
			}
			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "Dev: Show available exits";
			command_Action3.action = delegate
			{
				foreach (int item in CaravanExitMapUtility.AvailableExitTilesAt(mapParent.Map))
				{
					Find.WorldDebugDrawer.FlashTile(item, 0f, null, 10);
				}
			};
			yield return command_Action3;
		}
	}
}
