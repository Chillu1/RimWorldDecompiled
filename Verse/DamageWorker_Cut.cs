using System.Collections.Generic;
using System.Linq;

namespace Verse;

public class DamageWorker_Cut : DamageWorker_AddInjury
{
	protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
	{
		return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
	}

	protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
	{
		if (dinfo.HitPart.depth == BodyPartDepth.Inside)
		{
			List<BodyPartRecord> list = new List<BodyPartRecord>();
			for (BodyPartRecord bodyPartRecord = dinfo.HitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
			{
				list.Add(bodyPartRecord);
				if (bodyPartRecord.depth == BodyPartDepth.Outside)
				{
					break;
				}
			}
			float num = (float)(list.Count - 1) + 0.5f;
			for (int i = 0; i < list.Count; i++)
			{
				DamageInfo dinfo2 = dinfo;
				dinfo2.SetHitPart(list[i]);
				FinalizeAndAddInjury(pawn, totalDamage / num * ((i == 0) ? 0.5f : 1f), dinfo2, result);
			}
			return;
		}
		int num2 = ((def.cutExtraTargetsCurve != null) ? GenMath.RoundRandom(def.cutExtraTargetsCurve.Evaluate(Rand.Value)) : 0);
		List<BodyPartRecord> list2 = null;
		if (num2 != 0)
		{
			IEnumerable<BodyPartRecord> enumerable = dinfo.HitPart.GetDirectChildParts();
			if (dinfo.HitPart.parent != null)
			{
				enumerable = enumerable.Concat(dinfo.HitPart.parent);
				if (dinfo.HitPart.parent.parent != null)
				{
					enumerable = enumerable.Concat(dinfo.HitPart.parent.GetDirectChildParts());
				}
			}
			list2 = (from x in enumerable.Except(dinfo.HitPart).InRandomOrder().Take(num2)
				where !x.def.conceptual && x.coverageAbs > 0f
				select x).ToList();
		}
		else
		{
			list2 = new List<BodyPartRecord>();
		}
		list2.Add(dinfo.HitPart);
		float num3 = totalDamage * (1f + def.cutCleaveBonus) / ((float)list2.Count + def.cutCleaveBonus);
		if (num2 == 0)
		{
			num3 = ReduceDamageToPreserveOutsideParts(num3, dinfo, pawn);
		}
		for (int num4 = 0; num4 < list2.Count; num4++)
		{
			DamageInfo dinfo3 = dinfo;
			dinfo3.SetHitPart(list2[num4]);
			FinalizeAndAddInjury(pawn, num3, dinfo3, result);
		}
	}
}
