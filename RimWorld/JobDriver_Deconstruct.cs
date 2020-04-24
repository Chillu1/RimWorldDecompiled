using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Deconstruct : JobDriver_RemoveBuilding
	{
		private const float MaxDeconstructWork = 3000f;

		private const float MinDeconstructWork = 20f;

		protected override DesignationDef Designation => DesignationDefOf.Deconstruct;

		protected override float TotalNeededWork => Mathf.Clamp(base.Building.GetStatValue(StatDefOf.WorkToBuild), 20f, 3000f);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => base.Building == null || !base.Building.DeconstructibleBy(pawn.Faction));
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
		}

		protected override void FinishedRemoving()
		{
			base.Target.Destroy(DestroyMode.Deconstruct);
			pawn.records.Increment(RecordDefOf.ThingsDeconstructed);
		}

		protected override void TickAction()
		{
			if (base.Building.def.CostListAdjusted(base.Building.Stuff).Count > 0)
			{
				pawn.skills.Learn(SkillDefOf.Construction, 0.25f);
			}
		}
	}
}
