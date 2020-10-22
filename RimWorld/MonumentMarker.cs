using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MonumentMarker : Thing
	{
		public Sketch sketch = new Sketch();

		public int ticksSinceDisallowedBuilding;

		public bool complete;

		private static readonly Texture2D PlaceBlueprintsCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/PlaceBlueprints");

		private static readonly Texture2D CancelCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

		public const int DestroyAfterTicksSinceDisallowedBuilding = 60000;

		private const int MonumentCompletedCheckIntervalTicks = 177;

		private static List<ThingDef> tmpAllowedBuildings = new List<ThingDef>();

		private static HashSet<Pair<BuildableDef, ThingDef>> tmpUniqueBuildableDefs = new HashSet<Pair<BuildableDef, ThingDef>>();

		private static List<SketchBuildable> tmpBuildables = new List<SketchBuildable>();

		private static Dictionary<string, Pair<int, int>> tmpBuiltParts = new Dictionary<string, Pair<int, int>>();

		private static List<StuffCategoryDef> tmpStuffCategories = new List<StuffCategoryDef>();

		public override CellRect? CustomRectForSelector
		{
			get
			{
				if (!base.Spawned)
				{
					return null;
				}
				return sketch.OccupiedRect.MovedBy(base.Position);
			}
		}

		public bool AllDone
		{
			get
			{
				if (!base.Spawned)
				{
					return false;
				}
				foreach (SketchEntity entity in sketch.Entities)
				{
					if (!entity.IsSameSpawned(base.Position + entity.pos, base.Map))
					{
						return false;
					}
				}
				return true;
			}
		}

		public IntVec2 Size => sketch.OccupiedSize;

		public Thing FirstDisallowedBuilding
		{
			get
			{
				if (!base.Spawned)
				{
					return null;
				}
				List<SketchTerrain> terrain = sketch.Terrain;
				for (int i = 0; i < terrain.Count; i++)
				{
					tmpAllowedBuildings.Clear();
					sketch.ThingsAt(terrain[i].pos, out var singleResult, out var multipleResults);
					if (singleResult != null)
					{
						tmpAllowedBuildings.Add(singleResult.def);
					}
					if (multipleResults != null)
					{
						for (int j = 0; j < multipleResults.Count; j++)
						{
							tmpAllowedBuildings.Add(multipleResults[j].def);
						}
					}
					List<Thing> thingList = (terrain[i].pos + base.Position).GetThingList(base.Map);
					for (int k = 0; k < thingList.Count; k++)
					{
						if (thingList[k].def.IsBuildingArtificial && !thingList[k].def.IsBlueprint && !thingList[k].def.IsFrame && !tmpAllowedBuildings.Contains(thingList[k].def))
						{
							return thingList[k];
						}
					}
				}
				return null;
			}
		}

		public bool AnyDisallowedBuilding => FirstDisallowedBuilding != null;

		public SketchEntity FirstEntityWithMissingBlueprint
		{
			get
			{
				if (!base.Spawned)
				{
					return null;
				}
				foreach (SketchEntity entity in sketch.Entities)
				{
					if (!entity.IsSameSpawnedOrBlueprintOrFrame(base.Position + entity.pos, base.Map))
					{
						return entity;
					}
				}
				return null;
			}
		}

		public bool DisallowedBuildingTicksExpired => ticksSinceDisallowedBuilding >= 60000;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				sketch.Rotate(base.Rotation);
			}
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Monuments are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 774341);
				Destroy();
			}
		}

		public override void Tick()
		{
			if (!this.IsHashIntervalTick(177))
			{
				return;
			}
			bool allDone = AllDone;
			if (!complete && allDone)
			{
				complete = true;
				QuestUtility.SendQuestTargetSignals(questTags, "MonumentCompleted", this.Named("SUBJECT"));
			}
			if (complete && !allDone)
			{
				QuestUtility.SendQuestTargetSignals(questTags, "MonumentDestroyed", this.Named("SUBJECT"));
				if (!base.Destroyed)
				{
					Destroy();
				}
			}
			else
			{
				if (!allDone)
				{
					return;
				}
				if (AnyDisallowedBuilding)
				{
					ticksSinceDisallowedBuilding += 177;
					if (DisallowedBuildingTicksExpired)
					{
						Messages.Message("MessageMonumentDestroyedBecauseOfDisallowedBuilding".Translate(), new TargetInfo(base.Position, base.Map), MessageTypeDefOf.NegativeEvent);
						QuestUtility.SendQuestTargetSignals(questTags, "MonumentDestroyed", this.Named("SUBJECT"));
						if (!base.Destroyed)
						{
							Destroy();
						}
					}
				}
				else
				{
					ticksSinceDisallowedBuilding = 0;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref sketch, "sketch");
			Scribe_Values.Look(ref ticksSinceDisallowedBuilding, "ticksSinceDisallowedBuilding", 0);
			Scribe_Values.Look(ref complete, "complete", defaultValue: false);
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			DrawGhost_NewTmp(drawLoc.ToIntVec3(), placingMode: false, base.Rotation);
		}

		[Obsolete]
		public void DrawGhost(IntVec3 at, bool placingMode)
		{
			DrawGhost_NewTmp(at, placingMode, base.Rotation);
		}

		public void DrawGhost_NewTmp(IntVec3 at, bool placingMode, Rot4 rotation)
		{
			CellRect rect = sketch.OccupiedRect.MovedBy(at);
			Blueprint_Install thingToIgnore = FindMyBlueprint(rect, Find.CurrentMap);
			sketch.Rotate(rotation);
			Func<SketchEntity, IntVec3, List<Thing>, Map, bool> validator = null;
			if (placingMode)
			{
				validator = (SketchEntity entity, IntVec3 offset, List<Thing> things, Map map) => MonumentMarkerUtility.GetFirstAdjacentBuilding(entity, offset, things, map) == null;
			}
			sketch.DrawGhost_NewTmp(at, Sketch.SpawnPosType.Unchanged, placingMode, thingToIgnore, validator);
		}

		public Blueprint_Install FindMyBlueprint(CellRect rect, Map map)
		{
			foreach (IntVec3 item in rect)
			{
				if (!item.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Blueprint_Install blueprint_Install = thingList[i] as Blueprint_Install;
					if (blueprint_Install != null && blueprint_Install.ThingToInstall == this)
					{
						return blueprint_Install;
					}
				}
			}
			return null;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (!AllDone)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandCancelMonumentMarker".Translate();
				command_Action.defaultDesc = "CommandCancelMonumentMarkerDesc".Translate();
				command_Action.icon = CancelCommandTex;
				command_Action.action = delegate
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmCancelMonumentMarker".Translate(), delegate
					{
						QuestUtility.SendQuestTargetSignals(questTags, "MonumentCancelled", this.Named("SUBJECT"));
						RemovePossiblyRelatedBlueprints();
						this.Uninstall();
					}, destructive: true));
				};
				yield return command_Action;
			}
			bool flag = false;
			foreach (SketchEntity entity in sketch.Entities)
			{
				SketchBuildable sketchBuildable = entity as SketchBuildable;
				if (sketchBuildable != null && !entity.IsSameSpawnedOrBlueprintOrFrame(entity.pos + base.Position, base.Map) && !entity.IsSpawningBlocked(entity.pos + base.Position, base.Map) && BuildCopyCommandUtility.FindAllowedDesignator(sketchBuildable.Buildable) != null)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "CommandPlaceBlueprints".Translate();
				command_Action2.defaultDesc = "CommandPlaceBlueprintsDesc".Translate();
				command_Action2.icon = PlaceBlueprintsCommandTex;
				command_Action2.action = delegate
				{
					IEnumerable<ThingDef> enumerable = AllowedStuffs();
					if (!enumerable.Any())
					{
						PlaceAllBlueprints(null);
						SoundDefOf.Click.PlayOneShotOnCamera();
					}
					else if (enumerable.Count() == 1)
					{
						PlaceAllBlueprints(enumerable.First());
						SoundDefOf.Click.PlayOneShotOnCamera();
					}
					else
					{
						ListFloatMenuOptions(enumerable, delegate(ThingDef stuff)
						{
							PlaceAllBlueprints(stuff);
						});
					}
				};
				yield return command_Action2;
			}
			foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(this))
			{
				yield return questRelatedGizmo;
			}
			if (Prefs.DevMode)
			{
				bool flag2 = false;
				foreach (SketchEntity entity2 in sketch.Entities)
				{
					if (!entity2.IsSameSpawned(entity2.pos + base.Position, base.Map) && !entity2.IsSpawningBlocked(entity2.pos + base.Position, base.Map))
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					Command_Action command_Action3 = new Command_Action();
					command_Action3.defaultLabel = "Dev: Build all";
					command_Action3.action = delegate
					{
						DebugBuildAll();
						SoundDefOf.Click.PlayOneShotOnCamera();
					};
					yield return command_Action3;
				}
				if (AllDone && AnyDisallowedBuilding && !DisallowedBuildingTicksExpired)
				{
					Command_Action command_Action4 = new Command_Action();
					command_Action4.defaultLabel = "Dev: Disallowed building ticks +6 hours";
					command_Action4.action = delegate
					{
						ticksSinceDisallowedBuilding += 15000;
					};
					yield return command_Action4;
				}
			}
			tmpUniqueBuildableDefs.Clear();
			foreach (SketchEntity entity3 in sketch.Entities)
			{
				SketchBuildable buildable = entity3 as SketchBuildable;
				if (buildable == null || entity3.IsSameSpawnedOrBlueprintOrFrame(entity3.pos + base.Position, base.Map) || !tmpUniqueBuildableDefs.Add(new Pair<BuildableDef, ThingDef>(buildable.Buildable, buildable.Stuff)))
				{
					continue;
				}
				SketchTerrain sketchTerrain;
				if ((sketchTerrain = buildable as SketchTerrain) != null && sketchTerrain.treatSimilarAsSame)
				{
					TerrainDef terrain = buildable.Buildable as TerrainDef;
					if (terrain.designatorDropdown != null)
					{
						Designator designator = BuildCopyCommandUtility.FindAllowedDesignatorRoot(buildable.Buildable);
						if (designator != null)
						{
							yield return designator;
						}
					}
					else
					{
						IEnumerable<TerrainDef> allDefs = DefDatabase<TerrainDef>.AllDefs;
						foreach (TerrainDef item in allDefs)
						{
							if (!item.BuildableByPlayer || item.designatorDropdown != null)
							{
								continue;
							}
							bool flag3 = true;
							for (int i = 0; i < terrain.affordances.Count; i++)
							{
								if (!item.affordances.Contains(terrain.affordances[i]))
								{
									flag3 = false;
									break;
								}
							}
							if (flag3)
							{
								Command command = BuildCopyCommandUtility.BuildCommand(item, null, item.label, item.description, allowHotKey: false);
								if (command != null)
								{
									yield return command;
								}
							}
						}
					}
				}
				else
				{
					Command command2 = BuildCopyCommandUtility.BuildCommand(buildable.Buildable, buildable.Stuff, entity3.LabelCap, buildable.Buildable.description, allowHotKey: false);
					if (command2 != null)
					{
						yield return command2;
					}
				}
				Command_Action placeBlueprintsCommand = GetPlaceBlueprintsCommand(buildable);
				if (placeBlueprintsCommand != null)
				{
					yield return placeBlueprintsCommand;
				}
			}
			tmpUniqueBuildableDefs.Clear();
		}

		private Command_Action GetPlaceBlueprintsCommand(SketchBuildable buildable)
		{
			return new Command_Action
			{
				defaultLabel = "CommandPlaceBlueprintsSpecific".Translate(buildable.Label).CapitalizeFirst(),
				defaultDesc = "CommandPlaceBlueprintsSpecificDesc".Translate(buildable.Label).CapitalizeFirst(),
				icon = PlaceBlueprintsCommandTex,
				order = 20f,
				action = delegate
				{
					List<ThingDef> list = AllowedStuffsFor(buildable);
					if (!list.Any())
					{
						PlaceBlueprintsSimilarTo(buildable, null);
						SoundDefOf.Click.PlayOneShotOnCamera();
					}
					else if (list.Count() == 1)
					{
						PlaceBlueprintsSimilarTo(buildable, list.First());
						SoundDefOf.Click.PlayOneShotOnCamera();
					}
					else
					{
						ListFloatMenuOptions(list, delegate(ThingDef stuff)
						{
							PlaceBlueprintsSimilarTo(buildable, stuff);
						});
					}
				}
			};
		}

		private void ListFloatMenuOptions(IEnumerable<ThingDef> allowedStuff, Action<ThingDef> action)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			bool flag = false;
			foreach (ThingDef item in allowedStuff)
			{
				if (base.Map.listerThings.ThingsOfDef(item).Count > 0)
				{
					flag = true;
					break;
				}
			}
			foreach (ThingDef item2 in allowedStuff)
			{
				if (!flag || base.Map.listerThings.ThingsOfDef(item2).Count != 0)
				{
					ThingDef stuffLocal = item2;
					list.Add(new FloatMenuOption(stuffLocal.LabelCap, delegate
					{
						action(stuffLocal);
					}, item2));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public void DebugBuildAll()
		{
			sketch.Spawn(base.Map, base.Position, Faction.OfPlayer);
		}

		private void PlaceBlueprintsSimilarTo(SketchBuildable buildable, ThingDef preferredStuffIfNone)
		{
			bool flag = buildable is SketchTerrain;
			foreach (SketchBuildable buildable2 in sketch.Buildables)
			{
				SketchTerrain sketchTerrain;
				SketchThing sketchThing;
				if ((flag && (sketchTerrain = buildable2 as SketchTerrain) != null && sketchTerrain.IsSameOrSimilar(buildable.Buildable)) || (!flag && (sketchThing = buildable2 as SketchThing) != null && buildable.Buildable == sketchThing.def))
				{
					tmpBuildables.Add(buildable2);
				}
			}
			foreach (SketchBuildable tmpBuildable in tmpBuildables)
			{
				PlaceBlueprint(tmpBuildable, preferredStuffIfNone);
			}
			tmpBuildables.Clear();
		}

		private void PlaceAllBlueprints(ThingDef preferredStuffIfNone)
		{
			foreach (SketchEntity entity in sketch.Entities)
			{
				PlaceBlueprint(entity, preferredStuffIfNone);
			}
		}

		private void PlaceBlueprint(SketchEntity entity, ThingDef preferredStuffIfNone)
		{
			SketchBuildable sketchBuildable;
			if ((sketchBuildable = entity as SketchBuildable) != null && !entity.IsSameSpawnedOrBlueprintOrFrame(entity.pos + base.Position, base.Map) && !entity.IsSpawningBlocked(entity.pos + base.Position, base.Map) && BuildCopyCommandUtility.FindAllowedDesignator(sketchBuildable.Buildable) != null)
			{
				SketchThing sketchThing;
				SketchTerrain sketchTerrain;
				if ((sketchThing = entity as SketchThing) != null && sketchThing.def.MadeFromStuff && sketchThing.stuff == null && preferredStuffIfNone != null && preferredStuffIfNone.stuffProps.CanMake(sketchThing.def))
				{
					sketchThing.stuff = preferredStuffIfNone;
					entity.Spawn(entity.pos + base.Position, base.Map, Faction.OfPlayer, Sketch.SpawnMode.Blueprint);
					sketchThing.stuff = null;
				}
				else if ((sketchTerrain = entity as SketchTerrain) != null && sketchTerrain.stuffForComparingSimilar == null && preferredStuffIfNone != null)
				{
					sketchTerrain.stuffForComparingSimilar = preferredStuffIfNone;
					entity.Spawn(entity.pos + base.Position, base.Map, Faction.OfPlayer, Sketch.SpawnMode.Blueprint);
					sketchTerrain.stuffForComparingSimilar = null;
				}
				else
				{
					entity.Spawn(entity.pos + base.Position, base.Map, Faction.OfPlayer, Sketch.SpawnMode.Blueprint);
				}
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Quest quest = Find.QuestManager.QuestsListForReading.FirstOrDefault((Quest q) => q.QuestLookTargets.Contains(this));
			if (quest != null)
			{
				stringBuilder.Append("Quest".Translate() + ": " + quest.name);
			}
			QuestUtility.AppendInspectStringsFromQuestParts(stringBuilder, this);
			if (base.Spawned && !AllDone)
			{
				tmpBuiltParts.Clear();
				foreach (SketchEntity entity in sketch.Entities)
				{
					if (!tmpBuiltParts.TryGetValue(entity.LabelCap, out var value))
					{
						value = default(Pair<int, int>);
					}
					value = ((!entity.IsSameSpawned(entity.pos + base.Position, base.Map)) ? new Pair<int, int>(value.First, value.Second + 1) : new Pair<int, int>(value.First + 1, value.Second + 1));
					tmpBuiltParts[entity.LabelCap] = value;
				}
				foreach (KeyValuePair<string, Pair<int, int>> tmpBuiltPart in tmpBuiltParts)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(tmpBuiltPart.Key.CapitalizeFirst() + ": " + tmpBuiltPart.Value.First + " / " + tmpBuiltPart.Value.Second);
				}
				tmpBuiltParts.Clear();
			}
			return stringBuilder.ToString();
		}

		private void RemovePossiblyRelatedBlueprints()
		{
			if (!base.Spawned)
			{
				return;
			}
			foreach (SketchBuildable buildable in sketch.Buildables)
			{
				(buildable.GetSpawnedBlueprintOrFrame(base.Position + buildable.pos, base.Map) as Blueprint)?.Destroy();
			}
		}

		public bool IsPart(Thing thing)
		{
			if (!base.Spawned)
			{
				return false;
			}
			if (!sketch.OccupiedRect.MovedBy(base.Position).Contains(thing.Position))
			{
				return false;
			}
			sketch.ThingsAt(thing.Position - base.Position, out var singleResult, out var multipleResults);
			if (singleResult != null && IsPartInternal(singleResult))
			{
				return true;
			}
			if (multipleResults != null)
			{
				for (int i = 0; i < multipleResults.Count; i++)
				{
					if (IsPartInternal(multipleResults[i]))
					{
						return true;
					}
				}
			}
			if (thing.def.entityDefToBuild != null)
			{
				SketchTerrain sketchTerrain = sketch.SketchTerrainAt(thing.Position - base.Position);
				if (sketchTerrain != null && IsPartInternal(sketchTerrain))
				{
					return true;
				}
			}
			return false;
			bool IsPartInternal(SketchBuildable b)
			{
				BuildableDef buildable = b.Buildable;
				if (thing.def != buildable && thing.def.entityDefToBuild != buildable)
				{
					return false;
				}
				if (b.GetSpawnedBlueprintOrFrame(b.pos + base.Position, base.Map) == thing)
				{
					return true;
				}
				SketchThing sketchThing;
				if ((sketchThing = b as SketchThing) != null && sketchThing.GetSameSpawned(sketchThing.pos + base.Position, base.Map) == thing)
				{
					return true;
				}
				return false;
			}
		}

		public bool AllowsPlacingBlueprint(BuildableDef buildable, IntVec3 pos, Rot4 rot, ThingDef stuff)
		{
			if (!base.Spawned)
			{
				return true;
			}
			CellRect newRect = GenAdj.OccupiedRect(pos, rot, buildable.Size);
			if (!sketch.OccupiedRect.MovedBy(base.Position).Overlaps(newRect))
			{
				return true;
			}
			bool collided = false;
			foreach (IntVec3 item in newRect)
			{
				sketch.ThingsAt(item - base.Position, out var singleResult, out var multipleResults);
				if (singleResult != null && CheckEntity(singleResult))
				{
					return true;
				}
				if (multipleResults != null)
				{
					for (int i = 0; i < multipleResults.Count; i++)
					{
						if (CheckEntity(multipleResults[i]))
						{
							return true;
						}
					}
				}
				SketchTerrain sketchTerrain = sketch.SketchTerrainAt(item - base.Position);
				if (sketchTerrain != null && CheckEntity(sketchTerrain))
				{
					return true;
				}
			}
			return !collided;
			bool CheckEntity(SketchBuildable entity)
			{
				if (entity.IsSameSpawned(entity.pos + base.Position, base.Map))
				{
					return false;
				}
				if (entity.OccupiedRect.MovedBy(base.Position).Overlaps(newRect))
				{
					collided = true;
				}
				SketchThing sketchThing = entity as SketchThing;
				if (entity.OccupiedRect.MovedBy(base.Position).Equals(newRect) && IsSameOrSimilar(entity) && (stuff == null || entity.Stuff == null || stuff == entity.Stuff))
				{
					if (sketchThing != null && !(sketchThing.rot == rot) && !(sketchThing.rot == rot.Opposite))
					{
						return !sketchThing.def.rotatable;
					}
					return true;
				}
				return false;
			}
			bool IsSameOrSimilar(SketchBuildable entity)
			{
				SketchTerrain sketchTerrain2;
				if (buildable == entity.Buildable || ((sketchTerrain2 = entity as SketchTerrain) != null && sketchTerrain2.IsSameOrSimilar(buildable)))
				{
					return true;
				}
				return false;
			}
		}

		public IEnumerable<ThingDef> AllowedStuffs()
		{
			tmpStuffCategories.Clear();
			bool flag = true;
			List<SketchThing> things = sketch.Things;
			for (int i = 0; i < things.Count; i++)
			{
				if (!things[i].def.MadeFromStuff || things[i].stuff != null)
				{
					continue;
				}
				if (flag)
				{
					flag = false;
					tmpStuffCategories.AddRange(things[i].def.stuffCategories);
					continue;
				}
				bool flag2 = false;
				for (int j = 0; j < things[i].def.stuffCategories.Count; j++)
				{
					if (tmpStuffCategories.Contains(things[i].def.stuffCategories[j]))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					continue;
				}
				for (int num = tmpStuffCategories.Count - 1; num >= 0; num--)
				{
					if (!things[i].def.stuffCategories.Contains(tmpStuffCategories[num]))
					{
						tmpStuffCategories.RemoveAt(num);
					}
				}
			}
			return GenStuff.AllowedStuffs(tmpStuffCategories);
		}

		public List<ThingDef> AllowedStuffsFor(SketchBuildable buildable)
		{
			if (buildable.Buildable.MadeFromStuff && buildable.Stuff == null)
			{
				return GenStuff.AllowedStuffs(buildable.Buildable.stuffCategories).ToList();
			}
			SketchTerrain sketchTerrain;
			if ((sketchTerrain = buildable as SketchTerrain) != null)
			{
				List<ThingDef> list = new List<ThingDef>();
				{
					foreach (TerrainDef allDef in DefDatabase<TerrainDef>.AllDefs)
					{
						if (allDef.BuildableByPlayer && sketchTerrain.IsSameOrSimilar(allDef) && !allDef.costList.NullOrEmpty())
						{
							list.Add(allDef.costList.First().thingDef);
						}
					}
					return list;
				}
			}
			return null;
		}
	}
}
