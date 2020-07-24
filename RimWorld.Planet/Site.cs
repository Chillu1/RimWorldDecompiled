using System.Collections.Generic;
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

		private bool startedCountdown;

		private bool anyEnemiesInitially;

		private bool allEnemiesDefeatedSignalSent;

		private Material cachedMat;

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
					cachedMat = MaterialPool.MatFrom(color: (!MainSitePartDef.applyFactionColorToSiteTexture || base.Faction == null) ? Color.white : base.Faction.Color, texPath: MainSitePartDef.siteTexture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: WorldMaterials.WorldObjectRenderQueue);
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

		private SitePartDef MainSitePartDef => MainSitePart.def;

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
					GenStepParams partGenStepParams = default(GenStepParams);
					partGenStepParams.sitePart = parts[i];
					List<GenStepDef> partGenStepDefs = parts[i].def.ExtraGenSteps;
					for (int j = 0; j < partGenStepDefs.Count; j++)
					{
						yield return new GenStepWithParams(partGenStepDefs[j], partGenStepParams);
					}
					partGenStepParams = default(GenStepParams);
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
						QuestPart_WorldObjectTimeout questPart_WorldObjectTimeout = quest.PartsListForReading[j] as QuestPart_WorldObjectTimeout;
						if (questPart_WorldObjectTimeout != null && questPart_WorldObjectTimeout.State == QuestPartState.Enabled && questPart_WorldObjectTimeout.worldObject == this)
						{
							return questPart_WorldObjectTimeout.TicksLeft;
						}
					}
				}
				return -1;
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
			Scribe_Values.Look(ref startedCountdown, "startedCountdown", defaultValue: false);
			Scribe_Values.Look(ref anyEnemiesInitially, "anyEnemiesInitially", defaultValue: false);
			Scribe_Values.Look(ref allEnemiesDefeatedSignalSent, "allEnemiesDefeatedSignalSent", defaultValue: false);
			Scribe_Values.Look(ref factionMustRemainHostile, "factionMustRemainHostile", defaultValue: false);
			Scribe_Values.Look(ref desiredThreatPoints, "desiredThreatPoints", 0f);
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
				for (int i = 0; i < parts.Count; i++)
				{
					parts[i].site = this;
				}
				BackCompatibility.PostExposeData(this);
			}
		}

		public void AddPart(SitePart part)
		{
			parts.Add(part);
			part.def.Worker.Init(this, part);
		}

		public override void Tick()
		{
			base.Tick();
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i].SitePartTick();
			}
			for (int j = 0; j < parts.Count; j++)
			{
				parts[j].def.Worker.SitePartWorkerTick(parts[j]);
			}
			if (base.HasMap)
			{
				CheckStartForceExitAndRemoveMapCountdown();
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
			anyEnemiesInitially = GenHostility.AnyHostileActiveThreatToPlayer(base.Map);
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
			if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval)
			{
				alsoRemoveWorldObject = (!base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ConditionCauser).Any() || !parts.Any((SitePart x) => x.def.Worker is SitePartWorker_ConditionCauser));
				return true;
			}
			alsoRemoveWorldObject = false;
			return false;
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

		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			foreach (FloatMenuOption transportPodsFloatMenuOption in base.GetTransportPodsFloatMenuOptions(pods, representative))
			{
				yield return transportPodsFloatMenuOption;
			}
			foreach (FloatMenuOption floatMenuOption in TransportPodsArrivalAction_VisitSite.GetFloatMenuOptions(representative, pods, this))
			{
				yield return floatMenuOption;
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

		private void CheckStartForceExitAndRemoveMapCountdown()
		{
			if (startedCountdown)
			{
				if (GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
				{
					anyEnemiesInitially = true;
					startedCountdown = false;
					GetComponent<TimedForcedExit>().ResetForceExitAndRemoveMapCountdown();
				}
			}
			else if (!GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
			{
				startedCountdown = true;
				float num = 0f;
				for (int i = 0; i < parts.Count; i++)
				{
					num = Mathf.Max(num, parts[i].def.forceExitAndRemoveMapCountdownDurationDays);
				}
				int num2 = Mathf.RoundToInt(num * 60000f);
				Messages.Message(anyEnemiesInitially ? "MessageSiteCountdownBecauseNoMoreEnemies".Translate(TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(num2)) : "MessageSiteCountdownBecauseNoEnemiesInitially".Translate(TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(num2)), this, MessageTypeDefOf.PositiveEvent);
				GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown(num2);
				TaleRecorder.RecordTale(TaleDefOf.CaravanAssaultSuccessful, base.Map.mapPawns.FreeColonists.RandomElement());
			}
		}

		private void CheckAllEnemiesDefeated()
		{
			if (!allEnemiesDefeatedSignalSent && base.HasMap && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map, countDormantPawnsAsHostile: true))
			{
				QuestUtility.SendQuestTargetSignals(questTags, "AllEnemiesDefeated", this.Named("SUBJECT"));
				allEnemiesDefeatedSignalSent = true;
			}
		}

		public override bool AllMatchingObjectsOnScreenMatchesWith(WorldObject other)
		{
			Site site = other as Site;
			if (site != null)
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
				if (parts[i].hidden)
				{
					continue;
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
