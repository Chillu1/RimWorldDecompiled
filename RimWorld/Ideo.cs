using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Ideo : IExposable, ILoadReferenceable
	{
		public static int MinBelieversToEnableObligations = 3;

		public int id = -1;

		public IdeoFoundation foundation;

		public CultureDef culture;

		public List<MemeDef> memes = new List<MemeDef>();

		private List<Precept> precepts = new List<Precept>();

		public List<string> usedSymbols = new List<string>();

		public IdeoStyleTracker style;

		public bool createdFromNoExpansionGame;

		[LoadAlias("classic")]
		public bool classicExtraMode;

		[LoadAlias("hiddenIdeoMode")]
		public bool classicMode;

		public IdeoDevelopmentTracker development;

		private bool fluid;

		public bool hidden;

		public bool solid;

		public string fileName;

		public bool initialPlayerIdeo;

		public bool anyPreceptEdited;

		public int currentCacheId = 1;

		public string name;

		public string adjective;

		public string memberName;

		public IdeoIconDef iconDef;

		public ColorDef colorDef;

		public Color? primaryFactionColor;

		public string leaderTitleMale;

		public string leaderTitleFemale;

		public List<string> usedSymbolPacks = new List<string>();

		public string description;

		public string descriptionTemplate;

		public bool descriptionLocked;

		private string overriddenWorshipRoomLabel;

		public bool relicsCollected;

		private Texture2D icon;

		public HashSet<ThoughtDef> cachedPossibleSituationalThoughts = new HashSet<ThoughtDef>();

		public List<GoodwillSituationDef> cachedPossibleGoodwillSituations = new List<GoodwillSituationDef>();

		public List<Precept_Role> cachedPossibleRoles = new List<Precept_Role>();

		public HashSet<Precept_Building> cachedPossibleBuildings = new HashSet<Precept_Building>();

		public HashSet<BuildableDef> cachedPossibleBuildables = new HashSet<BuildableDef>();

		private HashSet<NeedDef> cachedEnabledNeeds = new HashSet<NeedDef>();

		private HashSet<NeedDef> cachedDisabledNeeds = new HashSet<NeedDef>();

		public List<ThingStyleCategoryWithPriority> thingStyleCategories = new List<ThingStyleCategoryWithPriority>();

		private List<ThingDef> cachedVeneratedAnimals;

		private List<XenotypeDef> cachedPreferredXenotypes;

		private List<CustomXenotype> cachedPreferredCustomXenotypes;

		private int requiredScarsCached;

		private bool warnPlayerOnDesignateChopTree;

		private bool warnPlayerOnDesignateMine;

		private List<RitualObligationTargetFilter> cachedRitualTargetFilters;

		private HashSet<PreceptDef> allPreceptDefs = new HashSet<PreceptDef>();

		private string memberNamePluralCached;

		public HashSet<MentalBreakDef> cachedPossibleMentalBreaks = new HashSet<MentalBreakDef>();

		private static List<Pawn> tmpPlayerIdeoFollowers = new List<Pawn>();

		private static List<Precept> tmpThoughtPrecepts = new List<Precept>();

		private int colonistBelieverCountCached = -1;

		public List<Precept> PreceptsListForReading => precepts;

		public Texture2D Icon => icon ?? (icon = ContentFinder<Texture2D>.Get((iconDef != null) ? iconDef.iconPath : BaseContent.BadTexPath));

		public List<Precept_Role> RolesListForReading => cachedPossibleRoles;

		public bool WarnPlayerOnDesignateChopTree => warnPlayerOnDesignateChopTree;

		public bool WarnPlayerOnDesignateMine => warnPlayerOnDesignateMine;

		public Color Color
		{
			get
			{
				if (!classicMode)
				{
					return primaryFactionColor ?? colorDef?.color ?? Color.white;
				}
				return Color.white;
			}
		}

		public Color ApparelColor
		{
			get
			{
				Color.RGBToHSV(Color, out var H, out var S, out var V);
				return Color.HSVToRGB(H, Mathf.Min(0.5f, S), Mathf.Min(0.8235294f, V));
			}
		}

		public Color TextColor
		{
			get
			{
				Color.RGBToHSV(Color, out var H, out var S, out var V);
				return Color.HSVToRGB(H, S, Mathf.Max(0.5f, V));
			}
		}

		public List<ThingDef> VeneratedAnimals
		{
			get
			{
				if (cachedVeneratedAnimals == null)
				{
					cachedVeneratedAnimals = new List<ThingDef>();
					for (int i = 0; i < precepts.Count; i++)
					{
						if (precepts[i] is Precept_Animal precept_Animal)
						{
							cachedVeneratedAnimals.Add(precept_Animal.ThingDef);
						}
					}
				}
				return cachedVeneratedAnimals;
			}
		}

		public List<XenotypeDef> PreferredXenotypes
		{
			get
			{
				if (cachedPreferredXenotypes == null)
				{
					cachedPreferredXenotypes = new List<XenotypeDef>();
					for (int i = 0; i < precepts.Count; i++)
					{
						if (precepts[i] is Precept_Xenotype { xenotype: not null } precept_Xenotype)
						{
							cachedPreferredXenotypes.Add(precept_Xenotype.xenotype);
						}
					}
				}
				return cachedPreferredXenotypes;
			}
		}

		public List<CustomXenotype> PreferredCustomXenotypes
		{
			get
			{
				if (cachedPreferredCustomXenotypes == null)
				{
					cachedPreferredCustomXenotypes = new List<CustomXenotype>();
					for (int i = 0; i < precepts.Count; i++)
					{
						if (precepts[i] is Precept_Xenotype { customXenotype: not null } precept_Xenotype)
						{
							cachedPreferredCustomXenotypes.Add(precept_Xenotype.customXenotype);
						}
					}
				}
				return cachedPreferredCustomXenotypes;
			}
		}

		public IntRange DeityCountRange
		{
			get
			{
				int num = 0;
				int num2 = int.MaxValue;
				for (int i = 0; i < memes.Count; i++)
				{
					if (memes[i].deityCount.min >= 0)
					{
						num = Mathf.Max(num, memes[i].deityCount.min);
					}
					if (memes[i].deityCount.max >= 0)
					{
						num2 = Mathf.Min(num2, memes[i].deityCount.max);
					}
				}
				return new IntRange(num, num2);
			}
		}

		public MemeDef StructureMeme
		{
			get
			{
				for (int i = 0; i < memes.Count; i++)
				{
					if (memes[i].category == MemeCategory.Structure)
					{
						return memes[i];
					}
				}
				return null;
			}
		}

		public Gender SupremeGender
		{
			get
			{
				if (HasMeme(MemeDefOf.MaleSupremacy))
				{
					return Gender.Male;
				}
				if (HasMeme(MemeDefOf.FemaleSupremacy))
				{
					return Gender.Female;
				}
				return Gender.None;
			}
		}

		public int RequiredScars
		{
			get
			{
				if (precepts.NullOrEmpty())
				{
					return 0;
				}
				if (requiredScarsCached == -1)
				{
					requiredScarsCached = 0;
					foreach (Precept precept in precepts)
					{
						if (precept.def.requiredScars > requiredScarsCached)
						{
							requiredScarsCached = precept.def.requiredScars;
						}
					}
				}
				return requiredScarsCached;
			}
		}

		public float BlindPawnChance
		{
			get
			{
				if (precepts.NullOrEmpty())
				{
					return 0f;
				}
				float num = 0f;
				int num2 = 0;
				foreach (Precept precept in precepts)
				{
					if (precept.def.blindPawnChance >= 0f)
					{
						num += precept.def.blindPawnChance;
						num2++;
					}
				}
				if (num2 <= 0)
				{
					return 0f;
				}
				return num / (float)num2;
			}
		}

		public string KeyDeityName
		{
			get
			{
				if (foundation is IdeoFoundation_Deity ideoFoundation_Deity && ideoFoundation_Deity.DeitiesListForReading.Any())
				{
					return ideoFoundation_Deity.DeitiesListForReading[0].name;
				}
				return null;
			}
		}

		public bool ObligationsActive
		{
			get
			{
				if (colonistBelieverCountCached == -1)
				{
					RecacheColonistBelieverCount();
				}
				if (colonistBelieverCountCached < MinBelieversToEnableObligations)
				{
					return Faction.OfPlayer.ideos.IsPrimary(this);
				}
				return true;
			}
		}

		public SoundDef SoundOngoingRitual
		{
			get
			{
				foreach (ThingStyleCategoryWithPriority thingStyleCategory in thingStyleCategories)
				{
					if (thingStyleCategory.category.soundOngoingRitual != null)
					{
						return thingStyleCategory.category.soundOngoingRitual;
					}
				}
				return SoundDefOf.RitualSustainer_Theist;
			}
		}

		public RitualVisualEffectDef RitualEffect
		{
			get
			{
				foreach (ThingStyleCategoryWithPriority thingStyleCategory in thingStyleCategories)
				{
					if (thingStyleCategory.category.ritualVisualEffectDef != null)
					{
						return thingStyleCategory.category.ritualVisualEffectDef;
					}
				}
				return RitualVisualEffectDefOf.Basic;
			}
		}

		public ThingDef RitualSeatDef
		{
			get
			{
				foreach (Precept item in PreceptsListForReading)
				{
					if (item is Precept_RitualSeat precept_RitualSeat)
					{
						return precept_RitualSeat.ThingDef;
					}
				}
				return null;
			}
		}

		public string WorshipRoomLabel
		{
			get
			{
				if (overriddenWorshipRoomLabel == null)
				{
					MemeDef structureMeme = StructureMeme;
					if (structureMeme != null)
					{
						return structureMeme.worshipRoomLabel;
					}
					return RoomRoleDefOf.WorshipRoom.label.CapitalizeFirst();
				}
				return overriddenWorshipRoomLabel;
			}
			set
			{
				overriddenWorshipRoomLabel = value;
			}
		}

		public string MemberNamePlural
		{
			get
			{
				if (memberNamePluralCached.NullOrEmpty())
				{
					memberNamePluralCached = Find.ActiveLanguageWorker.Pluralize(memberName);
				}
				return memberNamePluralCached;
			}
		}

		public bool Fluid
		{
			get
			{
				return fluid;
			}
			set
			{
				if (value && development == null)
				{
					development = new IdeoDevelopmentTracker(this);
				}
				fluid = value;
			}
		}

		public bool LikesHumanLeatherApparel
		{
			get
			{
				foreach (Precept precept in precepts)
				{
					if (precept.def.likesHumanLeatherApparel)
					{
						return true;
					}
				}
				return false;
			}
		}

		public int ColonistBelieverCountCached
		{
			get
			{
				if (colonistBelieverCountCached == -1)
				{
					return RecacheColonistBelieverCount();
				}
				return colonistBelieverCountCached;
			}
		}

		public bool IsWorkTypeConsideredDangerous(WorkTypeDef workType)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return false;
			}
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.opposedWorkTypes.Count; j++)
				{
					if (precepts[i].def.opposedWorkTypes[j] == workType)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void SortStyleCategories()
		{
			thingStyleCategories.SortBy((ThingStyleCategoryWithPriority x) => 0f - x.priority);
			RecachePossibleBuildables();
		}

		public Ideo()
		{
			style = new IdeoStyleTracker(this);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref overriddenWorshipRoomLabel, "overriddenWorshipRoomLabel");
			Scribe_Values.Look(ref createdFromNoExpansionGame, "createdFromNoExpansionGame", defaultValue: false);
			Scribe_Deep.Look(ref foundation, "foundation");
			Scribe_Values.Look(ref name, "name");
			Scribe_Values.Look(ref adjective, "adjective");
			Scribe_Values.Look(ref memberName, "memberName");
			Scribe_Values.Look(ref leaderTitleMale, "leaderTitleMale");
			Scribe_Values.Look(ref leaderTitleFemale, "leaderTitleFemale");
			Scribe_Values.Look(ref description, "description");
			Scribe_Values.Look(ref descriptionTemplate, "descriptionTemplate");
			Scribe_Values.Look(ref descriptionLocked, "descriptionLocked", defaultValue: false);
			Scribe_Values.Look(ref classicExtraMode, "classic", defaultValue: false);
			Scribe_Values.Look(ref classicMode, "hiddenIdeoMode", defaultValue: false);
			Scribe_Defs.Look(ref culture, "culture");
			Scribe_Defs.Look(ref iconDef, "iconDef");
			Scribe_Defs.Look(ref colorDef, "colorDef");
			Scribe_Values.Look(ref primaryFactionColor, "primaryFactionColor");
			Scribe_Collections.Look(ref memes, "memes", LookMode.Def);
			Scribe_Collections.Look(ref precepts, "precepts", LookMode.Deep);
			Scribe_Collections.Look(ref thingStyleCategories, "thingStyleCategories", LookMode.Deep);
			Scribe_Collections.Look(ref usedSymbols, "usedSymbols", LookMode.Value);
			Scribe_Collections.Look(ref usedSymbolPacks, "usedSymbolPacks", LookMode.Value);
			Scribe_Deep.Look(ref style, "style", this);
			Scribe_Values.Look(ref fluid, "fluid", defaultValue: false);
			Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
			Scribe_Values.Look(ref solid, "solid", defaultValue: false);
			if (!GameDataSaveLoader.IsSavingOrLoadingExternalIdeo)
			{
				Scribe_Values.Look(ref id, "id", 0);
				Scribe_Values.Look(ref relicsCollected, "relicsCollected", defaultValue: false);
				Scribe_Values.Look(ref initialPlayerIdeo, "initialPlayerIdeo", defaultValue: false);
				Scribe_Deep.Look(ref development, "development", this);
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (foundation != null)
				{
					foundation.ideo = this;
				}
				if (style == null)
				{
					style = new IdeoStyleTracker(this);
				}
				if (style.ideo == null)
				{
					style.ideo = this;
				}
				if (development != null && development.ideo == null)
				{
					development.ideo = this;
				}
				if (fluid && development == null)
				{
					development = new IdeoDevelopmentTracker(this);
				}
				if (memes.RemoveAll((MemeDef x) => x == null) != 0)
				{
					Log.Error("Some ideoligion memes were null after loading.");
				}
				if (precepts.RemoveAll((Precept x) => x == null || x.def == null || (x is Precept_Ritual precept_Ritual && precept_Ritual.behavior == null)) != 0)
				{
					Log.Error("Some ideoligion precepts were null after loading.");
				}
				if (thingStyleCategories == null)
				{
					thingStyleCategories = new List<ThingStyleCategoryWithPriority>();
				}
				if (thingStyleCategories.RemoveAll((ThingStyleCategoryWithPriority x) => x == null || x.category == null) != 0)
				{
					Log.Error("Some thing style categories were null after loading.");
				}
				if (culture == null)
				{
					culture = DefDatabase<CultureDef>.AllDefsListForReading.RandomElement();
					Log.Error("Ideoligion had null culture. Assigning random.");
				}
				if (usedSymbols == null)
				{
					usedSymbols = new List<string>();
				}
				if (usedSymbolPacks == null)
				{
					usedSymbolPacks = new List<string>();
				}
				for (int num = 0; num < precepts.Count; num++)
				{
					if (precepts[num] != null)
					{
						precepts[num].ideo = this;
					}
				}
				if (description == null)
				{
					RegenerateDescription();
				}
				if (createdFromNoExpansionGame && ModsConfig.IdeologyActive && iconDef == null)
				{
					SetIcon(IdeoFoundation.GetRandomIconDef(this), IdeoFoundation.GetRandomColorDef(this));
				}
			}
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (memberName.NullOrEmpty())
			{
				memberName = FactionDefOf.PlayerColony.basicMemberKind.label;
			}
			if (ModsConfig.IdeologyActive && !classicMode && RitualSeatDef == null && DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef x) => x.issue == IssueDefOf.IdeoRitualSeat).TryRandomElementByWeight((PreceptDef x) => x.selectionWeight, out var result))
			{
				AddPrecept(PreceptMaker.MakePrecept(result), init: true);
				Log.Warning("Adding missing ritual seat precept " + RitualSeatDef.LabelCap);
			}
			foreach (PreceptDef p in DefDatabase<PreceptDef>.AllDefsListForReading)
			{
				if ((p.takeNameFrom == null || precepts.Any((Precept _p) => _p.def == p.takeNameFrom)) && p.preceptClass.SameOrSubclassOf<Precept_Ritual>() && !p.visible && !precepts.Any((Precept o) => o.def.ritualPatternBase == p.ritualPatternBase) && (!Find.IdeoManager.classicMode || p.classic))
				{
					RitualPatternDef ritualPatternBase = p.ritualPatternBase;
					Precept precept = PreceptMaker.MakePrecept(p);
					AddPrecept(precept, init: true, null, ritualPatternBase);
					Log.Warning("A hidden ritual precept was missing, adding: " + precept.def.LabelCap);
				}
			}
			List<Precept> list = precepts.Where((Precept x) => x.def == PreceptDefOf.FuneralNoCorpse).ToList();
			if (list.Count > 1)
			{
				Log.Warning("Multiple FuneralNoCorpse precepts found, removing extras.");
				for (int num2 = 1; num2 < list.Count; num2++)
				{
					precepts.Remove(list[num2]);
				}
			}
			if (classicMode && createdFromNoExpansionGame)
			{
				classicMode = false;
			}
			RecachePrecepts();
		}

		public bool HasMeme(MemeDef meme)
		{
			return memes.Contains(meme);
		}

		public bool EnablesNeed(NeedDef def)
		{
			return cachedEnabledNeeds.Contains(def);
		}

		public bool DisablesNeed(NeedDef def)
		{
			return cachedDisabledNeeds.Contains(def);
		}

		public void IdeoTick()
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				precepts[i].Tick();
			}
			if (colonistBelieverCountCached == -1)
			{
				RecacheColonistBelieverCount();
			}
		}

		public void Notify_AddBedThoughts(Pawn pawn)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.comps.Count; j++)
				{
					precepts[i].def.comps[j].Notify_AddBedThoughts(pawn, precepts[i]);
				}
			}
		}

		public void Notify_MemberTookAction(HistoryEvent ev, bool canApplySelfTookThoughts)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.comps.Count; j++)
				{
					precepts[i].def.comps[j].Notify_MemberTookAction(ev, precepts[i], canApplySelfTookThoughts);
				}
			}
		}

		public void Notify_MemberKnows(HistoryEvent ev, Pawn member)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.comps.Count; j++)
				{
					precepts[i].def.comps[j].Notify_MemberWitnessedAction(ev, precepts[i], member);
				}
			}
		}

		public void Notify_MemberGuestStatusChanged(Pawn member)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				try
				{
					precepts[i].Notify_MemberGuestStatusChanged(member);
				}
				catch (Exception ex)
				{
					Log.Error("Error in Precept.Notify_MemberGuestStatusChanged(): " + ex);
				}
			}
		}

		public void Notify_MemberDied(Pawn member)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				try
				{
					precepts[i].Notify_MemberDied(member);
				}
				catch (Exception ex)
				{
					Log.Error("Error in Precept.Notify_MemberDied(): " + ex);
				}
			}
			if (Faction.OfPlayerSilentFail != null && member.Faction == Faction.OfPlayerSilentFail)
			{
				RecacheColonistBelieverCount();
			}
		}

		public void Notify_MemberLost(Pawn member, Map map)
		{
			if (member.IsColonist && map != null && member.workSettings != null && member.workSettings.WorkIsActive(WorkTypeDefOf.Warden))
			{
				bool flag = false;
				foreach (Pawn freeColonist in map.mapPawns.FreeColonists)
				{
					if (freeColonist != member && freeColonist.Ideo == this && freeColonist.workSettings.WorkIsActive(WorkTypeDefOf.Warden))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					foreach (Pawn item in map.mapPawns.PrisonersOfColonySpawned)
					{
						item.guest.Notify_WardensOfIdeoLost(this);
					}
				}
			}
			for (int i = 0; i < precepts.Count; i++)
			{
				try
				{
					precepts[i].Notify_MemberLost(member);
				}
				catch (Exception ex)
				{
					Log.Error("Error in Precept.Notify_MemberLost(): " + ex);
				}
			}
		}

		public void Notify_MemberGained(Pawn member)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				try
				{
					precepts[i].Notify_MemberGained(member);
				}
				catch (Exception ex)
				{
					Log.Error("Error in Precept.Notify_MemberGained(): " + ex);
				}
			}
		}

		public void Notify_MemberCorpseDestroyed(Pawn member)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				try
				{
					precepts[i].Notify_MemberCorpseDestroyed(member);
				}
				catch (Exception ex)
				{
					Log.Error("Error in Precept.Notify_MemberCorpseDestroyed(): " + ex);
				}
			}
		}

		public void Notify_MemberGenerated(Pawn member, bool newborn, bool ignoreApparel = false)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				try
				{
					precepts[i].Notify_MemberGenerated(member, newborn, ignoreApparel);
				}
				catch (Exception ex)
				{
					Log.Error("Error in Precept.Notify_MemberGenerated(): " + ex);
				}
			}
		}

		public void Notify_GameStarted()
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				try
				{
					precepts[i].Notify_GameStarted();
				}
				catch (Exception ex)
				{
					Log.Error("Error in Precept.Notify_GameStarted(): " + ex);
				}
			}
		}

		public void Notify_RelicSeenByPlayer(Thing relic)
		{
			if (!AllRelicsNewlyCollected())
			{
				return;
			}
			tmpPlayerIdeoFollowers.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsAndWorld_Alive)
			{
				if (item.Ideo == this && item.needs != null && item.needs.mood != null && item.needs.mood.thoughts != null && item.needs.mood.thoughts.memories != null)
				{
					item.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RelicsCollected);
					if (item.Faction == Faction.OfPlayer)
					{
						tmpPlayerIdeoFollowers.Add(item);
					}
				}
			}
			if (tmpPlayerIdeoFollowers.Count > 0)
			{
				TaggedString taggedString = (classicMode ? "LetterTextRelicsCollectedClassic".Translate() : "LetterTextRelicsCollected".Translate(this));
				ChoiceLetter choiceLetter = LetterMaker.MakeLetter("LetterLabelRelicsCollected".Translate() + ": " + name.ApplyTag(this).Resolve(), taggedString + ":\n\n" + tmpPlayerIdeoFollowers.Select((Pawn p) => p.LabelNoCountColored.Resolve()).ToList().ToLineList("- "), LetterDefOf.PositiveEvent, tmpPlayerIdeoFollowers);
				Find.LetterStack.ReceiveLetter(choiceLetter);
				tmpPlayerIdeoFollowers.Clear();
			}
			relicsCollected = true;
		}

		public void Notify_MemberGainedByConversion()
		{
			if (Fluid)
			{
				development.Notify_MemberGainedByConversion();
			}
		}

		private bool AllRelicsNewlyCollected()
		{
			if (relicsCollected)
			{
				return false;
			}
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i] is Precept_Relic { RelicInPlayerPossession: false })
				{
					return false;
				}
			}
			return true;
		}

		public bool MemberWillingToDo(HistoryEvent ev)
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.comps.Count; j++)
				{
					if (!precepts[i].def.comps[j].MemberWillingToDo(ev))
					{
						return false;
					}
				}
			}
			return true;
		}

		public Pair<Precept, Precept> FirstIncompatiblePreceptPair()
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts.Count; j++)
				{
					if (precepts[i] != precepts[j] && !precepts[i].CompatibleWith(precepts[j]))
					{
						return new Pair<Precept, Precept>(precepts[i], precepts[j]);
					}
				}
			}
			return default(Pair<Precept, Precept>);
		}

		public Precept FirstPreceptWithWarning()
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i].GetPlayerWarning(out var _, out var _))
				{
					return precepts[i];
				}
			}
			return null;
		}

		public Tuple<Precept_Ritual, List<string>> FirstRitualMissingTarget()
		{
			foreach (Precept precept in precepts)
			{
				if (precept is Precept_Ritual { obligationTargetFilter: not null } precept_Ritual)
				{
					List<string> list = precept_Ritual.obligationTargetFilter.MissingTargetBuilding(this);
					if (!list.NullOrEmpty())
					{
						return new Tuple<Precept_Ritual, List<string>>(precept_Ritual, list);
					}
				}
			}
			return null;
		}

		public Precept_Building FirstConsumableBuildingMissingRitual()
		{
			foreach (Precept precept in precepts)
			{
				if (precept is Precept_Building precept_Building && precept_Building.ThingDef.ritualFocus != null && precept_Building.ThingDef.ritualFocus.consumable && !HasRequiredRitualForBuilding(precept_Building))
				{
					return precept_Building;
				}
			}
			return null;
		}

		private bool HasRequiredRitualForBuilding(Precept_Building building)
		{
			if (cachedRitualTargetFilters == null)
			{
				cachedRitualTargetFilters = new List<RitualObligationTargetFilter>();
				foreach (Precept precept in precepts)
				{
					if (precept is Precept_Ritual { obligationTargetFilter: not null } precept_Ritual)
					{
						cachedRitualTargetFilters.Add(precept_Ritual.obligationTargetFilter.def.GetInstance());
					}
				}
			}
			foreach (RitualObligationTargetFilter cachedRitualTargetFilter in cachedRitualTargetFilters)
			{
				if (!cachedRitualTargetFilter.def.thingDefs.NullOrEmpty() && cachedRitualTargetFilter.def.thingDefs.Contains(building.ThingDef))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsPreferredXenotype(Pawn pawn)
		{
			if (!PreferredXenotypes.Any() && !PreferredCustomXenotypes.Any())
			{
				return false;
			}
			if (pawn.genes == null)
			{
				return false;
			}
			if (pawn.genes.CustomXenotype != null)
			{
				foreach (CustomXenotype preferredCustomXenotype in PreferredCustomXenotypes)
				{
					if (GeneUtility.PawnIsCustomXenotype(pawn, preferredCustomXenotype))
					{
						return true;
					}
				}
				return false;
			}
			return PreferredXenotypes.Contains(pawn.genes.Xenotype);
		}

		public void ClearPrecepts()
		{
			precepts.Clear();
			RecachePrecepts();
			anyPreceptEdited = false;
		}

		public void RecachePrecepts()
		{
			allPreceptDefs.Clear();
			foreach (Precept precept in precepts)
			{
				allPreceptDefs.Add(precept.def);
			}
			requiredScarsCached = -1;
			cachedVeneratedAnimals = null;
			cachedRitualTargetFilters = null;
			cachedPreferredXenotypes = null;
			cachedPreferredCustomXenotypes = null;
			RecachePossibleSituationalThoughts();
			RecachePossibleGoodwillSituations();
			RecachePossibleBuildings();
			RecachePossibleBuildables();
			RecachePossibleRoles();
			RecachePossibleMentalBreaks();
			RecacheNeeds();
			warnPlayerOnDesignateChopTree = false;
			warnPlayerOnDesignateMine = false;
			foreach (Precept precept2 in precepts)
			{
				if (precept2.def.warnPlayerOnDesignateChopTree)
				{
					warnPlayerOnDesignateChopTree = true;
				}
				if (precept2.def.warnPlayerOnDesignateMine)
				{
					warnPlayerOnDesignateMine = true;
				}
			}
			foreach (Precept precept3 in precepts)
			{
				precept3.Notify_RecachedPrecepts();
			}
			currentCacheId++;
		}

		public void AddPrecept(Precept precept, bool init = false, FactionDef generatingFor = null, RitualPatternDef fillWith = null)
		{
			if (precept == null)
			{
				Log.Error("Tried to add a null PreceptDef.");
				return;
			}
			if (precepts.Contains(precept))
			{
				Log.Error("Tried to add the same PreceptDef twice.");
				return;
			}
			precept.ideo = this;
			precepts.Add(precept);
			precepts.SortBy((Precept x) => 0f - GetPreceptImpact(x), (Precept x) => -x.def.displayOrderInImpact, (Precept x) => x.def.defName);
			if (init)
			{
				precept.Init(this, generatingFor);
				if (fillWith != null && precept is Precept_Ritual precept_Ritual)
				{
					fillWith.Fill(precept_Ritual);
					precept_Ritual.RegenerateName();
				}
			}
			RecachePrecepts();
			if (precept.def.alsoAdds != null)
			{
				Precept precept2 = precepts.FirstOrDefault((Precept p) => p.def == precept.def.alsoAdds);
				if (precept2 != null)
				{
					precept2.RegenerateName();
					return;
				}
				Precept precept3 = PreceptMaker.MakePrecept(precept.def.alsoAdds);
				AddPrecept(precept3, init: true, generatingFor, (fillWith != null) ? precept3.def.ritualPatternBase : null);
			}
		}

		private float GetPreceptImpact(Precept precept)
		{
			if (!precept.SortByImpact)
			{
				return 0f;
			}
			return (float)precept.def.impact + 1f;
		}

		public bool PreceptIsRequired(PreceptDef precept)
		{
			return GetMemeThatRequiresPrecept(precept) != null;
		}

		public MemeDef GetMemeThatRequiresPrecept(PreceptDef precept)
		{
			for (int i = 0; i < memes.Count; i++)
			{
				if (memes[i].requireOne != null && memes[i].requireOne.Any((List<PreceptDef> x) => x.Contains(precept)))
				{
					return memes[i];
				}
			}
			return null;
		}

		public AcceptanceReport CanAddPreceptAllFactions(PreceptDef preceptDef)
		{
			AcceptanceReport result = foundation.CanAdd(preceptDef, checkDuplicates: false);
			if (!result.Accepted)
			{
				return result;
			}
			if (Find.World != null)
			{
				foreach (Faction allFaction in Find.FactionManager.AllFactions)
				{
					if (allFaction.def.humanlikeFaction && allFaction.ideos != null && (allFaction.ideos.IsPrimary(this) || allFaction.ideos.IsMinor(this)) && !foundation.CanAddForFaction(preceptDef, allFaction.def, null, checkDuplicates: false))
					{
						return false;
					}
				}
			}
			return true;
		}

		public void RemovePrecept(Precept precept, bool replacing = false)
		{
			precepts.Remove(precept);
			for (int num = precepts.Count - 1; num >= 0; num--)
			{
				if (precepts[num].def.takeNameFrom == precept.def)
				{
					RemovePrecept(precepts[num]);
				}
			}
			if (precept is Precept_Role precept_Role)
			{
				foreach (Pawn item in precept_Role.ChosenPawns())
				{
					item.abilities?.Notify_TemporaryAbilitiesChanged();
				}
			}
			if (!replacing && precept.def.defaultSelectionWeight <= 0f && precept.def.issue.HasDefaultPrecept && DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef x) => x.issue == precept.def.issue && CanAddPreceptAllFactions(x).Accepted && x.defaultSelectionWeight > 0f).TryRandomElementByWeight((PreceptDef x) => x.defaultSelectionWeight, out var result))
			{
				AddPrecept(PreceptMaker.MakePrecept(result), init: true);
			}
			else
			{
				RecachePrecepts();
			}
		}

		public void SetIcon(IdeoIconDef iconDef, ColorDef colorDef, bool clearPrimaryFactionColor = false)
		{
			this.iconDef = iconDef;
			this.colorDef = colorDef;
			icon = null;
			if (clearPrimaryFactionColor)
			{
				primaryFactionColor = null;
			}
		}

		public void DrawIcon(Rect rect)
		{
			Color color = GUI.color;
			GUI.color = Color;
			GUI.DrawTexture(rect, Icon);
			GUI.color = color;
		}

		public List<Precept> GetAllPreceptsAllowingSituationalThought(ThoughtDef def)
		{
			tmpThoughtPrecepts.Clear();
			if (!cachedPossibleSituationalThoughts.Contains(def))
			{
				return tmpThoughtPrecepts;
			}
			for (int i = 0; i < precepts.Count; i++)
			{
				List<PreceptComp> comps = precepts[i].def.comps;
				for (int j = 0; j < comps.Count; j++)
				{
					if (comps[j] is PreceptComp_SituationalThought preceptComp_SituationalThought && preceptComp_SituationalThought.thought == def)
					{
						tmpThoughtPrecepts.Add(precepts[i]);
						break;
					}
				}
			}
			return tmpThoughtPrecepts;
		}

		public Precept GetFirstPreceptAllowingSituationalThought(ThoughtDef def)
		{
			if (!cachedPossibleSituationalThoughts.Contains(def))
			{
				return null;
			}
			for (int i = 0; i < precepts.Count; i++)
			{
				List<PreceptComp> comps = precepts[i].def.comps;
				for (int j = 0; j < comps.Count; j++)
				{
					if (comps[j] is PreceptComp_SituationalThought preceptComp_SituationalThought && preceptComp_SituationalThought.thought == def)
					{
						return precepts[i];
					}
				}
			}
			return null;
		}

		public bool HasPreceptForBuilding(ThingDef buildingDef)
		{
			return cachedPossibleBuildings.Any((Precept_Building b) => b.ThingDef == buildingDef);
		}

		public bool MembersCanBuild(Thing thing)
		{
			if (classicMode)
			{
				return true;
			}
			BuildableDef buildableDef = thing.def.entityDefToBuild ?? thing.def;
			if (buildableDef.canGenerateDefaultDesignator)
			{
				return true;
			}
			if (thing.StyleSourcePrecept?.ideo == this || cachedPossibleBuildables.Contains(buildableDef))
			{
				return true;
			}
			if (buildableDef is ThingDef buildingDef && HasPreceptForBuilding(buildingDef))
			{
				return true;
			}
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i].def.willingToConstructOtherIdeoBuildables)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsVeneratedAnimal(Pawn pawn)
		{
			return IsVeneratedAnimal(pawn.def);
		}

		public bool IsVeneratedAnimal(ThingDef thingDef)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return false;
			}
			return VeneratedAnimals.Contains(thingDef);
		}

		public IdeoWeaponDisposition GetDispositionForWeapon(ThingDef td)
		{
			if (!td.IsWeapon || td.weaponClasses.NullOrEmpty())
			{
				return IdeoWeaponDisposition.None;
			}
			foreach (Precept precept in precepts)
			{
				if (precept is Precept_Weapon precept_Weapon)
				{
					IdeoWeaponDisposition dispositionForWeapon = precept_Weapon.GetDispositionForWeapon(td);
					if (dispositionForWeapon != IdeoWeaponDisposition.None)
					{
						return dispositionForWeapon;
					}
				}
			}
			return IdeoWeaponDisposition.None;
		}

		public T GetFirstPreceptOfType<T>() where T : class
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i] is T result)
				{
					return result;
				}
			}
			return null;
		}

		public IEnumerable<T> GetAllPreceptsOfType<T>() where T : class
		{
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i] is T val)
				{
					yield return val;
				}
			}
		}

		public bool HasPrecept(PreceptDef preceptDef)
		{
			return allPreceptDefs.Contains(preceptDef);
		}

		public int GetPreceptCountOfDef(PreceptDef preceptDef)
		{
			if (preceptDef == null || !allPreceptDefs.Contains(preceptDef))
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i].def == preceptDef)
				{
					num++;
				}
			}
			return num;
		}

		public Precept GetPrecept(PreceptDef preceptDef)
		{
			if (preceptDef == null || !allPreceptDefs.Contains(preceptDef))
			{
				return null;
			}
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i].def == preceptDef)
				{
					return precepts[i];
				}
			}
			return null;
		}

		public void SortMemesInDisplayOrder()
		{
			memes.SortBy((MemeDef x) => x.category == MemeCategory.Normal, (MemeDef x) => x.index);
		}

		private void RecachePossibleSituationalThoughts()
		{
			cachedPossibleSituationalThoughts.Clear();
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.comps.Count; j++)
				{
					if (precepts[i].def.comps[j] is PreceptComp_SituationalThought preceptComp_SituationalThought && !cachedPossibleSituationalThoughts.Contains(preceptComp_SituationalThought.thought))
					{
						cachedPossibleSituationalThoughts.Add(preceptComp_SituationalThought.thought);
					}
				}
			}
		}

		private void RecachePossibleGoodwillSituations()
		{
			cachedPossibleGoodwillSituations.Clear();
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.comps.Count; j++)
				{
					if (precepts[i].def.comps[j] is PreceptComp_GoodwillSituation preceptComp_GoodwillSituation && !cachedPossibleGoodwillSituations.Contains(preceptComp_GoodwillSituation.goodwillSituation))
					{
						cachedPossibleGoodwillSituations.Add(preceptComp_GoodwillSituation.goodwillSituation);
					}
				}
			}
		}

		public void RecachePossibleBuildings()
		{
			cachedPossibleBuildings.Clear();
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i] is Precept_Building item)
				{
					cachedPossibleBuildings.Add(item);
				}
			}
		}

		public void RecachePossibleBuildables()
		{
			cachedPossibleBuildables.Clear();
			foreach (MemeDef meme in memes)
			{
				cachedPossibleBuildables.AddRange(meme.AllDesignatorBuildables);
			}
			foreach (ThingStyleCategoryWithPriority thingStyleCategory in thingStyleCategories)
			{
				cachedPossibleBuildables.AddRange(thingStyleCategory.category.AllDesignatorBuildables);
			}
			foreach (Precept precept in precepts)
			{
				if (precept is Precept_RitualSeat precept_RitualSeat)
				{
					cachedPossibleBuildables.Add(precept_RitualSeat.ThingDef);
				}
			}
		}

		public void RecachePossibleMentalBreaks()
		{
			cachedPossibleMentalBreaks.Clear();
			for (int i = 0; i < precepts.Count; i++)
			{
				for (int j = 0; j < precepts[i].def.comps.Count; j++)
				{
					if (precepts[i].def.comps[j] is PreceptComp_MentalBreak preceptComp_MentalBreak)
					{
						cachedPossibleMentalBreaks.Add(preceptComp_MentalBreak.mentalBreakDef);
					}
				}
			}
		}

		public void RecachePossibleRoles()
		{
			cachedPossibleRoles.Clear();
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i] is Precept_Role item)
				{
					cachedPossibleRoles.Add(item);
				}
			}
		}

		public void RecacheNeeds()
		{
			cachedEnabledNeeds.Clear();
			cachedDisabledNeeds.Clear();
			for (int i = 0; i < precepts.Count; i++)
			{
				Precept precept = precepts[i];
				if (!precept.def.enablesNeeds.NullOrEmpty())
				{
					cachedEnabledNeeds.AddRange(precept.def.enablesNeeds);
				}
				if (!precept.def.disablesNeeds.NullOrEmpty())
				{
					cachedDisabledNeeds.AddRange(precept.def.disablesNeeds);
				}
			}
		}

		public void RegenerateAllPreceptNames()
		{
			foreach (Precept precept in precepts)
			{
				if (precept.UsesGeneratedName && !precept.nameLocked)
				{
					precept.RegenerateName();
				}
				precept.ClearTipCache();
			}
		}

		public void RegenerateDescription(bool force = false)
		{
			if (description == null || !descriptionLocked)
			{
				IdeoDescriptionResult newDescription = GetNewDescription(force);
				description = newDescription.text;
				descriptionTemplate = newDescription.template;
			}
		}

		public IdeoDescriptionResult GetNewDescription(bool force = false)
		{
			List<IdeoDescriptionMaker.PatternEntry> list = (from entry in memes.Where((MemeDef meme) => meme.descriptionMaker?.patterns != null).SelectMany((MemeDef meme) => meme.descriptionMaker.patterns)
				group entry by entry.def into grp
				select grp.MaxBy((IdeoDescriptionMaker.PatternEntry entry) => entry.weight)).ToList();
			if (!list.Any())
			{
				if (ModsConfig.IdeologyActive && memes.Any())
				{
					Log.Error("Memes provided no description patterns");
				}
				return new IdeoDescriptionResult
				{
					text = string.Empty
				};
			}
			IdeoStoryPatternDef def = list.RandomElementByWeight((IdeoDescriptionMaker.PatternEntry entry) => entry.weight).def;
			return IdeoDescriptionUtility.ResolveDescription(this, def, force);
		}

		public int RecacheColonistBelieverCount()
		{
			if (Current.ProgramState != ProgramState.Playing || Find.WindowStack.IsOpen<Dialog_ConfigureIdeo>())
			{
				return 0;
			}
			int num = colonistBelieverCountCached;
			int num2 = 0;
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep)
			{
				if (item.Ideo == this && !item.IsSlave && !item.IsQuestLodger())
				{
					num2++;
				}
			}
			colonistBelieverCountCached = num2;
			foreach (Precept precept in precepts)
			{
				if (precept is Precept_Role precept_Role)
				{
					precept_Role.RecacheActivity();
				}
			}
			if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos.IsMinor(this))
			{
				if (colonistBelieverCountCached < MinBelieversToEnableObligations && num != -1 && num >= MinBelieversToEnableObligations)
				{
					Find.LetterStack.ReceiveLetter("LetterTitleObligationsDeactivated".Translate(name), "LetterTitleObligationsDeactivatedTooFewBelievers".Translate(this.Named("IDEO"), MinBelieversToEnableObligations), LetterDefOf.NeutralEvent);
				}
				if (colonistBelieverCountCached >= MinBelieversToEnableObligations && num != -1 && num < MinBelieversToEnableObligations)
				{
					Find.LetterStack.ReceiveLetter("LetterTitleObligationsActivated".Translate(name), "LetterTitleObligationsActivatedEnoughBelievers".Translate(this.Named("IDEO"), MinBelieversToEnableObligations), LetterDefOf.NeutralEvent);
				}
			}
			return num2;
		}

		public Precept_Role GetRole(Pawn p)
		{
			foreach (Precept_Role item in RolesListForReading)
			{
				if (item.IsAssigned(p))
				{
					return item;
				}
			}
			return null;
		}

		public StyleCategoryPair GetStyleAndCategoryFor(ThingDef thingDef)
		{
			Precept precept = null;
			for (int i = 0; i < precepts.Count; i++)
			{
				if (precepts[i] is Precept_ThingDef precept_ThingDef && precept_ThingDef.ThingDef == thingDef)
				{
					precept = precept_ThingDef;
					break;
				}
			}
			return style.StyleForThingDef(thingDef, precept);
		}

		public ThingStyleDef GetStyleFor(ThingDef thingDef)
		{
			return GetStyleAndCategoryFor(thingDef)?.styleDef;
		}

		public StyleCategoryDef GetStyleCategoryFor(ThingDef thingDef)
		{
			return style.StyleForThingDef(thingDef)?.category;
		}

		public void Notify_MemberChangedFaction(Pawn p, Faction oldFaction, Faction newFaction)
		{
			foreach (Precept item in PreceptsListForReading)
			{
				item.Notify_MemberChangedFaction(p, oldFaction, newFaction);
			}
			if (oldFaction == Faction.OfPlayer || newFaction == Faction.OfPlayer)
			{
				RecacheColonistBelieverCount();
			}
		}

		public void Notify_NotPrimaryAnymore(Ideo newIdeo)
		{
			foreach (Precept precept in precepts)
			{
				precept.Notify_IdeoNotPrimaryAnymore(newIdeo);
			}
		}

		public bool HasMaxPreceptsForIssue(IssueDef issue)
		{
			if (!issue.allowMultiplePrecepts)
			{
				for (int i = 0; i < precepts.Count; i++)
				{
					if (precepts[i].def.issue == issue)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void MakeMemeberNamePluralDirty()
		{
			memberNamePluralCached = null;
		}

		public string GetUniqueLoadID()
		{
			return "Ideo_" + id;
		}

		public override string ToString()
		{
			if (name != null)
			{
				return name;
			}
			return GetUniqueLoadID();
		}

		public void CopyTo(Ideo ideo)
		{
			ideo.foundation = IdeoGenerator.MakeFoundation(foundation.def);
			ideo.foundation.ideo = ideo;
			foundation.CopyTo(ideo.foundation);
			ideo.culture = culture;
			ideo.memes.Clear();
			ideo.memes.AddRange(memes);
			ideo.ClearPrecepts();
			foreach (Precept precept2 in precepts)
			{
				Precept precept = PreceptMaker.MakePrecept(precept2.def);
				precept2.CopyTo(precept);
				precept.ideo = ideo;
				ideo.precepts.Add(precept);
			}
			ideo.RecachePrecepts();
			ideo.usedSymbols.Clear();
			ideo.usedSymbols.AddRange(ideo.usedSymbols);
			style.CopyTo(ideo.style);
			ideo.createdFromNoExpansionGame = createdFromNoExpansionGame;
			ideo.fluid = fluid;
			if (ideo.fluid)
			{
				ideo.development = new IdeoDevelopmentTracker(ideo);
				development.CopyTo(ideo.development);
			}
			ideo.initialPlayerIdeo = initialPlayerIdeo;
			ideo.name = name;
			ideo.adjective = adjective;
			ideo.memberName = memberName;
			ideo.iconDef = iconDef;
			ideo.colorDef = colorDef;
			ideo.primaryFactionColor = primaryFactionColor;
			ideo.leaderTitleMale = leaderTitleMale;
			ideo.leaderTitleFemale = leaderTitleFemale;
			ideo.usedSymbolPacks.Clear();
			ideo.usedSymbolPacks.AddRange(usedSymbolPacks);
			ideo.thingStyleCategories.Clear();
			ideo.thingStyleCategories.AddRange(thingStyleCategories);
			ideo.description = description;
			ideo.descriptionTemplate = descriptionTemplate;
			ideo.descriptionLocked = descriptionLocked;
			ideo.overriddenWorshipRoomLabel = overriddenWorshipRoomLabel;
			ideo.relicsCollected = relicsCollected;
		}
	}
}
