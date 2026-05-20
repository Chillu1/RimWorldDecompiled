using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompToggleDrawAffectedMeditationFoci : ThingComp
{
	private bool enabled;

	private static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/PlaceBlueprints");

	public bool Enabled => enabled;

	public CompProperties_ToggleDrawAffectedMeditationFoci Props => (CompProperties_ToggleDrawAffectedMeditationFoci)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
		{
			enabled = Props.defaultEnabled;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		Command_Toggle command_Toggle = new Command_Toggle();
		command_Toggle.defaultLabel = "CommandWarnInBuildingRadius".Translate();
		command_Toggle.defaultDesc = "CommandWarnInBuildingRadiusDesc".Translate();
		command_Toggle.icon = CommandTex;
		command_Toggle.isActive = () => enabled;
		command_Toggle.toggleAction = (Action)Delegate.Combine(command_Toggle.toggleAction, (Action)delegate
		{
			enabled = !enabled;
		});
		yield return command_Toggle;
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref enabled, "enabled", defaultValue: false);
	}
}
