using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class RitualTargetFilter_UsableThrone : RitualTargetFilter
{
	public RitualTargetFilter_UsableThrone()
	{
	}

	public RitualTargetFilter_UsableThrone(RitualTargetFilterDef def)
		: base(def)
	{
	}

	public override bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason)
	{
		Pawn pawn = initiator.Thing as Pawn;
		rejectionReason = "";
		if (pawn == null)
		{
			return false;
		}
		Building_Throne building_Throne = RoyalTitleUtility.FindBestUsableThrone(pawn) ?? pawn.ownership.AssignedThrone;
		if (building_Throne == null)
		{
			rejectionReason = "AbilitySpeechDisabledNoThroneAssigned".Translate();
			return false;
		}
		if (!pawn.CanReserveAndReach(building_Throne, PathEndMode.InteractionCell, pawn.NormalMaxDanger()))
		{
			rejectionReason = "AbilitySpeechDisabledNoThroneIsNotAccessible".Translate();
			return false;
		}
		if (pawn.royalty.AnyUnmetThroneroomRequirements())
		{
			rejectionReason = "AbilitySpeechDisabledNoThroneUndignified".Translate();
			return false;
		}
		return true;
	}

	public override TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget)
	{
		Building_Throne building_Throne = RoyalTitleUtility.FindBestUsableThrone((Pawn)initiator.Thing);
		if (building_Throne == null)
		{
			return TargetInfo.Invalid;
		}
		return new TargetInfo(building_Throne.InteractionCell, building_Throne.Map);
	}

	public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
	{
		yield return "AbilitySpeechTargetDescThrone".Translate();
	}
}
