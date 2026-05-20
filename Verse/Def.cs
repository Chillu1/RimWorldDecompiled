using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RimWorld;

namespace Verse;

public class Def : Editable, IEquatable<Def>
{
	[Description("The name of this Def. It is used as an identifier by the game code.")]
	[NoTranslate]
	public string defName = "UnnamedDef";

	[Description("A human-readable label used to identify this in game.")]
	[DefaultValue(null)]
	[MustTranslate]
	public string label;

	[Description("A human-readable description given when the Def is inspected by players.")]
	[DefaultValue(null)]
	[MustTranslate]
	public string description;

	[XmlInheritanceAllowDuplicateNodes]
	public List<DefHyperlink> descriptionHyperlinks;

	[Description("Disables config error checking. Intended for mod use. (Be careful!)")]
	[DefaultValue(false)]
	[MustTranslate]
	public bool ignoreConfigErrors;

	public bool ignoreIllegalLabelCharacterConfigError;

	[Description("Mod-specific data. Not used by core game code.")]
	[DefaultValue(null)]
	public List<DefModExtension> modExtensions;

	[Unsaved(false)]
	public ushort shortHash;

	[Unsaved(false)]
	public ushort index = ushort.MaxValue;

	[Unsaved(false)]
	public ModContentPack modContentPack;

	[Unsaved(false)]
	public string fileName;

	[Unsaved(false)]
	public int defNameHash;

	[Unsaved(false)]
	protected TaggedString cachedLabelCap = null;

	[Unsaved(false)]
	public bool generated;

	[Unsaved(false)]
	public ushort debugRandomId = (ushort)Rand.RangeInclusive(0, 65535);

	public const string DefaultDefName = "UnnamedDef";

	private static readonly Regex AllowedDefNamesRegex = new Regex("^[a-zA-Z0-9\\-_]*$");

	private static readonly Regex DisallowedLabelCharsRegex = new Regex("\\[|\\]|\\{|\\}");

	public virtual TaggedString LabelCap
	{
		get
		{
			if (label.NullOrEmpty())
			{
				return null;
			}
			if (cachedLabelCap.NullOrEmpty())
			{
				cachedLabelCap = label.CapitalizeFirst();
			}
			return cachedLabelCap;
		}
	}

	public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		if (modContentPack != null && !modContentPack.IsCoreMod)
		{
			TaggedString taggedString = (modContentPack.IsOfficialMod ? "Stat_Source_OfficialExpansionReport".Translate() : "Stat_Source_ModReport".Translate());
			yield return new StatDrawEntry(StatCategoryDefOf.Source, "Stat_Source_Label".Translate(), modContentPack.Name, taggedString + ": " + modContentPack.Name, 90000, null, null, forceUnfinalizedMode: false, overridesHideStats: true);
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (modExtensions != null)
		{
			for (int i = 0; i < modExtensions.Count; i++)
			{
				modExtensions[i].ResolveReferences(this);
			}
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (defName == "UnnamedDef")
		{
			yield return GetType()?.ToString() + " lacks defName. Label=" + label;
		}
		if (defName == "null")
		{
			yield return "defName cannot be the string 'null'.";
		}
		if (!AllowedDefNamesRegex.IsMatch(defName))
		{
			yield return "defName " + defName + " should only contain letters, numbers, underscores, or dashes.";
		}
		if (modExtensions != null)
		{
			int i = 0;
			while (i < modExtensions.Count)
			{
				foreach (string item in modExtensions[i].ConfigErrors())
				{
					yield return item;
				}
				int num = i + 1;
				i = num;
			}
		}
		if (description != null)
		{
			if (description == "")
			{
				yield return "empty description";
			}
			if (char.IsWhiteSpace(description[0]))
			{
				yield return "description has leading whitespace";
			}
			if (char.IsWhiteSpace(description[description.Length - 1]))
			{
				yield return "description has trailing whitespace";
			}
		}
		if (descriptionHyperlinks != null && descriptionHyperlinks.Count > 0)
		{
			if (descriptionHyperlinks.RemoveAll((DefHyperlink x) => x.def == null) != 0)
			{
				Log.Warning("Some descriptionHyperlinks in " + defName + " had null def.");
			}
			int i2;
			for (i2 = descriptionHyperlinks.Count - 1; i2 > 0; i2--)
			{
				if (descriptionHyperlinks.FirstIndexOf((DefHyperlink h) => h.def == descriptionHyperlinks[i2].def) < i2)
				{
					yield return "Hyperlink to " + descriptionHyperlinks[i2].def.defName + " more than once on " + defName + " description";
				}
			}
		}
		if (label != null && !ignoreIllegalLabelCharacterConfigError && DisallowedLabelCharsRegex.IsMatch(label))
		{
			yield return "label contains illegal character(s): \"[]{}\". This can cause issues during grammar resolution. If this was intended, you can use the \"ignoreIllegalLabelCharacterConfigError\" flag.";
		}
	}

	public virtual void PostSetIndices()
	{
	}

	public virtual void ClearCachedData()
	{
		cachedLabelCap = null;
	}

	public override string ToString()
	{
		return defName;
	}

	public void ResolveDefNameHash()
	{
		defNameHash = defName.GetHashCode();
	}

	public override int GetHashCode()
	{
		return defNameHash;
	}

	public T GetModExtension<T>() where T : DefModExtension
	{
		if (modExtensions == null)
		{
			return null;
		}
		for (int i = 0; i < modExtensions.Count; i++)
		{
			if (modExtensions[i] is T)
			{
				return modExtensions[i] as T;
			}
		}
		return null;
	}

	public bool HasModExtension<T>() where T : DefModExtension
	{
		return GetModExtension<T>() != null;
	}

	public bool Equals(Def other)
	{
		if (other != null && other.defNameHash == defNameHash)
		{
			return other.GetType() == GetType();
		}
		return false;
	}
}
