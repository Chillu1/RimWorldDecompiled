using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;
using Verse.Sound;

namespace RimWorld;

public class IdeoFoundation_Deity : IdeoFoundation
{
	public class Deity : IExposable
	{
		public string name;

		public string type;

		public Gender gender;

		public string iconPath;

		public MemeDef relatedMeme;

		private Texture2D icon;

		public Texture2D Icon
		{
			get
			{
				if (icon == null)
				{
					icon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return icon;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref name, "name");
			Scribe_Values.Look(ref type, "type");
			Scribe_Values.Look(ref gender, "gender", Gender.None);
			Scribe_Values.Look(ref iconPath, "iconPath");
			Scribe_Defs.Look(ref relatedMeme, "relatedMeme");
		}

		public virtual void CopyTo(Deity other)
		{
			other.name = name;
			other.type = type;
			other.gender = gender;
			other.iconPath = iconPath;
			other.relatedMeme = relatedMeme;
		}
	}

	private enum SymbolSource
	{
		Pack,
		Deity
	}

	private List<Deity> deities = new List<Deity>();

	private static readonly Vector2 DeityBoxSize = IdeoUIUtility.PreceptBoxSize;

	private const float IconSize = 50f;

	private const float GenderIconSize = 20f;

	public List<Deity> DeitiesListForReading => deities;

	public override void Init(IdeoGenerationParms parms)
	{
		if (ModLister.CheckIdeology("Ideoligion"))
		{
			ideo.classicExtraMode = parms.classicExtra;
			RandomizeCulture(parms);
			RandomizePlace();
			RandomizeMemes(parms);
			GenerateDeities();
			GenerateTextSymbols();
			GenerateLeaderTitle();
			RandomizeIcon();
			RandomizePrecepts(init: true, parms);
			ideo.RegenerateDescription(force: true);
			RandomizeStyles();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref deities, "deities", LookMode.Deep);
	}

	public override void DoInfo(ref float curY, float width, IdeoEditMode editMode)
	{
		if (deities.Count == 0 && ideo.DeityCountRange.max <= 0)
		{
			return;
		}
		curY += 4f;
		Widgets.Label((width - IdeoUIUtility.PreceptBoxSize.x * 3f - 16f) / 2f, ref curY, width, "Deities".Translate());
		if (editMode != IdeoEditMode.None)
		{
			float num = width - (width - IdeoUIUtility.PreceptBoxSize.x * 3f - 16f) / 2f;
			Rect rect = new Rect(num - IdeoUIUtility.ButtonSize.x, curY - IdeoUIUtility.ButtonSize.y, IdeoUIUtility.ButtonSize.x, IdeoUIUtility.ButtonSize.y);
			Rect rect2 = rect;
			rect2.x = rect.xMin - rect.width - 10f;
			bool num2 = deities.Count < ideo.DeityCountRange.max;
			if (Widgets.ButtonText(num2 ? rect2 : rect, "RandomizeDeities".Translate()) && IdeoUIUtility.TutorAllowsInteraction(editMode))
			{
				GenerateDeities();
				ideo.RegenerateAllPreceptNames();
				ideo.RegenerateDescription();
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			if (num2 && Widgets.ButtonText(rect, "AddDeity".Translate()) && IdeoUIUtility.TutorAllowsInteraction(editMode))
			{
				deities.Add(GenerateNewDeity());
				ideo.RegenerateAllPreceptNames();
				ideo.RegenerateDescription();
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
		}
		curY += 4f;
		for (int i = 0; i < deities.Count; i++)
		{
			Deity curDeity = deities[i];
			int num3 = i / 3;
			int num4 = i % 3;
			int num5 = ((i >= deities.Count - deities.Count % 3) ? (deities.Count % 3) : 3);
			float num6 = (width - (float)num5 * DeityBoxSize.x - (float)((num5 - 1) * 8)) / 2f;
			Rect rect3 = new Rect(num6 + (float)num4 * DeityBoxSize.x + (float)(num4 * 8), curY + (float)num3 * DeityBoxSize.y + (float)(num3 * 8), DeityBoxSize.x, DeityBoxSize.y);
			Widgets.DrawLightHighlight(rect3);
			if (Mouse.IsOver(rect3))
			{
				Widgets.DrawHighlight(rect3);
				string text = curDeity.name.Colorize(ColoredText.TipSectionTitleColor) + "\n" + curDeity.type;
				if (curDeity.relatedMeme != null)
				{
					text = text + "\n\n" + "RelatedToMeme".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + curDeity.relatedMeme.LabelCap.Resolve();
				}
				if (editMode != IdeoEditMode.None)
				{
					text = text + "\n\n" + IdeoUIUtility.ClickToEdit;
				}
				TooltipHandler.TipRegion(rect3, text);
			}
			if (Widgets.ButtonInvisible(rect3) && editMode != IdeoEditMode.None && IdeoUIUtility.TutorAllowsInteraction(editMode))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				Action action = null;
				TaggedString taggedString = "Remove".Translate().CapitalizeFirst();
				int min = ideo.DeityCountRange.min;
				if (deities.Count > min)
				{
					action = delegate
					{
						deities.Remove(curDeity);
						ideo.RegenerateDescription();
					};
				}
				else
				{
					string arg = ((min <= 1) ? ((string)"Deity".Translate()) : Find.ActiveLanguageWorker.Pluralize("Deity".Translate(), min));
					taggedString += " (" + "DeitiesRequired".Translate(min, arg.Named("DEITYNOUN")) + ")";
				}
				list.Add(new FloatMenuOption(taggedString, action));
				list.Add(new FloatMenuOption("Regenerate".Translate().CapitalizeFirst(), delegate
				{
					FillDeity(curDeity);
					ideo.RegenerateDescription();
				}));
				list.Add(new FloatMenuOption("EditDeity".Translate().CapitalizeFirst(), delegate
				{
					Find.WindowStack.Add(new Dialog_EditDeity(curDeity, ideo));
				}));
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Rect position = new Rect(rect3.x + (rect3.height - 50f) / 2f, rect3.y + (rect3.height - 50f) / 2f, 50f, 50f);
			GUI.DrawTexture(position, deities[i].Icon);
			Rect rect4 = new Rect(position.xMax + 10f, rect3.y + 3f, rect3.xMax - position.xMax - 10f, rect3.height / 2f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect4, deities[i].name);
			GenUI.ResetLabelAlign();
			Rect rect5 = new Rect(position.xMax + 10f, rect3.y + rect3.height / 2f - 3f, rect3.xMax - position.xMax - 10f, rect3.height / 2f);
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(rect5, deities[i].type);
			GUI.color = Color.white;
			GenUI.ResetLabelAlign();
			GUI.DrawTexture(new Rect(rect3.xMax - 20f - 4f, rect3.y + 4f, 20f, 20f), deities[i].gender.GetIcon());
		}
		int num7 = Mathf.CeilToInt((float)deities.Count / 3f);
		curY += (float)num7 * DeityBoxSize.y + (float)((num7 - 1) * 8);
	}

	public override void GenerateTextSymbols()
	{
		if (ideo.culture == null)
		{
			return;
		}
		ideo.usedSymbols.Clear();
		ideo.usedSymbolPacks.Clear();
		List<MemeDef> usedMemes = new List<MemeDef>();
		GrammarRequest request = default(GrammarRequest);
		request.Includes.Add(ideo.culture.ideoNameMaker);
		AddPlaceRules(ref request);
		AddDeityRules(ref request);
		List<SymbolSource> list = new List<SymbolSource>();
		if (ideo.memes.Any((MemeDef m) => !m.symbolPacks.NullOrEmpty()))
		{
			list.Add(SymbolSource.Pack);
		}
		if (deities.Count >= 1 && !ideo.memes.Any((MemeDef m) => !m.allowSymbolsFromDeity))
		{
			list.Add(SymbolSource.Deity);
		}
		if (list.Count == 0)
		{
			Log.Error("No way to generate ideo symbols. Memes: " + ideo.memes.Select((MemeDef m) => m.defName).ToCommaList());
			ideo.name = "Errorism";
			ideo.adjective = "Errorist";
			ideo.memberName = "Errorist";
			return;
		}
		switch (list.RandomElementByWeight((SymbolSource s) => s switch
		{
			SymbolSource.Pack => 1f, 
			SymbolSource.Deity => 0.5f, 
			_ => throw new NotImplementedException(), 
		}))
		{
		case SymbolSource.Pack:
			SetupFromSymbolPack();
			break;
		case SymbolSource.Deity:
			SetupFromDeity();
			break;
		}
		ideo.name = GetResolvedText("r_ideoName", request);
		ideo.name = GenText.CapitalizeAsTitle(ideo.name);
		ideo.adjective = GetResolvedText("r_ideoAdjective", request, capitalizeFirstSentence: false);
		ideo.memberName = GetResolvedText("r_memberName", request);
		void AddMemeContent()
		{
			foreach (MemeDef item in ideo.memes.Where((MemeDef m) => usedMemes.Count == ideo.memes.Count || !usedMemes.Contains(m)))
			{
				if (item.generalRules != null)
				{
					request.IncludesBare.Add(item.generalRules);
				}
			}
		}
		void AddSymbolPack(IdeoSymbolPack pack, MemeCategory memeCategory)
		{
			ideo.usedSymbolPacks.Add(pack.PrimarySymbol);
			request.Constants.SetOrAdd("forcePrefix", pack.prefix.ToString());
			string text = (pack.prefix ? (GrammarResolver.Resolve("hyphenPrefix", request) + "-") : string.Empty);
			if (pack.ideoName != null)
			{
				if (memeCategory == MemeCategory.Structure)
				{
					request.Rules.Add(new Rule_String("packIdeoNameStructure", text + pack.ideoName));
				}
				else
				{
					request.Rules.Add(new Rule_String("packIdeoName", text + pack.ideoName));
				}
			}
			if (pack.theme != null)
			{
				request.Rules.Add(new Rule_String("packTheme", pack.theme));
			}
			if (pack.adjective != null)
			{
				request.Rules.Add(new Rule_String("packAdjective", text + pack.adjective));
			}
			if (pack.member != null)
			{
				request.Rules.Add(new Rule_String("packMember", text + pack.member));
			}
		}
		void SetupFromDeity()
		{
			request.Rules.Add(new Rule_String("keyDeity", ideo.KeyDeityName));
			AddMemeContent();
		}
		void SetupFromSymbolPack()
		{
			MemeDef result;
			if (ideo.StructureMeme.symbolPackOverride)
			{
				result = ideo.StructureMeme;
			}
			else if (!ideo.memes.Where((MemeDef m) => m.symbolPacks.HasData() && m.symbolPacks.Any((IdeoSymbolPack x) => CanUseSymbolPack(x))).TryRandomElement(out result))
			{
				result = ideo.memes.Where((MemeDef m) => m.symbolPacks.HasData()).RandomElement();
			}
			usedMemes.Add(result);
			AddMemeContent();
			if (result.symbolPacks.Where((IdeoSymbolPack x) => CanUseSymbolPack(x)).TryRandomElement(out var result2))
			{
				AddSymbolPack(result2, result.category);
			}
			else
			{
				AddSymbolPack(result.symbolPacks.RandomElement(), result.category);
			}
		}
	}

	public void AddDeityRules(ref GrammarRequest request)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		AddDeityRules(dictionary, ref request);
		request.Rules.AddRange(dictionary.Select((KeyValuePair<string, string> kv) => new Rule_String(kv.Key, kv.Value)));
	}

	public void AddDeityRules(Dictionary<string, string> tokens, ref GrammarRequest request)
	{
		for (int i = 0; i < deities.Count; i++)
		{
			Deity deity = deities[i];
			string text = $"deity{i}_";
			tokens.AddDistinct(text + "name", deity.name.ApplyTag(TagType.Name).Resolve());
			tokens.AddDistinct(text + "pronoun", deity.gender.GetPronoun());
			tokens.AddDistinct(text + "objective", deity.gender.GetObjective());
			tokens.AddDistinct(text + "possessive", deity.gender.GetPossessive());
			tokens.AddDistinct(text + "type", deity.type);
			request.Constants.SetOrAdd(text + "gender", deity.gender.ToString());
		}
	}

	public void SetDeities(List<Deity> deities)
	{
		this.deities = deities;
	}

	private bool CanUseSymbolPack(IdeoSymbolPack pack)
	{
		if (Find.World == null || Find.IdeoManager == null)
		{
			return true;
		}
		foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
		{
			if (item.usedSymbolPacks.Contains(pack.PrimarySymbol))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanUseSymbol(string symbol)
	{
		if (Find.World == null || Find.IdeoManager == null)
		{
			return true;
		}
		foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
		{
			if (item.usedSymbols.Contains(symbol))
			{
				return false;
			}
		}
		return true;
	}

	private string GetResolvedText(string key, GrammarRequest request, bool capitalizeFirstSentence = true)
	{
		string text = GrammarResolver.Resolve(key, request, null, forceLog: false, null, null, null, capitalizeFirstSentence);
		for (int i = 0; i < 10; i++)
		{
			if (CanUseSymbol(text))
			{
				ideo.usedSymbols.Add(text);
				return text;
			}
			text = GrammarResolver.Resolve(key, request, null, forceLog: false, null, null, null, capitalizeFirstSentence);
		}
		ideo.usedSymbols.Add(text);
		return text;
	}

	public void GenerateDeities()
	{
		deities.Clear();
		int value = ((!Rand.Chance(0.5f)) ? (Rand.Chance(0.5f) ? 1 : Rand.RangeInclusive(2, 4)) : 0);
		IntRange deityCountRange = ideo.DeityCountRange;
		value = Mathf.Clamp(value, deityCountRange.min, deityCountRange.max);
		for (int i = 0; i < value; i++)
		{
			Deity item = GenerateNewDeity();
			deities.Add(item);
		}
	}

	private void FillDeity(Deity deity)
	{
		Gender supremeGender = ideo.SupremeGender;
		if (supremeGender != Gender.None)
		{
			deity.gender = supremeGender;
		}
		else
		{
			deity.gender = Gen.RandomEnumValue<Gender>(disallowFirstValue: true);
		}
		MemeDef result2;
		if (ideo.memes.Where((MemeDef x) => !deities.Any((Deity y) => y.relatedMeme == x)).TryRandomElement(out var result))
		{
			deity.relatedMeme = result;
		}
		else if (ideo.memes.TryRandomElement(out result2))
		{
			deity.relatedMeme = result2;
		}
		if (ideo.StructureMeme.fixedDeityNameTypes != null)
		{
			int num = 0;
			DeityNameType deityNameType;
			while (true)
			{
				deityNameType = ideo.StructureMeme.fixedDeityNameTypes.RandomElement();
				if (!AllExistingDeities().Contains(deityNameType.name))
				{
					break;
				}
				num++;
				if (num > 20)
				{
					Log.Error("Could not get a unique fixed deity name and type after a reasonable number of tries.");
					break;
				}
			}
			deity.name = deityNameType.name;
			deity.type = deityNameType.type;
		}
		else
		{
			GrammarRequest request = default(GrammarRequest);
			RulePackDef item = ideo.StructureMeme.deityNameMakerOverride ?? ideo.culture.deityNameMaker;
			request.Includes.Add(item);
			deity.name = NameGenerator.GenerateName(request, (string x) => !AllExistingDeities().Contains(x), appendNumberIfNameUsed: false, "r_deityName");
			GrammarRequest request2 = default(GrammarRequest);
			RulePackDef item2 = ideo.StructureMeme.deityTypeMakerOverride ?? ideo.culture.deityTypeMaker;
			request2.Includes.Add(item2);
			if (deity.relatedMeme != null && deity.relatedMeme.generalRules != null)
			{
				request2.IncludesBare.Add(deity.relatedMeme.generalRules);
			}
			request2.Constants.SetOrAdd("gender", deity.gender.ToString());
			deity.type = NameGenerator.GenerateName(request2, null, appendNumberIfNameUsed: false, "r_deityType");
		}
		deity.iconPath = "UI/Deities/DeityGeneric";
		IEnumerable<string> AllExistingDeities()
		{
			for (int i = 0; i < deities.Count; i++)
			{
				yield return deities[i].name;
			}
			if (Find.World != null)
			{
				List<Ideo> ideos = Find.IdeoManager.IdeosListForReading;
				for (int i = 0; i < ideos.Count; i++)
				{
					IdeoFoundation foundation = ideos[i].foundation;
					if (foundation is IdeoFoundation_Deity deityFoundation)
					{
						for (int j = 0; j < deityFoundation.deities.Count; j++)
						{
							yield return deityFoundation.deities[j].name;
						}
					}
				}
			}
		}
	}

	private Deity GenerateNewDeity()
	{
		Deity deity = new Deity();
		FillDeity(deity);
		return deity;
	}

	public override void CopyTo(IdeoFoundation other)
	{
		base.CopyTo(other);
		IdeoFoundation_Deity ideoFoundation_Deity = (IdeoFoundation_Deity)other;
		ideoFoundation_Deity.deities.Clear();
		foreach (Deity deity2 in deities)
		{
			Deity deity = new Deity();
			deity2.CopyTo(deity);
			ideoFoundation_Deity.deities.Add(deity);
		}
	}
}
