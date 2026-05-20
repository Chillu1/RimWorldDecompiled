using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Expectations : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.IsSlave)
			{
				return ThoughtState.Inactive;
			}
			ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(p);
			if (expectationDef == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(expectationDef.thoughtStage);
		}

		public override string PostProcessDescription(Pawn p, string description)
		{
			string text = base.PostProcessDescription(p, description);
			if (ModsConfig.IdeologyActive)
			{
				Precept_Role precept_Role = p.Ideo?.GetRole(p);
				if (precept_Role != null && ExpectationsUtility.OffsetByRole(p))
				{
					if (MoveColonyUtility.TitleAndRoleRequirementsGracePeriodActive)
					{
						return text + "\n\n" + "RoleRaisedExpectationGracePeriod".Translate(precept_Role.LabelCap, MoveColonyUtility.TitleAndRoleRequirementGracePeriodTicksLeft.ToStringTicksToPeriod());
					}
					return text + "\n\n" + "RoleRaisedExpectation".Translate(precept_Role.LabelCap);
				}
			}
			return text;
		}
	}
}
