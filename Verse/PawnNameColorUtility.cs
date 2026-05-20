using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class PawnNameColorUtility
{
	private static readonly List<Color> ColorsNeutral;

	private static readonly List<Color> ColorsHostile;

	private static readonly List<Color> ColorsPrisoner;

	private static readonly Color ColorBaseNeutral;

	public static readonly Color ColorBaseHostile;

	private static readonly Color ColorBasePrisoner;

	private static readonly Color ColorSlave;

	private static readonly Color ColorColony;

	private static readonly Color ColorWildMan;

	private static readonly Color ColorUncontrolledPlayerMech;

	private const int ColorShiftCount = 10;

	private static readonly List<Color> ColorShifts;

	static PawnNameColorUtility()
	{
		ColorsNeutral = new List<Color>();
		ColorsHostile = new List<Color>();
		ColorsPrisoner = new List<Color>();
		ColorBaseNeutral = new Color(0.4f, 0.85f, 0.9f);
		ColorBaseHostile = new Color(0.9f, 0.2f, 0.2f);
		ColorBasePrisoner = new Color(1f, 0.85f, 0.5f);
		ColorSlave = new Color32(222, 192, 22, byte.MaxValue);
		ColorColony = new Color(0.9f, 0.9f, 0.9f);
		ColorWildMan = new Color(1f, 0.8f, 1f);
		ColorUncontrolledPlayerMech = new Color(0.9f, 0.2f, 0.2f);
		ColorShifts = new List<Color>
		{
			new Color(1f, 1f, 1f),
			new Color(0.8f, 1f, 1f),
			new Color(0.8f, 0.8f, 1f),
			new Color(0.8f, 0.8f, 0.8f),
			new Color(1.2f, 1f, 1f),
			new Color(0.8f, 1.2f, 1f),
			new Color(0.8f, 1.2f, 1.2f),
			new Color(1.2f, 1.2f, 1.2f),
			new Color(1f, 1.2f, 1f),
			new Color(1.2f, 1f, 0.8f)
		};
		for (int i = 0; i < 10; i++)
		{
			ColorsNeutral.Add(RandomShiftOf(ColorBaseNeutral, i));
			ColorsHostile.Add(RandomShiftOf(ColorBaseHostile, i));
			ColorsPrisoner.Add(RandomShiftOf(ColorBasePrisoner, i));
		}
	}

	private static Color RandomShiftOf(Color color, int i)
	{
		return new Color(Mathf.Clamp01(color.r * ColorShifts[i].r), Mathf.Clamp01(color.g * ColorShifts[i].g), Mathf.Clamp01(color.b * ColorShifts[i].b), color.a);
	}

	public static Color PawnNameColorOf(Pawn pawn)
	{
		if (pawn.MentalStateDef != null)
		{
			return pawn.MentalStateDef.nameColor;
		}
		int index = ((pawn.Faction != null) ? (pawn.Faction.randomKey % 10) : 0);
		if (pawn.IsPrisoner)
		{
			return ColorsPrisoner[index];
		}
		if (pawn.IsSlave && SlaveRebellionUtility.IsRebelling(pawn))
		{
			return ColorBaseHostile;
		}
		if (pawn.IsSlave)
		{
			return ColorSlave;
		}
		if (pawn.IsWildMan())
		{
			return ColorWildMan;
		}
		if (pawn.Faction == null)
		{
			return ColorsNeutral[index];
		}
		if (pawn.IsColonyMechRequiringMechanitor())
		{
			return ColorUncontrolledPlayerMech;
		}
		if (pawn.Faction == Faction.OfPlayer)
		{
			return ColorColony;
		}
		if (pawn.Faction.HostileTo(Faction.OfPlayer))
		{
			return ColorsHostile[index];
		}
		return ColorsNeutral[index];
	}
}
