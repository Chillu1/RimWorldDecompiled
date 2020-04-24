using Verse;

namespace RimWorld
{
	public static class SpouseRelationUtility
	{
		public const float NoNameChangeOnMarriageChance = 0.25f;

		public const float WomansNameChangeOnMarriageChance = 0.05f;

		public const float MansNameOnMarriageChance = 0.7f;

		public const float ChanceForSpousesToHaveTheSameName = 0.75f;

		public static Pawn GetSpouse(this Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
		}

		public static Pawn GetSpouseOppositeGender(this Pawn pawn)
		{
			Pawn spouse = pawn.GetSpouse();
			if (spouse == null)
			{
				return null;
			}
			if ((pawn.gender == Gender.Male && spouse.gender == Gender.Female) || (pawn.gender == Gender.Female && spouse.gender == Gender.Male))
			{
				return spouse;
			}
			return null;
		}

		public static MarriageNameChange Roll_NameChangeOnMarriage()
		{
			float value = Rand.Value;
			if (value < 0.25f)
			{
				return MarriageNameChange.NoChange;
			}
			if (value < 0.3f)
			{
				return MarriageNameChange.WomansName;
			}
			return MarriageNameChange.MansName;
		}

		public static bool Roll_BackToBirthNameAfterDivorce()
		{
			return Rand.Value < 0.6f;
		}

		public static void DetermineManAndWomanSpouses(Pawn firstPawn, Pawn secondPawn, out Pawn man, out Pawn woman)
		{
			if (firstPawn.gender == secondPawn.gender)
			{
				man = firstPawn;
				woman = secondPawn;
			}
			else
			{
				man = ((firstPawn.gender == Gender.Male) ? firstPawn : secondPawn);
				woman = ((firstPawn.gender == Gender.Female) ? firstPawn : secondPawn);
			}
		}

		public static bool ChangeNameAfterMarriage(Pawn firstPawn, Pawn secondPawn, MarriageNameChange changeName)
		{
			if (changeName != 0)
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
				string last = (changeName == MarriageNameChange.MansName) ? nameTriple.Last : nameTriple2.Last;
				man.Name = new NameTriple(nameTriple.First, nameTriple.Nick, last);
				woman.Name = new NameTriple(nameTriple2.First, nameTriple2.Nick, last);
				return true;
			}
			return false;
		}

		public static bool ChangeNameAfterDivorce(Pawn pawn, float chance = -1f)
		{
			NameTriple nameTriple = pawn.Name as NameTriple;
			if (nameTriple != null && pawn.story != null && pawn.story.birthLastName != null && nameTriple.Last != pawn.story.birthLastName && Roll_BackToBirthNameAfterDivorce())
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
			return (PawnBioAndNameGenerator.GeneratePawnName(forPawn) as NameTriple).Last;
		}

		public static void ResolveNameForSpouseOnGeneration(ref PawnGenerationRequest request, Pawn generated)
		{
			if (request.FixedLastName != null)
			{
				return;
			}
			MarriageNameChange marriageNameChange = Roll_NameChangeOnMarriage();
			if (marriageNameChange == MarriageNameChange.NoChange)
			{
				return;
			}
			Pawn spouse = generated.GetSpouse();
			DetermineManAndWomanSpouses(generated, spouse, out Pawn man, out Pawn woman);
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
}
