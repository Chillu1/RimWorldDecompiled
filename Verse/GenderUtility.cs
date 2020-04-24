using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class GenderUtility
	{
		private static readonly Texture2D GenderlessIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gender/Genderless");

		private static readonly Texture2D MaleIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gender/Male");

		private static readonly Texture2D FemaleIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gender/Female");

		public static string GetGenderLabel(this Pawn pawn)
		{
			return pawn.gender.GetLabel(pawn.RaceProps.Animal);
		}

		public static string GetLabel(this Gender gender, bool animal = false)
		{
			switch (gender)
			{
			case Gender.None:
				return "NoneLower".Translate();
			case Gender.Male:
				return animal ? "MaleAnimal".Translate() : "Male".Translate();
			case Gender.Female:
				return animal ? "FemaleAnimal".Translate() : "Female".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static string GetPronoun(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return "Proit".Translate();
			case Gender.Male:
				return "Prohe".Translate();
			case Gender.Female:
				return "Proshe".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static string GetPossessive(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return "Proits".Translate();
			case Gender.Male:
				return "Prohis".Translate();
			case Gender.Female:
				return "Proher".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static string GetObjective(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return "ProitObj".Translate();
			case Gender.Male:
				return "ProhimObj".Translate();
			case Gender.Female:
				return "ProherObj".Translate();
			default:
				throw new ArgumentException();
			}
		}

		public static Texture2D GetIcon(this Gender gender)
		{
			switch (gender)
			{
			case Gender.None:
				return GenderlessIcon;
			case Gender.Male:
				return MaleIcon;
			case Gender.Female:
				return FemaleIcon;
			default:
				throw new ArgumentException();
			}
		}

		public static Gender Opposite(this Gender gender)
		{
			switch (gender)
			{
			case Gender.Female:
				return Gender.Male;
			case Gender.Male:
				return Gender.Female;
			default:
				return Gender.None;
			}
		}
	}
}
