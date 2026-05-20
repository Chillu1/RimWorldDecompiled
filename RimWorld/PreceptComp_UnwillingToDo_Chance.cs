using Verse;

namespace RimWorld
{
	public class PreceptComp_UnwillingToDo_Chance : PreceptComp_UnwillingToDo
	{
		public float chance;

		public override bool MemberWillingToDo(HistoryEvent ev)
		{
			if (Rand.Value >= chance)
			{
				return true;
			}
			return base.MemberWillingToDo(ev);
		}

		public override string GetProhibitionText()
		{
			return null;
		}
	}
}
