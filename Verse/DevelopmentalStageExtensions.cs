using System;
using System.Collections.Generic;

namespace Verse;

public static class DevelopmentalStageExtensions
{
	public struct EnumerationCodebehind
	{
		private uint bit;

		private uint stages;

		public DevelopmentalStage Current => (DevelopmentalStage)bit;

		public EnumerationCodebehind(DevelopmentalStage stages)
		{
			bit = 0u;
			this.stages = (uint)stages;
		}

		public bool MoveNext()
		{
			if (bit == 0)
			{
				bit = 1u;
			}
			else
			{
				bit <<= 1;
			}
			while (bit <= 8)
			{
				if ((bit & stages) != 0)
				{
					return true;
				}
				bit <<= 1;
			}
			return false;
		}

		public EnumerationCodebehind GetEnumerator()
		{
			return this;
		}

		public void Dispose()
		{
		}
	}

	private static List<string> developmentalStageStrings = new List<string>();

	private const DevelopmentalStage lastdevelopmentalStage = DevelopmentalStage.Adult;

	public static CachedTexture BabyTex = new CachedTexture("UI/Icons/DevelopmentalStages/Baby");

	public static CachedTexture ChildTex = new CachedTexture("UI/Icons/DevelopmentalStages/Child");

	public static CachedTexture AdultTex = new CachedTexture("UI/Icons/DevelopmentalStages/Adult");

	public static CachedTexture Icon(this DevelopmentalStage developmentalStage)
	{
		if (!ExactlyOneDevelopmentalStageSet(developmentalStage))
		{
			throw new ArgumentException($"Exactly one developmental stage may be set to get an icon, but was {developmentalStage}.");
		}
		return developmentalStage switch
		{
			DevelopmentalStage.Baby => BabyTex, 
			DevelopmentalStage.Child => ChildTex, 
			DevelopmentalStage.Adult => AdultTex, 
			_ => throw new NotImplementedException(), 
		};
	}

	public static bool ExactlyOneDevelopmentalStageSet(DevelopmentalStage developmentalStage)
	{
		if (developmentalStage != DevelopmentalStage.None)
		{
			return (developmentalStage & (developmentalStage - 1)) == 0;
		}
		return false;
	}

	public static bool Newborn(this DevelopmentalStage developmentalStage)
	{
		return (developmentalStage & DevelopmentalStage.Newborn) != 0;
	}

	public static bool Baby(this DevelopmentalStage developmentalStage)
	{
		return (developmentalStage & DevelopmentalStage.Baby) != 0;
	}

	public static bool Child(this DevelopmentalStage developmentalStage)
	{
		return (developmentalStage & DevelopmentalStage.Child) != 0;
	}

	public static bool Adult(this DevelopmentalStage developmentalStage)
	{
		return (developmentalStage & DevelopmentalStage.Adult) != 0;
	}

	public static bool Juvenile(this DevelopmentalStage developmentalStage)
	{
		if (!developmentalStage.Baby())
		{
			return developmentalStage.Child();
		}
		return true;
	}

	public static bool Has(this DevelopmentalStage developmentalStage, DevelopmentalStage query)
	{
		if (!ExactlyOneDevelopmentalStageSet(query))
		{
			Log.ErrorOnce("A single DevelopmentalStage was expected but multiple were set.", 845116642);
		}
		return developmentalStage.HasAny(query);
	}

	public static bool HasAny(this DevelopmentalStage developmentalStage, DevelopmentalStage query)
	{
		return (developmentalStage & query) != 0;
	}

	public static bool HasAll(this DevelopmentalStage developmentalStage, DevelopmentalStage query)
	{
		return (developmentalStage & query) == query;
	}

	public static string ToCommaListOr(this DevelopmentalStage developmentalStages)
	{
		developmentalStageStrings.Clear();
		EnumerationCodebehind enumerator = developmentalStages.Enumerate().GetEnumerator();
		while (enumerator.MoveNext())
		{
			DevelopmentalStage current = enumerator.Current;
			developmentalStageStrings.Add(current.ToString().Translate());
		}
		return developmentalStageStrings.ToCommaListOr();
	}

	public static string ToCommaList(this DevelopmentalStage developmentalStages, bool capitalize = false)
	{
		developmentalStageStrings.Clear();
		EnumerationCodebehind enumerator = developmentalStages.Enumerate().GetEnumerator();
		while (enumerator.MoveNext())
		{
			TaggedString taggedString = enumerator.Current.ToString().Translate();
			developmentalStageStrings.Add(capitalize ? taggedString.CapitalizeFirst() : taggedString);
		}
		return developmentalStageStrings.ToCommaList();
	}

	public static EnumerationCodebehind Enumerate(this DevelopmentalStage developmentalStages)
	{
		return new EnumerationCodebehind(developmentalStages);
	}
}
