using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;

namespace Verse;

public static class DebugToolsMisc
{
	private static List<WeaponTraitDef> tmpTraitDefs = new List<WeaponTraitDef>();

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void AttachFire()
	{
		using List<Thing>.Enumerator enumerator = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList().GetEnumerator();
		if (enumerator.MoveNext())
		{
			Thing current = enumerator.Current;
			if (current.CanEverAttachFire())
			{
				current.TryAttachFire(1f, null);
			}
			else
			{
				FireUtility.TryStartFireIn(UI.MouseCell(), Find.CurrentMap, 1.75f, null);
			}
		}
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static DebugActionNode SetStuff()
	{
		DebugActionNode debugActionNode = new DebugActionNode();
		foreach (ThingDef stuff in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.IsStuff))
		{
			ThingDef stuffInner = stuff;
			debugActionNode.AddChild(new DebugActionNode(stuff.LabelCap, DebugActionType.ToolMap, delegate
			{
				foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					if (item.Stuff != null)
					{
						float num = (float)item.HitPoints / (float)item.MaxHitPoints;
						item.SetStuffDirect(stuffInner);
						StatDefOf.MaxHitPoints.Worker.ClearCacheForThing(item);
						item.HitPoints = Mathf.CeilToInt((float)item.MaxHitPoints * num);
						item.Notify_ColorChanged();
						item.DirtyMapMesh(item.Map);
						if (item is UnfinishedThing unfinishedThing)
						{
							int num2 = 0;
							int num3 = 100;
							foreach (Thing ingredient in unfinishedThing.ingredients)
							{
								num2 += ingredient.stackCount;
							}
							unfinishedThing.ingredients.Clear();
							while (num2 > 0 && num3-- > 0)
							{
								Thing thing = ThingMaker.MakeThing(stuff);
								thing.stackCount = Mathf.Min(num2, stuff.stackLimit);
								unfinishedThing.ingredients.Add(thing);
								num2 -= thing.stackCount;
							}
						}
					}
				}
			}));
		}
		return debugActionNode;
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static DebugActionNode SetQuality()
	{
		DebugActionNode debugActionNode = new DebugActionNode();
		foreach (QualityCategory value in Enum.GetValues(typeof(QualityCategory)))
		{
			QualityCategory qualityInner = value;
			debugActionNode.AddChild(new DebugActionNode(qualityInner.ToString(), DebugActionType.ToolMap, delegate
			{
				foreach (Thing thing in UI.MouseCell().GetThingList(Find.CurrentMap))
				{
					thing.TryGetComp<CompQuality>()?.SetQuality(qualityInner, ArtGenerationContext.Outsider);
				}
			}));
		}
		return debugActionNode;
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
	public static void MeasureDrawSize()
	{
		Vector3 first = Vector3.zero;
		DebugTools.curMeasureTool = new DrawMeasureTool("first corner...", delegate
		{
			first = UI.MouseMapPosition();
			DebugTools.curMeasureTool = new DrawMeasureTool("second corner...", delegate
			{
				Vector3 vector = UI.MouseMapPosition();
				Rect rect = new Rect
				{
					xMin = Mathf.Min(first.x, vector.x),
					yMin = Mathf.Min(first.z, vector.z),
					xMax = Mathf.Max(first.x, vector.x),
					yMax = Mathf.Max(first.z, vector.z)
				};
				string text = $"Center: ({rect.center.x},{rect.center.y})";
				text += $"\nSize: ({rect.size.x},{rect.size.y})";
				if (Find.Selector.SingleSelectedObject != null)
				{
					Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
					Vector3 drawPos = singleSelectedThing.DrawPos;
					Vector2 v = rect.center - new Vector2(drawPos.x, drawPos.z);
					text += $"\nOffset: ({v.x},{v.y})";
					Vector2 vector2 = v.RotatedBy(0f - singleSelectedThing.Rotation.AsAngle);
					text += $"\nUnrotated offset: ({vector2.x},{vector2.y})";
				}
				Log.Message(text);
				MeasureDrawSize();
			}, first);
		});
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
	public static void MeasureWorldDistance()
	{
		Vector3 first = Vector3.zero;
		DebugTools.curMeasureTool = new MeasureWorldDistanceTool("First Point...", delegate
		{
			first = UI.MouseMapPosition();
			DebugTools.curMeasureTool = new MeasureWorldDistanceTool("Second Point...", delegate
			{
				Vector3 vector = UI.MouseMapPosition() - first;
				Log.Message($"Vector: {vector}, Distance:{vector.magnitude}");
				MeasureWorldDistance();
			}, first);
		});
	}

	[DebugAction("General", "Draw Attach Points", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
	public static void DrawAttachPoints()
	{
		foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
		{
			if (!(item is ThingWithComps thingWithComps))
			{
				continue;
			}
			CompAttachPoints comp = thingWithComps.GetComp<CompAttachPoints>();
			if (comp == null)
			{
				continue;
			}
			int ttl = 500;
			DebugTools.curTool = new DebugTool("attach point drawer", delegate
			{
			}, delegate
			{
				ttl--;
				if (ttl <= 0)
				{
					DebugTools.curTool = null;
				}
				foreach (AttachPointType item2 in comp.points.PointTypes())
				{
					Vector3 worldPos = comp.points.GetWorldPos(item2);
					GenMapUI.DrawText(new Vector2(worldPos.x, worldPos.z), item2.ToString(), Color.red);
					GenDraw.DrawCircleOutline(worldPos, 0.4f);
				}
			});
		}
	}

	[DebugAction("General", "Pollution +1%", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresBiotech = true)]
	private static void IncreasePollutionSmall()
	{
		PlanetTile root = GenWorld.MouseTile();
		if (root.Valid)
		{
			WorldPollutionUtility.PolluteWorldAtTile(root, 0.01f);
		}
	}

	[DebugAction("General", "Pollution +25%", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresBiotech = true)]
	private static void IncreasePollutionLarge()
	{
		PlanetTile root = GenWorld.MouseTile();
		if (root.Valid)
		{
			WorldPollutionUtility.PolluteWorldAtTile(root, 0.25f);
		}
	}

	[DebugAction("General", "Pollution -25%", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresBiotech = true)]
	private static void DecreasePollutionLarge()
	{
		PlanetTile root = GenWorld.MouseTile();
		if (root.Valid)
		{
			WorldPollutionUtility.PolluteWorldAtTile(root, -0.25f);
		}
	}

	[DebugAction("General", "Set biome", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
	private static List<DebugActionNode> SetBiome()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (BiomeDef item in DefDatabase<BiomeDef>.AllDefsListForReading)
		{
			BiomeDef local = item;
			if (!item.implemented || !item.generatesNaturally)
			{
				continue;
			}
			list.Add(new DebugActionNode(item.label, DebugActionType.ToolWorld, delegate
			{
				PlanetTile planetTile = GenWorld.MouseTile();
				if (planetTile.Valid && planetTile.Tile.PrimaryBiome != local)
				{
					planetTile.Tile.PrimaryBiome = local;
					Find.World.renderer.GetLayer<WorldDrawLayer_Terrain>(PlanetLayer.Selected).RegenerateNow();
				}
			}));
		}
		return list;
	}

	[DebugAction("General", "Set landmark", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresOdyssey = true)]
	private static List<DebugActionNode> SetLandmark()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (LandmarkDef item in DefDatabase<LandmarkDef>.AllDefsListForReading)
		{
			LandmarkDef local = item;
			list.Add(new DebugActionNode(item.defName, DebugActionType.ToolWorld, delegate
			{
				PlanetTile tile = GenWorld.MouseTile();
				if (tile.Valid && local.IsValidTile(tile, tile.Layer))
				{
					Find.World.landmarks.AddLandmark(local, tile);
					Find.World.renderer.GetLayer<WorldDrawLayer_Terrain>(tile.Layer).RegenerateNow();
					Find.World.renderer.GetLayer<WorldDrawLayer_Landmarks>(Find.WorldGrid.Surface).RegenerateNow();
					Find.World.renderer.GetLayer<WorldDrawLayer_Hills>(Find.WorldGrid.Surface).RegenerateNow();
				}
			}));
		}
		return list;
	}

	[DebugAction("General", "Clear Landmark", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresOdyssey = true)]
	private static void ClearLandmark()
	{
		PlanetTile tile = GenWorld.MouseTile();
		if (tile.Valid)
		{
			Find.World.landmarks.RemoveLandmark(tile);
			Find.World.renderer.GetLayer<WorldDrawLayer_Terrain>(tile.Layer).RegenerateNow();
		}
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresBiotech = true)]
	private static void ResetBossgroupCooldown()
	{
		Find.BossgroupManager.lastBossgroupCalled = Find.TickManager.TicksGame - 120000;
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresBiotech = true)]
	private static void ResetBossgroupKilledPawns()
	{
		Find.BossgroupManager.DebugResetDefeatedPawns();
	}

	[DebugAction("Insect", "Spawn cocoon infestation", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, hideInSubMenu = true, requiresBiotech = true)]
	private static List<DebugActionNode> SpawnCocoonInfestationWithPoints()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (float item2 in DebugActionsUtility.PointsOptions(extended: false))
		{
			float localP = item2;
			DebugActionNode item = new DebugActionNode(localP + " points", DebugActionType.ToolMap, delegate
			{
				CocoonInfestationUtility.SpawnCocoonInfestation(UI.MouseCell(), Find.CurrentMap, localP);
			});
			list.Add(item);
		}
		return list;
	}

	[DebugAction("Anomaly", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresAnomaly = true)]
	private static void SpawnPitGate()
	{
		IntVec3 loc = UI.MouseMapPosition().ToIntVec3();
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.PitGateSpawner), loc, Find.CurrentMap);
	}

	[DebugAction("Anomaly", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresAnomaly = true)]
	private static void SpawnPitBurrow()
	{
		FleshbeastUtility.SpawnFleshbeastsFromPitBurrowEmergence(UI.MouseMapPosition().ToIntVec3(), Find.CurrentMap, 200f, new IntRange(600, 600), new IntRange(60, 180));
	}

	[DebugAction("Anomaly", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresAnomaly = true)]
	private static void SpawnFleshmassHeart()
	{
		IntVec3 loc = UI.MouseMapPosition().ToIntVec3();
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.FleshmassHeartSpawner), loc, Find.CurrentMap);
	}

	[DebugAction("Anomaly", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresAnomaly = true)]
	private static void DiscoverAllEntities()
	{
		Find.EntityCodex.Debug_DiscoverAll();
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action)]
	private static void BenchmarkPerformance()
	{
		Messages.Message($"Running benchmark, results displayed in {30f} seconds", MessageTypeDefOf.NeutralEvent, historical: false);
		PerformanceBenchmarkUtility.StartBenchmark();
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void CompareLineOfSight(Pawn pawn)
	{
		foreach (IntVec3 item in GenRadial.RadialCellsAround(UI.MouseCell(), 50f, useCenter: true))
		{
			if (!item.InBounds(Find.CurrentMap))
			{
				continue;
			}
			Pawn firstPawn = item.GetFirstPawn(Find.CurrentMap);
			if (firstPawn != null)
			{
				bool num = pawn.CanSee(firstPawn);
				bool flag = firstPawn.CanSee(pawn);
				if (num != flag)
				{
					Find.CurrentMap.debugDrawer.FlashCell(item, 1f);
				}
			}
		}
	}

	[DebugAction("General", "Enable wound debug draw", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
	private static void WoundDebug()
	{
		IntVec3 c = UI.MouseCell();
		Pawn pawn = c.GetFirstPawn(Find.CurrentMap);
		if (pawn == null || pawn.def.race == null || pawn.def.race.body == null)
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		list.Add(new DebugMenuOption("All", DebugMenuOptionMode.Action, delegate
		{
			pawn.Drawer.renderer.WoundOverlays.debugDrawAllParts = true;
			pawn.Drawer.renderer.WoundOverlays.ClearCache();
			PortraitsCache.SetDirty(pawn);
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
		}));
		List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
		for (int num = 0; num < allParts.Count; num++)
		{
			BodyPartRecord part = allParts[num];
			list.Add(new DebugMenuOption(part.LabelCap, DebugMenuOptionMode.Action, delegate
			{
				pawn.Drawer.renderer.WoundOverlays.debugDrawPart = part;
				pawn.Drawer.renderer.WoundOverlays.ClearCache();
				PortraitsCache.SetDirty(pawn);
				GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("General", "Wound debug export (non-humanlike)", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
	private static void WoundDebugExport()
	{
		string text = Application.dataPath + "\\woundDump";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		HashSet<RaceProperties> hashSet = new HashSet<RaceProperties>();
		foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef pkd) => !pkd.RaceProps.Humanlike))
		{
			if (!hashSet.Contains(item.RaceProps))
			{
				Pawn pawn = PawnGenerator.GeneratePawn(item);
				for (int num = 0; num < 4; num++)
				{
					Rot4 rot = new Rot4((byte)num);
					RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 32, RenderTextureFormat.ARGB32);
					temporary.name = "WoundDebugExport";
					pawn.Drawer.renderer.WoundOverlays.debugDrawAllParts = true;
					pawn.Drawer.renderer.WoundOverlays.ClearCache();
					Find.PawnCacheRenderer.RenderPawn(pawn, temporary, Vector3.zero, 1f, 0f, rot);
					pawn.Drawer.renderer.WoundOverlays.debugDrawAllParts = false;
					pawn.Drawer.renderer.WoundOverlays.ClearCache();
					Texture2D texture2D = new Texture2D(temporary.width, temporary.height, TextureFormat.ARGB32, 0, linear: false);
					RenderTexture.active = temporary;
					texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0, recalculateMipMaps: true);
					RenderTexture.active = null;
					RenderTexture.ReleaseTemporary(temporary);
					string text2 = text + "\\" + pawn.def.LabelCap + "_";
					Rot4 rot2 = rot;
					File.WriteAllBytes(text2 + rot2.ToString() + ".png", texture2D.EncodeToPNG());
				}
				pawn.Destroy();
				hashSet.Add(item.RaceProps);
			}
		}
		Log.Message("Dumped to " + text);
	}

	[DebugAction("Anomaly", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresAnomaly = true)]
	private static List<DebugActionNode> EmergeMetalhorrors()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		for (int i = 1; i <= 10; i++)
		{
			int delaySeconds = i;
			list.Add(new DebugActionNode($"{i}s delay")
			{
				action = delegate
				{
					int delayTicks = delaySeconds * 60;
					DelayedMetalhorrorEmerger.Spawn(Find.CurrentMap, delayTicks);
				}
			});
		}
		return list;
	}

	[DebugAction("Mechanoid", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresOdyssey = true)]
	private static void DisableMechs()
	{
		if (Faction.OfMechanoids != null)
		{
			Faction.OfMechanoids.deactivated = true;
		}
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresOdyssey = true)]
	private static void AddTraitToUniqueWeapon()
	{
		CompUniqueWeapon comp = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).FirstOrDefault((Thing x) => x.HasComp<CompUniqueWeapon>()).TryGetComp<CompUniqueWeapon>();
		if (comp == null)
		{
			return;
		}
		tmpTraitDefs.Clear();
		tmpTraitDefs.AddRange(DefDatabase<WeaponTraitDef>.AllDefs.Where((WeaponTraitDef trait) => comp.CanAddTrait(trait)));
		if (!tmpTraitDefs.Any())
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (WeaponTraitDef tmpTraitDef in tmpTraitDefs)
		{
			WeaponTraitDef localTrait = tmpTraitDef;
			list.Add(new DebugMenuOption(tmpTraitDef.LabelCap, DebugMenuOptionMode.Action, delegate
			{
				comp.AddTrait(localTrait);
				comp.Setup(fromSave: true);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("General", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresOdyssey = true)]
	private static void RemoveTraitFromUniqueWeapon()
	{
		CompUniqueWeapon comp = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).FirstOrDefault((Thing x) => x.HasComp<CompUniqueWeapon>()).TryGetComp<CompUniqueWeapon>();
		if (comp == null)
		{
			return;
		}
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (WeaponTraitDef item in comp.TraitsListForReading)
		{
			WeaponTraitDef localTrait = item;
			list.Add(new DebugMenuOption(item.LabelCap, DebugMenuOptionMode.Action, delegate
			{
				comp.TraitsListForReading.Remove(localTrait);
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}
}
