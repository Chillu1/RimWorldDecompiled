using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class SpouseRelationUtility
{
	public const float NoNameChangeOnMarriageChance = 0.25f;

	public const float WomansNameChangeOnMarriageChance = 0.05f;

	public const float MansNameOnMarriageChance = 0.7f;

	public const float ChanceForSpousesToHaveTheSameName = 0.75f;

	private static List<Pawn> tmpSpouses = new List<Pawn>();

	private static List<DirectPawnRelation> tmpLoveRelations = new List<DirectPawnRelation>();

	private static List<Pawn> tmpLoveClusterPawns = new List<Pawn>();

	private static Stack<Pawn> tmpStack = new Stack<Pawn>();

	private static readonly List<string> tmpDivorcedPawnNames = new List<string>();

	public static Pawn GetFirstSpouse(this Pawn pawn)
	{
		if (!pawn.RaceProps.IsFlesh)
		{
			return null;
		}
		return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
	}

	public static List<Pawn> GetSpouses(this Pawn pawn, bool includeDead)
	{
		tmpSpouses.Clear();
		if (!pawn.RaceProps.IsFlesh)
		{
			return tmpSpouses;
		}
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (directRelations[i].def == PawnRelationDefOf.Spouse && (includeDead || !directRelations[i].otherPawn.Dead))
			{
				tmpSpouses.Add(directRelations[i].otherPawn);
			}
		}
		return tmpSpouses;
	}

	public static List<DirectPawnRelation> GetLoveRelations(this Pawn pawn, bool includeDead, bool orderByCommitmentLevel = false)
	{
		tmpLoveRelations.Clear();
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (LovePartnerRelationUtility.IsLovePartnerRelation(directRelations[i].def) && (includeDead || !directRelations[i].otherPawn.Dead))
			{
				tmpLoveRelations.Add(directRelations[i]);
			}
		}
		if (orderByCommitmentLevel)
		{
			tmpLoveRelations.SortBy((DirectPawnRelation r) => (r.def != PawnRelationDefOf.Spouse) ? int.MaxValue : (-pawn.relations.OpinionOf(r.otherPawn)), (DirectPawnRelation r) => (r.def != PawnRelationDefOf.Fiance) ? int.MaxValue : (-pawn.relations.OpinionOf(r.otherPawn)), (DirectPawnRelation r) => (r.def != PawnRelationDefOf.Lover) ? int.MaxValue : (-pawn.relations.OpinionOf(r.otherPawn)));
		}
		return tmpLoveRelations;
	}

	public static DirectPawnRelation GetMostLikedSpouseRelation(this Pawn pawn)
	{
		if (!pawn.RaceProps.IsFlesh)
		{
			return null;
		}
		DirectPawnRelation directPawnRelation = null;
		int num = int.MinValue;
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (directRelations[i].def == PawnRelationDefOf.Spouse && !directRelations[i].otherPawn.Dead && (directPawnRelation == null || pawn.relations.OpinionOf(directRelations[i].otherPawn) > num))
			{
				directPawnRelation = directRelations[i];
				num = pawn.relations.OpinionOf(directRelations[i].otherPawn);
			}
		}
		return directPawnRelation;
	}

	public static DirectPawnRelation GetLeastLikedSpouseRelation(this Pawn pawn)
	{
		if (!pawn.RaceProps.IsFlesh)
		{
			return null;
		}
		DirectPawnRelation directPawnRelation = null;
		int num = int.MaxValue;
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			if (directRelations[i].def == PawnRelationDefOf.Spouse && !directRelations[i].otherPawn.Dead && (directPawnRelation == null || pawn.relations.OpinionOf(directRelations[i].otherPawn) < num))
			{
				directPawnRelation = directRelations[i];
				num = pawn.relations.OpinionOf(directRelations[i].otherPawn);
			}
		}
		return directPawnRelation;
	}

	public static int GetSpouseCount(this Pawn pawn, bool includeDead)
	{
		if (!pawn.RaceProps.IsFlesh)
		{
			return 0;
		}
		return pawn.relations.GetDirectRelationsCount(PawnRelationDefOf.Spouse, (Pawn x) => includeDead || !x.Dead);
	}

	public static List<Pawn> GetLoveCluster(this Pawn pawn)
	{
		tmpLoveClusterPawns.Clear();
		tmpLoveClusterPawns.Add(pawn);
		tmpStack.Clear();
		tmpStack.Push(pawn);
		int num = 200;
		while (tmpStack.Count > 0)
		{
			List<DirectPawnRelation> loveRelations = tmpStack.Pop().GetLoveRelations(includeDead: false);
			for (int i = 0; i < loveRelations.Count; i++)
			{
				if (!tmpLoveClusterPawns.Contains(loveRelations[i].otherPawn))
				{
					tmpLoveClusterPawns.Add(loveRelations[i].otherPawn);
					tmpStack.Push(loveRelations[i].otherPawn);
				}
			}
			num--;
			if (num <= 0)
			{
				Log.ErrorOnce("GetLoveCluster exceeded iterations limit.", 1462229);
				break;
			}
		}
		return tmpLoveClusterPawns;
	}

	public static void RemoveSpousesAsForbiddenByIdeo(Pawn pawn)
	{
		tmpDivorcedPawnNames.Clear();
		HistoryEvent ev = new HistoryEvent(pawn.GetHistoryEventForSpouseCount(), pawn.Named(HistoryEventArgsNames.Doer));
		int num = 200;
		while (!ev.DoerWillingToDo())
		{
			DirectPawnRelation leastLikedSpouseRelation = pawn.GetLeastLikedSpouseRelation();
			if (leastLikedSpouseRelation == null)
			{
				break;
			}
			DoDivorce(pawn, leastLikedSpouseRelation.otherPawn);
			tmpDivorcedPawnNames.Add(leastLikedSpouseRelation.otherPawn.NameShortColored.Resolve());
			ev.def = pawn.GetHistoryEventForSpouseCount();
			num--;
			if (num <= 0)
			{
				Log.ErrorOnce("RemoveSpousesAsForbiddenByIdeo exceeded iterations limit.", 18483836);
				break;
			}
		}
		if (tmpDivorcedPawnNames.Any() && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Find.LetterStack.ReceiveLetter("LetterIdeoChangedDivorce".Translate() + ": " + pawn.LabelShortCap, "LetterIdeoChangedDivorcedPawns".Translate(pawn.Named("PAWN"), tmpDivorcedPawnNames.ToLineList("  - ")), LetterDefOf.NeutralEvent, new LookTargets(pawn));
		}
		tmpDivorcedPawnNames.Clear();
	}

	public static void DoDivorce(Pawn initiator, Pawn recipient)
	{
		initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
		initiator.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, recipient);
		RemoveGotMarriedThoughts(initiator, recipient);
		if (initiator.ownership.OwnedBed != null && initiator.ownership.OwnedBed == recipient.ownership.OwnedBed)
		{
			((Rand.Value < 0.5f) ? initiator : recipient).ownership.UnclaimBed();
		}
		ChangeNameAfterDivorce(initiator);
		ChangeNameAfterDivorce(recipient);
		TaleRecorder.RecordTale(TaleDefOf.Breakup, initiator, recipient);
	}

	public static void RemoveGotMarriedThoughts(Pawn initiator, Pawn recipient)
	{
		if (initiator.needs.mood != null)
		{
			initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
			initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, recipient);
		}
		if (recipient.needs.mood != null)
		{
			recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DivorcedMe, initiator);
			recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
			recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, initiator);
		}
	}

	public static HistoryEventDef GetHistoryEventForSpouseCount(this Pawn pawn)
	{
		return GetHistoryEventForSpouseCount(pawn.GetSpouseCount(includeDead: false));
	}

	public static HistoryEventDef GetHistoryEventForSpouseCount(int spouseCount)
	{
		if (spouseCount <= 1)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_OneOrFewer;
		}
		if (spouseCount <= 2)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Two;
		}
		if (spouseCount <= 3)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Three;
		}
		if (spouseCount <= 4)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Four;
		}
		return HistoryEventDefOf.GotMarried_SpouseCount_FiveOrMore;
	}

	public static HistoryEventDef GetHistoryEventForSpouseCountPlusOne(this Pawn pawn)
	{
		return GetHistoryEventForSpouseCount(pawn.GetSpouseCount(includeDead: false) + 1);
	}

	public static HistoryEventDef GetHistoryEventForSpouseAndFianceCountPlusOne(this Pawn pawn)
	{
		List<DirectPawnRelation> list = LovePartnerRelationUtility.ExistingLovePartners(pawn, allowDead: false);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def != PawnRelationDefOf.Lover)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_OneOrFewer;
		}
		if (num < 2)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Two;
		}
		if (num < 3)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Three;
		}
		if (num < 4)
		{
			return HistoryEventDefOf.GotMarried_SpouseCount_Four;
		}
		return HistoryEventDefOf.GotMarried_SpouseCount_FiveOrMore;
	}

	public static Pawn GetFirstSpouseOfOppositeGender(this Pawn pawn)
	{
		foreach (Pawn spouse in pawn.GetSpouses(includeDead: true))
		{
			if (pawn.gender.Opposite() == spouse.gender)
			{
				return spouse;
			}
		}
		return null;
	}

	public static MarriageNameChange Roll_NameChangeOnMarriage(Pawn pawn)
	{
		List<MarriageNameChange> list = new List<MarriageNameChange>();
		if (new HistoryEvent(HistoryEventDefOf.GotMarried_TookMansName, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			list.Add(MarriageNameChange.MansName);
		}
		if (new HistoryEvent(HistoryEventDefOf.GotMarried_TookWomansName, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			list.Add(MarriageNameChange.WomansName);
		}
		if (new HistoryEvent(HistoryEventDefOf.GotMarried_KeptName, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			list.Add(MarriageNameChange.NoChange);
		}
		if (!list.Any())
		{
			return MarriageNameChange.NoChange;
		}
		return list.RandomElement();
	}

	public static bool Roll_BackToBirthNameAfterDivorce()
	{
		return Rand.Value < 0.6f;
	}

	public static void DetermineManAndWomanSpouses(Pawn firstPawn, Pawn secondPawn, out Pawn man, out Pawn woman)
	{
		if (firstPawn.gender == secondPawn.gender)
		{
			man = ((firstPawn.thingIDNumber < secondPawn.thingIDNumber) ? firstPawn : secondPawn);
			woman = ((firstPawn.thingIDNumber < secondPawn.thingIDNumber) ? secondPawn : firstPawn);
		}
		else
		{
			man = ((firstPawn.gender == Gender.Male) ? firstPawn : secondPawn);
			woman = ((firstPawn.gender == Gender.Female) ? firstPawn : secondPawn);
		}
	}

	public static bool ChangeNameAfterMarriage(Pawn firstPawn, Pawn secondPawn, MarriageNameChange changeName)
	{
		if (changeName != MarriageNameChange.NoChange)
		{
			Pawn man = null;
			Pawn woman = null;
			DetermineManAndWomanSpouses(firstPawn, secondPawn, out man, out woman);
			NameTriple nameTriple = man.Name as NameTriple;
			NameTriple nameTriple2 = woman.Name as NameTriple;
			if (nameTriple == null || nameTriple2 == null)
			{
				return false;
			}
			string last = ((changeName == MarriageNameChange.MansName) ? nameTriple.Last : nameTriple2.Last);
			man.Name = new NameTriple(nameTriple.First, nameTriple.Nick, last);
			woman.Name = new NameTriple(nameTriple2.First, nameTriple2.Nick, last);
			return true;
		}
		return false;
	}

	public static bool ChangeNameAfterDivorce(Pawn pawn, float chance = -1f)
	{
		if (pawn.Name is NameTriple nameTriple && pawn.story != null && pawn.story.birthLastName != null && nameTriple.Last != pawn.story.birthLastName && Roll_BackToBirthNameAfterDivorce())
		{
			pawn.Name = new NameTriple(nameTriple.First, nameTriple.Nick, pawn.story.birthLastName);
			return true;
		}
		return false;
	}

	public static void Notify_PawnRegenerated(Pawn regenerated)
	{
		if (regenerated.relations != null)
		{
			Pawn firstDirectRelationPawn = regenerated.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
			if (firstDirectRelationPawn != null && regenerated.Name is NameTriple && firstDirectRelationPawn.Name is NameTriple)
			{
				NameTriple nameTriple = firstDirectRelationPawn.Name as NameTriple;
				firstDirectRelationPawn.Name = new NameTriple(nameTriple.First, nameTriple.Nick, nameTriple.Last);
			}
		}
	}

	public static string GetRandomBirthName(Pawn forPawn)
	{
		return (PawnBioAndNameGenerator.GeneratePawnName(forPawn, NameStyle.Full, null, forceNoNick: false, forPawn.genes?.Xenotype) as NameTriple).Last;
	}

	public static void ResolveNameForSpouseOnGeneration(ref PawnGenerationRequest request, Pawn generated)
	{
		if (request.FixedLastName != null)
		{
			return;
		}
		MarriageNameChange marriageNameChange = Roll_NameChangeOnMarriage(generated);
		if (marriageNameChange == MarriageNameChange.NoChange)
		{
			return;
		}
		Pawn firstSpouse = generated.GetFirstSpouse();
		DetermineManAndWomanSpouses(generated, firstSpouse, out var man, out var woman);
		NameTriple nameTriple = man.Name as NameTriple;
		NameTriple nameTriple2 = woman.Name as NameTriple;
		if (generated == woman && marriageNameChange == MarriageNameChange.WomansName)
		{
			man.Name = new NameTriple(nameTriple.First, nameTriple.Nick, nameTriple.Last);
			if (man.story != null)
			{
				man.story.birthLastName = GetRandomBirthName(man);
			}
			request.SetFixedLastName(nameTriple.Last);
		}
		else if (generated == man && marriageNameChange == MarriageNameChange.WomansName)
		{
			request.SetFixedLastName(nameTriple2.Last);
			request.SetFixedBirthName(GetRandomBirthName(man));
		}
		else if (generated == woman && marriageNameChange == MarriageNameChange.MansName)
		{
			request.SetFixedLastName(nameTriple.Last);
			request.SetFixedBirthName(GetRandomBirthName(woman));
		}
		else if (generated == man && marriageNameChange == MarriageNameChange.MansName)
		{
			woman.Name = new NameTriple(nameTriple2.First, nameTriple2.Nick, nameTriple2.Last);
			if (woman.story != null)
			{
				woman.story.birthLastName = GetRandomBirthName(man);
			}
			request.SetFixedLastName(nameTriple2.Last);
		}
	}
}
