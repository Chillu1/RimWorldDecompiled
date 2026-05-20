using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_AssaultColonySappers : LordToil
{
	private static readonly FloatRange EscortRadiusRanged = new FloatRange(15f, 19f);

	private static readonly FloatRange EscortRadiusMelee = new FloatRange(23f, 26f);

	private LordToilData_AssaultColonySappers Data => (LordToilData_AssaultColonySappers)data;

	public override bool AllowSatisfyLongNeeds => false;

	public override bool ForceHighStoryDanger => true;

	public LordToil_AssaultColonySappers()
	{
		data = new LordToilData_AssaultColonySappers();
	}

	public override void Init()
	{
		base.Init();
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
	}

	public override void UpdateAllDuties()
	{
		if (!Data.sapperDest.IsValid && lord.ownedPawns.Any())
		{
			Data.sapperDest = GenAI.RandomRaidDest(lord.ownedPawns[0].PositionHeld, base.Map);
		}
		List<Pawn> list = null;
		if (Data.sapperDest.IsValid)
		{
			list = new List<Pawn>();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (SappersUtility.IsGoodSapper(pawn))
				{
					list.Add(pawn);
				}
			}
			if (list.Count == 0 && lord.ownedPawns.Count >= 2)
			{
				Pawn pawn2 = null;
				int num = 0;
				for (int j = 0; j < lord.ownedPawns.Count; j++)
				{
					if (SappersUtility.IsGoodBackupSapper(lord.ownedPawns[j]))
					{
						int level = lord.ownedPawns[j].skills.GetSkill(SkillDefOf.Mining).Level;
						if (pawn2 == null || level > num)
						{
							pawn2 = lord.ownedPawns[j];
							num = level;
						}
					}
				}
				if (pawn2 != null)
				{
					list.Add(pawn2);
				}
			}
		}
		for (int k = 0; k < lord.ownedPawns.Count; k++)
		{
			Pawn pawn3 = lord.ownedPawns[k];
			if (list != null && list.Contains(pawn3))
			{
				pawn3.mindState.duty = new PawnDuty(DutyDefOf.Sapper, Data.sapperDest);
			}
			else if (!list.NullOrEmpty())
			{
				float radius = ((pawn3.equipment == null || pawn3.equipment.Primary == null || !pawn3.equipment.Primary.def.IsRangedWeapon) ? EscortRadiusMelee.RandomInRange : EscortRadiusRanged.RandomInRange);
				pawn3.mindState.duty = new PawnDuty(DutyDefOf.Escort, list.RandomElement(), radius);
			}
			else
			{
				pawn3.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
			}
		}
	}

	public override void Notify_ReachedDutyLocation(Pawn pawn)
	{
		Data.sapperDest = IntVec3.Invalid;
		UpdateAllDuties();
	}
}
