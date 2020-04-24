using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompForbiddable : ThingComp
	{
		private bool forbiddenInt;

		public bool Forbidden
		{
			get
			{
				return forbiddenInt;
			}
			set
			{
				if (value == forbiddenInt)
				{
					return;
				}
				forbiddenInt = value;
				if (parent.Spawned)
				{
					if (forbiddenInt)
					{
						parent.Map.listerHaulables.Notify_Forbidden(parent);
						parent.Map.listerMergeables.Notify_Forbidden(parent);
					}
					else
					{
						parent.Map.listerHaulables.Notify_Unforbidden(parent);
						parent.Map.listerMergeables.Notify_Unforbidden(parent);
					}
					if (parent is Building_Door)
					{
						parent.Map.reachability.ClearCache();
					}
				}
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref forbiddenInt, "forbidden", defaultValue: false);
		}

		public override void PostDraw()
		{
			if (!forbiddenInt)
			{
				return;
			}
			if (parent is Blueprint || parent is Frame)
			{
				if (parent.def.size.x > 1 || parent.def.size.z > 1)
				{
					parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.ForbiddenBig);
				}
				else
				{
					parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.Forbidden);
				}
			}
			else if (parent.def.category == ThingCategory.Building)
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.ForbiddenBig);
			}
			else
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.Forbidden);
			}
		}

		public override void PostSplitOff(Thing piece)
		{
			piece.SetForbidden(forbiddenInt);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!(parent is Building) || parent.Faction == Faction.OfPlayer)
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.hotKey = KeyBindingDefOf.Command_ItemForbid;
				command_Toggle.icon = TexCommand.ForbidOff;
				command_Toggle.isActive = (() => !Forbidden);
				command_Toggle.defaultLabel = "CommandAllow".TranslateWithBackup("DesignatorUnforbid");
				command_Toggle.activateIfAmbiguous = false;
				if (forbiddenInt)
				{
					command_Toggle.defaultDesc = "CommandForbiddenDesc".TranslateWithBackup("DesignatorUnforbidDesc");
				}
				else
				{
					command_Toggle.defaultDesc = "CommandNotForbiddenDesc".TranslateWithBackup("DesignatorForbidDesc");
				}
				if (parent.def.IsDoor)
				{
					command_Toggle.tutorTag = "ToggleForbidden-Door";
					command_Toggle.toggleAction = delegate
					{
						Forbidden = !Forbidden;
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ForbiddingDoors, KnowledgeAmount.SpecificInteraction);
					};
				}
				else
				{
					command_Toggle.tutorTag = "ToggleForbidden";
					command_Toggle.toggleAction = delegate
					{
						Forbidden = !Forbidden;
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Forbidding, KnowledgeAmount.SpecificInteraction);
					};
				}
				yield return command_Toggle;
			}
		}
	}
}
