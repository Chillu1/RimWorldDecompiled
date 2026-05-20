using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public class CompressibilityDecider
{
	private Map map;

	private HashSet<Thing> referencedThings = new HashSet<Thing>();

	public CompressibilityDecider(Map map)
	{
		this.map = map;
	}

	public void DetermineReferences()
	{
		referencedThings.Clear();
		foreach (Thing item in map.designationManager.AllDesignations.Select((Designation des) => des.target.Thing))
		{
			referencedThings.Add(item);
		}
		foreach (Thing item2 in map.reservationManager.AllReservedThings())
		{
			referencedThings.Add(item2);
		}
		IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
		for (int num = 0; num < allPawnsSpawned.Count; num++)
		{
			Job curJob = allPawnsSpawned[num].jobs.curJob;
			if (curJob != null)
			{
				if (curJob.targetA.HasThing)
				{
					referencedThings.Add(curJob.targetA.Thing);
				}
				if (curJob.targetB.HasThing)
				{
					referencedThings.Add(curJob.targetB.Thing);
				}
				if (curJob.targetC.HasThing)
				{
					referencedThings.Add(curJob.targetC.Thing);
				}
			}
		}
		List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			Projectile projectile = (Projectile)list[num2];
			if (projectile.usedTarget.HasThing)
			{
				referencedThings.Add(projectile.usedTarget.Thing);
			}
			if (projectile.intendedTarget.HasThing)
			{
				referencedThings.Add(projectile.intendedTarget.Thing);
			}
		}
		List<Lord> lords = map.lordManager.lords;
		for (int num3 = 0; num3 < lords.Count; num3++)
		{
			if (!(lords[num3].LordJob is LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan))
			{
				continue;
			}
			for (int num4 = 0; num4 < lordJob_FormAndSendCaravan.transferables.Count; num4++)
			{
				TransferableOneWay transferableOneWay = lordJob_FormAndSendCaravan.transferables[num4];
				for (int num5 = 0; num5 < transferableOneWay.things.Count; num5++)
				{
					referencedThings.Add(transferableOneWay.things[num5]);
				}
			}
		}
		List<Thing> list2 = map.listerThings.ThingsInGroup(ThingRequestGroup.Transporter);
		for (int num6 = 0; num6 < list2.Count; num6++)
		{
			CompTransporter compTransporter = list2[num6].TryGetComp<CompTransporter>();
			if (compTransporter.leftToLoad == null)
			{
				continue;
			}
			for (int num7 = 0; num7 < compTransporter.leftToLoad.Count; num7++)
			{
				TransferableOneWay transferableOneWay2 = compTransporter.leftToLoad[num7];
				for (int num8 = 0; num8 < transferableOneWay2.things.Count; num8++)
				{
					referencedThings.Add(transferableOneWay2.things[num8]);
				}
			}
		}
	}

	public bool IsReferenced(Thing th)
	{
		return referencedThings.Contains(th);
	}
}
