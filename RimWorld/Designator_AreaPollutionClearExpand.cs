using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaPollutionClearExpand : Designator_AreaPollutionClear
	{
		public Designator_AreaPollutionClearExpand()
			: base(DesignateMode.Add)
		{
			defaultLabel = "DesignatorAreaPollutionClearExpand".Translate();
			defaultDesc = "DesignatorAreaPollutionClearExpandDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Icons/PollutionClearArea");
			soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
			soundSucceeded = SoundDefOf.Designate_ZoneAdd_RemovePollution;
			tutorTag = "AreaPollutionClearExpand";
		}
	}
}
