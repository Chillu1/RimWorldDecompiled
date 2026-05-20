using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Need_Indoors : Need
{
	private static readonly float[] Thresholds = new float[5] { 0.8f, 0.6f, 0.4f, 0.2f, 0.05f };

	private const float Delta_Indoors_ThickRoof = 2f;

	private const float Delta_Indoors_ThinRoof = 1f;

	private const float Delta_Indoors_NoRoof = 0f;

	private const float Delta_Outdoors_ThickRoof = 0f;

	private const float Delta_Outdoors_ThinRoof = -0.25f;

	private const float Delta_Outdoors_NoRoof = -0.25f;

	private float lastEffectiveDelta;

	public override bool ShowOnNeedList => !Disabled;

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

	public IndoorsCategory CurCategory
	{
		get
		{
			if (CurLevel > Thresholds[0])
			{
				return IndoorsCategory.ComfortablyIndoors;
			}
			if (CurLevel > Thresholds[1])
			{
				return IndoorsCategory.JustOutdoors;
			}
			if (CurLevel > Thresholds[2])
			{
				return IndoorsCategory.Outdoors;
			}
			if (CurLevel > Thresholds[3])
			{
				return IndoorsCategory.LongOutdoors;
			}
			if (CurLevel > Thresholds[4])
			{
				return IndoorsCategory.VeryLongOutdoors;
			}
			return IndoorsCategory.BrutalOutdoors;
		}
	}

	private bool Disabled
	{
		get
		{
			if (!pawn.Dead)
			{
				return pawn.needs.PrefersOutdoors;
			}
			return true;
		}
	}

	public Need_Indoors(Pawn pawn)
		: base(pawn)
	{
		threshPercents = new List<float>(Thresholds);
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
			float num = 0f;
			bool num2 = !pawn.Spawned || pawn.Position.UsesOutdoorTemperature(pawn.Map);
			RoofDef roofDef = (pawn.Spawned ? pawn.Position.GetRoof(pawn.Map) : null);
			float curLevel = CurLevel;
			num = ((!num2) ? ((roofDef == null) ? 0f : (roofDef.isThickRoof ? 2f : 1f)) : ((roofDef == null) ? (-0.25f) : ((!roofDef.isThickRoof) ? (-0.25f) : 0f)));
			num *= 0.0025f;
			CurLevel = ((num < 0f) ? Mathf.Min(CurLevel, CurLevel + num) : Mathf.Min(CurLevel + num, 1f));
			lastEffectiveDelta = CurLevel - curLevel;
		}
	}
}
