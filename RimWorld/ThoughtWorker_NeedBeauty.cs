using System;
using Verse;

namespace RimWorld;

public class ThoughtWorker_NeedBeauty : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.needs.beauty == null)
		{
			return ThoughtState.Inactive;
		}
		return p.needs.beauty.CurCategory switch
		{
			BeautyCategory.Hideous => ThoughtState.ActiveAtStage(0), 
			BeautyCategory.VeryUgly => ThoughtState.ActiveAtStage(1), 
			BeautyCategory.Ugly => ThoughtState.ActiveAtStage(2), 
			BeautyCategory.Neutral => ThoughtState.Inactive, 
			BeautyCategory.Pretty => ThoughtState.ActiveAtStage(3), 
			BeautyCategory.VeryPretty => ThoughtState.ActiveAtStage(4), 
			BeautyCategory.Beautiful => ThoughtState.ActiveAtStage(5), 
			_ => throw new InvalidOperationException("Unknown BeautyCategory"), 
		};
	}
}
