using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompPlantPreventCutting : ThingComp
{
	private bool preventCutting = true;

	private static readonly Texture2D IconTexture = ContentFinder<Texture2D>.Get("UI/Designators/PreventCutting");

	public CompProperties_PlantPreventCutting Props => (CompProperties_PlantPreventCutting)props;

	public bool PreventCutting
	{
		get
		{
			return preventCutting;
		}
		set
		{
			preventCutting = value;
			if (preventCutting)
			{
				parent.Map.designationManager.TryRemoveDesignationOn(parent, DesignationDefOf.CutPlant);
				parent.Map.designationManager.TryRemoveDesignationOn(parent, DesignationDefOf.HarvestPlant);
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref preventCutting, "canCut", defaultValue: false);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		yield return new Command_Toggle
		{
			defaultLabel = "PreventCutting".Translate(),
			defaultDesc = "PreventCuttingDesc".Translate(),
			toggleAction = delegate
			{
				PreventCutting = !preventCutting;
			},
			isActive = () => preventCutting,
			icon = IconTexture
		};
	}
}
