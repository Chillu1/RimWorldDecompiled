namespace Verse
{
	public class HediffComp_MessageBase : HediffComp
	{
		private HediffCompProperties_MessageBase Props => (HediffCompProperties_MessageBase)props;

		protected virtual void Message()
		{
			if (!Props.onlyMessageForColonistsOrPrisoners || base.Pawn.IsColonist || base.Pawn.IsPrisonerOfColony)
			{
				Messages.Message(Props.message.Formatted(base.Pawn), base.Pawn, Props.messageType);
			}
		}
	}
}
