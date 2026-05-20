using Verse;

namespace RimWorld
{
	public class QuestPart_TransporterPawns_Tend : QuestPart_TransporterPawns
	{
		public override void Process(Pawn pawn)
		{
			int num = 0;
			while (pawn.health.HasHediffsNeedingTend())
			{
				num++;
				if (num > 10000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				DoTend(pawn);
			}
		}

		protected virtual void DoTend(Pawn pawn)
		{
			TendUtility.DoTend(null, pawn, null);
		}
	}
}
