using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class Site : MapParent
	{
		public string customLabel;

		public List<SitePart> parts = new List<SitePart>();

		public bool sitePartsKnown = true;

		public bool factionMustRemainHostile;

		public float desiredThreatPoints;

		private SiteCoreBackCompat coreBackCompat;

		private bool anyEnemiesInitially;

		private bool caravanAssaultSuccessfulTaleRecorded;

		private bool allEnemiesDefeatedSignalSent;

		public bool preventGravshipLanding;

		public bool firedNoActiveThreatsSignal;

		private Material cachedMat;

		private static readonly IntVec3 DefaultMapSize = new IntVec3(200, 1, 200);

		private static List<string> tmpSitePartsLabels = new List<string>();

		public override string Label
		{
			get
			{
				if (!customLabel.NullOrEmpty())
				{
					return customLabel;
				}
				if (MainSitePartDef == SitePartDefOf.PreciousLump && MainSitePart.parms.preciousLumpResources != null)
				{
					return "PreciousLumpLabel".Translate(MainSitePart.parms.preciousLumpResources.label);
				}
				return MainSitePartDef.label;
			}
		}

		public override Texture2D ExpandingIcon => MainSitePartDef.ExpandingIconTexture;

		public override bool HandlesConditionCausers => true;

		public override Material Material
		{
			get
			{
				if (cachedMat == null)
				{
					cachedMat = MaterialPool.MatFrom(color: (!MainSitePartDef.applyFactionColorToSiteTexture || base.Faction == null) ? Color.white : base.Faction.Color, texPath: MainSitePartDef.siteTexture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: 3550);
				}
				return cachedMat;
			}
		}

		public override bool AppendFactionToInspectString
		{
			get
			{
				if (!MainSitePartDef.applyFactionColorToSiteTexture)
				{
					return MainSitePartDef.showFactionInInspectString;
				}
				return true;
			}
		}

		private SitePart MainSitePart
		{
			get
			{
				if (!parts.Any())
				{
					Log.ErrorOnce("Site without any SitePart at " + base.Tile, ID ^ 0x598A95D);
					return null;
				}
				if (parts[0].hidden)
				{
					Log.ErrorOnce("Site with first SitePart hidden at " + base.Tile, ID ^ 0x2E39CC7);
					return parts[0];
				}
				return parts[0];
			}
		}

		public SitePartDef MainSitePartDef => MainSitePart.def;

		public override bool GravShipCanLandOn
		{
			get
			{
				if (MainSitePartDef.gravShipsCanLandOn)
				{
					return !base.HasMap;
				}
				return false;
			}
		}

		public override IEnumerable<GenStepWithParams> ExtraGenStepDefs
		{
			get
			{
				foreach (GenStepWithParams extraGenStepDef in base.ExtraGenStepDefs)
				{
					yield return extraGenStepDef;
				}
				for (int i = 0; i < parts.Count; i++)
				{
					GenStepParams partGenStepParams = new GenStepParams
					{
						sitePart = parts[i]
					};
					List<GenStepDef> partGenStepDefs = parts[i].def.ExtraGenSteps;
					for (int j = 0; j < partGenStepDefs.Count; j++)
					{
						yield return new GenStepWithParams(partGenStepDefs[j], partGenStepParams);
					}
				}
			}
		}

		public string ApproachOrderString => MainSitePartDef.approachOrderString.NullOrEmpty() ? "ApproachSite".Translate(Label) : MainSitePartDef.approachOrderString.Formatted(Label);

		public string ApproachingReportString => MainSitePartDef.approachingReportString.NullOrEmpty() ? "ApproachingSite".Translate(Label) : MainSitePartDef.approachingReportString.Formatted(Label);

		public float ActualThreatPoints
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < parts.Count; i++)
				{
					num += parts[i].parms.threatPoints;
				}
				return num;
			}
		}

		public bool IncreasesPopulation
		{
			get
			{
				if (base.HasMap)
				{
					return false;
				}
				for (int i = 0; i < parts.Count; i++)
				{
					if (parts[i].def.Worker.IncreasesPopulation(parts[i].parms))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool BadEvenIfNoMap
		{
			get
			{
				for (int i = 0; i < parts.Count; i++)
				{
					if (parts[i].def.badEvenIfNoMap)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasWorldObjectTimeout => WorldObjectTimeoutTicksLeft != -1;

		public int WorldObjectTimeoutTicksLeft
		{
			get
			{
				List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
				for (int i = 0; i < questsListForReading.Count; i++)
				{
					Quest quest = questsListForReading[i];
					if (quest.State != QuestState.Ongoing)
					{
						continue;
					}
					for (int j = 0; j < quest.PartsListForReading.Count; j++)
					{
						if (quest.PartsListForReading[j] is QuestPart_WorldObjectTimeout { State: QuestPartState.Enabled } questPart_WorldObjectTimeout && questPart_WorldObjectTimeout.worldObject == this)
						{
							return questPart_WorldObjectTimeout.TicksLeft;
						}
					}
				}
				return -1;
			}
		}

		public IntVec3 PreferredMapSize
		{
			get
			{
				IntVec3 defaultMapSize = DefaultMapSize;
				for (int i = 0; i < parts.Count; i++)
				{
					SitePart sitePart = parts[i];
					if (sitePart.def.minMapSize.HasValue)
					{
						IntVec3 value = sitePart.def.minMapSize.Value;
						defaultMapSize.x = Mathf.Max(value.x, defaultMapSize.x);
						defaultMapSize.y = Mathf.Max(value.y, defaultMapSize.y);
						defaultMapSize.z = Mathf.Max(value.z, defaultMapSize.z);
					}
				}
				return defaultMapSize;
			}
		}

		public override AcceptanceReport CanBeSettled
		{
			get
			{
				foreach (Quest item in Find.QuestManager.QuestsListForReading)
				{
					if (item.State != QuestState.Ongoing)
					{
						continue;
					}
					foreach (QuestPart item2 in item.PartsListForReading)
					{
						if (item2 is QuestPart_SpawnWorldObject questPart_SpawnWorldObject && questPart_SpawnWorldObject.worldObject == this)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].def.Worker.PostDestroy(parts[i]);
			}
			for (int j = 0; j < parts.Count; j++)
			{
				parts[j].PostDestroy();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref customLabel, "customLabel");
			Scribe_Deep.Look(ref coreBackCompat, "core");
			Scribe_Collections.Look(ref parts, "parts", LookMode.Deep);
			Scribe_Values.Look(ref anyEnemiesInitially, "anyEnemiesInitially", defaultValue: false);
			Scribe_Values.Look(ref caravanAssaultSuccessfulTaleRecorded, "caravanAssaultSuccessfulTaleRecorded", defaultValue: false);
			Scribe_Values.Look(ref allEnemiesDefeatedSignalSent, "allEnemiesDefeatedSignalSent", defaultValue: false);
			Scribe_Values.Look(ref factionMustRemainHostile, "factionMustRemainHostile", defaultValue: false);
			Scribe_Values.Look(ref desiredThreatPoints, "desiredThreatPoints", 0f);
			Scribe_Values.Look(ref preventGravshipLanding, "preventGravshipLanding", defaultValue: false);
			Scribe_Values.Look(ref firedNoActiveThreatsSignal, "firedNoActiveThreatsSignal", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (coreBackCompat != null && coreBackCompat.def != null)
				{
					parts.Insert(0, new SitePart(this, coreBackCompat.def, coreBackCompat.parms));
					coreBackCompat = null;
				}
				if (parts.RemoveAll((SitePart x) => x == null || x.def == null) != 0)
				{
					Log.Error("Some site parts were null after loading.");
				}
				for (int num = 0; num < parts.Count; num++)
				{
					parts[num].site = this;
				}
				BackCompatibility.PostExposeData(this);
			}
		}

		public void AddPart(SitePart part)
		{
			if (!part.def.forceMutators.NullOrEmpty())
			{
				foreach (TileMutatorDef forceMutator in part.def.forceMutators)
				{
					base.Tile.Tile.AddMutator(forceMutator);
				}
			}
			parts.Add(part);
			part.def.Worker.Init(this, part);
		}

		protected override void Tick()
		{
			base.Tick();
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].def.Worker.SitePartWorkerTick(parts[i]);
			}
			if (base.HasMap && !firedNoActiveThreatsSignal && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map, countDormantPawnsAsHostile: true, !CanReformFoggedEnemies))
			{
				QuestUtility.SendQuestTargetSignals(questTags, "NoActiveThreats");
				firedNoActiveThreatsSignal = true;
			}
		}

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].SitePartTickInterval(delta);
			}
			if (base.HasMap)
			{
				CheckRecordAssaultSuccessfulTale();
				CheckAllEnemiesDefeated();
			}
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			Map map = base.Map;
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].def.Worker.PostMapGenerate(map);
			}
			float num = 0f;
			for (int j = 0; j < parts.Count; j++)
			{
				num = Mathf.Max(num, parts[j].def.forceExitAndRemoveMapCountdownDurationDays);
			}
			num *= MapParentTuning.SiteDetectionCountdownMultiplier.RandomInRange;
			if (!parts.Any((SitePart p) => p.def.disallowsAutomaticDetectionTimerStart) && !map.wasSpawnedViaGravShipLanding)
			{
				int ticks = Mathf.RoundToInt(num * 60000f);
				GetComponent<TimedDetectionRaids>().StartDetectionCountdown(ticks);
			}
			allEnemiesDefeatedSignalSent = false;
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].def.Worker.PostDrawExtraSelectionOverlays(parts[i]);
			}
		}

		public override void Notify_MyMapAboutToBeRemoved()
		{
			base.Notify_MyMapAboutToBeRemoved();
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].def.Worker.Notify_SiteMapAboutToBeRemoved(parts[i]);
			}
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			if (base.Map.mapPawns.AnyPawnBlockingMapRemoval)
			{
				return false;
			}
			foreach (PocketMapParent item in Find.World.pocketMaps.ToList())
			{
				if (item.sourceMap == base.Map && item.Map.mapPawns.AnyPawnBlockingMapRemoval)
				{
					return false;
				}
			}
			if (base.Map.AnyBuildingBlockingMapRemoval)
			{
				return false;
			}
			alsoRemoveWorldObject = !parts.Any((SitePart x) => (x.def.Worker is SitePartWorker_ConditionCauser && x.conditionCauser != null && !x.conditionCauser.Destroyed) || (x.def.Worker is SitePartWorker_RaidSource && GenHostility.AnyHostileActiveThreatToPlayer(base.Map, countDormantPawnsAsHostile: true)));
			if (parts.Any((SitePart x) => x.def.Worker is SitePartWorker_AncientAltar sitePartWorker_AncientAltar && sitePartWorker_AncientAltar.ShouldKeepMapForRelic(x)))
			{
				alsoRemoveWorldObject = false;
			}
			if (TransporterUtility.IncomingTransporterPreventingMapRemoval(base.Map))
			{
				return false;
			}
			return true;
		}

		public override void GetChildHolders(List<IThingHolder> outChildren)
		{
			base.GetChildHolders(outChildren);
			for (int i = 0; i < parts.Count; i++)
			{
				outChildren.Add(parts[i]);
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
			{
				yield return floatMenuOption;
			}
			if (base.HasMap)
			{
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitSite.GetFloatMenuOptions(caravan, this))
			{
				yield return floatMenuOption2;
			}
		}

		public override IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
		{
			foreach (FloatMenuOption transportersFloatMenuOption in base.GetTransportersFloatMenuOptions(pods, launchAction))
			{
				yield return transportersFloatMenuOption;
			}
			foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitSite.GetFloatMenuOptions(launchAction, pods, this))
			{
				yield return floatMenuOption;
			}
		}

		public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
		{
			foreach (FloatMenuOption shuttleFloatMenuOption in base.GetShuttleFloatMenuOptions(pods, launchAction))
			{
				yield return shuttleFloatMenuOption;
			}
			if (def.mapGenerator == MapGeneratorDefOf.Space)
			{
				foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_VisitSpace.GetFloatMenuOptions(launchAction, pods, this))
				{
					yield return floatMenuOption;
				}
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption2 in TransportersArrivalAction_VisitSite.GetFloatMenuOptions(launchAction, pods, this))
			{
				yield return floatMenuOption2;
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (base.HasMap && Find.WorldSelector.SingleSelectedObject == this)
			{
				yield return SettleInExistingMapUtility.SettleCommand(base.Map, requiresNoEnemies: true);
			}
		}

		private void CheckRecordAssaultSuccessfulTale()
		{
			if (anyEnemiesInitially && !caravanAssaultSuccessfulTaleRecorded && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
			{
				caravanAssaultSuccessfulTaleRecorded = true;
				if (base.Map.mapPawns.FreeColonists.Any())
				{
					TaleRecorder.RecordTale(TaleDefOf.CaravanAssaultSuccessful, base.Map.mapPawns.FreeColonists.RandomElement());
				}
			}
		}

		private void CheckAllEnemiesDefeated()
		{
			if (!allEnemiesDefeatedSignalSent && base.HasMap && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map, countDormantPawnsAsHostile: true, canBeFogged: true))
			{
				QuestUtility.SendQuestTargetSignals(questTags, "AllEnemiesDefeated", this.Named("SUBJECT"));
				allEnemiesDefeatedSignalSent = true;
			}
		}

		public override bool AllMatchingObjectsOnScreenMatchesWith(WorldObject other)
		{
			if (other is Site site)
			{
				return site.MainSitePartDef == MainSitePartDef;
			}
			return false;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			tmpSitePartsLabels.Clear();
			for (int i = 0; i < parts.Count; i++)
			{
				if (parts[i].hidden || !parts[i].def.displayOnInspectPane)
				{
					continue;
				}
				if (MainSitePart == parts[i] && !parts[i].def.mainPartAllThreatsLabel.NullOrEmpty() && ActualThreatPoints > 0f)
				{
					stringBuilder.Length = 0;
					stringBuilder.Append(parts[i].def.mainPartAllThreatsLabel.CapitalizeFirst());
					break;
				}
				string postProcessedThreatLabel = parts[i].def.Worker.GetPostProcessedThreatLabel(this, parts[i]);
				if (!postProcessedThreatLabel.NullOrEmpty())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(postProcessedThreatLabel.CapitalizeFirst());
				}
			}
			return stringBuilder.ToString();
		}

		public override string GetDescription()
		{
			string text = MainSitePartDef.description;
			string description = base.GetDescription();
			if (!description.NullOrEmpty())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += description;
			}
			return text;
		}
	}
}
