using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
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
			foreach (Thing item in map.designationManager.allDesignations.Select((Designation des) => des.target.Thing))
			{
				referencedThings.Add(item);
			}
			foreach (Thing item2 in map.reservationManager.AllReservedThings())
			{
				referencedThings.Add(item2);
			}
			List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Job curJob = allPawnsSpawned[i].jobs.curJob;
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
			for (int j = 0; j < list.Count; j++)
			{
				Projectile projectile = (Projectile)list[j];
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
			for (int k = 0; k < lords.Count; k++)
			{
				LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = lords[k].LordJob as LordJob_FormAndSendCaravan;
				if (lordJob_FormAndSendCaravan == null)
				{
					continue;
				}
				for (int l = 0; l < lordJob_FormAndSendCaravan.transferables.Count; l++)
				{
					TransferableOneWay transferableOneWay = lordJob_FormAndSendCaravan.transferables[l];
					for (int m = 0; m < transferableOneWay.things.Count; m++)
					{
						referencedThings.Add(transferableOneWay.things[m]);
					}
				}
			}
			List<Thing> list2 = map.listerThings.ThingsInGroup(ThingRequestGroup.Transporter);
			for (int n = 0; n < list2.Count; n++)
			{
				CompTransporter compTransporter = list2[n].TryGetComp<CompTransporter>();
				if (compTransporter.leftToLoad == null)
				{
					continue;
				}
				for (int num = 0; num < compTransporter.leftToLoad.Count; num++)
				{
					TransferableOneWay transferableOneWay2 = compTransporter.leftToLoad[num];
					for (int num2 = 0; num2 < transferableOneWay2.things.Count; num2++)
					{
						referencedThings.Add(transferableOneWay2.things[num2]);
					}
				}
			}
		}

		public bool IsReferenced(Thing th)
		{
			return referencedThings.Contains(th);
		}
	}
}
