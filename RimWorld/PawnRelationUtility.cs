using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PawnRelationUtility
{
	public static IEnumerable<PawnRelationDef> GetRelations(this Pawn me, Pawn other)
	{
		if (me == other || !me.RaceProps.IsFlesh || !other.RaceProps.IsFlesh || !me.relations.RelatedToAnyoneOrAnyoneRelatedToMe || !other.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
		{
			yield break;
		}
		try
		{
			bool anyNonKinFamilyByBloodRelation = false;
			List<PawnRelationDef> defs = DefDatabase<PawnRelationDef>.AllDefsListForReading;
			int i = 0;
			for (int count = defs.Count; i < count; i++)
			{
				PawnRelationDef pawnRelationDef = defs[i];
				if (pawnRelationDef != PawnRelationDefOf.Kin && pawnRelationDef.Worker.InRelation(me, other))
				{
					if (pawnRelationDef.familyByBloodRelation)
					{
						anyNonKinFamilyByBloodRelation = true;
					}
					yield return pawnRelationDef;
				}
			}
			if (!anyNonKinFamilyByBloodRelation && PawnRelationDefOf.Kin.Worker.InRelation(me, other))
			{
				yield return PawnRelationDefOf.Kin;
			}
		}
		finally
		{
		}
	}

	public static PawnRelationDef GetMostImportantRelation(this Pawn me, Pawn other)
	{
		PawnRelationDef pawnRelationDef = null;
		foreach (PawnRelationDef relation in me.GetRelations(other))
		{
			if (pawnRelationDef == null || relation.importance > pawnRelationDef.importance)
			{
				pawnRelationDef = relation;
			}
		}
		return pawnRelationDef;
	}

	public static void Notify_PawnsSeenByPlayer(IEnumerable<Pawn> seenPawns, out string pawnRelationsInfo, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<Pawn> list = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners.Where((Pawn x) => x.relations.everSeenByPlayer).ToList();
		bool flag = false;
		foreach (Pawn seenPawn in seenPawns)
		{
			if (!seenPawn.RaceProps.IsFlesh || (!informEvenIfSeenBefore && seenPawn.relations.everSeenByPlayer) || (seenPawn.Spawned && seenPawn.Fogged()))
			{
				continue;
			}
			seenPawn.relations.everSeenByPlayer = true;
			bool flag2 = false;
			foreach (Pawn item in list)
			{
				if (seenPawn == item)
				{
					continue;
				}
				PawnRelationDef mostImportantRelation = item.GetMostImportantRelation(seenPawn);
				if (mostImportantRelation == null)
				{
					continue;
				}
				if (!flag2)
				{
					flag2 = true;
					if (flag)
					{
						stringBuilder.AppendLine();
					}
					if (writeSeenPawnsNames)
					{
						stringBuilder.AppendLine(seenPawn.KindLabel.CapitalizeFirst() + " " + seenPawn.Name.ToStringShort.Colorize(ColoredText.NameColor) + ":");
					}
				}
				flag = true;
				stringBuilder.AppendLine("  - " + "Relationship".Translate(mostImportantRelation.GetGenderSpecificLabelCap(seenPawn), item.KindLabel + " " + item.NameShortColored.Resolve(), item));
			}
		}
		if (flag)
		{
			pawnRelationsInfo = stringBuilder.ToString().TrimEndNewlines();
		}
		else
		{
			pawnRelationsInfo = null;
		}
	}

	public static void Notify_PawnsSeenByPlayer_Letter(IEnumerable<Pawn> seenPawns, ref TaggedString letterLabel, ref TaggedString letterText, string relationsInfoHeader, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
	{
		Notify_PawnsSeenByPlayer(seenPawns, out var pawnRelationsInfo, informEvenIfSeenBefore, writeSeenPawnsNames);
		if (!pawnRelationsInfo.NullOrEmpty())
		{
			if (letterLabel.NullOrEmpty())
			{
				letterLabel = "LetterLabelNoticedRelatedPawns".Translate();
			}
			else
			{
				letterLabel += ": " + "RelationshipAppendedLetterSuffix".Translate().CapitalizeFirst();
			}
			if (!letterText.NullOrEmpty())
			{
				letterText += "\n\n";
			}
			letterText += relationsInfoHeader + "\n\n" + pawnRelationsInfo;
		}
	}

	public static void Notify_PawnsSeenByPlayer_Letter_Send(IEnumerable<Pawn> seenPawns, string relationsInfoHeader, LetterDef letterDef, bool informEvenIfSeenBefore = false, bool writeSeenPawnsNames = true)
	{
		TaggedString letterLabel = "";
		TaggedString letterText = "";
		seenPawns = seenPawns.ToList();
		Notify_PawnsSeenByPlayer_Letter(seenPawns, ref letterLabel, ref letterText, relationsInfoHeader, informEvenIfSeenBefore, writeSeenPawnsNames);
		if (letterText.NullOrEmpty())
		{
			return;
		}
		Pawn pawn = null;
		foreach (Pawn seenPawn in seenPawns)
		{
			if (GetMostImportantColonyRelative(seenPawn) != null)
			{
				pawn = seenPawn;
				break;
			}
		}
		if (pawn == null)
		{
			pawn = seenPawns.FirstOrDefault();
		}
		Find.LetterStack.ReceiveLetter(letterLabel, letterText, letterDef, pawn);
	}

	public static bool TryAppendRelationsWithColonistsInfo(ref TaggedString text, Pawn pawn)
	{
		TaggedString title = null;
		return TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
	}

	public static bool TryAppendRelationsWithColonistsInfo(ref TaggedString text, ref TaggedString title, Pawn pawn)
	{
		Pawn mostImportantColonyRelative = GetMostImportantColonyRelative(pawn);
		if (mostImportantColonyRelative == null)
		{
			return false;
		}
		if ((string)title != null)
		{
			title += " (" + "RelationshipAppendedLetterSuffix".Translate() + ")";
		}
		string text2 = mostImportantColonyRelative.GetMostImportantRelation(pawn)?.GetGenderSpecificLabel(pawn);
		if (text2 == null)
		{
			return false;
		}
		if (mostImportantColonyRelative.IsColonist)
		{
			text += "\n\n" + "RelationshipAppendedLetterTextColonist".Translate(mostImportantColonyRelative.LabelShort, text2, mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", addRelationInfoSymbol: false);
		}
		else
		{
			text += "\n\n" + "RelationshipAppendedLetterTextPrisoner".Translate(mostImportantColonyRelative.LabelShort, text2, mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", addRelationInfoSymbol: false);
		}
		return true;
	}

	public static Pawn GetMostImportantColonyRelative(Pawn pawn)
	{
		if (pawn.relations == null || !pawn.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
		{
			return null;
		}
		IEnumerable<Pawn> enumerable = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonistsAndPrisoners.Where((Pawn x) => x.relations.everSeenByPlayer);
		float num = 0f;
		Pawn pawn2 = null;
		foreach (Pawn item in enumerable)
		{
			PawnRelationDef mostImportantRelation = pawn.GetMostImportantRelation(item);
			if (mostImportantRelation != null && (pawn2 == null || mostImportantRelation.importance > num))
			{
				num = mostImportantRelation.importance;
				pawn2 = item;
			}
		}
		return pawn2;
	}

	public static float MaxPossibleBioAgeAt(float myBiologicalAge, float myChronologicalAge, float atChronologicalAge)
	{
		float num = Mathf.Min(myBiologicalAge, myChronologicalAge - atChronologicalAge);
		if (num < 0f)
		{
			return -1f;
		}
		return num;
	}

	public static float MinPossibleBioAgeAt(float myBiologicalAge, float atChronologicalAge)
	{
		return Mathf.Max(myBiologicalAge - atChronologicalAge, 0f);
	}
}
