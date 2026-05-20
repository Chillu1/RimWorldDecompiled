using Verse;

namespace RimWorld
{
	public class PreceptComp_UnwillingToDo_Gendered : PreceptComp_UnwillingToDo
	{
		public Gender gender;

		public bool displayDescription = true;

		public override bool MemberWillingToDo(HistoryEvent ev)
		{
			if (!ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn arg) || arg.gender != gender)
			{
				return true;
			}
			return base.MemberWillingToDo(ev);
		}

		public override string GetProhibitionText()
		{
			if (!displayDescription)
			{
				return null;
			}
			return base.GetProhibitionText() + " (" + gender.GetLabel() + ")";
		}
	}
}
