using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Blueprint_Install : Blueprint
	{
		private MinifiedThing miniToInstall;

		private Building buildingToReinstall;

		public Thing MiniToInstallOrBuildingToReinstall
		{
			get
			{
				if (miniToInstall != null)
				{
					return miniToInstall;
				}
				if (buildingToReinstall != null)
				{
					return buildingToReinstall;
				}
				throw new InvalidOperationException("Nothing to install.");
			}
		}

		public Thing ThingToInstall => MiniToInstallOrBuildingToReinstall.GetInnerIfMinified();

		public override Graphic Graphic => ThingToInstall.def.installBlueprintDef.graphic.ExtractInnerGraphicFor(ThingToInstall);

		protected override float WorkTotal => 150f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref miniToInstall, "miniToInstall");
			Scribe_References.Look(ref buildingToReinstall, "buildingToReinstall");
		}

		public override ThingDef EntityToBuildStuff()
		{
			return ThingToInstall.Stuff;
		}

		public override List<ThingDefCountClass> MaterialsNeeded()
		{
			Log.Error("Called MaterialsNeeded on a Blueprint_Install.");
			return new List<ThingDefCountClass>();
		}

		protected override Thing MakeSolidThing()
		{
			Thing thingToInstall = ThingToInstall;
			if (miniToInstall != null)
			{
				miniToInstall.InnerThing = null;
				miniToInstall.Destroy();
			}
			return thingToInstall;
		}

		public override bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
		{
			Map map = base.Map;
			bool num = base.TryReplaceWithSolidThing(workerPawn, out createdThing, out jobEnded);
			if (num)
			{
				SoundDefOf.Building_Complete.PlayOneShot(new TargetInfo(base.Position, map));
				workerPawn.records.Increment(RecordDefOf.ThingsInstalled);
			}
			return num;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Command command = BuildCopyCommandUtility.BuildCopyCommand(ThingToInstall.def, ThingToInstall.Stuff);
			if (command != null)
			{
				yield return command;
			}
			if (base.Faction != Faction.OfPlayer)
			{
				yield break;
			}
			foreach (Command item in BuildFacilityCommandUtility.BuildFacilityCommands(ThingToInstall.def))
			{
				yield return item;
			}
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (buildingToReinstall != null)
			{
				GenDraw.DrawLineBetween(buildingToReinstall.TrueCenter(), this.TrueCenter());
			}
			if (ThingToInstall.def.drawPlaceWorkersWhileInstallBlueprintSelected && ThingToInstall.def.PlaceWorkers != null)
			{
				List<PlaceWorker> placeWorkers = ThingToInstall.def.PlaceWorkers;
				for (int i = 0; i < placeWorkers.Count; i++)
				{
					placeWorkers[i].DrawGhost(ThingToInstall.def, base.Position, base.Rotation, Color.white, ThingToInstall);
				}
			}
		}

		internal void SetThingToInstallFromMinified(MinifiedThing itemToInstall)
		{
			miniToInstall = itemToInstall;
			buildingToReinstall = null;
		}

		internal void SetBuildingToReinstall(Building buildingToReinstall)
		{
			if (!buildingToReinstall.def.Minifiable)
			{
				Log.Error("Tried to reinstall non-minifiable building.");
				return;
			}
			miniToInstall = null;
			this.buildingToReinstall = buildingToReinstall;
		}
	}
}
