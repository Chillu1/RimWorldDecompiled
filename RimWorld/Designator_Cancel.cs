using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Cancel : Designator
	{
		private static HashSet<Thing> seenThings = new HashSet<Thing>();

		public override int DraggableDimensions => 2;

		public Designator_Cancel()
		{
			defaultLabel = "DesignatorCancel".Translate();
			defaultDesc = "DesignatorCancelDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
			useMouseIcon = true;
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			soundSucceeded = SoundDefOf.Designate_Cancel;
			hotKey = KeyBindingDefOf.Designator_Cancel;
			tutorTag = "Cancel";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (CancelableDesignationsAt(c).Count() > 0)
			{
				return true;
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (CanDesignateThing(thingList[i]).Accepted)
				{
					return true;
				}
			}
			return false;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			foreach (Designation item in CancelableDesignationsAt(c).ToList())
			{
				if (item.def.designateCancelable)
				{
					base.Map.designationManager.RemoveDesignation(item);
				}
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				if (CanDesignateThing(thingList[num]).Accepted)
				{
					DesignateThing(thingList[num]);
				}
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (base.Map.designationManager.DesignationOn(t) != null)
			{
				foreach (Designation item in base.Map.designationManager.AllDesignationsOn(t))
				{
					if (item.def.designateCancelable)
					{
						return true;
					}
				}
			}
			if (t.def.mineable && base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) != null)
			{
				return true;
			}
			if (t.def.IsSmoothable && base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.SmoothWall) != null)
			{
				return true;
			}
			return t.Faction == Faction.OfPlayer && (t is Frame || t is Blueprint);
		}

		public override void DesignateThing(Thing t)
		{
			if (t is Frame || t is Blueprint)
			{
				t.Destroy(DestroyMode.Cancel);
				return;
			}
			base.Map.designationManager.RemoveAllDesignationsOn(t, standardCanceling: true);
			if (t.def.mineable)
			{
				Designation designation = base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine);
				if (designation != null)
				{
					base.Map.designationManager.RemoveDesignation(designation);
				}
			}
			if (t.def.IsSmoothable)
			{
				Designation designation2 = base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.SmoothWall);
				if (designation2 != null)
				{
					base.Map.designationManager.RemoveDesignation(designation2);
				}
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}

		private IEnumerable<Designation> CancelableDesignationsAt(IntVec3 c)
		{
			return from x in base.Map.designationManager.AllDesignationsAt(c)
				where x.def != DesignationDefOf.Plan
				select x;
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			seenThings.Clear();
			for (int i = 0; i < dragCells.Count; i++)
			{
				if (base.Map.designationManager.HasMapDesignationAt(dragCells[i]))
				{
					Graphics.DrawMesh(MeshPool.plane10, dragCells[i].ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays.AltitudeFor()), Quaternion.identity, DesignatorUtility.DragHighlightCellMat, 0);
					if (base.Map.designationManager.DesignationAt(dragCells[i], DesignationDefOf.Mine) != null)
					{
						continue;
					}
				}
				List<Thing> thingList = dragCells[i].GetThingList(base.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					if (!seenThings.Contains(thing) && CanDesignateThing(thing).Accepted)
					{
						Vector3 drawPos = thing.DrawPos;
						drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
						Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, DesignatorUtility.DragHighlightThingMat, 0);
						seenThings.Add(thing);
					}
				}
			}
			seenThings.Clear();
		}
	}
}
