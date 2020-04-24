using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaHomeExpand : Designator_AreaHome
	{
		public Designator_AreaHomeExpand()
			: base(DesignateMode.Add)
		{
			defaultLabel = "DesignatorAreaHomeExpand".Translate();
			defaultDesc = "DesignatorAreaHomeExpandDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOn");
			soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_ZoneAdd;
			tutorTag = "AreaHomeExpand";
			hotKey = KeyBindingDefOf.Misc4;
		}
	}
}
