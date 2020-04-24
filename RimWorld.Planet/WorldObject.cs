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

		private int tileInt = -1;

		private Faction factionInt;

		public int creationGameTicks = -1;

		public List<string> questTags;

		private bool destroyed;

		public ThingOwner<Thing> rewards;

		private List<WorldObjectComp> comps = new List<WorldObjectComp>();

		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

		private const float BaseDrawSize = 0.7f;

		private static readonly Texture2D ViewQuestCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/ViewQuest");

		public List<WorldObjectComp> AllComps => comps;

		public virtual bool ShowRelatedQuests => true;

		public bool Destroyed => destroyed;

		public int Tile
		{
			get
			{
				return tileInt;
			}
			set
			{
				if (tileInt != value)
				{
					tileInt = value;
					if (Spawned && !def.useDynamicDrawer)
					{
						Find.World.renderer.Notify_StaticWorldObjectPosChanged();
					}
					PositionChanged();
				}
			}
		}

		public bool Spawned => Find.WorldObjects.Contains(this);

		public virtual Vector3 DrawPos => Find.WorldGrid.GetTileCenter(Tile);

		public Faction Faction => factionInt;

		public virtual string Label => def.label;

		public string LabelCap => Label.CapitalizeFirst(def);

		public virtual string LabelShort => Label;

		public virtual string LabelShortCap => LabelShort.CapitalizeFirst(def);

		public virtual bool HasName => false;

		public virtual Material Material => def.Material;

		public virtual bool SelectableNow => def.selectable;

		public virtual bool NeverMultiSelect => def.neverMultiSelect;

		public virtual Texture2D ExpandingIcon => def.ExpandingIconTexture ?? ((Texture2D)Material.mainTexture);

		public virtual Color ExpandingIconColor => Material.color;

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

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats
		{
			get
			{
				yield break;
			}
		}

		public BiomeDef Biome
		{
			get
			{
				if (!Spawned)
				{
					return null;
				}
				return Find.WorldGrid[Tile].biome;
			}
		}

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
			Scribe_Values.Look(ref ID, "ID", -1);
			Scribe_Values.Look(ref tileInt, "tile", -1);
			Scribe_References.Look(ref factionInt, "faction");
			Scribe_Values.Look(ref creationGameTicks, "creationGameTicks", 0);
			Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
			Scribe_Values.Look(ref destroyed, "destroyed", defaultValue: false);
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
				catch (Exception arg)
				{
					Log.Error("Could not instantiate or initialize a WorldObjectComp: " + arg);
					comps.Remove(worldObjectComp);
				}
			}
		}

		public virtual void SetFaction(Faction newFaction)
		{
			if (!def.canHaveFaction && newFaction != null)
			{
				Log.Warning("Tried to set faction to " + newFaction + " but this world object (" + this + ") cannot have faction.");
			}
			else
			{
				factionInt = newFaction;
			}
		}

		public virtual string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Faction != null && AppendFactionToInspectString)
			{
				stringBuilder.Append("Faction".Translate() + ": " + Faction.Name);
			}
			for (int i = 0; i < comps.Count; i++)
			{
				string text = comps[i].CompInspectStringExtra();
				if (!text.NullOrEmpty())
				{
					if (Prefs.DevMode && char.IsWhiteSpace(text[text.Length - 1]))
					{
						Log.ErrorOnce(comps[i].GetType() + " CompInspectStringExtra ended with whitespace: " + text, 25612);
						text = text.TrimEndNewlines();
					}
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(text);
				}
			}
			QuestUtility.AppendInspectStringsFromQuestParts(stringBuilder, this);
			return stringBuilder.ToString();
		}

		public virtual void Tick()
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompTick();
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

		protected virtual void PositionChanged()
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
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostDestroy();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "Destroyed", this.Named("SUBJECT"));
		}

		public virtual void Print(LayerSubMesh subMesh)
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			WorldRendererUtility.PrintQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, subMesh, counterClockwise: false, randomizeRotation: true);
		}

		public virtual void Draw()
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
			if (def.expandingIcon && transitionPct > 0f)
			{
				Color color = Material.color;
				float num = 1f - transitionPct;
				propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num));
				WorldRendererUtility.DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, Material, counterClockwise: false, useSkyboxLayer: false, propertyBlock);
			}
			else
			{
				WorldRendererUtility.DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, Material);
			}
		}

		public T GetComponent<T>() where T : WorldObjectComp
		{
			for (int i = 0; i < comps.Count; i++)
			{
				T val = comps[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		public WorldObjectComp GetComponent(Type type)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (type.IsAssignableFrom(comps[i].GetType()))
				{
					return comps[i];
				}
			}
			return null;
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			int j;
			if (ShowRelatedQuests)
			{
				List<Quest> quests = Find.QuestManager.QuestsListForReading;
				for (j = 0; j < quests.Count; j++)
				{
					Quest quest = quests[j];
					if (!quest.Historical && !quest.dismissed && quest.QuestLookTargets.Contains(this))
					{
						Command_Action command_Action = new Command_Action();
						command_Action.defaultLabel = "CommandViewQuest".Translate(quest.name);
						command_Action.defaultDesc = "CommandViewQuestDesc".Translate();
						command_Action.icon = ViewQuestCommandTex;
						command_Action.action = delegate
						{
							Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
							((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
						};
						yield return command_Action;
					}
				}
			}
			j = 0;
			while (j < comps.Count)
			{
				foreach (Gizmo gizmo in comps[j].GetGizmos())
				{
					yield return gizmo;
				}
				int num = j + 1;
				j = num;
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

		public virtual IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				foreach (FloatMenuOption transportPodsFloatMenuOption in comps[i].GetTransportPodsFloatMenuOptions(pods, representative))
				{
					yield return transportPodsFloatMenuOption;
				}
			}
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
			return GetType().Name + " " + LabelCap + " (tile=" + Tile + ")";
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
