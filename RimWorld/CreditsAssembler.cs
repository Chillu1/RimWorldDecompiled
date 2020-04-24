using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CreditsAssembler
	{
		public static IEnumerable<CreditsEntry> AllCredits()
		{
			yield return new CreditRecord_Space(200f);
			yield return new CreditRecord_Title("Credits_Developers".Translate());
			yield return new CreditRecord_Role("", "Tynan Sylvester");
			yield return new CreditRecord_Role("", "Piotr Walczak");
			yield return new CreditRecord_Role("", "Ben Rog-Wilhelm");
			yield return new CreditRecord_Role("", "Kenneth Ellersdorfer");
			yield return new CreditRecord_Role("", "Igor Lebedev");
			yield return new CreditRecord_Role("", "Matt Ritchie");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credit_MusicAndSound".Translate());
			yield return new CreditRecord_Role("", "Alistair Lindsay");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credit_GameArt".Translate());
			yield return new CreditRecord_Role("", "Tynan Sylvester");
			yield return new CreditRecord_Role("", "Rhopunzel");
			yield return new CreditRecord_Role("", "Oskar Potocki");
			yield return new CreditRecord_Role("", "Ricardo Tome");
			yield return new CreditRecord_Role("", "Kay Fedewa");
			yield return new CreditRecord_Role("", "Jon Larson");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credits_AdditionalDevelopment".Translate());
			yield return new CreditRecord_Role("", "Gavan Woolery");
			yield return new CreditRecord_Role("", "David 'Rez' Graham");
			yield return new CreditRecord_Role("", "Ben Grob");
			yield return new CreditRecord_Space(50f);
			yield return new CreditRecord_Title("Credits_TitleCommunity".Translate());
			yield return new CreditRecord_Role("Credit_ModDonation", "Zhentar");
			yield return new CreditRecord_Role("Credit_ModDonation", "Haplo");
			yield return new CreditRecord_Role("Credit_ModDonation", "iame6162013");
			yield return new CreditRecord_Role("Credit_ModDonation", "Shinzy");
			yield return new CreditRecord_Role("Credit_WritingDonation", "John Woolley");
			yield return new CreditRecord_Role("Credit_Moderator", "ItchyFlea");
			yield return new CreditRecord_Role("Credit_Moderator", "Ramsis");
			yield return new CreditRecord_Role("Credit_Moderator", "Calahan");
			yield return new CreditRecord_Role("Credit_Moderator", "milon");
			yield return new CreditRecord_Role("Credit_Moderator", "Evul");
			yield return new CreditRecord_Role("Credit_Moderator", "MarvinKosh");
			yield return new CreditRecord_Role("Credit_WikiMaster", "ZestyLemons");
			yield return new CreditRecord_Role("Credit_Tester", "ItchyFlea");
			yield return new CreditRecord_Role("Credit_Tester", "Haplo");
			yield return new CreditRecord_Role("Credit_Tester", "Mehni");
			yield return new CreditRecord_Role("Credit_Tester", "Vas");
			yield return new CreditRecord_Role("Credit_Tester", "XeoNovaDan");
			yield return new CreditRecord_Role("Credit_Tester", "JimmyAgnt007");
			yield return new CreditRecord_Role("Credit_Tester", "Goldenpotatoes");
			yield return new CreditRecord_Role("Credit_Tester", "_alphaBeta_");
			yield return new CreditRecord_Role("Credit_Tester", "TheDee05");
			yield return new CreditRecord_Role("Credit_Tester", "Drb89");
			yield return new CreditRecord_Role("Credit_Tester", "Skissor");
			yield return new CreditRecord_Role("Credit_Tester", "MarvinKosh");
			yield return new CreditRecord_Role("Credit_Tester", "Evul");
			yield return new CreditRecord_Role("Credit_Tester", "Jimyoda");
			yield return new CreditRecord_Role("Credit_Tester", "Pheanox");
			yield return new CreditRecord_Role("Credit_Tester", "Semmy");
			yield return new CreditRecord_Role("Credit_Tester", "Letharion");
			yield return new CreditRecord_Role("Credit_Tester", "Laos");
			yield return new CreditRecord_Role("Credit_Tester", "Coenmjc");
			yield return new CreditRecord_Role("Credit_Tester", "Gaesatae");
			yield return new CreditRecord_Role("Credit_Tester", "Skullywag");
			yield return new CreditRecord_Role("Credit_Tester", "Enystrom8734");
			yield return new CreditRecord_Role("", "Many other gracious volunteers!");
			yield return new CreditRecord_Space(200f);
			foreach (LoadedLanguage lang in LanguageDatabase.AllLoadedLanguages)
			{
				lang.LoadMetadata();
				if (lang.info.credits.Count > 0)
				{
					yield return new CreditRecord_Title("Credits_TitleLanguage".Translate(lang.FriendlyNameEnglish));
				}
				foreach (CreditsEntry credit in lang.info.credits)
				{
					CreditRecord_Role creditRecord_Role = credit as CreditRecord_Role;
					if (creditRecord_Role != null)
					{
						creditRecord_Role.compressed = true;
					}
					yield return credit;
				}
			}
		}
	}
}
