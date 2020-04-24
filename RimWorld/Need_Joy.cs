using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
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

		private float FallPerInterval
		{
			get
			{
				switch (CurCategory)
				{
				case JoyCategory.Empty:
					return 0.0015f;
				case JoyCategory.VeryLow:
					return 0.0006f;
				case JoyCategory.Low:
					return 0.00105f;
				case JoyCategory.Satisfied:
					return 0.0015f;
				case JoyCategory.High:
					return 0.0015f;
				case JoyCategory.Extreme:
					return 0.0015f;
				default:
					throw new InvalidOperationException();
				}
			}
		}

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

		private bool GainingJoy => Find.TickManager.TicksGame < lastGainTick + 10;

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
					CurLevel -= FallPerInterval;
				}
			}
		}

		public override string GetTipString()
		{
			string text = base.GetTipString();
			string text2 = tolerances.TolerancesString();
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + "\n\n" + text2;
			}
			if (pawn.MapHeld != null)
			{
				ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(pawn);
				text += "\n\n" + "CurrentExpectationsAndRecreation".Translate(expectationDef.label, expectationDef.joyToleranceDropPerDay.ToStringPercent(), expectationDef.joyKindsNeeded);
				text = text + "\n\n" + JoyUtility.JoyKindsOnMapString(pawn.MapHeld);
			}
			else
			{
				Caravan caravan = pawn.GetCaravan();
				if (caravan != null)
				{
					float num = caravan.needs.GetCurrentJoyGainPerTick(pawn) * 2500f;
					if (num > 0f)
					{
						text += "\n\n" + "GainingJoyBecauseCaravanNotMoving".Translate() + ": +" + num.ToStringPercent() + "/" + "LetterHour".Translate();
					}
				}
			}
			return text;
		}
	}
}
