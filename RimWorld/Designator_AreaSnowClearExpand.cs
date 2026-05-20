using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_AreaSnowClearExpand : Designator_AreaSnowClear
{
	public Designator_AreaSnowClearExpand()
		: base(DesignateMode.Add)
	{
		string key = "DesignatorAreaSnowClearExpand";
		string key2 = "DesignatorAreaSnowClearExpandDesc";
		if (ModsConfig.OdysseyActive)
		{
			key = "DesignatorAreaSnowOrSandClearExpand";
			key2 = "DesignatorAreaSnowOrSandClearExpandDesc";
		}
		defaultLabel = key.Translate();
		defaultDesc = key2.Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOn");
		soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
		soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
		soundSucceeded = SoundDefOf.Designate_ZoneAdd_RemoveSnow;
	}
}
