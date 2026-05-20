using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_AreaSnowClearClear : Designator_AreaSnowClear
{
	public Designator_AreaSnowClearClear()
		: base(DesignateMode.Remove)
	{
		string key = "DesignatorAreaSnowClearClear";
		string key2 = "DesignatorAreaSnowClearClearDesc";
		if (ModsConfig.OdysseyActive)
		{
			key = "DesignatorAreaSnowOrSandClearClear";
			key2 = "DesignatorAreaSnowOrSandClearClearDesc";
		}
		defaultLabel = key.Translate();
		defaultDesc = key2.Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOff");
		soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
		soundDragChanged = null;
		soundSucceeded = SoundDefOf.Designate_ZoneDelete;
	}
}
