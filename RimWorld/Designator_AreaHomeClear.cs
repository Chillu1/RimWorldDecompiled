using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaHomeClear : Designator_AreaHome
	{
		public Designator_AreaHomeClear()
			: base(DesignateMode.Remove)
		{
			defaultLabel = "DesignatorAreaHomeClear".Translate();
			defaultDesc = "DesignatorAreaHomeClearDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOff");
			soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_ZoneDelete;
			hotKey = KeyBindingDefOf.Misc7;
		}
	}
}
