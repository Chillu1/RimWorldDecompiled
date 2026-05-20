using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaAllowedExpand : Designator_AreaAllowed
	{
		public Designator_AreaAllowedExpand()
			: base(DesignateMode.Add)
		{
			defaultLabel = "DesignatorExpandAreaAllowed".Translate();
			defaultDesc = "DesignatorExpandAreaAllowedDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/AreaAllowedExpand");
			soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
			soundSucceeded = SoundDefOf.Designate_ZoneAdd_AllowedArea;
			hotKey = KeyBindingDefOf.Misc8;
			tutorTag = "AreaAllowedExpand";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			return c.InBounds(base.Map) && Designator_AreaAllowed.SelectedArea != null && !Designator_AreaAllowed.SelectedArea[c];
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Designator_AreaAllowed.SelectedArea[c] = true;
		}
	}
}
