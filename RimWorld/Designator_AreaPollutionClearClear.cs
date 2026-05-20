using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaPollutionClearClear : Designator_AreaPollutionClear
	{
		public Designator_AreaPollutionClearClear()
			: base(DesignateMode.Remove)
		{
			defaultLabel = "DesignatorAreaPollutionClearClear".Translate();
			defaultDesc = "DesignatorAreaPollutionClearClearDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Icons/PollutionClearAreaOff");
			soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_ZoneDelete;
		}
	}
}
