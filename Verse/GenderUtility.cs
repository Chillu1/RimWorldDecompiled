using System;
using UnityEngine;

namespace Verse;

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
		return gender switch
		{
			Gender.None => "NoneLower".Translate(), 
			Gender.Male => animal ? "MaleAnimal".Translate() : "Male".Translate(), 
			Gender.Female => animal ? "FemaleAnimal".Translate() : "Female".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}

	public static string GetPronoun(this Gender gender)
	{
		return gender switch
		{
			Gender.None => "Proit".Translate(), 
			Gender.Male => "Prohe".Translate(), 
			Gender.Female => "Proshe".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}

	public static string GetPossessive(this Gender gender)
	{
		return gender switch
		{
			Gender.None => "Proits".Translate(), 
			Gender.Male => "Prohis".Translate(), 
			Gender.Female => "Proher".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}

	public static string GetObjective(this Gender gender)
	{
		return gender switch
		{
			Gender.None => "ProitObj".Translate(), 
			Gender.Male => "ProhimObj".Translate(), 
			Gender.Female => "ProherObj".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}

	public static string GetGenderNoun(this Gender gender)
	{
		return gender switch
		{
			Gender.None => "ProItNoun".Translate(), 
			Gender.Male => "ProHeNoun".Translate(), 
			Gender.Female => "ProSheNoun".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}

	public static Texture2D GetIcon(this Gender gender)
	{
		return gender switch
		{
			Gender.None => GenderlessIcon, 
			Gender.Male => MaleIcon, 
			Gender.Female => FemaleIcon, 
			_ => throw new ArgumentException(), 
		};
	}

	public static Gender Opposite(this Gender gender)
	{
		return gender switch
		{
			Gender.Female => Gender.Male, 
			Gender.Male => Gender.Female, 
			_ => Gender.None, 
		};
	}
}
