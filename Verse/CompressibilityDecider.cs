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
			List<Lord> lords = map.lordManager.lords;
			for (int j = 0; j < lords.Count; j++)
			{
				LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = lords[j].LordJob as LordJob_FormAndSendCaravan;
				if (lordJob_FormAndSendCaravan == null)
				{
					continue;
				}
				for (int k = 0; k < lordJob_FormAndSendCaravan.transferables.Count; k++)
				{
					TransferableOneWay transferableOneWay = lordJob_FormAndSendCaravan.transferables[k];
					for (int l = 0; l < transferableOneWay.things.Count; l++)
					{
						referencedThings.Add(transferableOneWay.things[l]);
					}
				}
			}
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Transporter);
			for (int m = 0; m < list.Count; m++)
			{
				CompTransporter compTransporter = list[m].TryGetComp<CompTransporter>();
				if (compTransporter.leftToLoad == null)
				{
					continue;
				}
				for (int n = 0; n < compTransporter.leftToLoad.Count; n++)
				{
					TransferableOneWay transferableOneWay2 = compTransporter.leftToLoad[n];
					for (int num = 0; num < transferableOneWay2.things.Count; num++)
					{
						referencedThings.Add(transferableOneWay2.things[num]);
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
