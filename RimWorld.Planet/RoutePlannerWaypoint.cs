using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public class RoutePlannerWaypoint : WorldObject
	{
		public override string Label
		{
			get
			{
				WorldRoutePlanner worldRoutePlanner = Find.WorldRoutePlanner;
				if (worldRoutePlanner.Active)
				{
					int num = worldRoutePlanner.waypoints.IndexOf(this);
					if (num >= 0)
					{
						return base.Label + " " + (num + 1);
					}
				}
				return base.Label;
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			WorldRoutePlanner worldRoutePlanner = Find.WorldRoutePlanner;
			if (worldRoutePlanner.Active)
			{
				int num = worldRoutePlanner.waypoints.IndexOf(this);
				if (num >= 1)
				{
					int ticksToWaypoint = worldRoutePlanner.GetTicksToWaypoint(num);
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append("EstimatedTimeToWaypoint".Translate(ticksToWaypoint.ToStringTicksToDays("0.#")));
					if (num >= 2)
					{
						int ticksToWaypoint2 = worldRoutePlanner.GetTicksToWaypoint(num - 1);
						stringBuilder.AppendLine();
						stringBuilder.Append("EstimatedTimeToWaypointFromPrevious".Translate((ticksToWaypoint - ticksToWaypoint2).ToStringTicksToDays("0.#")));
					}
				}
			}
			return stringBuilder.ToString();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandRemoveWaypointLabel".Translate();
			command_Action.defaultDesc = "CommandRemoveWaypointDesc".Translate();
			command_Action.icon = TexCommand.RemoveRoutePlannerWaypoint;
			command_Action.action = delegate
			{
				Find.WorldRoutePlanner.TryRemoveWaypoint(this);
			};
			yield return command_Action;
		}
	}
}
