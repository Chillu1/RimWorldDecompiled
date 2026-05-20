using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldObject : IExposable, ILoadReferenceable, ISelectable
	{
		public WorldObjectDef def;

		public int ID = -1;

		private PlanetTile tile = PlanetTile.Invalid;

		private Faction factionInt;

		public int creationGameTicks = -1;

		public List<string> questTags;

		private bool destroyed;

		public ThingOwner<Thing> rewards;

		private int tickDelta;

		public bool isGeneratedLocation;

		private List<WorldObjectComp> comps = new List<WorldObjectComp>();

		private Material expandingMaterial;

		private int drawPosCacheTick = -1;

		private Vector3 drawPosCached;

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		private const float BaseDrawSize = 0.7f;

		private const float DrawOffsetRange = 0.01f;

		private static readonly Texture2D ViewQuestCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/ViewQuest");

		private List<IThingHolder> tmpHolders;

		private bool cached;

		private bool cachedIsHolder;

		private IThingHolder cachedHolder;

		private IThingHolderTickable cachedTickable;

		public List<WorldObjectComp> AllComps => comps;

		public virtual bool ShowRelatedQuests => true;

		public virtual bool CanReformFoggedEnemies => false;

		public bool Destroyed => destroyed;

		public virtual bool VisibleInBackground
		{
			get
			{
				if (Tile.LayerDef.isSpace)
				{
					return Find.WorldSelector.SelectedLayer.Def.isSpace;
				}
				return false;
			}
		}

		public virtual bool RequiresSignalJammerToReach
		{
			get
			{
				if (def.requiresSignalJammerToReach)
				{
					return !(this is MapParent mapParent) || !mapParent.HasMap;
				}
				return false;
			}
		}

		public virtual float DrawAltitude => 0.03f;

		public PlanetTile Tile
		{
			get
			{
				return tile;
			}
			set
			{
				if (!(tile == value))
				{
					PlanetTile previous = tile;
					tile = value;
					if (Spawned && !def.useDynamicDrawer)
					{
						Find.World.renderer.Notify_StaticWorldObjectPosChanged();
					}
					if (previous.Valid)
					{
						previous.Tile.Layer.FastTileFinder.DirtyTile(previous);
					}
					tile.Tile.Layer.FastTileFinder.DirtyTile(tile);
					PositionChanged(previous, tile);
				}
			}
		}

		public bool Spawned => Find.WorldObjects.Contains(this);

		public virtual Vector3 DrawPos
		{
			get
			{
				if (Find.TickManager.TicksGame != drawPosCacheTick)
				{
					drawPosCached = Tile.Layer.Origin + Find.WorldGrid.GetTileCenter(Tile);
					drawPosCacheTick = Find.TickManager.TicksGame;
				}
				return drawPosCached;
			}
		}

		public Faction Faction => factionInt;

		public virtual string Label => def.label;

		public string LabelCap => Label.CapitalizeFirst(def);

		public virtual string LabelShort => Label;

		public virtual string LabelShortCap => LabelShort.CapitalizeFirst(def);

		public virtual bool HasName => false;

		public virtual Material Material => def.Material;

		public virtual Material ExpandingMaterial
		{
			get
			{
				if (def.expandingShader == null)
				{
					return null;
				}
				if (expandingMaterial != null)
				{
					return expandingMaterial;
				}
				MaterialRequest req = new MaterialRequest
				{
					mainTex = def.ExpandingIconTexture,
					shader = def.expandingShader.Shader,
					color = Color.white,
					maskTex = def.ExpandingIconTextureMask
				};
				return expandingMaterial = MaterialPool.MatFrom(req);
			}
		}

		public virtual bool SelectableNow => def.selectable;

		public virtual bool NeverMultiSelect => def.neverMultiSelect;

		public virtual Texture2D ExpandingIcon
		{
			get
			{
				if (def.ExpandingIconTexture != null)
				{
					return def.ExpandingIconTexture;
				}
				if (Material != null)
				{
					return (Texture2D)Material.mainTexture;
				}
				return BaseContent.BadTex;
			}
		}

		public virtual Color ExpandingIconColor => def.expandingIconColor ?? Material.color;

		public virtual float ExpandingIconPriority => def.expandingIconPriority;

		public virtual bool ExpandMore => def.expandMore;

		public virtual bool AppendFactionToInspectString => true;

		public IThingHolder ParentHolder
		{
			get
			{
				if (!Spawned)
				{
					return null;
				}
				return Find.World;
			}
		}

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats => Enumerable.Empty<StatDrawEntry>();

		public virtual BiomeDef Biome
		{
			get
			{
				if (!Spawned)
				{
					return null;
				}
				return Find.WorldGrid[Tile].PrimaryBiome;
			}
		}

		public virtual float ExpandingIconRotation => 0f;

		public virtual bool ExpandingIconFlipHorizontal => false;

		public virtual bool GravShipCanLandOn => false;

		protected virtual int UpdateRateTicks
		{
			get
			{
				if (!WorldRendererUtility.WorldSelected)
				{
					return 15;
				}
				return 1;
			}
		}

		protected virtual int UpdateRateTickOffset => this.HashOffset();

		public virtual IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			if (def.IncidentTargetTags != null)
			{
				foreach (IncidentTargetTagDef incidentTargetTag in def.IncidentTargetTags)
				{
					yield return incidentTargetTag;
				}
			}
			for (int i = 0; i < comps.Count; i++)
			{
				foreach (IncidentTargetTagDef item in comps[i].IncidentTargetTags())
				{
					yield return item;
				}
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				InitializeComps();
			}
			Scribe_Values.Look(ref tile, "tile");
			Scribe_Values.Look(ref ID, "ID", -1);
			Scribe_Values.Look(ref creationGameTicks, "creationGameTicks", 0);
			Scribe_Values.Look(ref destroyed, "destroyed", defaultValue: false);
			Scribe_Values.Look(ref tickDelta, "tickDelta", 0);
			Scribe_Values.Look(ref isGeneratedLocation, "isGeneratedLocation", defaultValue: false);
			Scribe_References.Look(ref factionInt, "faction");
			Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
			if (Scribe.mode != LoadSaveMode.Saving)
			{
				Scribe_Deep.Look(ref rewards, "rewards");
			}
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostExposeData();
			}
		}

		private void InitializeComps()
		{
			for (int i = 0; i < def.comps.Count; i++)
			{
				WorldObjectComp worldObjectComp = null;
				try
				{
					worldObjectComp = (WorldObjectComp)Activator.CreateInstance(def.comps[i].compClass);
					worldObjectComp.parent = this;
					comps.Add(worldObjectComp);
					worldObjectComp.Initialize(def.comps[i]);
				}
				catch (Exception ex)
				{
					Log.Error("Could not instantiate or initialize a WorldObjectComp: " + ex);
					comps.Remove(worldObjectComp);
				}
			}
		}

		public virtual void SetFaction(Faction newFaction)
		{
			if (!def.canHaveFaction && newFaction != null)
			{
				Log.Warning("Tried to set faction to " + newFaction?.ToString() + " but this world object (" + this?.ToString() + ") cannot have faction.");
			}
			else
			{
				factionInt = newFaction;
			}
		}

		public virtual string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (ModsConfig.OdysseyActive && def.requiresSignalJammerToReach)
			{
				stringBuilder.Append("RequiresSignalJammer".Translate());
			}
			if (Faction != null && AppendFactionToInspectString)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(string.Format("{0}: {1}", "Faction".Translate(), Faction.Name));
			}
			for (int i = 0; i < comps.Count; i++)
			{
				string text = comps[i].CompInspectStringExtra();
				if (text.NullOrEmpty())
				{
					continue;
				}
				if (Prefs.DevMode)
				{
					string text2 = text;
					if (char.IsWhiteSpace(text2[text2.Length - 1]))
					{
						Log.ErrorOnce($"{comps[i].GetType()} CompInspectStringExtra ended with whitespace: {text}", 25612);
						text = text.TrimEndNewlines();
					}
				}
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(text);
			}
			QuestUtility.AppendInspectStringsFromQuestParts(stringBuilder, this);
			return stringBuilder.ToString();
		}

		public void DoTick()
		{
			using (ProfilerBlock.Scope("DoTick()"))
			{
				using (ProfilerBlock.Scope("Tick()"))
				{
					Tick();
				}
				tickDelta++;
				if (tickDelta > UpdateRateTicks || GenTicks.IsTickInterval(UpdateRateTickOffset, UpdateRateTicks))
				{
					using (ProfilerBlock.Scope("TickInterval()"))
					{
						TickInterval(tickDelta);
					}
					tickDelta = 0;
				}
			}
			if (Destroyed)
			{
				return;
			}
			if (!cached)
			{
				cached = true;
				cachedHolder = this as IThingHolder;
				cachedTickable = this as IThingHolderTickable;
				cachedIsHolder = cachedHolder != null;
			}
			if (!cachedIsHolder || (cachedTickable != null && !cachedTickable.ShouldTickContents))
			{
				return;
			}
			if (tmpHolders == null)
			{
				tmpHolders = new List<IThingHolder>(8);
			}
			tmpHolders.Add(cachedHolder);
			cachedHolder.GetChildHolders(tmpHolders);
			for (int i = 0; i < tmpHolders.Count; i++)
			{
				ThingOwner directlyHeldThings = tmpHolders[i].GetDirectlyHeldThings();
				if (directlyHeldThings == null)
				{
					continue;
				}
				IThingHolder owner = directlyHeldThings.Owner;
				if (!(owner is Map) && !(owner is Caravan))
				{
					directlyHeldThings.DoTick();
					if (Destroyed)
					{
						break;
					}
				}
			}
			tmpHolders.Clear();
		}

		protected virtual void Tick()
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompTick();
			}
		}

		protected virtual void TickInterval(int delta)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompTickInterval(delta);
			}
		}

		public virtual void ExtraSelectionOverlaysOnGUI()
		{
		}

		public virtual void DrawExtraSelectionOverlays()
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostDrawExtraSelectionOverlays();
			}
		}

		public virtual void PostMake()
		{
			InitializeComps();
		}

		public virtual void PostAdd()
		{
			QuestUtility.SendQuestTargetSignals(questTags, "Spawned", this.Named("SUBJECT"));
		}

		protected virtual void PositionChanged(PlanetTile previous, PlanetTile current)
		{
		}

		public virtual void SpawnSetup()
		{
			if (!def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.RegisterDrawable(this);
			}
			Tile.Layer.FastTileFinder.DirtyTile(Tile);
		}

		public virtual void PostRemove()
		{
			if (!def.useDynamicDrawer)
			{
				Find.World.renderer.Notify_StaticWorldObjectPosChanged();
			}
			if (def.useDynamicDrawer)
			{
				Find.WorldDynamicDrawManager.DeRegisterDrawable(this);
			}
			Find.WorldSelector.Deselect(this);
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostPostRemove();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "Despawned", this.Named("SUBJECT"));
			Tile.Layer.FastTileFinder.DirtyTile(Tile);
		}

		public virtual void Destroy()
		{
			if (Destroyed)
			{
				Log.Error("Tried to destroy already-destroyed world object " + this);
				return;
			}
			if (Spawned)
			{
				Find.WorldObjects.Remove(this);
			}
			destroyed = true;
			Find.FactionManager.Notify_WorldObjectDestroyed(this);
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostDestroy();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "Destroyed", this.Named("SUBJECT"));
		}

		public virtual void Print(LayerSubMesh subMesh)
		{
			float averageTileSize = Tile.Layer.AverageTileSize;
			float num = Rand.RangeSeeded(0f, 0.01f, ID) + def.drawAltitudeOffset;
			WorldRendererUtility.PrintQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.03f + num, subMesh, counterClockwise: false, Rand.Range(0f, 360f));
		}

		public virtual void Draw()
		{
			float averageTileSize = Tile.Layer.AverageTileSize;
			float rawTransitionPct = ExpandableWorldObjectsUtility.RawTransitionPct;
			if (!Tile.LayerDef.isSpace && (bool)Material)
			{
				float num = Rand.RangeSeeded(0f, 0.01f, ID) + def.drawAltitudeOffset;
				if (def.expandingIcon && rawTransitionPct > 0f && !ExpandableWorldObjectsUtility.HiddenByRules(this))
				{
					Color color = Material.color;
					float num2 = 1f - rawTransitionPct;
					propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num2));
					WorldRendererUtility.DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, DrawAltitude + num, Material, 0f, counterClockwise: false, useSkyboxLayer: false, propertyBlock);
				}
				else
				{
					WorldRendererUtility.DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, DrawAltitude + num, Material);
				}
			}
		}

		public T GetComponent<T>() where T : WorldObjectComp
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i] is T result)
				{
					return result;
				}
			}
			return null;
		}

		public bool TryGetComponent<T>(out T comp) where T : WorldObjectComp
		{
			comp = GetComponent<T>();
			return comp != null;
		}

		public WorldObjectComp GetComponent(Type type)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (type.IsInstanceOfType(comps[i]))
				{
					return comps[i];
				}
			}
			return null;
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			int i;
			if (ShowRelatedQuests)
			{
				List<Quest> quests = Find.QuestManager.QuestsListForReading;
				for (i = 0; i < quests.Count; i++)
				{
					Quest quest = quests[i];
					if (!quest.hidden && !quest.Historical && !quest.dismissed && quest.QuestLookTargets.Contains(this))
					{
						yield return new Command_Action
						{
							defaultLabel = "CommandViewQuest".Translate(quest.name),
							defaultDesc = "CommandViewQuestDesc".Translate(),
							icon = ViewQuestCommandTex,
							Order = -1f,
							action = delegate
							{
								Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
								((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
							}
						};
					}
				}
			}
			if (DebugSettings.ShowDevGizmos && this is MapParent { HasMap: false })
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Generate",
					action = delegate
					{
						LongEventHandler.QueueLongEvent(delegate
						{
							SetFaction(Faction.OfPlayer);
							Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(Tile, new IntVec3(200, 1, 200), null);
							Current.Game.CurrentMap = orGenerateMap;
							CameraJumper.TryJump(orGenerateMap.Center, orGenerateMap);
						}, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
					}
				};
			}
			i = 0;
			while (i < comps.Count)
			{
				foreach (Gizmo gizmo in comps[i].GetGizmos())
				{
					yield return gizmo;
				}
				int num = i + 1;
				i = num;
			}
		}

		public virtual IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				foreach (Gizmo caravanGizmo in comps[i].GetCaravanGizmos(caravan))
				{
					yield return caravanGizmo;
				}
			}
		}

		public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			int i = 0;
			while (i < comps.Count)
			{
				foreach (FloatMenuOption floatMenuOption in comps[i].GetFloatMenuOptions(caravan))
				{
					yield return floatMenuOption;
				}
				int num = i + 1;
				i = num;
			}
		}

		public virtual IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}

		public virtual IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}

		public virtual IEnumerable<InspectTabBase> GetInspectTabs()
		{
			return def.inspectorTabsResolved;
		}

		public virtual bool AllMatchingObjectsOnScreenMatchesWith(WorldObject other)
		{
			return Faction == other.Faction;
		}

		public override string ToString()
		{
			return GetType().Name + " " + LabelCap + " (tile=" + Tile.ToString() + ")";
		}

		public override int GetHashCode()
		{
			return ID;
		}

		public string GetUniqueLoadID()
		{
			return "WorldObject_" + ID;
		}

		public virtual string GetDescription()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(def.description);
			for (int i = 0; i < comps.Count; i++)
			{
				string descriptionPart = comps[i].GetDescriptionPart();
				if (!descriptionPart.NullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(descriptionPart);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
