using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaAllowedClear : Designator_AreaAllowed
	{
		public Designator_AreaAllowedClear()
			: base(DesignateMode.Remove)
		{
			defaultLabel = "DesignatorClearAreaAllowed".Translate();
			defaultDesc = "DesignatorClearAreaAllowedDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/AreaAllowedClear");
			soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_ZoneDelete;
			hotKey = KeyBindingDefOf.Misc10;
			tutorTag = "AreaAllowedClear";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			return c.InBounds(base.Map) && Designator_AreaAllowed.SelectedArea != null && Designator_AreaAllowed.SelectedArea[c];
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Designator_AreaAllowed.SelectedArea[c] = false;
		}
	}
}
