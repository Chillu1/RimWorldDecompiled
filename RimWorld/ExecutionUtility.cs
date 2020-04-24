using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ExecutionUtility
	{
		public static void DoExecutionByCut(Pawn executioner, Pawn victim)
		{
			_ = victim.Map;
			_ = victim.Position;
			int num = Mathf.Max(GenMath.RoundRandom(victim.BodySize * 8f), 1);
			for (int i = 0; i < num; i++)
			{
				victim.health.DropBloodFilth();
			}
			BodyPartRecord bodyPartRecord = ExecuteCutPart(victim);
			int num2 = Mathf.Clamp((int)victim.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 20);
			DamageInfo damageInfo = new DamageInfo(DamageDefOf.ExecutionCut, num2, 999f, -1f, executioner, bodyPartRecord);
			victim.TakeDamage(damageInfo);
			if (!victim.Dead)
			{
				victim.Kill(damageInfo);
			}
		}

		private static BodyPartRecord ExecuteCutPart(Pawn pawn)
		{
			BodyPartRecord bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.InsectHead);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Body);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.SnakeBody);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Torso);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			Log.Error("No good slaughter cut part found for " + pawn);
			return pawn.health.hediffSet.GetNotMissingParts().RandomElementByWeight((BodyPartRecord x) => x.coverageAbsWithChildren);
		}
	}
}
