using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Install : Designator_Place
	{
		private Thing MiniToInstallOrBuildingToReinstall
		{
			get
			{
				Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
				if (singleSelectedThing is MinifiedThing)
				{
					return singleSelectedThing;
				}
				Building building = singleSelectedThing as Building;
				if (building != null && building.def.Minifiable)
				{
					return singleSelectedThing;
				}
				return null;
			}
		}

		private Thing ThingToInstall => MiniToInstallOrBuildingToReinstall.GetInnerIfMinified();

		protected override bool DoTooltip => true;

		public override BuildableDef PlacingDef => ThingToInstall.def;

		public override string Label
		{
			get
			{
				if (MiniToInstallOrBuildingToReinstall is MinifiedThing)
				{
					return "CommandInstall".Translate();
				}
				return "CommandReinstall".Translate();
			}
		}

		public override string Desc
		{
			get
			{
				if (MiniToInstallOrBuildingToReinstall is MinifiedThing)
				{
					return "CommandInstallDesc".Translate();
				}
				return "CommandReinstallDesc".Translate();
			}
		}

		public override Color IconDrawColor => Color.white;

		public override bool Visible
		{
			get
			{
				if (Find.Selector.SingleSelectedThing == null)
				{
					return false;
				}
				return base.Visible;
			}
		}

		public Designator_Install()
		{
			icon = TexCommand.Install;
			iconProportions = new Vector2(1f, 1f);
			order = -10f;
		}

		public override bool CanRemainSelected()
		{
			return MiniToInstallOrBuildingToReinstall != null;
		}

		public override void ProcessInput(Event ev)
		{
			Thing miniToInstallOrBuildingToReinstall = MiniToInstallOrBuildingToReinstall;
			if (miniToInstallOrBuildingToReinstall != null)
			{
				InstallBlueprintUtility.CancelBlueprintsFor(miniToInstallOrBuildingToReinstall);
				if (!((ThingDef)PlacingDef).rotatable)
				{
					placingRot = Rot4.North;
				}
			}
			base.ProcessInput(ev);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!(MiniToInstallOrBuildingToReinstall is MinifiedThing) && c.GetThingList(base.Map).Find((Thing x) => x.Position == c && x.Rotation == placingRot && x.def == PlacingDef) != null)
			{
				return new AcceptanceReport("IdenticalThingExists".Translate());
			}
			return GenConstruct.CanPlaceBlueprintAt(PlacingDef, c, placingRot, base.Map, godMode: false, MiniToInstallOrBuildingToReinstall, ThingToInstall);
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			GenSpawn.WipeExistingThings(c, placingRot, PlacingDef.installBlueprintDef, base.Map, DestroyMode.Deconstruct);
			MinifiedThing minifiedThing = MiniToInstallOrBuildingToReinstall as MinifiedThing;
			if (minifiedThing != null)
			{
				GenConstruct.PlaceBlueprintForInstall(minifiedThing, c, base.Map, placingRot, Faction.OfPlayer);
			}
			else
			{
				GenConstruct.PlaceBlueprintForReinstall((Building)MiniToInstallOrBuildingToReinstall, c, base.Map, placingRot, Faction.OfPlayer);
			}
			MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, placingRot, PlacingDef.Size), base.Map);
			Find.DesignatorManager.Deselect();
		}

		protected override void DrawGhost(Color ghostCol)
		{
			ThingDef def;
			if ((def = (PlacingDef as ThingDef)) != null)
			{
				MeditationUtility.DrawMeditationFociAffectedByBuildingOverlay(base.Map, def, Faction.OfPlayer, UI.MouseCell(), placingRot);
			}
			Graphic baseGraphic = ThingToInstall.Graphic.ExtractInnerGraphicFor(ThingToInstall);
			GhostDrawer.DrawGhostThing(UI.MouseCell(), placingRot, (ThingDef)PlacingDef, baseGraphic, ghostCol, AltitudeLayer.Blueprint, ThingToInstall);
		}

		protected override bool CanDrawNumbersBetween(Thing thing, ThingDef def, IntVec3 a, IntVec3 b, Map map)
		{
			if (ThingToInstall != thing)
			{
				return !GenThing.CloserThingBetween(def, a, b, map, ThingToInstall);
			}
			return false;
		}

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			BuildDesignatorUtility.TryDrawPowerGridAndAnticipatedConnection(PlacingDef, placingRot);
		}
	}
}
