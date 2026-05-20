using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class QualityUtility
{
	public static List<QualityCategory> AllQualityCategories;

	public static int QualityCount { get; }

	static QualityUtility()
	{
		AllQualityCategories = new List<QualityCategory>();
		foreach (QualityCategory value in Enum.GetValues(typeof(QualityCategory)))
		{
			AllQualityCategories.Add(value);
		}
		QualityCount = Enum.GetValues(typeof(QualityCategory)).Length;
	}

	public static bool TryGetQuality(this Thing t, out QualityCategory qc)
	{
		CompQuality compQuality = ((!(t is MinifiedThing minifiedThing)) ? (t as ThingWithComps)?.compQuality : (minifiedThing.InnerThing as ThingWithComps)?.compQuality);
		if (compQuality == null)
		{
			qc = QualityCategory.Normal;
			return false;
		}
		qc = compQuality.Quality;
		return true;
	}

	public static string GetLabel(this QualityCategory cat)
	{
		return cat switch
		{
			QualityCategory.Awful => "QualityCategory_Awful".Translate(), 
			QualityCategory.Poor => "QualityCategory_Poor".Translate(), 
			QualityCategory.Normal => "QualityCategory_Normal".Translate(), 
			QualityCategory.Good => "QualityCategory_Good".Translate(), 
			QualityCategory.Excellent => "QualityCategory_Excellent".Translate(), 
			QualityCategory.Masterwork => "QualityCategory_Masterwork".Translate(), 
			QualityCategory.Legendary => "QualityCategory_Legendary".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}

	public static string GetLabelShort(this QualityCategory cat)
	{
		return cat switch
		{
			QualityCategory.Awful => "QualityCategoryShort_Awful".Translate(), 
			QualityCategory.Poor => "QualityCategoryShort_Poor".Translate(), 
			QualityCategory.Normal => "QualityCategoryShort_Normal".Translate(), 
			QualityCategory.Good => "QualityCategoryShort_Good".Translate(), 
			QualityCategory.Excellent => "QualityCategoryShort_Excellent".Translate(), 
			QualityCategory.Masterwork => "QualityCategoryShort_Masterwork".Translate(), 
			QualityCategory.Legendary => "QualityCategoryShort_Legendary".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}

	public static bool FollowQualityThingFilter(this ThingDef def)
	{
		if (def.HasComp(typeof(CompQuality)))
		{
			return true;
		}
		return false;
	}

	public static QualityCategory GenerateQuality(QualityGenerator qualityGenerator)
	{
		return qualityGenerator switch
		{
			QualityGenerator.BaseGen => GenerateQualityBaseGen(), 
			QualityGenerator.Reward => GenerateQualityReward(), 
			QualityGenerator.Gift => GenerateQualityGift(), 
			QualityGenerator.Super => GenerateQualitySuper(), 
			QualityGenerator.Trader => GenerateQualityTraderItem(), 
			_ => throw new NotImplementedException(qualityGenerator.ToString()), 
		};
	}

	public static QualityCategory GenerateQualityRandomEqualChance()
	{
		return AllQualityCategories.RandomElement();
	}

	public static QualityCategory GenerateQualityReward()
	{
		return GenerateFromGaussian(1f, QualityCategory.Legendary, QualityCategory.Excellent, QualityCategory.Good);
	}

	public static QualityCategory GenerateQualityGift()
	{
		return GenerateFromGaussian(1f, QualityCategory.Legendary, QualityCategory.Normal, QualityCategory.Normal);
	}

	public static QualityCategory GenerateQualitySuper()
	{
		return GenerateFromGaussian(1f, QualityCategory.Legendary, QualityCategory.Masterwork, QualityCategory.Masterwork);
	}

	public static QualityCategory GenerateQualityTraderItem()
	{
		return GenerateFromGaussian(1f, QualityCategory.Masterwork, QualityCategory.Normal, QualityCategory.Normal);
	}

	public static QualityCategory GenerateQualityBaseGen()
	{
		if (Rand.Value < 0.3f)
		{
			return QualityCategory.Normal;
		}
		return GenerateFromGaussian(1f, QualityCategory.Excellent);
	}

	public static QualityCategory GenerateQualityGeneratingPawn(PawnKindDef pawnKind, ThingDef forThing)
	{
		if (pawnKind.forceNormalGearQuality)
		{
			return QualityCategory.Normal;
		}
		if (forThing.IsWeapon && pawnKind.forceWeaponQuality.HasValue)
		{
			return pawnKind.forceWeaponQuality.Value;
		}
		if (!forThing.IsWeapon && pawnKind.specificApparelRequirements != null)
		{
			for (int i = 0; i < pawnKind.specificApparelRequirements.Count; i++)
			{
				if (pawnKind.specificApparelRequirements[i].Quality.HasValue && PawnApparelGenerator.ApparelRequirementHandlesThing(pawnKind.specificApparelRequirements[i], forThing))
				{
					return pawnKind.specificApparelRequirements[i].Quality.Value;
				}
			}
		}
		int itemQuality = (int)pawnKind.itemQuality;
		float value = Rand.Value;
		int value2 = ((value < 0.1f) ? (itemQuality - 1) : ((!(value < 0.2f)) ? itemQuality : (itemQuality + 1)));
		value2 = Mathf.Clamp(value2, (int)pawnKind.minApparelQuality, (int)pawnKind.maxApparelQuality);
		return (QualityCategory)value2;
	}

	public static QualityCategory GenerateQualityCreatedByPawn(int relevantSkillLevel, bool inspired)
	{
		float num = 0f;
		switch (relevantSkillLevel)
		{
		case 0:
			num += 0.7f;
			break;
		case 1:
			num += 1.1f;
			break;
		case 2:
			num += 1.5f;
			break;
		case 3:
			num += 1.8f;
			break;
		case 4:
			num += 2f;
			break;
		case 5:
			num += 2.2f;
			break;
		case 6:
			num += 2.4f;
			break;
		case 7:
			num += 2.6f;
			break;
		case 8:
			num += 2.8f;
			break;
		case 9:
			num += 2.95f;
			break;
		case 10:
			num += 3.1f;
			break;
		case 11:
			num += 3.25f;
			break;
		case 12:
			num += 3.4f;
			break;
		case 13:
			num += 3.5f;
			break;
		case 14:
			num += 3.6f;
			break;
		case 15:
			num += 3.7f;
			break;
		case 16:
			num += 3.8f;
			break;
		case 17:
			num += 3.9f;
			break;
		case 18:
			num += 4f;
			break;
		case 19:
			num += 4.1f;
			break;
		case 20:
			num += 4.2f;
			break;
		}
		int value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.8f);
		value = Mathf.Clamp(value, 0, 5);
		if (value == 5 && Rand.Value < 0.5f)
		{
			value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.95f);
			value = Mathf.Clamp(value, 0, 5);
		}
		QualityCategory qualityCategory = (QualityCategory)value;
		if (inspired)
		{
			qualityCategory = AddLevels(qualityCategory, 2);
		}
		return qualityCategory;
	}

	public static QualityCategory GenerateQualityCreatedByPawn(Pawn pawn, SkillDef relevantSkill, bool consumeInspiration = true)
	{
		int relevantSkillLevel = (pawn.RaceProps.IsMechanoid ? pawn.RaceProps.mechFixedSkillLevel : pawn.skills.GetSkill(relevantSkill).Level);
		bool flag = consumeInspiration && pawn.InspirationDef == InspirationDefOf.Inspired_Creativity;
		QualityCategory qualityCategory = GenerateQualityCreatedByPawn(relevantSkillLevel, flag);
		if (ModsConfig.IdeologyActive && pawn.Ideo != null)
		{
			Precept_Role role = pawn.Ideo.GetRole(pawn);
			if (role != null && role.def.roleEffects != null)
			{
				RoleEffect roleEffect = role.def.roleEffects.FirstOrDefault((RoleEffect eff) => eff is RoleEffect_ProductionQualityOffset);
				if (roleEffect != null)
				{
					qualityCategory = AddLevels(qualityCategory, ((RoleEffect_ProductionQualityOffset)roleEffect).offset);
				}
			}
		}
		if (flag)
		{
			pawn.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Creativity);
		}
		return qualityCategory;
	}

	public static QualityCategory GenerateFromGaussian(float widthFactor, QualityCategory max = QualityCategory.Legendary, QualityCategory center = QualityCategory.Normal, QualityCategory min = QualityCategory.Awful)
	{
		float num = Rand.Gaussian((float)(int)center + 0.5f, widthFactor);
		if (num < (float)(int)min)
		{
			num = (int)min;
		}
		if (num > (float)(int)max)
		{
			num = (int)max;
		}
		return (QualityCategory)(int)num;
	}

	private static QualityCategory AddLevels(QualityCategory quality, int levels)
	{
		return (QualityCategory)Mathf.Min((int)quality + levels, 6);
	}

	public static void SendCraftNotification(Thing thing, Pawn worker)
	{
		if (worker == null)
		{
			return;
		}
		CompQuality compQuality = (thing as ThingWithComps)?.compQuality;
		if (compQuality == null)
		{
			return;
		}
		CompArt compArt = thing.TryGetComp<CompArt>();
		if (compArt == null || compArt.Props.mustBeFullGrave)
		{
			if (compQuality.Quality == QualityCategory.Masterwork)
			{
				Find.LetterStack.ReceiveLetter("LetterCraftedMasterworkLabel".Translate(), "LetterCraftedMasterworkMessage".Translate().Formatted(worker.LabelShort, thing.LabelShort, worker.Named("WORKER"), thing.Named("CRAFTED")), LetterDefOf.PositiveEvent, thing);
			}
			else if (compQuality.Quality == QualityCategory.Legendary)
			{
				Find.LetterStack.ReceiveLetter("LetterCraftedLegendaryLabel".Translate(), "LetterCraftedLegendaryMessage".Translate().Formatted(worker.LabelShort, thing.LabelShort, worker.Named("WORKER"), thing.Named("CRAFTED")), LetterDefOf.PositiveEvent, thing);
			}
		}
		else if (compQuality.Quality == QualityCategory.Masterwork)
		{
			Find.LetterStack.ReceiveLetter("LetterCraftedMasterworkLabel".Translate(), "LetterCraftedMasterworkMessageArt".Translate().Formatted(compArt.GenerateImageDescription(), worker.LabelShort, thing.LabelShort, worker.Named("WORKER"), thing.Named("CRAFTED")), LetterDefOf.PositiveEvent, thing);
		}
		else if (compQuality.Quality == QualityCategory.Legendary)
		{
			Find.LetterStack.ReceiveLetter("LetterCraftedLegendaryLabel".Translate(), "LetterCraftedLegendaryMessageArt".Translate().Formatted(compArt.GenerateImageDescription(), worker.LabelShort, thing.LabelShort, worker.Named("WORKER"), thing.Named("CRAFTED")), LetterDefOf.PositiveEvent, thing);
		}
	}

	[DebugOutput]
	private static void QualityGenerationData()
	{
		List<TableDataGetter<QualityCategory>> list = new List<TableDataGetter<QualityCategory>>();
		list.Add(new TableDataGetter<QualityCategory>("quality", (QualityCategory q) => q.ToString()));
		list.Add(new TableDataGetter<QualityCategory>("Rewards\n(quests,\netc...? )", (QualityCategory q) => DebugQualitiesStringSingle(q, () => GenerateQualityReward())));
		list.Add(new TableDataGetter<QualityCategory>("Trader\nitems", (QualityCategory q) => DebugQualitiesStringSingle(q, () => GenerateQualityTraderItem())));
		list.Add(new TableDataGetter<QualityCategory>("Map generation\nitems and\nbuildings\n(e.g. NPC bases)", (QualityCategory q) => DebugQualitiesStringSingle(q, () => GenerateQualityBaseGen())));
		list.Add(new TableDataGetter<QualityCategory>("Gifts", (QualityCategory q) => DebugQualitiesStringSingle(q, () => GenerateQualityGift())));
		for (int num = 0; num <= 20; num++)
		{
			int localLevel = num;
			list.Add(new TableDataGetter<QualityCategory>("Made\nat skill\n" + num, (QualityCategory q) => DebugQualitiesStringSingle(q, () => GenerateQualityCreatedByPawn(localLevel, inspired: false))));
		}
		foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefs.OrderBy((PawnKindDef k) => k.combatPower))
		{
			PawnKindDef localPk = item;
			if (!localPk.RaceProps.Humanlike)
			{
				continue;
			}
			list.Add(new TableDataGetter<QualityCategory>("Gear for\n" + localPk.defName + "\nPower " + localPk.combatPower.ToString("F0") + "\nitemQuality:\n" + localPk.itemQuality, (QualityCategory q) => DebugQualitiesStringSingle(q, () => GenerateQualityGeneratingPawn(localPk, null))));
		}
		DebugTables.MakeTablesDialog(AllQualityCategories, list.ToArray());
	}

	private static string DebugQualitiesStringSingle(QualityCategory quality, Func<QualityCategory> qualityGenerator)
	{
		int num = 10000;
		List<QualityCategory> list = new List<QualityCategory>();
		for (int i = 0; i < num; i++)
		{
			list.Add(qualityGenerator());
		}
		return ((float)list.Where((QualityCategory q) => q == quality).Count() / (float)num).ToStringPercent();
	}

	private static string DebugQualitiesString(Func<QualityCategory> qualityGenerator)
	{
		int num = 10000;
		StringBuilder stringBuilder = new StringBuilder();
		List<QualityCategory> list = new List<QualityCategory>();
		for (int i = 0; i < num; i++)
		{
			list.Add(qualityGenerator());
		}
		foreach (QualityCategory qu in AllQualityCategories)
		{
			stringBuilder.AppendLine(qu.ToString() + " - " + ((float)list.Where((QualityCategory q) => q == qu).Count() / (float)num).ToStringPercent());
		}
		return stringBuilder.ToString();
	}
}
