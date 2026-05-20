using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Thought_IdeoRoleApparelRequirementNotMet : Thought_Situational
{
	private List<string> unmetReqsListTmp = new List<string>();

	public Precept_Role Role => (Precept_Role)sourcePrecept;

	public override string LabelCap => base.CurStage.LabelCap.Formatted(Role.Named("ROLE"));

	public override string Description => base.CurStage.description.Formatted(Role.Named("ROLE")) + ":\n\n" + GetAllRequiredApparel(pawn).ToLineList(" -  ");

	protected override ThoughtState CurrentStateInternal()
	{
		foreach (Pawn item in Role.ChosenPawns())
		{
			if (item == pawn && GetAllRequiredApparel(pawn).Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	private List<string> GetAllRequiredApparel(Pawn p)
	{
		unmetReqsListTmp.Clear();
		if (ModsConfig.IdeologyActive)
		{
			Precept_Role precept_Role = p.Ideo?.GetRole(p);
			if (precept_Role != null && !precept_Role.apparelRequirements.NullOrEmpty())
			{
				for (int i = 0; i < precept_Role.apparelRequirements.Count; i++)
				{
					ApparelRequirement requirement = precept_Role.apparelRequirements[i].requirement;
					if (!requirement.IsActive(p) || requirement.IsMet(p))
					{
						continue;
					}
					if (!requirement.groupLabel.NullOrEmpty())
					{
						unmetReqsListTmp.Add(requirement.groupLabel);
						continue;
					}
					foreach (ThingDef item in requirement.AllRequiredApparelForPawn(p))
					{
						unmetReqsListTmp.Add(item.LabelCap);
					}
				}
			}
		}
		return unmetReqsListTmp;
	}
}
