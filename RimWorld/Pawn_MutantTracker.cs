using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Pawn_MutantTracker : IExposable, IVerbOwner
{
	public struct ThinkTrees
	{
		public ThinkTreeDef main;

		public ThinkTreeDef constant;
	}

	private const float RottenDropCorpseBileMTBHours = 5f;

	private static readonly SimpleCurve DropBloodMTBSecondsFromHealthPctCurve = new SimpleCurve
	{
		new CurvePoint(0f, 5f),
		new CurvePoint(1f, 120f)
	};

	private Pawn pawn;

	private MutantDef def;

	public RotStage rotStage;

	private bool hasTurned;

	private Hediff mutantHediff;

	public VerbTracker verbTracker;

	private Faction originalFaction;

	private Ideo originalIdeo;

	private List<Ability> abilities;

	public MutantDef Def => def;

	public bool HasTurned => hasTurned;

	public bool IsPassive => def.passive;

	public Hediff Hediff => mutantHediff;

	public List<Verb> AllVerbs => verbTracker.AllVerbs;

	public VerbTracker VerbTracker => verbTracker;

	public List<VerbProperties> VerbProperties => Def.verbs;

	public List<Tool> Tools => Def.tools;

	public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

	public Thing ConstantCaster => pawn;

	public List<Ability> AllAbilitiesForReading
	{
		get
		{
			if (abilities == null)
			{
				abilities = new List<Ability>();
				foreach (AbilityDef ability in def.abilities)
				{
					abilities.Add(AbilityUtility.MakeAbility(ability, pawn));
				}
			}
			return abilities;
		}
	}

	public string UniqueVerbOwnerID()
	{
		return pawn.GetUniqueLoadID() + "_mutant";
	}

	public bool VerbsStillUsableBy(Pawn p)
	{
		return true;
	}

	public Pawn_MutantTracker(Pawn pawn, MutantDef def, RotStage rotStage)
	{
		this.pawn = pawn;
		this.def = def;
		this.rotStage = rotStage;
		verbTracker = new VerbTracker(this);
		originalFaction = pawn.HomeFaction;
		if (pawn.RaceProps.Humanlike)
		{
			originalIdeo = pawn.Ideo ?? pawn.Faction?.ideos?.PrimaryIdeo ?? Find.IdeoManager.IdeosListForReading.RandomElement();
			originalFaction = originalFaction ?? Find.FactionManager.AllFactionsVisible.FirstOrDefault((Faction f) => f.def == FactionDefOf.OutlanderCivil) ?? Find.FactionManager.AllFactionsVisible.FirstOrDefault((Faction f) => f.def == FactionDefOf.OutlanderRough) ?? Find.FactionManager.AllFactionsListForReading.Where((Faction f) => !f.IsPlayer && f.def.humanlikeFaction && !f.Hidden && !f.temporary).RandomElement();
		}
	}

	public Pawn_MutantTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public ThinkTrees GetThinkTrees()
	{
		return new ThinkTrees
		{
			main = def.thinkTree,
			constant = def.thinkTreeConstant
		};
	}

	public void MutantTrackerTick()
	{
		verbTracker.VerbsTick();
	}

	public void MutantTrackerTickInterval(int delta)
	{
		if (!pawn.Drawer.renderer.HasAnimation && Def.standingAnimation != null)
		{
			pawn.Drawer.renderer.SetAnimation(Def.standingAnimation);
		}
		if (pawn.Spawned && pawn.GetPosture() == PawnPosture.Standing)
		{
			if (rotStage == RotStage.Rotting && Rand.MTBEventOccurs(5f, 2500f, 1f))
			{
				FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, ThingDefOf.Filth_CorpseBile, pawn.LabelIndefinite());
			}
			float summaryHealthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
			if (!Def.canBleed && Def.bloodDef != null && rotStage != RotStage.Dessicated && summaryHealthPercent < 1f && Rand.MTBEventOccurs(DropBloodMTBSecondsFromHealthPctCurve.Evaluate(summaryHealthPercent), 60f, 1f))
			{
				pawn.health.DropBloodFilth();
			}
		}
		if (Def.hediffGivers != null && pawn.IsHashIntervalTick(60, delta))
		{
			for (int i = 0; i < Def.hediffGivers.Count; i++)
			{
				Def.hediffGivers[i].OnIntervalPassed(pawn, mutantHediff);
			}
		}
	}

	public void Turn(bool clearLord = false)
	{
		if (hasTurned)
		{
			Log.Error("Tried to turn mutant " + pawn?.ToString() + " again");
			return;
		}
		hasTurned = true;
		if (def.hediff != null)
		{
			mutantHediff = pawn.health.GetOrAddHediff(def.hediff);
		}
		if (!def.givesHediffs.NullOrEmpty())
		{
			HealthUtility.AddStartingHediffs(pawn, def.givesHediffs);
		}
		if (def.terminatePregnancy)
		{
			PregnancyUtility.ForceEndPregnancy(pawn, preventLetter: true);
		}
		if (pawn.caller == null)
		{
			pawn.caller = new Pawn_CallTracker(pawn);
		}
		pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		pawn.Notify_DisabledWorkTypesChanged();
		if (def.disablesIdeo)
		{
			pawn.ideo?.SetIdeo(null);
		}
		pawn.guest?.SetGuestStatus(null);
		if (pawn.royalty != null)
		{
			pawn.royalty.Notify_PawnKilled();
			pawn.royalty.UpdateAvailableAbilities();
		}
		pawn.playerSettings?.ResetMedicalCare();
		if (pawn.Spawned)
		{
			pawn.Map.attackTargetsCache.UpdateTarget(pawn);
		}
		if (clearLord)
		{
			pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.Killed);
		}
		if (!Def.passive && pawn.playerSettings != null)
		{
			pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
		}
		pawn.ownership.UnclaimAll();
		if (pawn.Faction != null && pawn.Faction.IsPlayer)
		{
			BillUtility.Notify_ColonistUnavailable(pawn);
		}
		QuestUtility.SendQuestTargetSignals(pawn.questTags, "BecameMutant", pawn.Named("SUBJECT"));
		Find.QuestManager.Notify_PawnKilled(pawn, null);
		if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(pawn))
		{
			Find.History.Notify_MechanitorDied();
		}
		if (Def.relativeTurnedThought != null)
		{
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
			{
				if (item.needs != null && item.RaceProps.IsFlesh && item.needs.mood != null && (item.relations.OpinionOf(pawn) >= 20 || item.relations.FamilyByBlood.Contains(pawn)))
				{
					item.needs.mood.thoughts.memories.TryGainMemory(Def.relativeTurnedThought, pawn);
				}
			}
		}
		pawn.genes?.Reset();
		pawn.abilities?.Notify_TemporaryAbilitiesChanged();
		if (pawn.outfits != null)
		{
			pawn.outfits.CurrentApparelPolicy = null;
		}
		if (pawn.foodRestriction != null)
		{
			pawn.foodRestriction.CurrentFoodPolicy = null;
		}
		if (pawn.drugs != null)
		{
			pawn.drugs.CurrentPolicy = null;
		}
		if (pawn.reading != null)
		{
			pawn.reading.CurrentPolicy = null;
		}
		HandleEquipment();
		ResolveGraphics();
		pawn.Drawer.renderer.SetAnimation(Def.standingAnimation);
		if (Current.ProgramState == ProgramState.Playing)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
		if (def.codexEntry != null)
		{
			Find.EntityCodex.SetDiscovered(def.codexEntry, pawn.def, pawn);
		}
		if (def.clearsEgo)
		{
			pawn.everLostEgo = true;
		}
	}

	public void Revert(bool beingKilled = false)
	{
		if (!hasTurned)
		{
			Log.Error("Tried to revert mutant " + pawn?.ToString() + " who hasn't turned");
			return;
		}
		hasTurned = false;
		pawn.mutant = null;
		if (mutantHediff != null)
		{
			HediffComp_DisappearsAndKills hediffComp_DisappearsAndKills = mutantHediff.TryGetComp<HediffComp_DisappearsAndKills>();
			if (hediffComp_DisappearsAndKills != null)
			{
				hediffComp_DisappearsAndKills.disabled = true;
			}
			pawn.health.RemoveHediff(mutantHediff);
		}
		foreach (StartingHediff givesHediff in def.givesHediffs)
		{
			if (pawn.health.hediffSet.TryGetHediff(givesHediff.def, out var hediff))
			{
				pawn.health.RemoveHediff(hediff);
			}
		}
		pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		pawn.Notify_DisabledWorkTypesChanged();
		if (pawn.Faction != originalFaction)
		{
			pawn.SetFaction(originalFaction);
		}
		pawn.ideo?.SetIdeo(originalIdeo);
		if (pawn.royalty != null)
		{
			if (!beingKilled)
			{
				pawn.royalty.Notify_Resurrected();
			}
			pawn.royalty.UpdateAvailableAbilities();
		}
		pawn.Drawer.renderer.SetAnimation(null);
		abilities = null;
		pawn.abilities?.Notify_TemporaryAbilitiesChanged();
		pawn.genes?.Reset();
		pawn.Drawer.renderer.SetAllGraphicsDirty();
		Find.ColonistBar.MarkColonistsDirty();
	}

	private void ResolveGraphics()
	{
		if (!Def.forcedHeadTypes.NullOrEmpty())
		{
			pawn.story.TryGetRandomHeadFromSet(Def.forcedHeadTypes);
		}
		if (Def.hairTagFilter != null)
		{
			pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(pawn);
		}
		if (Def.beardTagFilter != null)
		{
			pawn.style.beardDef = PawnStyleItemChooser.RandomBeardFor(pawn);
		}
		if (Def.hairColorOverride.HasValue)
		{
			pawn.story.HairColor = Def.hairColorOverride.Value;
		}
		pawn.Drawer.renderer.SetAllGraphicsDirty();
	}

	private void HandleEquipment()
	{
		if (pawn.equipment?.Primary != null)
		{
			pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out var _, pawn.PositionHeld);
		}
		if (pawn.apparel == null)
		{
			return;
		}
		if (def.disableApparel)
		{
			if (pawn.MapHeld != null)
			{
				pawn.apparel.DropAll(pawn.Position);
			}
			else
			{
				pawn.apparel.DestroyAll();
			}
		}
		if (!def.isConsideredCorpse)
		{
			return;
		}
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			if (item.def.apparel.careIfWornByCorpse)
			{
				item.WornByCorpse = true;
			}
		}
	}

	public bool HediffGiversCanGive(HediffDef hediffDef)
	{
		if (Def.preventIllnesses && hediffDef.chronic)
		{
			return false;
		}
		List<HediffDef> removesHediffs = Def.removesHediffs;
		if (removesHediffs != null && removesHediffs.Contains(hediffDef))
		{
			return false;
		}
		return true;
	}

	public void Notify_Downed()
	{
		pawn.Drawer.renderer.SetAnimation(null);
	}

	public void Notify_Died(Corpse corpse, DamageInfo? dinfo, Hediff hediff = null)
	{
		if (def.isConsideredCorpse)
		{
			if (rotStage == RotStage.Fresh)
			{
				rotStage = RotStage.Rotting;
			}
			(corpse?.GetComp<CompRottable>())?.RotImmediately(rotStage);
		}
		if (pawn.Faction == Faction.OfPlayer && def.deathLetter != null)
		{
			TaggedString label = "Death".Translate(pawn.LabelShortCap) + ": " + pawn.LabelShortCap;
			TaggedString text = def.deathLetter + " " + HealthUtility.GetDiedLetterText(pawn, dinfo, hediff) + "\n\n" + def.deathLetterExtra;
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Death, corpse);
		}
	}

	public void Notify_Spawned(bool respawningAfterLoad)
	{
		pawn.Map.attackTargetsCache.UpdateTarget(pawn);
		if (pawn.Faction == Faction.OfPlayer && Def == MutantDefOf.Ghoul)
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ColonyGhouls, OpportunityType.GoodToKnow);
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		Gizmo gizmo = AnomalyUtility.OpenCodexGizmo(pawn);
		if (gizmo != null)
		{
			yield return gizmo;
		}
	}

	public IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (pawn.TryGetComp<CompProducesBioferrite>() == null && def.producesBioferrite)
		{
			StatDrawEntry statDrawEntry = CompProducesBioferrite.BioferriteStatDrawEntry(pawn);
			statDrawEntry.overridesHideStats = true;
			yield return statDrawEntry;
		}
	}

	public string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (def.showJobReport && pawn.Faction == Faction.OfPlayer)
		{
			string jobReport = pawn.GetJobReport();
			if (!jobReport.NullOrEmpty())
			{
				stringBuilder.AppendLineIfNotEmpty();
				stringBuilder.Append(jobReport);
			}
		}
		return stringBuilder.ToString();
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			if (originalFaction != null && !Find.FactionManager.AllFactions.Contains(originalFaction))
			{
				originalFaction = null;
			}
			if (originalIdeo != null && !Find.IdeoManager.IdeosListForReading.Contains(originalIdeo))
			{
				originalIdeo = null;
			}
		}
		Scribe_Defs.Look(ref def, "shamblerType");
		Scribe_Values.Look(ref rotStage, "rotStage", RotStage.Fresh);
		Scribe_Values.Look(ref hasTurned, "hasTurned", defaultValue: false);
		Scribe_References.Look(ref mutantHediff, "mutantHediff");
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
		Scribe_References.Look(ref originalFaction, "originalFaction");
		Scribe_References.Look(ref originalIdeo, "originalIdeo");
		Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (abilities != null)
		{
			foreach (Ability ability in abilities)
			{
				ability.pawn = pawn;
				ability.verb.caster = pawn;
			}
		}
		if (verbTracker == null)
		{
			verbTracker = new VerbTracker(this);
		}
	}
}
