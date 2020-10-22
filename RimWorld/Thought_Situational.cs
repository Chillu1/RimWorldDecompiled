using Verse;

namespace RimWorld
{
	public class Thought_Situational : Thought
	{
		private int curStageIndex = -1;

		protected string reason;

		public bool Active => curStageIndex >= 0;

		public override int CurStageIndex => curStageIndex;

		public override string LabelCap
		{
			get
			{
				if (!reason.NullOrEmpty())
				{
					string text = base.CurStage.label.Formatted(reason.Named("REASON"), pawn.Named("PAWN")).CapitalizeFirst();
					if (def.Worker != null)
					{
						text = def.Worker.PostProcessLabel(pawn, text);
					}
					return text;
				}
				return base.LabelCap;
			}
		}

		public void RecalculateState()
		{
			ThoughtState thoughtState = CurrentStateInternal();
			if (thoughtState.ActiveFor(def))
			{
				curStageIndex = thoughtState.StageIndexFor(def);
				reason = thoughtState.Reason;
			}
			else
			{
				curStageIndex = -1;
			}
		}

		protected virtual ThoughtState CurrentStateInternal()
		{
			return def.Worker.CurrentState(pawn);
		}
	}
}
