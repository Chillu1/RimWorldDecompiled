using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Haul : Designator
	{
		public override int DraggableDimensions => 2;

		protected override DesignationDef Designation => DesignationDefOf.Haul;

		public Designator_Haul()
		{
			defaultLabel = "DesignatorHaulThings".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/Haul");
			defaultDesc = "DesignatorHaulThingsDesc".Translate();
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Haul;
			hotKey = KeyBindingDefOf.Misc12;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map) || c.Fogged(base.Map))
			{
				return false;
			}
			Thing firstHaulable = c.GetFirstHaulable(base.Map);
			if (firstHaulable == null)
			{
				return "MessageMustDesignateHaulable".Translate();
			}
			AcceptanceReport result = CanDesignateThing(firstHaulable);
			if (!result.Accepted)
			{
				return result;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			DesignateThing(c.GetFirstHaulable(base.Map));
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (!t.def.designateHaulable)
			{
				return false;
			}
			if (base.Map.designationManager.DesignationOn(t, Designation) != null)
			{
				return false;
			}
			if (t.IsInValidStorage())
			{
				return "MessageAlreadyInStorage".Translate();
			}
			return true;
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.AddDesignation(new Designation(t, Designation));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
