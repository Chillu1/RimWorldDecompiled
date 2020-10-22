using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PrisonerGetDressed : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.guest.PrisonerIsSecure || pawn.apparel == null)
			{
				return null;
			}
			if (ThoughtUtility.CanGetThought_NewTemp(pawn, ThoughtDefOf.ClothedNudist, checkIfNullified: true) && pawn.AmbientTemperature >= pawn.SafeTemperatureRange().min)
			{
				return null;
			}
			if (pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0)
			{
				RoyalTitleDef def = pawn.royalty.MostSeniorTitle.def;
				if (def != null && def.requiredApparel != null)
				{
					for (int i = 0; i < def.requiredApparel.Count; i++)
					{
						if (!def.requiredApparel[i].IsMet(pawn))
						{
							Apparel apparel = FindGarmentSatisfyingTitleRequirement(pawn, def.requiredApparel[i]);
							if (apparel != null)
							{
								Job job = JobMaker.MakeJob(JobDefOf.Wear, apparel);
								job.ignoreForbidden = true;
								return job;
							}
						}
					}
				}
			}
			if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Legs))
			{
				Apparel apparel2 = FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.Legs);
				if (apparel2 != null)
				{
					Job job2 = JobMaker.MakeJob(JobDefOf.Wear, apparel2);
					job2.ignoreForbidden = true;
					return job2;
				}
			}
			if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Torso))
			{
				Apparel apparel3 = FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.Torso);
				if (apparel3 != null)
				{
					Job job3 = JobMaker.MakeJob(JobDefOf.Wear, apparel3);
					job3.ignoreForbidden = true;
					return job3;
				}
			}
			return null;
		}

		private Apparel FindGarmentCoveringPart(Pawn pawn, BodyPartGroupDef bodyPartGroupDef)
		{
			Room room = pawn.GetRoom();
			if (room.isPrisonCell)
			{
				foreach (IntVec3 cell in room.Cells)
				{
					List<Thing> thingList = cell.GetThingList(pawn.Map);
					for (int i = 0; i < thingList.Count; i++)
					{
						Apparel apparel = thingList[i] as Apparel;
						if (apparel != null && apparel.def.apparel.bodyPartGroups.Contains(bodyPartGroupDef) && pawn.CanReserve(apparel) && !apparel.IsBurning() && (!EquipmentUtility.IsBiocoded(apparel) || EquipmentUtility.IsBiocodedFor(apparel, pawn)) && ApparelUtility.HasPartsToWear(pawn, apparel.def))
						{
							return apparel;
						}
					}
				}
			}
			return null;
		}

		private Apparel FindGarmentSatisfyingTitleRequirement(Pawn pawn, RoyalTitleDef.ApparelRequirement req)
		{
			Room room = pawn.GetRoom();
			if (room.isPrisonCell)
			{
				foreach (IntVec3 cell in room.Cells)
				{
					List<Thing> thingList = cell.GetThingList(pawn.Map);
					for (int i = 0; i < thingList.Count; i++)
					{
						Apparel apparel = thingList[i] as Apparel;
						if (apparel != null && req.ApparelMeetsRequirement(thingList[i].def, allowUnmatched: false) && pawn.CanReserve(apparel) && !apparel.IsBurning() && (!EquipmentUtility.IsBiocoded(apparel) || EquipmentUtility.IsBiocodedFor(apparel, pawn)) && ApparelUtility.HasPartsToWear(pawn, apparel.def))
						{
							return apparel;
						}
					}
				}
			}
			return null;
		}
	}
}
