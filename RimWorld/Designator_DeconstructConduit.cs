using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_DeconstructConduit : Designator_Deconstruct
{
	public Designator_DeconstructConduit()
	{
		defaultLabel = "DesignatorDeconstructConduit".Translate();
		defaultDesc = "DesignatorDeconstructConduitDesc".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/DeconstructConduit");
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Deconstruct;
		hotKey = KeyBindingDefOf.Designator_Deconstruct;
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (t.def.building == null || !t.def.building.isPowerConduit)
		{
			return false;
		}
		return base.CanDesignateThing(t);
	}

	public override void SelectedUpdate()
	{
		base.SelectedUpdate();
		OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
	}
}
