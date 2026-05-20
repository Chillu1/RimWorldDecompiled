using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_PodLauncher : Building, INotifyLaunchableLaunch
{
	public bool autoPlacePods;

	private static readonly Texture2D AutoBuildIcon = ContentFinder<Texture2D>.Get("UI/Commands/AutoBuildTransportPod");

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		Designator_Build designator_Build = BuildCopyCommandUtility.FindAllowedDesignator(ThingDefOf.TransportPod);
		if (designator_Build != null)
		{
			AcceptanceReport acceptanceReport = GenConstruct.CanPlaceBlueprintAt(ThingDefOf.TransportPod, FuelingPortUtility.GetFuelingPortCell(this), ThingDefOf.TransportPod.defaultPlacingRot, base.Map);
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = "BuildThing".Translate(ThingDefOf.TransportPod.label),
				icon = designator_Build.icon,
				defaultDesc = designator_Build.Desc,
				action = delegate
				{
					IntVec3 fuelingPortCell = FuelingPortUtility.GetFuelingPortCell(this);
					GenConstruct.PlaceBlueprintForBuild(ThingDefOf.TransportPod, fuelingPortCell, base.Map, ThingDefOf.TransportPod.defaultPlacingRot, Faction.OfPlayer, null);
				}
			};
			if (!acceptanceReport.Accepted)
			{
				command_Action.Disable(acceptanceReport.Reason);
			}
			yield return command_Action;
		}
		yield return new Command_Toggle
		{
			icon = AutoBuildIcon,
			defaultLabel = "AutoBuildTransportPod".Translate(),
			defaultDesc = "AutoBuildTransportPodDesc".Translate(),
			isActive = () => autoPlacePods,
			toggleAction = ToggleAutoBuildTransportPods
		};
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref autoPlacePods, "autoPlacePods", defaultValue: false);
	}

	private void ToggleAutoBuildTransportPods()
	{
		autoPlacePods = !autoPlacePods;
		if (autoPlacePods)
		{
			CheckPlacePod();
		}
	}

	private void CheckPlacePod()
	{
		if (autoPlacePods)
		{
			IntVec3 fuelingPortCell = FuelingPortUtility.GetFuelingPortCell(this);
			if (GenConstruct.CanPlaceBlueprintAt(ThingDefOf.TransportPod, fuelingPortCell, ThingDefOf.TransportPod.defaultPlacingRot, base.Map).Accepted)
			{
				GenConstruct.PlaceBlueprintForBuild(ThingDefOf.TransportPod, fuelingPortCell, base.Map, ThingDefOf.TransportPod.defaultPlacingRot, Faction.OfPlayer, null);
			}
		}
	}

	public void Notify_LaunchableLaunched(CompLaunchable launchable)
	{
		CheckPlacePod();
	}
}
