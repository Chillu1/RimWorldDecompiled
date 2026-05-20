using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Need_Outdoors : Need
{
	private const float Delta_IndoorsThickRoof = -0.45f;

	private const float Delta_OutdoorsThickRoof = -0.4f;

	private const float Delta_IndoorsThinRoof = -0.32f;

	private const float Minimum_IndoorsThinRoof = 0.2f;

	private const float Delta_OutdoorsThinRoof = 1f;

	private const float Delta_IndoorsNoRoof = 5f;

	private const float Delta_OutdoorsNoRoof = 8f;

	private const float DeltaFactor_InBed = 0.2f;

	private float lastEffectiveDelta;

	public override int GUIChangeArrow
	{
		get
		{
			if (IsFrozen)
			{
				return 0;
			}
			return Math.Sign(lastEffectiveDelta);
		}
	}

	public OutdoorsCategory CurCategory
	{
		get
		{
			if (CurLevel > 0.8f)
			{
				return OutdoorsCategory.Free;
			}
			if (CurLevel > 0.6f)
			{
				return OutdoorsCategory.NeedFreshAir;
			}
			if (CurLevel > 0.4f)
			{
				return OutdoorsCategory.CabinFeverLight;
			}
			if (CurLevel >= 0.2f)
			{
				return OutdoorsCategory.CabinFeverSevere;
			}
			if (CurLevel > 0.05f)
			{
				return OutdoorsCategory.Trapped;
			}
			return OutdoorsCategory.Entombed;
		}
	}

	public override bool ShowOnNeedList => !Disabled;

	private bool Disabled
	{
		get
		{
			if (!pawn.Dead)
			{
				return pawn.needs.PrefersIndoors;
			}
			return true;
		}
	}

	public Need_Outdoors(Pawn pawn)
		: base(pawn)
	{
		threshPercents = new List<float>();
		threshPercents.Add(0.8f);
		threshPercents.Add(0.6f);
		threshPercents.Add(0.4f);
		threshPercents.Add(0.2f);
		threshPercents.Add(0.05f);
	}

	public override void SetInitialLevel()
	{
		CurLevel = 1f;
	}

	public override void NeedInterval()
	{
		if (Disabled)
		{
			CurLevel = 1f;
		}
		else if (!IsFrozen)
		{
			float b = 0.2f;
			float num = 0f;
			bool num2 = !pawn.Spawned || pawn.Position.UsesOutdoorTemperature(pawn.Map);
			RoofDef roofDef = (pawn.Spawned ? pawn.Position.GetRoof(pawn.Map) : null);
			if (num2)
			{
				num = ((roofDef == null) ? 8f : ((!roofDef.isThickRoof) ? 1f : (-0.4f)));
			}
			else if (roofDef == null)
			{
				num = 5f;
			}
			else if (!roofDef.isThickRoof)
			{
				num = -0.32f;
			}
			else
			{
				num = -0.45f;
				b = 0f;
			}
			if (pawn.InBed() && num < 0f)
			{
				num *= 0.2f;
			}
			num *= 0.0025f;
			float curLevel = CurLevel;
			if (num < 0f)
			{
				CurLevel = Mathf.Min(CurLevel, Mathf.Max(CurLevel + num, b));
			}
			else
			{
				CurLevel = Mathf.Min(CurLevel + num, 1f);
			}
			lastEffectiveDelta = CurLevel - curLevel;
		}
	}
}
