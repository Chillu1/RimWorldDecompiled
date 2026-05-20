using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Need_Joy : Need
{
	public JoyToleranceSet tolerances = new JoyToleranceSet();

	private int lastGainTick = -999;

	public JoyCategory CurCategory
	{
		get
		{
			if (CurLevel < 0.01f)
			{
				return JoyCategory.Empty;
			}
			if (CurLevel < 0.15f)
			{
				return JoyCategory.VeryLow;
			}
			if (CurLevel < 0.3f)
			{
				return JoyCategory.Low;
			}
			if (CurLevel < 0.7f)
			{
				return JoyCategory.Satisfied;
			}
			if (CurLevel < 0.85f)
			{
				return JoyCategory.High;
			}
			return JoyCategory.Extreme;
		}
	}

	private float FallPerInterval => CurCategory switch
	{
		JoyCategory.Empty => 0.0015f, 
		JoyCategory.VeryLow => 0.0006f, 
		JoyCategory.Low => 0.00105f, 
		JoyCategory.Satisfied => 0.0015f, 
		JoyCategory.High => 0.0015f, 
		JoyCategory.Extreme => 0.0015f, 
		_ => throw new InvalidOperationException(), 
	};

	public override int GUIChangeArrow
	{
		get
		{
			if (IsFrozen)
			{
				return 0;
			}
			if (!GainingJoy)
			{
				return -1;
			}
			return 1;
		}
	}

	private bool GainingJoy => Find.TickManager.TicksGame < lastGainTick + 15;

	public Need_Joy(Pawn pawn)
		: base(pawn)
	{
		threshPercents = new List<float>();
		threshPercents.Add(0.15f);
		threshPercents.Add(0.3f);
		threshPercents.Add(0.7f);
		threshPercents.Add(0.85f);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		tolerances.ExposeData();
	}

	public override void SetInitialLevel()
	{
		CurLevel = Rand.Range(0.5f, 0.6f);
	}

	public void GainJoy(float amount, JoyKindDef joyKind)
	{
		if (!(amount <= 0f))
		{
			amount *= tolerances.JoyFactorFromTolerance(joyKind);
			amount = Mathf.Min(amount, 1f - CurLevel);
			curLevelInt += amount;
			if (joyKind != null)
			{
				tolerances.Notify_JoyGained(amount, joyKind);
			}
			lastGainTick = Find.TickManager.TicksGame;
		}
	}

	public override void NeedInterval()
	{
		if (!IsFrozen)
		{
			tolerances.NeedInterval(pawn);
			if (!GainingJoy)
			{
				CurLevel -= FallPerInterval * (pawn.IsFormingCaravan() ? 0.5f : 1f) * pawn.GetStatValue(StatDefOf.JoyFallRateFactor);
			}
		}
	}

	public override string GetTipString()
	{
		TaggedString taggedString = base.GetTipString();
		string text = tolerances.TolerancesString();
		if (!string.IsNullOrEmpty(text))
		{
			taggedString += "\n\n" + text;
		}
		if (pawn.MapHeld != null)
		{
			ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(pawn);
			taggedString += "\n\n" + "CurrentExpectationsAndRecreation".Translate(expectationDef.label, expectationDef.joyToleranceDropPerDay.ToStringPercent(), expectationDef.joyKindsNeeded);
			taggedString += "\n\n" + JoyUtility.JoyKindsOnMapString(pawn.MapHeld);
		}
		else
		{
			Caravan caravan = pawn.GetCaravan();
			if (caravan != null)
			{
				float num = caravan.needs.GetCurrentJoyGainPerTick(pawn) * 2500f;
				if (num > 0f)
				{
					taggedString += "\n\n" + "GainingJoyBecauseCaravanNotMoving".Translate() + ": +" + num.ToStringPercent() + "/" + "LetterHour".Translate();
				}
			}
		}
		return taggedString.Resolve();
	}
}
