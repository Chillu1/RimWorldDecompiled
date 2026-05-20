using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class ParentRelationUtility
{
	private static List<string> workingParentagePieces = new List<string>();

	public static TaggedString GetParentage(this Pawn pawn)
	{
		workingParentagePieces.Clear();
		Pawn mother = pawn.GetMother();
		if (mother != null)
		{
			workingParentagePieces.Add(FactionDesc(mother));
		}
		Pawn father = pawn.GetFather();
		if (father != null)
		{
			workingParentagePieces.Add(FactionDesc(father));
		}
		string text = workingParentagePieces.ToCommaList(useAnd: true, emptyIfNone: true);
		if (string.IsNullOrEmpty(text))
		{
			text = "ParentsUnknown".Translate();
		}
		return "BornOfParents".Translate(text);
		static string FactionDesc(Pawn parent)
		{
			return parent.FactionDesc(parent.NameFullColored, extraFactionsInfo: false, parent.NameFullColored, parent.gender.GetLabel(parent.RaceProps.Animal)).Resolve();
		}
	}

	public static Pawn GetFather(this Pawn pawn)
	{
		return pawn.GetParent(Gender.Male);
	}

	public static Pawn GetMother(this Pawn pawn)
	{
		return pawn.GetParent(Gender.Female);
	}

	public static bool HasSameFather(this Pawn pawn, Pawn other)
	{
		return HasSameParent(pawn, other, Gender.Male);
	}

	public static bool HasSameMother(this Pawn pawn, Pawn other)
	{
		return HasSameParent(pawn, other, Gender.Female);
	}

	private static Pawn GetParent(this Pawn pawn, Gender parentGender)
	{
		if (!pawn.RaceProps.IsFlesh)
		{
			return null;
		}
		if (pawn.relations == null)
		{
			return null;
		}
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			DirectPawnRelation directPawnRelation = directRelations[i];
			if (directPawnRelation.def == PawnRelationDefOf.Parent && directPawnRelation.otherPawn.gender == parentGender)
			{
				return directPawnRelation.otherPawn;
			}
		}
		return null;
	}

	private static bool HasSameParent(Pawn pawn, Pawn other, Gender parentGender)
	{
		if (!pawn.RaceProps.IsFlesh)
		{
			return false;
		}
		if (pawn.relations == null)
		{
			return false;
		}
		Pawn parent = pawn.GetParent(parentGender);
		Pawn parent2 = other.GetParent(parentGender);
		if (parent != null && parent2 != null && parent == parent2)
		{
			return true;
		}
		VirtualPawnRelation virtualParent = GetVirtualParent(pawn, parentGender);
		VirtualPawnRelation virtualParent2 = GetVirtualParent(other, parentGender);
		if (virtualParent != null && virtualParent2 != null && virtualParent.record.ID == virtualParent2.record.ID)
		{
			return true;
		}
		return false;
	}

	private static VirtualPawnRelation GetVirtualParent(Pawn pawn, Gender gender)
	{
		if (!pawn.RaceProps.IsFlesh)
		{
			return null;
		}
		if (pawn.relations == null)
		{
			return null;
		}
		List<VirtualPawnRelation> virtualRelations = pawn.relations.VirtualRelations;
		for (int i = 0; i < virtualRelations.Count; i++)
		{
			VirtualPawnRelation virtualPawnRelation = virtualRelations[i];
			if (virtualPawnRelation.def == PawnRelationDefOf.Parent && virtualPawnRelation.record.Gender == gender)
			{
				return virtualPawnRelation;
			}
		}
		return null;
	}

	public static Pawn GetBirthParent(this Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return null;
		}
		if (!pawn.RaceProps.IsFlesh)
		{
			return null;
		}
		if (pawn.relations == null)
		{
			return null;
		}
		List<DirectPawnRelation> directRelations = pawn.relations.DirectRelations;
		for (int i = 0; i < directRelations.Count; i++)
		{
			DirectPawnRelation directPawnRelation = directRelations[i];
			if (directPawnRelation.def == PawnRelationDefOf.ParentBirth)
			{
				return directPawnRelation.otherPawn;
			}
		}
		return null;
	}

	public static void SetFather(this Pawn pawn, Pawn newFather)
	{
		if (newFather != null && newFather.gender == Gender.Female)
		{
			Log.Warning("Tried to set " + newFather?.ToString() + " with gender " + newFather.gender.ToString() + " as " + pawn?.ToString() + "'s father.");
			return;
		}
		Pawn father = pawn.GetFather();
		if (father != newFather)
		{
			if (father != null)
			{
				pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, father);
			}
			if (newFather != null)
			{
				pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newFather);
			}
		}
	}

	public static void SetMother(this Pawn pawn, Pawn newMother)
	{
		if (newMother != null && newMother.gender != Gender.Female)
		{
			Log.Warning("Tried to set " + newMother?.ToString() + " with gender " + newMother.gender.ToString() + " as " + pawn?.ToString() + "'s mother.");
			return;
		}
		Pawn mother = pawn.GetMother();
		if (mother != newMother)
		{
			if (mother != null)
			{
				pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, mother);
			}
			if (newMother != null)
			{
				pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newMother);
			}
		}
	}
}
