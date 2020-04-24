using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
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
			CompLaunchable launchable = transComp.Launchable;
			if (launchable != null)
			{
				Building fuelingPortSource = launchable.FuelingPortSource;
				if (fuelingPortSource != null)
				{
					Map map = transComp.Map;
					tmpFuelingPortGivers.Clear();
					map.floodFiller.FloodFill(fuelingPortSource.Position, (IntVec3 x) => FuelingPortUtility.AnyFuelingPortGiverAt(x, map), delegate(IntVec3 x)
					{
						tmpFuelingPortGivers.Add(FuelingPortUtility.FuelingPortGiverAt(x, map));
					});
					for (int i = 0; i < transporters.Count; i++)
					{
						Building fuelingPortSource2 = transporters[i].Launchable.FuelingPortSource;
						if (fuelingPortSource2 != null && !tmpFuelingPortGivers.Contains(fuelingPortSource2))
						{
							Messages.Message("MessageTransportersNotAdjacent".Translate(), fuelingPortSource2, MessageTypeDefOf.RejectInput, historical: false);
							return;
						}
					}
				}
			}
			for (int j = 0; j < transporters.Count; j++)
			{
				if (transporters[j] != transComp && !transComp.Map.reachability.CanReach(transComp.parent.Position, transporters[j].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)))
				{
					Messages.Message("MessageTransporterUnreachable".Translate(), transporters[j].parent, MessageTypeDefOf.RejectInput, historical: false);
					return;
				}
			}
			Find.WindowStack.Add(new Dialog_LoadTransporters(transComp.Map, transporters));
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
}
