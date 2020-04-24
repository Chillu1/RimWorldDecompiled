using UnityEngine;

namespace Verse
{
	public class FeedbackItem_HealthGain : FeedbackItem
	{
		protected Pawn Healer;

		protected int Amount;

		public FeedbackItem_HealthGain(Vector2 ScreenPos, int Amount, Pawn Healer)
			: base(ScreenPos)
		{
			this.Amount = Amount;
			this.Healer = Healer;
		}

		public override void FeedbackOnGUI()
		{
			string text = "";
			text = ((Amount < 0) ? "-" : "+");
			text += Amount;
			DrawFloatingText(text, Color.red);
		}
	}
}
