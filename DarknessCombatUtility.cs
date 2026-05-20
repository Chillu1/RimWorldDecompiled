using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

public static class DarknessCombatUtility
{
	private const float SkyGlowDarkThreshold = 0.35f;

	private static List<StatModifier> tmpStatDefs = new List<StatModifier>();

	public static bool IsOutdoorsAndDark(Thing thing)
	{
		if (Outdoors(thing))
		{
			return !IsOutdoorsLit(thing.Map);
		}
		return false;
	}

	public static bool IsOutdoorsAndLit(Thing thing)
	{
		if (Outdoors(thing))
		{
			return IsOutdoorsLit(thing.Map);
		}
		return false;
	}

	public static bool IsIndoorsAndDark(Thing thing)
	{
		if (!Outdoors(thing))
		{
			if (thing.Map.glowGrid.PsychGlowAt(thing.Position) != PsychGlow.Dark)
			{
				return DarklightUtility.IsDarklightAt(thing.Position, thing.Map);
			}
			return true;
		}
		return false;
	}

	public static bool IsIndoorsAndLit(Thing thing)
	{
		if (!Outdoors(thing) && thing.Map.glowGrid.PsychGlowAt(thing.Position) == PsychGlow.Lit)
		{
			return !DarklightUtility.IsDarklightAt(thing.Position, thing.Map);
		}
		return false;
	}

	private static bool Outdoors(Thing thing)
	{
		RoofDef roof = thing.Position.GetRoof(thing.Map);
		if (roof != null && (roof.isNatural || roof.isThickRoof))
		{
			return false;
		}
		return thing.Position.GetRoom(thing.Map)?.PsychologicallyOutdoors ?? false;
	}

	private static bool IsOutdoorsLit(Map map)
	{
		return map.skyManager.CurSkyGlow > 0.35f;
	}

	public static IEnumerable<StatDrawEntry> GetStatEntriesForPawn(Pawn pawn)
	{
		if (pawn.Ideo == null)
		{
			yield break;
		}
		List<Precept> precepts = pawn.Ideo.PreceptsListForReading;
		tmpStatDefs.Clear();
		for (int i = 0; i < precepts.Count; i++)
		{
			List<StatModifier> statOffsets = precepts[i].def.statOffsets;
			if (statOffsets == null)
			{
				continue;
			}
			for (int j = 0; j < statOffsets.Count; j++)
			{
				StatModifier statModifier = statOffsets[j];
				if (statModifier.stat == StatDefOf.ShootingAccuracyOutdoorsLitOffset || statModifier.stat == StatDefOf.ShootingAccuracyOutdoorsDarkOffset || statModifier.stat == StatDefOf.ShootingAccuracyIndoorsDarkOffset || statModifier.stat == StatDefOf.ShootingAccuracyIndoorsLitOffset)
				{
					tmpStatDefs.Add(statModifier);
				}
			}
		}
		if (tmpStatDefs.Count > 0)
		{
			Ideo ideo = pawn.Ideo;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Stat_Pawn_DarknessCombatShooting_Desc".Translate() + " " + "Stat_PawnDarkness_FollowingOffset".Translate() + ":");
			stringBuilder.AppendLine();
			for (int k = 0; k < tmpStatDefs.Count; k++)
			{
				stringBuilder.AppendLine(tmpStatDefs[k].stat.LabelCap + ": " + tmpStatDefs[k].ValueToStringAsOffset);
			}
			stringBuilder.AppendLine("\n" + "CausedBy".Translate() + ": " + "BeliefInIdeo".Translate() + " " + ideo.name);
			yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, "Stat_Pawn_DarknessCombatShooting_Name".Translate(), tmpStatDefs.MinBy((StatModifier s) => s.value).ValueToStringAsOffset + " ~ " + tmpStatDefs.MaxBy((StatModifier s) => s.value).ValueToStringAsOffset, stringBuilder.ToString(), 4051, null, new Dialog_InfoCard.Hyperlink[1]
			{
				new Dialog_InfoCard.Hyperlink(ideo)
			});
		}
		tmpStatDefs.Clear();
		for (int num = 0; num < precepts.Count; num++)
		{
			List<StatModifier> statOffsets2 = precepts[num].def.statOffsets;
			if (statOffsets2 == null)
			{
				continue;
			}
			for (int num2 = 0; num2 < statOffsets2.Count; num2++)
			{
				StatModifier statModifier2 = statOffsets2[num2];
				if (statModifier2.stat == StatDefOf.MeleeHitChanceIndoorsDarkOffset || statModifier2.stat == StatDefOf.MeleeHitChanceIndoorsLitOffset || statModifier2.stat == StatDefOf.MeleeHitChanceOutdoorsLitOffset || statModifier2.stat == StatDefOf.MeleeHitChanceOutdoorsDarkOffset)
				{
					tmpStatDefs.Add(statModifier2);
				}
			}
		}
		if (tmpStatDefs.Count > 0)
		{
			Ideo ideo2 = pawn.Ideo;
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendLine("Stat_Pawn_DarknessMeleeHitChance_Desc".Translate() + " " + "Stat_PawnDarkness_FollowingOffset".Translate() + ":");
			stringBuilder2.AppendLine();
			for (int num3 = 0; num3 < tmpStatDefs.Count; num3++)
			{
				stringBuilder2.AppendLine(tmpStatDefs[num3].stat.LabelCap + ": " + tmpStatDefs[num3].ValueToStringAsOffset);
			}
			stringBuilder2.AppendLine("\n" + "CausedBy".Translate() + ": " + "BeliefInIdeo".Translate() + " " + ideo2.name);
			yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, "Stat_Pawn_DarknessMeleeHitChance_Name".Translate(), tmpStatDefs.MinBy((StatModifier s) => s.value).ValueToStringAsOffset + " ~ " + tmpStatDefs.MaxBy((StatModifier s) => s.value).ValueToStringAsOffset, stringBuilder2.ToString(), 4101, null, new Dialog_InfoCard.Hyperlink[1]
			{
				new Dialog_InfoCard.Hyperlink(ideo2)
			});
		}
		tmpStatDefs.Clear();
		for (int num4 = 0; num4 < precepts.Count; num4++)
		{
			List<StatModifier> statOffsets3 = precepts[num4].def.statOffsets;
			if (statOffsets3 == null)
			{
				continue;
			}
			for (int num5 = 0; num5 < statOffsets3.Count; num5++)
			{
				StatModifier statModifier3 = statOffsets3[num5];
				if (statModifier3.stat == StatDefOf.MeleeDodgeChanceIndoorsDarkOffset || statModifier3.stat == StatDefOf.MeleeDodgeChanceIndoorsLitOffset || statModifier3.stat == StatDefOf.MeleeDodgeChanceOutdoorsLitOffset || statModifier3.stat == StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset)
				{
					tmpStatDefs.Add(statModifier3);
				}
			}
		}
		if (tmpStatDefs.Count > 0)
		{
			Ideo ideo3 = pawn.Ideo;
			StringBuilder stringBuilder3 = new StringBuilder();
			stringBuilder3.AppendLine("Stat_Pawn_DarknessMeleeDodgeChance_Desc".Translate() + " " + "Stat_PawnDarkness_FollowingOffset".Translate() + ":");
			stringBuilder3.AppendLine();
			for (int num6 = 0; num6 < tmpStatDefs.Count; num6++)
			{
				stringBuilder3.AppendLine(tmpStatDefs[num6].stat.LabelCap + ": " + tmpStatDefs[num6].ValueToStringAsOffset);
			}
			stringBuilder3.AppendLine("\n" + "CausedBy".Translate() + ": " + "BeliefInIdeo".Translate() + " " + ideo3.name);
			yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, "Stat_Pawn_DarknessMeleeDodgeChance_Name".Translate(), tmpStatDefs.MinBy((StatModifier s) => s.value).ValueToStringAsOffset + " ~ " + tmpStatDefs.MaxBy((StatModifier s) => s.value).ValueToStringAsOffset, stringBuilder3.ToString(), 4101, null, new Dialog_InfoCard.Hyperlink[1]
			{
				new Dialog_InfoCard.Hyperlink(ideo3)
			});
		}
		tmpStatDefs.Clear();
	}
}
