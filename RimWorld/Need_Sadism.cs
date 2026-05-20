using Verse;

namespace RimWorld
{
	public class Need_Sadism : Need
	{
		public bool IsHigh => CurLevel < 0.3f;

		public bool IsCritical => CurLevel < 0.1f;

		public Need_Sadism(Pawn newPawn)
			: base(newPawn)
		{
		}

		public override void NeedInterval()
		{
			if (!IsFrozen)
			{
				CurLevel -= def.fallPerDay * 0.0025f;
			}
		}
	}
}
