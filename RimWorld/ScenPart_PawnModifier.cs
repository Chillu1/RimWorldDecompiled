using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_PawnModifier : ScenPart
{
	protected float chance = 1f;

	protected PawnGenerationContext context;

	protected bool hideOffMap;

	private string chanceBuf;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref chance, "chance", 0f);
		Scribe_Values.Look(ref context, "context", PawnGenerationContext.All);
		Scribe_Values.Look(ref hideOffMap, "hideOffMap", defaultValue: false);
	}

	protected void DoPawnModifierEditInterface(Rect rect)
	{
		Rect rect2 = rect.TopHalf();
		Rect rect3 = rect2.LeftPart(0.333f).Rounded();
		Rect rect4 = rect2.RightPart(0.666f).Rounded();
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.Label(rect3, "chance".Translate());
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.TextFieldPercent(rect4, ref chance, ref chanceBuf);
		Rect rect5 = rect.BottomHalf();
		Rect rect6 = rect5.LeftPart(0.333f).Rounded();
		Rect rect7 = rect5.RightPart(0.666f).Rounded();
		Text.Anchor = TextAnchor.MiddleRight;
		Widgets.Label(rect6, "context".Translate());
		Text.Anchor = TextAnchor.UpperLeft;
		if (!Widgets.ButtonText(rect7, context.ToStringHuman()))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (PawnGenerationContext value in Enum.GetValues(typeof(PawnGenerationContext)))
		{
			PawnGenerationContext localCont = value;
			list.Add(new FloatMenuOption(localCont.ToStringHuman(), delegate
			{
				context = localCont;
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public override void Randomize()
	{
		chance = GenMath.RoundedHundredth(Rand.Range(0.05f, 1f));
		context = PawnGenerationContextUtility.GetRandom();
		hideOffMap = false;
	}

	public override void Notify_NewPawnGenerating(Pawn pawn, PawnGenerationContext context)
	{
		if (this.context.Includes(context) && (!hideOffMap || context != PawnGenerationContext.PlayerStarter) && Rand.Chance(chance) && pawn.RaceProps.Humanlike)
		{
			ModifyNewPawn(pawn);
		}
	}

	public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context, bool redressed)
	{
		if (this.context.Includes(context) && (!hideOffMap || context != PawnGenerationContext.PlayerStarter) && Rand.Chance(chance) && pawn.RaceProps.Humanlike)
		{
			ModifyPawnPostGenerate(pawn, redressed);
		}
	}

	public override void PostMapGenerate(Map map)
	{
		if (Find.GameInitData == null || !hideOffMap || !context.Includes(PawnGenerationContext.PlayerStarter))
		{
			return;
		}
		foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
		{
			if (Rand.Chance(chance) && startingAndOptionalPawn.RaceProps.Humanlike)
			{
				ModifyHideOffMapStartingPawnPostMapGenerate(startingAndOptionalPawn);
			}
		}
	}

	protected virtual void ModifyNewPawn(Pawn p)
	{
	}

	protected virtual void ModifyPawnPostGenerate(Pawn p, bool redressed)
	{
	}

	protected virtual void ModifyHideOffMapStartingPawnPostMapGenerate(Pawn p)
	{
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ context.GetHashCode() ^ chance.GetHashCode() ^ (hideOffMap ? 1 : 0);
	}
}
