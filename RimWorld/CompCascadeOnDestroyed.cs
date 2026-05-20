using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompCascadeOnDestroyed : ThingComp
{
	private CompProperties_CascadeOnDestroyed Props => (CompProperties_CascadeOnDestroyed)props;

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
		if (dinfo.HasValue && dinfo.Value.PreventCascade)
		{
			return;
		}
		int randomInRange = Props.cascadeCountRange.RandomInRange;
		int i = 0;
		Queue<Thing> queue = new Queue<Thing>();
		foreach (Thing item in AdjacentCascadableThings(parent, prevMap))
		{
			queue.Enqueue(item);
		}
		for (; i < randomInRange; i++)
		{
			if (queue.Empty())
			{
				break;
			}
			Thing thing = queue.Dequeue();
			foreach (Thing item2 in AdjacentCascadableThings(thing, prevMap))
			{
				if (!queue.Contains(item2))
				{
					queue.Enqueue(item2);
				}
			}
			thing.Kill(new DamageInfo(null, 99999f, 0f, -1f, dinfo?.Instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty: true, spawnFilth: true, QualityCategory.Normal, checkForJobOverride: true, preventCascade: true));
		}
	}

	private IEnumerable<Thing> AdjacentCascadableThings(Thing mass, Map map)
	{
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = mass.Position + GenAdj.CardinalDirections[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			Building edifice = c.GetEdifice(map);
			if (edifice != null)
			{
				if (Props.cascadeThingDefs.NullOrEmpty() && edifice.def == parent.def)
				{
					yield return edifice;
				}
				else if (Props.cascadeThingDefs.NotNullAndContains(edifice.def))
				{
					yield return edifice;
				}
			}
		}
	}
}
