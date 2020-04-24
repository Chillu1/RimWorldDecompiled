using RimWorld;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Verse
{
	public class Def : Editable
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
		private TaggedString cachedLabelCap = null;

		[Unsaved(false)]
		public bool generated;

		[Unsaved(false)]
		public ushort debugRandomId = (ushort)Rand.RangeInclusive(0, 65535);

		public const string DefaultDefName = "UnnamedDef";

		private static Regex AllowedDefnamesRegex = new Regex("^[a-zA-Z0-9\\-_]*$");

		public TaggedString LabelCap
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
			yield break;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (defName == "UnnamedDef")
			{
				yield return GetType() + " lacks defName. Label=" + label;
			}
			if (defName == "null")
			{
				yield return "defName cannot be the string 'null'.";
			}
			if (!AllowedDefnamesRegex.IsMatch(defName))
			{
				yield return "defName " + defName + " should only contain letters, numbers, underscores, or dashes.";
			}
			if (modExtensions != null)
			{
				int j = 0;
				while (j < modExtensions.Count)
				{
					foreach (string item in modExtensions[j].ConfigErrors())
					{
						yield return item;
					}
					int num = j + 1;
					j = num;
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
			if (descriptionHyperlinks == null || descriptionHyperlinks.Count <= 0)
			{
				yield break;
			}
			if (descriptionHyperlinks.RemoveAll((DefHyperlink x) => x.def == null) != 0)
			{
				Log.Warning("Some descriptionHyperlinks in " + defName + " had null def.");
			}
			int i;
			for (i = descriptionHyperlinks.Count - 1; i > 0; i--)
			{
				if (descriptionHyperlinks.FirstIndexOf((DefHyperlink h) => h.def == descriptionHyperlinks[i].def) < i)
				{
					yield return "Hyperlink to " + descriptionHyperlinks[i].def.defName + " more than once on " + defName + " description";
				}
			}
		}

		public virtual void ClearCachedData()
		{
			cachedLabelCap = null;
		}

		public override string ToString()
		{
			return defName;
		}

		public override int GetHashCode()
		{
			return defName.GetHashCode();
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
	}
}
