using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CreditsAssembler
{
	public static IEnumerable<CreditsEntry> AllCredits()
	{
		yield return new CreditRecord_Space(200f);
		yield return new CreditRecord_Title("Credits_Design".Translate());
		yield return new CreditRecord_Role("", "Tynan Sylvester");
		yield return new CreditRecord_Role("", "Will Stacey");
		yield return new CreditRecord_Role("", "Thorsten Klotz");
		yield return new CreditRecord_Title("Credits_Developers".Translate());
		yield return new CreditRecord_Role("", "Piotr Walczak");
		yield return new CreditRecord_Role("", "Tynan Sylvester");
		yield return new CreditRecord_Role("", "Igor Lebedev");
		yield return new CreditRecord_Role("", "Matt Ritchie");
		yield return new CreditRecord_Role("", "Alex Mulford");
		yield return new CreditRecord_Role("", "Joe Gasparich");
		yield return new CreditRecord_Role("", "Máté Mészáros");
		yield return new CreditRecord_Role("", "Liam Harrison");
		yield return new CreditRecord_Role("", "Kenneth Ellersdorfer");
		yield return new CreditRecord_Role("", "Sam Byass");
		yield return new CreditRecord_Role("", "Ben Rog-Wilhelm");
		yield return new CreditRecord_Role("", "Matt Quail");
		yield return new CreditRecord_Role("", "Mark Kaldas");
		yield return new CreditRecord_Role("", "Nick Barrash");
		yield return new CreditRecord_Role("", "Don Bellenger");
		yield return new CreditRecord_Role("", "Jay Lemmon");
		yield return new CreditRecord_Space(50f);
		yield return new CreditRecord_Title("Credit_MusicAndSound".Translate());
		yield return new CreditRecord_Role("", "Alistair Lindsay");
		yield return new CreditRecord_Space(50f);
		yield return new CreditRecord_Title("Credit_GameArt".Translate());
		yield return new CreditRecord_Role("", "Oskar Potocki");
		yield return new CreditRecord_Role("", "Tynan Sylvester");
		yield return new CreditRecord_Role("", "Hayden Duvall");
		yield return new CreditRecord_Role("", "Mehran Iranloo");
		yield return new CreditRecord_Role("", "Ricardo Tomé");
		yield return new CreditRecord_Role("", "Rhopunzel");
		yield return new CreditRecord_Role("", "Tamara Osborn");
		yield return new CreditRecord_Space(50f);
		yield return new CreditRecord_Title("Credits_AdditionalDevelopment".Translate());
		List<CreditsEntry> list = new List<CreditsEntry>();
		list.Add(new CreditRecord_Role("", "August Trollbäck").Compress());
		list.Add(new CreditRecord_Role("", "John Fernandes-Salling").Compress());
		list.Add(new CreditRecord_Role("", "Tate Conlon").Compress());
		list.Add(new CreditRecord_Role("", "Gavan Woolery").Compress());
		list.Add(new CreditRecord_Role("", "David 'Rez' Graham").Compress());
		foreach (CreditsEntry item in Reformat2Cols(list))
		{
			yield return item;
		}
		yield return new CreditRecord_Space(50f);
		yield return new CreditRecord_Title("Credit_Testers".Translate());
		List<CreditsEntry> list2 = new List<CreditsEntry>();
		list2.Add(new CreditRecord_Role("", "Fey Nickel"));
		list2.Add(new CreditRecord_Role("", "Morg"));
		list2.Add(new CreditRecord_Role("", "ItchyFlea"));
		list2.Add(new CreditRecord_Role("", "Sneaks"));
		list2.Add(new CreditRecord_Role("", "Elliott"));
		list2.Add(new CreditRecord_Role("", "James Miura"));
		foreach (CreditsEntry item2 in Reformat2Cols(list2))
		{
			yield return item2;
		}
		yield return new CreditRecord_Space(50f);
		yield return new CreditRecord_Title("Credit_SpecialThanks".Translate());
		List<CreditsEntry> list3 = new List<CreditsEntry>();
		list3.Add(new CreditRecord_Role("", "Tia Young").Compress());
		list3.Add(new CreditRecord_Role("", "Kay Fedewa").Compress());
		list3.Add(new CreditRecord_Role("", "Jon Larson").Compress());
		list3.Add(new CreditRecord_Role("", "Zhentar").Compress());
		list3.Add(new CreditRecord_Role("", "Haplo").Compress());
		list3.Add(new CreditRecord_Role("", "iame6162013").Compress());
		list3.Add(new CreditRecord_Role("", "Shinzy").Compress());
		list3.Add(new CreditRecord_Role("", "John Woolley").Compress());
		list3.Add(new CreditRecord_Role("", "Marta Fijak").Compress());
		list3.Add(new CreditRecord_Role("", "Simon Warrener").Compress());
		foreach (CreditsEntry item3 in Reformat2Cols(list3))
		{
			yield return item3;
		}
		yield return new CreditRecord_Space(50f);
		yield return new CreditRecord_Title("Credits_TitleTester".Translate());
		List<CreditsEntry> list4 = new List<CreditsEntry>();
		list4.Add(new CreditRecord_Role("", "Ramsis").Compress());
		list4.Add(new CreditRecord_Role("", "Haplo").Compress());
		list4.Add(new CreditRecord_Role("", "DubskiDude").Compress());
		list4.Add(new CreditRecord_Role("", "Harry Bryant").Compress());
		list4.Add(new CreditRecord_Role("", "ChJees").Compress());
		list4.Add(new CreditRecord_Role("", "AWiseCorn").Compress());
		list4.Add(new CreditRecord_Role("", "Zero747").Compress());
		list4.Add(new CreditRecord_Role("", "Mehni").Compress());
		list4.Add(new CreditRecord_Role("", "XeoNovaDan").Compress());
		list4.Add(new CreditRecord_Role("", "alphaBeta").Compress());
		list4.Add(new CreditRecord_Role("", "TheDee05").Compress());
		list4.Add(new CreditRecord_Role("", "Oglis").Compress());
		list4.Add(new CreditRecord_Role("", "Vas").Compress());
		list4.Add(new CreditRecord_Role("", "Kiaayo").Compress());
		list4.Add(new CreditRecord_Role("", "JimmyAgnt007").Compress());
		list4.Add(new CreditRecord_Role("", "Gouda Quiche").Compress());
		list4.Add(new CreditRecord_Role("", "Drb89").Compress());
		list4.Add(new CreditRecord_Role("", "Jimyoda").Compress());
		list4.Add(new CreditRecord_Role("", "Semmy").Compress());
		list4.Add(new CreditRecord_Role("", "DianaWinters").Compress());
		list4.Add(new CreditRecord_Role("", "Goldenpotatoes").Compress());
		list4.Add(new CreditRecord_Role("", "Skissor").Compress());
		list4.Add(new CreditRecord_Role("", "Laos").Compress());
		list4.Add(new CreditRecord_Role("", "Evul").Compress());
		list4.Add(new CreditRecord_Role("", "SoraHjort").Compress());
		list4.Add(new CreditRecord_Role("", "Coenmjc").Compress());
		list4.Add(new CreditRecord_Role("", "Boris(Eqz)").Compress());
		list4.Add(new CreditRecord_Role("", "MarvinKosh").Compress());
		list4.Add(new CreditRecord_Role("", "Gaesatae").Compress());
		list4.Add(new CreditRecord_Role("", "Letharion").Compress());
		list4.Add(new CreditRecord_Role("", "HeftySmurf").Compress());
		list4.Add(new CreditRecord_Role("", "Skullywag").Compress());
		list4.Add(new CreditRecord_Role("", "Jaxxa").Compress());
		list4.Add(new CreditRecord_Role("", "Helixien").Compress());
		list4.Add(new CreditRecord_Role("", "DeeGee").Compress());
		list4.Add(new CreditRecord_Role("", "ReZpawner").Compress());
		list4.Add(new CreditRecord_Role("", "Doomdrvk").Compress());
		list4.Add(new CreditRecord_Role("", "tedvs").Compress());
		list4.Add(new CreditRecord_Role("", "OneSpellPerDay").Compress());
		list4.Add(new CreditRecord_Role("", "Turbulent Caterwocky").Compress());
		list4.Add(new CreditRecord_Role("", "RawCode").Compress());
		list4.Add(new CreditRecord_Role("", "Enystrom8734").Compress());
		list4.Add(new CreditRecord_Role("", "TeiXeR").Compress());
		list4.Add(new CreditRecord_Role("", "MortalSmurph").Compress());
		list4.Add(new CreditRecord_Role("", "AdamVsEverything").Compress());
		list4.Add(new CreditRecord_Role("", "Fawkes").Compress());
		list4.Add(new CreditRecord_Role("", "LucyFeonix").Compress());
		list4.Add(new CreditRecord_Role("", "Erin").Compress());
		list4.Add(new CreditRecord_Role("", "Ragnar-F").Compress());
		foreach (CreditsEntry item4 in Reformat2Cols(list4))
		{
			yield return item4;
		}
		yield return new CreditRecord_Space(25f);
		yield return new CreditRecord_Role("", "... " + "AndOtherVolunteers".Translate());
		yield return new CreditRecord_Space(200f);
		foreach (LoadedLanguage lang in LanguageDatabase.AllLoadedLanguages)
		{
			lang.LoadMetadata();
			if (lang.info.credits.Count > 0)
			{
				yield return new CreditRecord_Title("Credits_TitleLanguage".Translate(lang.FriendlyNameEnglish, lang.FriendlyNameNative));
			}
			foreach (CreditsEntry item5 in Reformat2Cols(lang.info.credits))
			{
				yield return item5;
			}
		}
		if (ModLister.AnomalyInstalled)
		{
			yield return new CreditRecord_TitleLocalization("Credits_Localization_Anomaly".Translate());
			yield return new CreditRecord_TitleLocalization("Local Heroes Worldwide B.V.");
			yield return new CreditRecord_Role("Credits_LeadLocalizationProjectManager".Translate(), "Iris Kuppen").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_LocalizationProjectManager".Translate(), "Maikel Roelofs").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_French".Translate() + ": Loc-3 Ltd");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Eric Emanuel").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Florie Abélard").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Ophélie Colin").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Claire Deiller").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Japanese".Translate() + ": DICO Co., Ltd.");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Nilgül Durali").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Ziya Sarper Ekim").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Karien Harimoto").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Keigo Yonemura").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Henry Buckley").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Korean".Translate() + ": DICO Co., Ltd.");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Nilgül Durali").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Ziya Sarper Ekim").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Doyeon Jeong").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Junglim Kim").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Lim Yoon").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Polish".Translate() + ": Albion Localisations");
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Aleksandra Lubińska").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Łukasz Gładkowski").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Marcin Bojko").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Russian".Translate() + ": Levsha");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Ekaterina Yamenskova").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Vitaliy Hristyuk").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Polina Karpova").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Artyom Petrov").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_ChineseSimplified".Translate() + ": Yeehe");
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Jean-Luc Wu").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Roy Liu").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Twig Yu (爽朗的kk23)").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Marcos Wang").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "ZISHA").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_ChineseTraditional".Translate() + ": Cowbay Entertainment");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Sean Chen").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Shiou Chen").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Michelle Wu").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Teddy Wu").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Jerry Lee").Compress().WithSmallFont();
		}
		if (ModLister.OdysseyInstalled)
		{
			yield return new CreditRecord_TitleLocalization("Credits_Localization_Odyssey".Translate());
			yield return new CreditRecord_TitleLocalization("Local Heroes Worldwide B.V.");
			yield return new CreditRecord_Role("Credits_LeadLocalizationProjectManager".Translate(), "Iris Kuppen").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_LocalizationProjectManager".Translate(), "Maikel Roelofs").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Dutch".Translate() + ": The Translation Fource");
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Sandra Vos").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "René van Vemde").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Maikel Roelofs").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Sandra Vos").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Russian".Translate() + ": Levsha");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Ekaterina Yamenskova").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Nadezhda Lynova").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Turkish".Translate() + ": Locpick");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Batuhan Öztürk").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Bigem Gözen").Compress().WithSmallFont();
			yield return new CreditRecord_TitleLocalization("Credits_Ukrainian".Translate() + ": Levsha");
			yield return new CreditRecord_Role("Credits_ProjectManager".Translate(), "Ekaterina Yamenskova").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Translator".Translate(), "Vitalii Khrystiuk").Compress().WithSmallFont();
			yield return new CreditRecord_Role("Credits_Editor".Translate(), "Konstantin Kopin").Compress().WithSmallFont();
		}
		bool firstModCredit = false;
		HashSet<string> allModders = new HashSet<string>();
		List<string> tmpModders = new List<string>();
		foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder.InRandomOrder())
		{
			if (mod.Official)
			{
				continue;
			}
			tmpModders.Clear();
			tmpModders.AddRange(mod.Authors);
			for (int num = tmpModders.Count - 1; num >= 0; num--)
			{
				tmpModders[num] = tmpModders[num].Trim();
				if (!allModders.Add(tmpModders[num].ToLowerInvariant()))
				{
					tmpModders.RemoveAt(num);
				}
			}
			if (tmpModders.Count <= 0)
			{
				continue;
			}
			foreach (string modder in tmpModders)
			{
				if (!firstModCredit)
				{
					yield return new CreditRecord_Title("Credits_TitleMods".Translate());
					firstModCredit = true;
				}
				yield return new CreditRecord_Role(mod.Name, modder).Compress();
			}
		}
		static IEnumerable<CreditsEntry> Reformat2Cols(List<CreditsEntry> entries)
		{
			string crediteePrev = null;
			for (int i = 0; i < entries.Count; i++)
			{
				CreditsEntry langCred = entries[i];
				if (langCred is CreditRecord_Role creditRecord_Role)
				{
					if (crediteePrev != null)
					{
						yield return new CreditRecord_RoleTwoCols(crediteePrev, creditRecord_Role.creditee).Compress();
						crediteePrev = null;
					}
					else
					{
						crediteePrev = creditRecord_Role.creditee;
					}
				}
				else
				{
					if (crediteePrev != null)
					{
						yield return new CreditRecord_RoleTwoCols(crediteePrev, "").Compress();
						crediteePrev = null;
					}
					yield return langCred;
				}
			}
			if (crediteePrev != null)
			{
				yield return new CreditRecord_RoleTwoCols(crediteePrev, "").Compress();
			}
		}
	}
}
