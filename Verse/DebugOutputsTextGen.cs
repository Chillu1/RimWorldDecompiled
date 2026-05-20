using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using LudeonTK;
using RimWorld;
using Verse.Grammar;

namespace Verse;

public static class DebugOutputsTextGen
{
	[DebugOutput("Text generation", false)]
	public static void FlavorfulCombatTest()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		IEnumerable<ManeuverDef> maneuvers = DefDatabase<ManeuverDef>.AllDefsListForReading;
		Func<ManeuverDef, RulePackDef>[] results = new Func<ManeuverDef, RulePackDef>[5]
		{
			(ManeuverDef m) => new RulePackDef[4] { m.combatLogRulesHit, m.combatLogRulesDeflect, m.combatLogRulesMiss, m.combatLogRulesDodge }.RandomElement(),
			(ManeuverDef m) => m.combatLogRulesHit,
			(ManeuverDef m) => m.combatLogRulesDeflect,
			(ManeuverDef m) => m.combatLogRulesMiss,
			(ManeuverDef m) => m.combatLogRulesDodge
		};
		string[] array = new string[5] { "(random)", "Hit", "Deflect", "Miss", "Dodge" };
		foreach (Pair<ManeuverDef, int> maneuverresult in maneuvers.Concat(null).Cross(Enumerable.Range(0, array.Length)))
		{
			DebugMenuOption item = new DebugMenuOption(string.Format("{0}/{1}", (maneuverresult.First == null) ? "(random)" : maneuverresult.First.defName, array[maneuverresult.Second]), DebugMenuOptionMode.Action, delegate
			{
				CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						ManeuverDef maneuver = maneuverresult.First;
						if (maneuver == null)
						{
							maneuver = maneuvers.RandomElement();
						}
						RulePackDef rulePackDef = results[maneuverresult.Second](maneuver);
						List<BodyPartRecord> list2 = null;
						List<bool> list3 = null;
						if (rulePackDef == maneuver.combatLogRulesHit)
						{
							list2 = new List<BodyPartRecord>();
							list3 = new List<bool>();
							bodyPartCreator(list2, list3);
						}
						ImplementOwnerTypeDef implementOwnerTypeDef;
						string toolLabel;
						if (!(from ttp in DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef td) => td.IsMeleeWeapon && !td.tools.NullOrEmpty()).SelectMany((ThingDef td) => td.tools.Select((Tool tool) => new Pair<ThingDef, Tool>(td, tool)))
							where ttp.Second.capacities.Contains(maneuver.requiredCapacity)
							select ttp).TryRandomElement(out var result))
						{
							Log.Warning("Melee weapon with tool with capacity " + maneuver.requiredCapacity?.ToString() + " not found.");
							implementOwnerTypeDef = ImplementOwnerTypeDefOf.Bodypart;
							toolLabel = "(" + implementOwnerTypeDef.defName + ")";
						}
						else
						{
							implementOwnerTypeDef = ((result.Second == null) ? ImplementOwnerTypeDefOf.Bodypart : ImplementOwnerTypeDefOf.Weapon);
							toolLabel = ((result.Second != null) ? result.Second.label : ("(" + implementOwnerTypeDef.defName + ")"));
						}
						BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(rulePackDef, alwaysShowInCompact: false, RandomPawnForCombat(), RandomPawnForCombat(), implementOwnerTypeDef, toolLabel, result.First);
						battleLogEntry_MeleeCombat.FillTargets(list2, list3, battleLogEntry_MeleeCombat.RuleDef.defName.Contains("Deflect"));
						battleLogEntry_MeleeCombat.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_MeleeCombat.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder.ToString());
				});
			});
			list.Add(item);
		}
		int rf = 0;
		while (rf < 2)
		{
			list.Add(new DebugMenuOption((rf == 0) ? "Ranged fire singleshot" : "Ranged fire burst", DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 100; i++)
				{
					ThingDef thingDef = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef td) => td.IsRangedWeapon && td.IsWeaponUsingProjectiles && td.PlayerAcquirable).RandomElement();
					bool flag = Rand.Value < 0.2f;
					bool flag2 = !flag && Rand.Value < 0.95f;
					BattleLogEntry_RangedFire battleLogEntry_RangedFire = new BattleLogEntry_RangedFire(RandomPawnForCombat(), flag ? null : RandomPawnForCombat(), flag2 ? null : thingDef, null, rf != 0);
					battleLogEntry_RangedFire.Debug_OverrideTicks(Rand.Int);
					stringBuilder.AppendLine(battleLogEntry_RangedFire.ToGameStringFromPOV(null));
				}
				Log.Message(stringBuilder.ToString());
			}));
			int num = rf + 1;
			rf = num;
		}
		list.Add(new DebugMenuOption("Ranged impact hit", DebugMenuOptionMode.Action, delegate
		{
			CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 100; i++)
				{
					ThingDef weaponDef = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef td) => td.IsRangedWeapon && td.IsWeaponUsingProjectiles && td.PlayerAcquirable).RandomElement();
					List<BodyPartRecord> list2 = new List<BodyPartRecord>();
					List<bool> list3 = new List<bool>();
					bodyPartCreator(list2, list3);
					Pawn pawn = RandomPawnForCombat();
					BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(RandomPawnForCombat(), pawn, pawn, weaponDef, null, ThingDefOf.Wall);
					battleLogEntry_RangedImpact.FillTargets(list2, list3, Rand.Chance(0.5f));
					battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
					stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null));
				}
				Log.Message(stringBuilder.ToString());
			});
		}));
		list.Add(new DebugMenuOption("Ranged impact miss", DebugMenuOptionMode.Action, delegate
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 100; i++)
			{
				ThingDef weaponDef = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef td) => td.IsRangedWeapon && td.IsWeaponUsingProjectiles && td.PlayerAcquirable).RandomElement();
				BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(RandomPawnForCombat(), null, RandomPawnForCombat(), weaponDef, null, ThingDefOf.Wall);
				battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
				stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null));
			}
			Log.Message(stringBuilder.ToString());
		}));
		list.Add(new DebugMenuOption("Ranged impact hit incorrect", DebugMenuOptionMode.Action, delegate
		{
			CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 100; i++)
				{
					ThingDef weaponDef = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef td) => td.IsRangedWeapon && td.IsWeaponUsingProjectiles && td.PlayerAcquirable).RandomElement();
					List<BodyPartRecord> list2 = new List<BodyPartRecord>();
					List<bool> list3 = new List<bool>();
					bodyPartCreator(list2, list3);
					BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(RandomPawnForCombat(), RandomPawnForCombat(), RandomPawnForCombat(), weaponDef, null, ThingDefOf.Wall);
					battleLogEntry_RangedImpact.FillTargets(list2, list3, Rand.Chance(0.5f));
					battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
					stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null));
				}
				Log.Message(stringBuilder.ToString());
			});
		}));
		foreach (RulePackDef transition in DefDatabase<RulePackDef>.AllDefsListForReading.Where((RulePackDef def) => def.defName.Contains("Transition") && !def.defName.Contains("Include")))
		{
			list.Add(new DebugMenuOption(transition.defName, DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 100; i++)
				{
					Pawn pawn = RandomPawnForCombat();
					Pawn initiator = RandomPawnForCombat();
					BodyPartRecord partRecord = pawn.health.hediffSet.GetNotMissingParts().RandomElement();
					BattleLogEntry_StateTransition battleLogEntry_StateTransition = new BattleLogEntry_StateTransition(pawn, transition, initiator, HediffMaker.MakeHediff(DefDatabase<HediffDef>.AllDefsListForReading.RandomElement(), pawn, partRecord), pawn.RaceProps.body.AllParts.RandomElement());
					battleLogEntry_StateTransition.Debug_OverrideTicks(Rand.Int);
					stringBuilder.AppendLine(battleLogEntry_StateTransition.ToGameStringFromPOV(null));
				}
				Log.Message(stringBuilder.ToString());
			}));
		}
		foreach (RulePackDef damageEvent in DefDatabase<RulePackDef>.AllDefsListForReading.Where((RulePackDef def) => def.defName.Contains("DamageEvent") && !def.defName.Contains("Include")))
		{
			list.Add(new DebugMenuOption(damageEvent.defName, DebugMenuOptionMode.Action, delegate
			{
				CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						List<BodyPartRecord> list2 = new List<BodyPartRecord>();
						List<bool> list3 = new List<bool>();
						bodyPartCreator(list2, list3);
						BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(RandomPawnForCombat(), damageEvent);
						battleLogEntry_DamageTaken.FillTargets(list2, list3, deflected: false);
						battleLogEntry_DamageTaken.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_DamageTaken.ToGameStringFromPOV(null));
					}
					Log.Message(stringBuilder.ToString());
				});
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput("Text generation", false)]
	public static void CubeDescriptions()
	{
		if (ModsConfig.AnomalyActive)
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < 10; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale());
			}
			List<(string, string)> list2 = new List<(string, string)>();
			for (int j = 0; j < 40; j++)
			{
				TaleReference taleReference = new TaleReference(list[j % 10]);
				TaggedString taggedString = taleReference.GenerateText(TextGenerationPurpose.ArtName, RulePackDefOf.NamerArtCubeSculpture);
				TaggedString taggedString2 = taleReference.GenerateText(TextGenerationPurpose.ArtDescription, RulePackDefOf.ArtDescription_CubeSculpture);
				list2.Add((taggedString, taggedString2));
			}
			DebugTables.MakeTablesDialog(list2, new TableDataGetter<(string, string)>("title", ((string title, string desc) g) => g.title), new TableDataGetter<(string, string)>("description", ((string title, string desc) g) => g.desc));
		}
	}

	[DebugOutput("Text generation", false)]
	public static void VoidSculptureDescriptions()
	{
		if (ModsConfig.AnomalyActive)
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < 10; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale());
			}
			List<(string, string)> list2 = new List<(string, string)>();
			for (int j = 0; j < 40; j++)
			{
				TaleReference taleReference = new TaleReference(list[j % 10]);
				TaggedString taggedString = taleReference.GenerateText(TextGenerationPurpose.ArtName, RulePackDefOf.NamerArtVoidSculpture);
				TaggedString taggedString2 = taleReference.GenerateText(TextGenerationPurpose.ArtDescription, RulePackDefOf.ArtDescription_VoidSculpture);
				list2.Add((taggedString, taggedString2));
			}
			DebugTables.MakeTablesDialog(list2, new TableDataGetter<(string, string)>("title", ((string title, string desc) g) => g.title), new TableDataGetter<(string, string)>("description", ((string title, string desc) g) => g.desc));
		}
	}

	[DebugOutput("Text generation", false)]
	public static void Books()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.HasComp<CompBook>()).ToList())
		{
			list.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Action, delegate
			{
				BookDefGenerated(def);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	private static void BookDefGenerated(ThingDef def)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 30; i++)
		{
			Book book = BookUtility.MakeBook(def, ArtGenerationContext.Outsider);
			stringBuilder.AppendLine(book.Label);
			stringBuilder.AppendLine(book.FlavorUI);
			stringBuilder.AppendLine();
		}
		Log.Message(stringBuilder.ToString());
	}

	public static Pawn RandomPawnForCombat()
	{
		PawnKindDef pawnKindDef = DefDatabase<PawnKindDef>.AllDefsListForReading.RandomElementByWeight(delegate(PawnKindDef pawnkind)
		{
			if (pawnkind.RaceProps.Humanlike)
			{
				return 8f;
			}
			return pawnkind.RaceProps.IsMechanoid ? 8f : 1f;
		});
		Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionDef);
		return PawnGenerator.GeneratePawn(pawnKindDef, faction);
	}

	private static void CreateDamagedDestroyedMenu(Action<Action<List<BodyPartRecord>, List<bool>>> callback)
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		IEnumerable<int> damagedes = Enumerable.Range(0, 5);
		IEnumerable<int> destroyedes = Enumerable.Range(0, 5);
		foreach (Pair<int, int> damageddestroyed in damagedes.Concat(-1).Cross(destroyedes.Concat(-1)))
		{
			DebugMenuOption item = new DebugMenuOption(string.Format("{0} damaged/{1} destroyed", (damageddestroyed.First == -1) ? "(random)" : damageddestroyed.First.ToString(), (damageddestroyed.Second == -1) ? "(random)" : damageddestroyed.Second.ToString()), DebugMenuOptionMode.Action, delegate
			{
				callback(delegate(List<BodyPartRecord> bodyparts, List<bool> flags)
				{
					int num = damageddestroyed.First;
					int destroyed = damageddestroyed.Second;
					if (num == -1)
					{
						num = damagedes.RandomElement();
					}
					if (destroyed == -1)
					{
						destroyed = destroyedes.RandomElement();
					}
					Pair<BodyPartRecord, bool>[] source = (from idx in Enumerable.Range(0, num + destroyed)
						select new Pair<BodyPartRecord, bool>(BodyDefOf.Human.AllParts.RandomElement(), idx < destroyed)).InRandomOrder().ToArray();
					bodyparts.Clear();
					flags.Clear();
					bodyparts.AddRange(source.Select((Pair<BodyPartRecord, bool> part) => part.First));
					flags.AddRange(source.Select((Pair<BodyPartRecord, bool> part) => part.Second));
				});
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput("Text generation", false)]
	public static void ArtDescsSpecificTale()
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (TaleDef item2 in DefDatabase<TaleDef>.AllDefs.OrderBy((TaleDef def) => def.defName))
		{
			TaleDef localDef = item2;
			FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
			{
				LogSpecificTale(localDef, 40);
			});
			list.Add(item);
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Text generation", false)]
	public static void NamesFromRulepack()
	{
		IOrderedEnumerable<RulePackDef> orderedEnumerable = from d in DefDatabase<RulePackDef>.AllDefsListForReading
			where d.directTestable || d.defName.StartsWith("Namer")
			orderby d.defName
			select d;
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (RulePackDef item in orderedEnumerable)
		{
			RulePackDef localNamer = item;
			list.Add(new DebugMenuOption(localNamer.defName, DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Testing RulePack " + localNamer.defName + " as a name generator:");
				for (int i = 0; i < 200; i++)
				{
					string testPawnNameSymbol = ((i % 2 == 0) ? "Smithee" : null);
					stringBuilder.AppendLine(NameGenerator.GenerateName(localNamer, null, appendNumberIfNameUsed: false, localNamer.FirstRuleKeyword, testPawnNameSymbol));
				}
				Log.Message(stringBuilder.ToString());
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput("Text generation", false)]
	public static void LandmarkNames()
	{
		List<LandmarkDef> allDefsListForReading = DefDatabase<LandmarkDef>.AllDefsListForReading;
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (LandmarkDef landmark in allDefsListForReading)
		{
			RulePackDef localNamer = landmark.nameMaker;
			list.Add(new DebugMenuOption(landmark.defName, DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Testing Landmark Namer " + landmark.defName + " using namer " + localNamer.defName);
				for (int i = 0; i < 200; i++)
				{
					string testPawnNameSymbol = ((i % 2 == 0) ? "Smithee" : null);
					stringBuilder.AppendLine(NameGenerator.GenerateName(localNamer, null, appendNumberIfNameUsed: false, "r_name", testPawnNameSymbol));
				}
				Log.Message(stringBuilder.ToString());
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugOutput("Text generation", true)]
	public static void DatabaseTalesList()
	{
		Find.TaleManager.LogTales();
	}

	[DebugOutput("Text generation", true)]
	public static void DatabaseTalesInterest()
	{
		Find.TaleManager.LogTaleInterestSummary();
	}

	[DebugOutput("Text generation", true)]
	public static void ArtDescsDatabaseTales()
	{
		LogTales(Find.TaleManager.AllTalesListForReading.Where((Tale t) => t.def.usableForArt));
	}

	[DebugOutput("Text generation", true)]
	public static void ArtDescsRandomTales()
	{
		int num = 40;
		List<Tale> list = new List<Tale>();
		for (int i = 0; i < num; i++)
		{
			list.Add(TaleFactory.MakeRandomTestTale());
		}
		LogTales(list);
	}

	[DebugOutput("Text generation", true)]
	public static void ArtDescsTaleless()
	{
		List<Tale> list = new List<Tale>();
		for (int i = 0; i < 20; i++)
		{
			list.Add(null);
		}
		LogTales(list);
	}

	[DebugOutput("Text generation", false)]
	public static void InteractionLogs()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (InteractionDef def in DefDatabase<InteractionDef>.AllDefsListForReading)
		{
			list.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
				Pawn recipient = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
				for (int i = 0; i < 100; i++)
				{
					PlayLogEntry_Interaction playLogEntry_Interaction = new PlayLogEntry_Interaction(def, pawn, recipient, null);
					stringBuilder.AppendLine(playLogEntry_Interaction.ToGameStringFromPOV(pawn));
				}
				Log.Message(stringBuilder.ToString());
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	private static void LogSpecificTale(TaleDef def, int count)
	{
		List<Tale> list = new List<Tale>();
		for (int i = 0; i < count; i++)
		{
			list.Add(TaleFactory.MakeRandomTestTale(def));
		}
		LogTales(list);
	}

	private static void LogTales(IEnumerable<Tale> tales)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (Tale tale in tales)
		{
			TaleReference tr = new TaleReference(tale);
			stringBuilder.AppendLine(RandomArtworkName(tr));
			stringBuilder.AppendLine(RandomArtworkDescription(tr));
			stringBuilder.AppendLine();
			num++;
			if (num % 20 == 0)
			{
				Log.Message(stringBuilder.ToString());
				stringBuilder = new StringBuilder();
			}
		}
		if (!stringBuilder.ToString().NullOrEmpty())
		{
			Log.Message(stringBuilder.ToString());
		}
	}

	private static string RandomArtworkName(TaleReference tr)
	{
		RulePackDef extraInclude = null;
		switch (Rand.RangeInclusive(0, 4))
		{
		case 0:
			extraInclude = RulePackDefOf.NamerArtSculpture;
			break;
		case 1:
			extraInclude = RulePackDefOf.NamerArtWeaponMelee;
			break;
		case 2:
			extraInclude = RulePackDefOf.NamerArtWeaponGun;
			break;
		case 3:
			extraInclude = RulePackDefOf.NamerArtFurniture;
			break;
		case 4:
			extraInclude = RulePackDefOf.NamerArtSarcophagusPlate;
			break;
		}
		return tr.GenerateText(TextGenerationPurpose.ArtName, extraInclude);
	}

	private static string RandomArtworkDescription(TaleReference tr)
	{
		RulePackDef extraInclude = null;
		switch (Rand.RangeInclusive(0, 4))
		{
		case 0:
			extraInclude = RulePackDefOf.ArtDescription_Sculpture;
			break;
		case 1:
			extraInclude = RulePackDefOf.ArtDescription_WeaponMelee;
			break;
		case 2:
			extraInclude = RulePackDefOf.ArtDescription_WeaponGun;
			break;
		case 3:
			extraInclude = RulePackDefOf.ArtDescription_Furniture;
			break;
		case 4:
			extraInclude = RulePackDefOf.ArtDescription_SarcophagusPlate;
			break;
		}
		return tr.GenerateText(TextGenerationPurpose.ArtDescription, extraInclude);
	}

	[DebugOutput("Text generation", false)]
	private static void WordCount()
	{
		int words = 0;
		Regex wordRegex = new Regex("\\w(?![^{]*})(?![^[]*])[^ \\W]*");
		HashSet<string> results = new HashSet<string>();
		foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> keyedReplacement in LanguageDatabase.activeLanguage.keyedReplacements)
		{
			TryAdd(keyedReplacement.Value.value);
		}
		foreach (Type item in GenDefDatabase.AllDefTypesWithDatabases())
		{
			foreach (Def item2 in GenDefDatabase.GetAllDefsInDatabaseForDef(item))
			{
				if (item2.generated || item2 is RulePackDef)
				{
					continue;
				}
				FieldInfo[] fields = item.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.HasAttribute<MustTranslateAttribute>() || fieldInfo.HasAttribute<MayTranslateAttribute>())
					{
						ProcessField(fieldInfo.GetValue(item2));
					}
				}
			}
		}
		foreach (RulePackDef item3 in DefDatabase<RulePackDef>.AllDefsListForReading)
		{
			List<Rule> rulesImmediate = item3.RulesImmediate;
			if (rulesImmediate.NullOrEmpty())
			{
				continue;
			}
			foreach (Rule item4 in rulesImmediate)
			{
				if (item4 is Rule_String rule_String)
				{
					string text = ProcessString(rule_String.Generate());
					if (results.Add(text))
					{
						words += wordRegex.Matches(text).Count;
					}
				}
			}
		}
		Log.Message($"Word count: {words}");
		void ProcessField(object obj)
		{
			if (obj != null)
			{
				if (obj is string str)
				{
					TryAdd(str);
				}
				else if (obj is TaggedString taggedString)
				{
					TryAdd(taggedString.RawText);
				}
				else
				{
					if (obj is IEnumerable<string> enumerable)
					{
						{
							foreach (string item5 in enumerable)
							{
								TryAdd(item5);
							}
							return;
						}
					}
					if (obj is IEnumerable<TaggedString> enumerable2)
					{
						foreach (TaggedString item6 in enumerable2)
						{
							TryAdd(item6);
						}
					}
				}
			}
		}
		static string ProcessString(string str)
		{
			return str.Flatten().Replace("-", "");
		}
		void TryAdd(string str)
		{
			if (!str.NullOrEmpty())
			{
				string text2 = ProcessString(str);
				if (results.Add(text2))
				{
					words += wordRegex.Matches(text2).Count;
				}
			}
		}
	}
}
