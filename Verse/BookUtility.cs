using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI;

namespace Verse;

public static class BookUtility
{
	private static readonly List<Thing> TmpCandidates = new List<Thing>();

	private static readonly List<Thing> TmpOutcomeCandidates = new List<Thing>();

	private static readonly SimpleCurve QualityResearchExpTick = new SimpleCurve
	{
		new CurvePoint(0f, 0.008f),
		new CurvePoint(1f, 0.012f),
		new CurvePoint(2f, 0.016f),
		new CurvePoint(3f, 0.02f),
		new CurvePoint(4f, 0.024f),
		new CurvePoint(5f, 0.028f),
		new CurvePoint(6f, 0.032f)
	};

	private static readonly SimpleCurve QualityAnomalyExpTick = new SimpleCurve
	{
		new CurvePoint(0f, 3E-05f),
		new CurvePoint(1f, 6E-05f),
		new CurvePoint(2f, 9E-05f),
		new CurvePoint(3f, 0.00012f),
		new CurvePoint(4f, 0.00015f),
		new CurvePoint(5f, 0.00018f),
		new CurvePoint(6f, 0.00021f)
	};

	private static readonly SimpleCurve QualitySkillExpTick = new SimpleCurve
	{
		new CurvePoint(0f, 0.05f),
		new CurvePoint(1f, 0.075f),
		new CurvePoint(2f, 0.1f),
		new CurvePoint(3f, 0.125f),
		new CurvePoint(4f, 0.15f),
		new CurvePoint(5f, 0.175f),
		new CurvePoint(6f, 0.2f)
	};

	private static readonly SimpleCurve QualityJoyFactor = new SimpleCurve
	{
		new CurvePoint(0f, 1.2f),
		new CurvePoint(1f, 1.4f),
		new CurvePoint(2f, 1.6f),
		new CurvePoint(3f, 1.8f),
		new CurvePoint(4f, 2f),
		new CurvePoint(5f, 2.25f),
		new CurvePoint(6f, 2.5f)
	};

	public static bool CanReadEver(Pawn reader)
	{
		if (reader.DevelopmentalStage == DevelopmentalStage.Baby)
		{
			return false;
		}
		if (StatDefOf.ReadingSpeed.Worker.IsDisabledFor(reader))
		{
			return false;
		}
		if (!reader.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return false;
		}
		if (!reader.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
		{
			return false;
		}
		return true;
	}

	public static bool CanReadNow(Pawn reader)
	{
		if (!CanReadEver(reader))
		{
			return false;
		}
		if (GetReadingModifier(reader) <= 0f)
		{
			return false;
		}
		return true;
	}

	public static bool CanReadBook(Book book, Pawn reader, out string reason)
	{
		if (!book.IsReadable)
		{
			reason = "BookNotReadable".Translate(book.Named("BOOK"));
			return false;
		}
		DevelopmentalStage developmentalStageFilter = book.BookComp.Props.developmentalStageFilter;
		if (!developmentalStageFilter.HasAny(reader.DevelopmentalStage))
		{
			string arg = developmentalStageFilter.ToCommaList();
			reason = "BookCantBeStage".Translate(reader.Named("PAWN"), arg.Named("STAGES"));
			return false;
		}
		if (!reader.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
		{
			reason = "BookBlind".Translate(reader.Named("PAWN"));
			return false;
		}
		if (!CanReadEver(reader))
		{
			reason = "BookCantRead".Translate(reader.Named("PAWN"));
			return false;
		}
		reason = null;
		return true;
	}

	public static float GetReadingModifier(Pawn reader)
	{
		if (reader == null || StatDefOf.ReadingSpeed.Worker.IsDisabledFor(reader))
		{
			return 1f;
		}
		return reader.GetStatValue(StatDefOf.ReadingSpeed);
	}

	public static Book MakeBook(ArtGenerationContext context, QualityGenerator? qualityGenerator = null)
	{
		return MakeBook(GetBookDefs().RandomElementByWeight((ThingDef x) => x.GetCompProperties<CompProperties_Book>().pickWeight), context, qualityGenerator);
	}

	public static Book MakeBook(ThingDef def, ArtGenerationContext context, QualityGenerator? qualityGenerator = null)
	{
		ThingDef stuff = GenStuff.RandomStuffFor(def);
		Thing thing = ThingMaker.MakeThing(def, stuff);
		CompQuality compQuality = thing.TryGetComp<CompQuality>();
		if (compQuality != null)
		{
			QualityCategory q = (qualityGenerator.HasValue ? QualityUtility.GenerateQuality(qualityGenerator.Value) : QualityUtility.GenerateQualityRandomEqualChance());
			compQuality.SetQuality(q, context);
		}
		return thing as Book;
	}

	private static List<ThingDef> GetBookDefs()
	{
		return DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.HasComp<CompBook>()).ToList();
	}

	public static float GetReadingBonus(Thing thing)
	{
		Room room = thing.GetRoom();
		if (room != null && room.ProperRoom && !room.PsychologicallyOutdoors)
		{
			return room.GetStat(RoomStatDefOf.ReadingBonus);
		}
		return 1f;
	}

	public static bool TryGetRandomBookToRead(Pawn pawn, out Book book)
	{
		book = null;
		TmpCandidates.Clear();
		TmpOutcomeCandidates.Clear();
		TmpCandidates.AddRange(from thing in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Book)
			where IsValidBook(thing, pawn)
			select thing);
		TmpCandidates.AddRange(from thing in pawn.Map.listerThings.GetThingsOfType<Building_Bookcase>().SelectMany((Building_Bookcase x) => x.HeldBooks)
			where IsValidBook(thing, pawn)
			select thing);
		if (TmpCandidates.Empty())
		{
			return false;
		}
		foreach (Thing tmpCandidate in TmpCandidates)
		{
			if (tmpCandidate is Book book2 && book2.ProvidesOutcome(pawn))
			{
				TmpOutcomeCandidates.Add(tmpCandidate);
			}
		}
		book = (Book)(TmpOutcomeCandidates.Any() ? TmpOutcomeCandidates.RandomElement() : TmpCandidates.RandomElement());
		TmpCandidates.Clear();
		TmpOutcomeCandidates.Clear();
		return true;
	}

	private static bool IsValidBook(Thing thing, Pawn pawn)
	{
		if (thing is Book && !thing.IsForbiddenHeld(pawn) && pawn.reading?.CurrentPolicy != null && pawn.reading.CurrentPolicy.defFilter.Allows(thing) && pawn.reading.CurrentPolicy.effectFilter.Allows(thing) && pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None) && !thing.Fogged() && thing.IsPoliticallyProper(pawn))
		{
			return !thing.VacuumConcernTo(pawn);
		}
		return false;
	}

	public static float GetResearchExpForQuality(QualityCategory quality)
	{
		return QualityResearchExpTick.Evaluate((int)quality);
	}

	public static float GetAnomalyExpForQuality(QualityCategory quality)
	{
		return QualityAnomalyExpTick.Evaluate((int)quality);
	}

	public static float GetSkillExpForQuality(QualityCategory quality)
	{
		return QualitySkillExpTick.Evaluate((int)quality);
	}

	public static float GetNovelJoyFactorForQuality(QualityCategory quality)
	{
		return QualityJoyFactor.Evaluate((int)quality);
	}
}
