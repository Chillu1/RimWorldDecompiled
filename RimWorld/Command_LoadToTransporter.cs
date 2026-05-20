using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Command_LoadToTransporter : Command
{
	public CompTransporter transComp;

	private List<CompTransporter> transporters;

	private static HashSet<Building> tmpFuelingPortGivers = new HashSet<Building>();

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		if (transporters == null)
		{
			transporters = new List<CompTransporter>();
		}
		if (!transporters.Contains(transComp))
		{
			transporters.Add(transComp);
		}
		if (transComp.Launchable is CompLaunchable_TransportPod compLaunchable_TransportPod)
		{
			ThingWithComps thingWithComps = compLaunchable_TransportPod.FuelingPortSource?.parent;
			if (thingWithComps != null)
			{
				Map map = transComp.Map;
				tmpFuelingPortGivers.Clear();
				map.floodFiller.FloodFill(thingWithComps.Position, (IntVec3 x) => FuelingPortUtility.AnyFuelingPortGiverAt(x, map), delegate(IntVec3 x)
				{
					tmpFuelingPortGivers.Add(FuelingPortUtility.FuelingPortGiverAt(x, map));
				});
				foreach (CompTransporter transporter in transporters)
				{
					ThingWithComps thingWithComps2 = (transporter.Launchable as CompLaunchable_TransportPod)?.FuelingPortSource?.parent;
					if (thingWithComps2 != null && !tmpFuelingPortGivers.Contains(thingWithComps2))
					{
						Messages.Message("MessageTransportersNotAdjacent".Translate(), thingWithComps2, MessageTypeDefOf.RejectInput, historical: false);
						return;
					}
				}
			}
		}
		foreach (CompTransporter transporter2 in transporters)
		{
			if (transporter2 != transComp && !transComp.Map.reachability.CanReach(transComp.parent.Position, transporter2.parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)))
			{
				Messages.Message("MessageTransporterUnreachable".Translate(), transporter2.parent, MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
		}
		Dialog_LoadTransporters window = new Dialog_LoadTransporters(transComp.Map, transporters);
		Find.WindowStack.Add(window);
	}

	public override bool InheritInteractionsFrom(Gizmo other)
	{
		if (transComp.Props.max1PerGroup)
		{
			return false;
		}
		Command_LoadToTransporter command_LoadToTransporter = (Command_LoadToTransporter)other;
		if (command_LoadToTransporter.transComp.parent.def != transComp.parent.def)
		{
			return false;
		}
		if (transporters == null)
		{
			transporters = new List<CompTransporter>();
		}
		transporters.Add(command_LoadToTransporter.transComp);
		return false;
	}
}
