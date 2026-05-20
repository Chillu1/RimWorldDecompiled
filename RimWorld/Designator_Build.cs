using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Build : Designator_Place
{
	protected BuildableDef entDef;

	public Precept_Building sourcePrecept;

	public ThingStyleDef styleDef;

	public bool styleOverridden;

	public ColorInt? glowerColorOverride;

	private ThingDef stuffDef;

	private ThingDef resetStuffAfterDeselect;

	private bool writeStuff;

	public const float DragPriceDrawNumberX = 29f;

	public const float ResLineSpacing = 29f;

	private static List<ThingDefCountClass> tmpcostList = new List<ThingDefCountClass>();

	public override BuildableDef PlacingDef => entDef;

	public override ThingDef StuffDef
	{
		get
		{
			if (stuffDef != null)
			{
				return stuffDef;
			}
			ThingDef thingDef = null;
			if (entDef is ThingDef { MadeFromStuff: not false } thingDef2)
			{
				thingDef = GenStuff.DefaultStuffFor(thingDef2);
				if (Find.CurrentMap != null && Find.CurrentMap.resourceCounter.GetCount(thingDef) < thingDef2.CostStuffCount)
				{
					ThingDef thingDef3 = null;
					foreach (KeyValuePair<ThingDef, int> allCountedAmount in Find.CurrentMap.resourceCounter.AllCountedAmounts)
					{
						if (allCountedAmount.Key.IsStuff && allCountedAmount.Key.stuffProps.canSuggestUseDefaultStuff && allCountedAmount.Key.stuffProps.CanMake(thingDef2) && allCountedAmount.Value >= thingDef2.CostStuffCount)
						{
							thingDef3 = allCountedAmount.Key;
							break;
						}
					}
					if (thingDef3 != null)
					{
						thingDef = thingDef3;
					}
				}
			}
			return thingDef;
		}
	}

	public ThingDef StuffDefRaw => stuffDef;

	public override string Label
	{
		get
		{
			ThingDef thingDef = entDef as ThingDef;
			string text = ((thingDef == null || !writeStuff) ? entDef.label : GenLabel.ThingLabel(thingDef, StuffDef));
			if (sourcePrecept != null)
			{
				text = sourcePrecept.TransformThingLabel(text);
			}
			if (thingDef != null && !writeStuff && thingDef.MadeFromStuff)
			{
				text += "...";
			}
			return text;
		}
	}

	public override string Desc => entDef.description;

	public override Color IconDrawColor
	{
		get
		{
			if (entDef is ThingDef def)
			{
				Color? ideoColorForBuilding = IdeoUtility.GetIdeoColorForBuilding(def, Faction.OfPlayer);
				if (ideoColorForBuilding.HasValue)
				{
					return ideoColorForBuilding.Value;
				}
			}
			if (StuffDef != null)
			{
				return entDef.GetColorForStuff(StuffDef);
			}
			return entDef.uiIconColor;
		}
	}

	public override bool Visible
	{
		get
		{
			if (DebugSettings.godMode)
			{
				return true;
			}
			if (entDef.minTechLevelToBuild != TechLevel.Undefined && (int)Faction.OfPlayer.def.techLevel < (int)entDef.minTechLevelToBuild)
			{
				return false;
			}
			if (entDef.maxTechLevelToBuild != TechLevel.Undefined && (int)Faction.OfPlayer.def.techLevel > (int)entDef.maxTechLevelToBuild)
			{
				return false;
			}
			if (!entDef.IsResearchFinished)
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && entDef.minMonolithLevel > Find.Anomaly.HighestLevelReached && Find.Anomaly.GenerateMonolith)
			{
				return false;
			}
			if (!Find.Storyteller.difficulty.AllowedToBuild(entDef))
			{
				return false;
			}
			if (entDef.PlaceWorkers != null)
			{
				foreach (PlaceWorker placeWorker in entDef.PlaceWorkers)
				{
					if (!placeWorker.IsBuildDesignatorVisible(entDef))
					{
						return false;
					}
				}
			}
			if (entDef.buildingPrerequisites != null)
			{
				for (int i = 0; i < entDef.buildingPrerequisites.Count; i++)
				{
					if (!base.Map.listerBuildings.ColonistsHaveBuilding(entDef.buildingPrerequisites[i]))
					{
						return false;
					}
				}
			}
			if (entDef.discoveryPrerequisites != null)
			{
				for (int j = 0; j < entDef.discoveryPrerequisites.Count; j++)
				{
					if (Find.HiddenItemsManager.Hidden(entDef.discoveryPrerequisites[j]))
					{
						return false;
					}
				}
			}
			if (ModsConfig.OdysseyActive && entDef.requireInspectedGravEngine && !Find.ResearchManager.gravEngineInspected)
			{
				return false;
			}
			return true;
		}
	}

	public override DrawStyleCategoryDef DrawStyleCategory
	{
		get
		{
			if (!eyedropMode)
			{
				return entDef.drawStyleCategory;
			}
			return eyedropper.DrawStyleCategory;
		}
	}

	public override bool DragDrawMeasurements => true;

	public override float PanelReadoutTitleExtraRightMargin => 20f;

	public override string HighlightTag
	{
		get
		{
			if (cachedHighlightTag == null && tutorTag != null)
			{
				cachedHighlightTag = "Designator-Build-" + tutorTag;
			}
			return cachedHighlightTag;
		}
	}

	public override ThingStyleDef ThingStyleDefForPreview
	{
		get
		{
			if (sourcePrecept == null)
			{
				return ThingStyleDefNonPreceptSource;
			}
			if (entDef is ThingDef thingDef)
			{
				return sourcePrecept.ideo.GetStyleFor(thingDef);
			}
			return null;
		}
	}

	public ThingStyleDef ThingStyleDefNonPreceptSource
	{
		get
		{
			if (PlacingDef is ThingDef thingDef)
			{
				if (Find.IdeoManager.classicMode && styleOverridden)
				{
					return styleDef;
				}
				return Faction.OfPlayer.ideos?.PrimaryIdeo?.GetStyleFor(thingDef);
			}
			return null;
		}
	}

	public override float Order => entDef.uiOrder;

	public override void RenderHighlight(List<IntVec3> dragCells)
	{
		DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
	}

	private void UpdateIcon()
	{
		icon = entDef.GetUIIconForStuff(StuffDef);
		if (entDef is TerrainDef { cropIcon: not false })
		{
			iconTexCoords = Widgets.CroppedTerrainTextureRect((Texture2D)icon);
		}
	}

	public override void Deselected()
	{
		if (resetStuffAfterDeselect != null)
		{
			stuffDef = resetStuffAfterDeselect;
		}
		styleOverridden = false;
		glowerColorOverride = null;
		resetStuffAfterDeselect = null;
		UpdateIcon();
	}

	public Designator_Build(BuildableDef entDef)
	{
		this.entDef = entDef;
		iconAngle = entDef.uiIconAngle;
		iconOffset = entDef.uiIconOffset;
		hotKey = entDef.designationHotKey;
		tutorTag = entDef.defName;
		Order = 2000f;
		if (entDef is ThingDef thingDef)
		{
			iconProportions = (thingDef.defaultPlacingRot.IsHorizontal ? thingDef.graphicData.drawSize.Rotated() : thingDef.graphicData.drawSize);
			iconDrawScale = GenUI.IconDrawScale(thingDef);
		}
		else
		{
			iconProportions = new Vector2(1f, 1f);
			iconDrawScale = 1f;
		}
		ResetStuffToDefault();
		BuildableDef placingDef = PlacingDef;
		TerrainDef terrain = placingDef as TerrainDef;
		if (terrain == null || terrain.colorDef == null || terrain.designatorDropdown == null || !terrain.designatorDropdown.includeEyeDropperTool)
		{
			return;
		}
		eyedropper = new Designator_Eyedropper(delegate(ColorDef newCol)
		{
			foreach (TerrainDef item in DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef x) => x.designatorDropdown == terrain.designatorDropdown))
			{
				if (item.colorDef == newCol && BuildCopyCommandUtility.FindAllowedDesignatorRoot(item) is Designator_Dropdown designator_Dropdown)
				{
					Log.Message("Selecting dropdown");
					Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(item);
					designator_Dropdown.SetActiveDesignator(des);
					Find.DesignatorManager.Select(des);
					break;
				}
			}
		}, "SelectColoredFloor".Translate(), "DesignatorEyeDropperDesc_Carpet".Translate());
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
		if (entDef is ThingDef { MadeFromStuff: not false })
		{
			Designator_Dropdown.DrawExtraOptionsIcon(topLeft, GetWidth(maxWidth));
		}
		return result;
	}

	public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
	{
		ThingStyleDef thingStyleDef = ((PlacingDef is ThingDef) ? ThingStyleDefForPreview : null);
		Color? color = (parms.lowLight ? new Color?(Command.LowLightIconColor) : ((Color?)null));
		Color valueOrDefault = color.GetValueOrDefault();
		if (!color.HasValue)
		{
			valueOrDefault = IconDrawColor;
			color = valueOrDefault;
		}
		Widgets.DefIcon(rect, PlacingDef, StuffDef, 0.85f, thingStyleDef, drawPlaceholder: false, color, overrideMaterial);
	}

	public Texture ResolvedIcon(ThingStyleDef styleDef = null)
	{
		if (entDef is ThingDef thingDef)
		{
			ThingStyleDef thingStyleDef = styleDef ?? ThingStyleDefNonPreceptSource;
			return Widgets.GetIconFor(thingDef, StuffDef, thingStyleDef);
		}
		if (StuffDef != null && entDef.graphic is Graphic_Appearances graphic_Appearances)
		{
			return (Texture2D)graphic_Appearances.SubGraphicFor(StuffDef).MatAt(entDef.defaultPlacingRot).mainTexture;
		}
		return icon;
	}

	public override void DrawMouseAttachments()
	{
		eyedropMode = eyedropper != null && KeyBindingDefOf.ShowEyedropper.IsDown;
		if (eyedropMode)
		{
			eyedropper.DrawMouseAttachments();
		}
		else
		{
			base.DrawMouseAttachments();
		}
	}

	public void ResetStuffToDefault()
	{
		stuffDef = null;
		UpdateIcon();
	}

	protected override void DrawPlaceMouseAttachments(float curX, ref float curY)
	{
		base.DrawPlaceMouseAttachments(curX, ref curY);
		if (ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
		{
			return;
		}
		if (eyedropper != null)
		{
			Rect rect = new Rect(curX, curY, 9999f, Text.LineHeight * 2f);
			Widgets.Label(rect, "Color".Translate() + ": " + ((TerrainDef)PlacingDef).colorDef.LabelCap + "\n" + KeyBindingDefOf.ShowEyedropper.MainKeyLabel + ": " + "GrabExistingColor".Translate());
			curY += rect.height;
		}
		DesignationDragger dragger = Find.DesignatorManager.Dragger;
		int num = ((!dragger.Dragging) ? 1 : dragger.DragCells.Count());
		List<ThingDefCountClass> list = entDef.CostListAdjusted(StuffDef);
		for (int i = 0; i < list.Count; i++)
		{
			ThingDefCountClass thingDefCountClass = list[i];
			float y = curY;
			Widgets.ThingIcon(new Rect(curX, y, 27f, 27f), thingDefCountClass.thingDef);
			Rect rect2 = new Rect(curX + 29f, y, 999f, 29f);
			int num2 = num * thingDefCountClass.count;
			string text = num2.ToString();
			if (base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < num2)
			{
				GUI.color = Color.red;
				text += " (" + "NotEnoughStoredLower".Translate() + ")";
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			curY += 29f;
		}
	}

	public override void ProcessInput(Event ev)
	{
		if (!CheckCanInteract())
		{
			return;
		}
		if (entDef is ThingDef { MadeFromStuff: not false } thingDef)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ThingDef item in from d in base.Map.resourceCounter.AllCountedAmounts.Keys
				orderby d.stuffProps?.commonality ?? float.PositiveInfinity descending, d.BaseMarketValue
				select d)
			{
				if (item.IsStuff && item.stuffProps.CanMake(thingDef) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(item).Count > 0))
				{
					ThingDef localStuffDef = item;
					string str = ((sourcePrecept == null) ? GenLabel.ThingLabel(entDef, localStuffDef) : ((string)"ThingMadeOfStuffLabel".Translate(localStuffDef.LabelAsStuff, sourcePrecept.Label)));
					str = str.CapitalizeFirst();
					FloatMenuOption floatMenuOption = new FloatMenuOption(str, delegate
					{
						base.ProcessInput(ev);
						Find.DesignatorManager.Select(this);
						stuffDef = localStuffDef;
						writeStuff = true;
					}, item);
					floatMenuOption.tutorTag = "SelectStuff-" + thingDef.defName + "-" + localStuffDef.defName;
					list.Add(floatMenuOption);
				}
			}
			if (list.Count == 0)
			{
				Messages.Message("NoStuffsToBuildWith".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			FloatMenu floatMenu = new FloatMenu(list);
			floatMenu.onCloseCallback = delegate
			{
				writeStuff = true;
			};
			Find.WindowStack.Add(floatMenu);
			Find.DesignatorManager.Select(this);
		}
		else
		{
			base.ProcessInput(ev);
		}
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (eyedropMode)
		{
			return eyedropper.CanDesignateCell(c);
		}
		return GenConstruct.CanPlaceBlueprintAt(entDef, c, placingRot, base.Map, DebugSettings.godMode, null, null, StuffDef);
	}

	public override void DesignateSingleCell(IntVec3 c)
	{
		if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
		{
			return;
		}
		if (eyedropMode)
		{
			eyedropper.DesignateSingleCell(c);
			return;
		}
		List<Thing> thingList = c.GetThingList(base.Map);
		for (int num = thingList.Count - 1; num >= 0; num--)
		{
			if (thingList[num] is Frame frame && entDef.blueprintDef.replaceTags.NotNullAndContainsAnyElement(frame.def.replaceTags))
			{
				frame.Destroy(DestroyMode.Cancel);
			}
		}
		if (DebugSettings.godMode || entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, StuffDef) == 0f)
		{
			if (entDef is TerrainDef terrainDef)
			{
				base.Map.terrainGrid.RemoveTempTerrain(c);
				if (terrainDef.isFoundation)
				{
					if (base.Map.terrainGrid.CanRemoveTopLayerAt(c))
					{
						base.Map.terrainGrid.RemoveTopLayer(c, !DebugSettings.godMode);
					}
					base.Map.terrainGrid.SetFoundation(c, terrainDef);
				}
				else if (terrainDef.temporary)
				{
					base.Map.terrainGrid.SetTempTerrain(c, terrainDef);
				}
				else
				{
					base.Map.terrainGrid.SetTerrain(c, terrainDef);
				}
			}
			else
			{
				Thing thing = ThingMaker.MakeThing((ThingDef)entDef, StuffDef);
				thing.SetFactionDirect(Faction.OfPlayer);
				Thing thing2 = GenSpawn.Spawn(thing, c, base.Map, placingRot);
				if (sourcePrecept != null && thing2 is Building building)
				{
					building.StyleSourcePrecept = sourcePrecept;
				}
				else if (sourcePrecept == null)
				{
					thing2.StyleDef = ThingStyleDefNonPreceptSource;
				}
				if (glowerColorOverride.HasValue)
				{
					CompGlower compGlower = thing2.TryGetComp<CompGlower>();
					if (compGlower != null)
					{
						compGlower.GlowColor = glowerColorOverride.Value;
					}
				}
			}
		}
		else
		{
			GenSpawn.WipeExistingThings(c, placingRot, entDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
			Blueprint_Build blueprint_Build = GenConstruct.PlaceBlueprintForBuild(entDef, c, base.Map, placingRot, Faction.OfPlayer, StuffDef);
			blueprint_Build.StyleSourcePrecept = sourcePrecept;
			if (blueprint_Build.StyleSourcePrecept == null)
			{
				blueprint_Build.StyleDef = ThingStyleDefNonPreceptSource;
			}
			blueprint_Build.glowerColorOverride = glowerColorOverride;
		}
		FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, placingRot, entDef.Size), base.Map);
		if (entDef is ThingDef { IsOrbitalTradeBeacon: not false })
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);
		}
		if (TutorSystem.TutorialMode)
		{
			TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));
		}
		if (entDef.PlaceWorkers != null)
		{
			for (int i = 0; i < entDef.PlaceWorkers.Count; i++)
			{
				entDef.PlaceWorkers[i].PostPlace(base.Map, entDef, c, placingRot);
			}
		}
	}

	public override void SelectedUpdate()
	{
		base.SelectedUpdate();
		BuildDesignatorUtility.TryDrawPowerGridAndAnticipatedConnection(entDef, placingRot);
	}

	public override void DrawPanelReadout(ref float curY, float width)
	{
		if (entDef.CostStuffCount <= 0 && stuffDef != null)
		{
			stuffDef = null;
		}
		ThingDef thingDef = entDef as ThingDef;
		if (thingDef != null)
		{
			Widgets.InfoCardButton(width - 24f - 2f, 6f, thingDef, StuffDef);
		}
		else
		{
			Widgets.InfoCardButton(width - 24f - 2f, 6f, entDef);
		}
		Text.Font = GameFont.Small;
		tmpcostList.Clear();
		tmpcostList.AddRange(entDef.CostListAdjusted(StuffDef, errorOnNullStuff: false));
		tmpcostList.Sort((ThingDefCountClass a, ThingDefCountClass b) => b.count - a.count);
		for (int num = 0; num < tmpcostList.Count; num++)
		{
			ThingDefCountClass thingDefCountClass = tmpcostList[num];
			Color color = GUI.color;
			Widgets.ThingIcon(new Rect(0f, curY, 20f, 20f), thingDefCountClass.thingDef);
			GUI.color = color;
			if (thingDefCountClass.thingDef != null && thingDefCountClass.thingDef.resourceReadoutPriority != ResourceCountPriority.Uncounted && base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < thingDefCountClass.count)
			{
				GUI.color = Color.red;
			}
			Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingDefCountClass.count.ToString());
			GUI.color = Color.white;
			string text = ((thingDefCountClass.thingDef != null) ? ((string)thingDefCountClass.thingDef.LabelCap) : ((string)("(" + "UnchosenStuff".Translate() + ")")));
			float width2 = width - 60f;
			float num2 = Text.CalcHeight(text, width2) - 5f;
			Widgets.Label(new Rect(60f, curY + 2f, width2, num2 + 5f), text);
			curY += num2;
		}
		StyleCategoryDef styleCategoryDef = entDef.dominantStyleCategory;
		if (styleCategoryDef == null && thingDef != null)
		{
			styleCategoryDef = Faction.OfPlayer.ideos?.PrimaryIdeo?.GetStyleCategoryFor(thingDef);
		}
		if (styleCategoryDef != null)
		{
			TaggedString taggedString = "DominantStyle".Translate().CapitalizeFirst() + ": " + styleCategoryDef.LabelCap;
			Rect rect = new Rect(0f, curY + 2f, width, Text.CalcHeight(taggedString, width));
			Widgets.Label(rect, taggedString);
			curY += rect.height;
		}
		if (entDef.constructionSkillPrerequisite > 0)
		{
			DrawSkillRequirement(SkillDefOf.Construction, entDef.constructionSkillPrerequisite, width, ref curY);
		}
		if (entDef.artisticSkillPrerequisite > 0)
		{
			DrawSkillRequirement(SkillDefOf.Artistic, entDef.artisticSkillPrerequisite, width, ref curY);
		}
		bool flag = false;
		foreach (Pawn freeColonist in Find.CurrentMap.mapPawns.FreeColonists)
		{
			if (freeColonist.skills.GetSkill(SkillDefOf.Construction).Level >= entDef.constructionSkillPrerequisite && freeColonist.skills.GetSkill(SkillDefOf.Artistic).Level >= entDef.artisticSkillPrerequisite)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			TaggedString taggedString2 = "NoColonistWithAllSkillsForConstructing".Translate(Faction.OfPlayer.def.pawnsPlural);
			GUI.color = Color.red;
			if (AnyMechWithSkillsRequired(entDef, out var pawn))
			{
				taggedString2 += " " + "MechCanBuildThis".Translate(pawn.kindDef.label);
				GUI.color = Color.white;
			}
			Rect rect2 = new Rect(0f, curY + 2f, width, Text.CalcHeight(taggedString2, width));
			Widgets.Label(rect2, taggedString2);
			GUI.color = Color.white;
			curY += rect2.height;
		}
		curY += 4f;
	}

	private bool AnyColonistWithSkill(int skill, SkillDef skillDef, bool careIfDisabled)
	{
		foreach (Pawn freeColonist in Find.CurrentMap.mapPawns.FreeColonists)
		{
			if (freeColonist.skills.GetSkill(skillDef).Level >= skill && (!careIfDisabled || freeColonist.workSettings.WorkIsActive(WorkTypeDefOf.Construction)))
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyMechWithSkillsRequired(BuildableDef def, out Pawn pawn)
	{
		return MechanitorUtility.AnyPlayerMechCanDoWork(WorkTypeDefOf.Construction, def.constructionSkillPrerequisite, out pawn);
	}

	private void DrawSkillRequirement(SkillDef skillDef, int requirement, float width, ref float curY)
	{
		Rect rect = new Rect(0f, curY + 2f, width, 24f);
		if (!AnyColonistWithSkill(requirement, skillDef, careIfDisabled: false))
		{
			GUI.color = Color.red;
			TooltipHandler.TipRegionByKey(rect, "NoColonistWithSkillTip", Faction.OfPlayer.def.pawnsPlural);
		}
		else if (!AnyColonistWithSkill(requirement, skillDef, careIfDisabled: true))
		{
			GUI.color = Color.yellow;
			TooltipHandler.TipRegionByKey(rect, "AllColonistsWithSkillHaveDisabledConstructingTip", Faction.OfPlayer.def.pawnsPlural, WorkTypeDefOf.Construction.gerundLabel);
		}
		else
		{
			GUI.color = new Color(0.72f, 0.87f, 0.72f);
		}
		Widgets.Label(rect, string.Format("{0}: {1}", "SkillNeededForConstructing".Translate(skillDef.LabelCap), requirement));
		GUI.color = Color.white;
		curY += 18f;
	}

	public void SetStuffDef(ThingDef stuffDef)
	{
		this.stuffDef = stuffDef;
		UpdateIcon();
	}

	public void SetTemporaryVars(ThingDef stuffDef, bool styleOverridden)
	{
		resetStuffAfterDeselect = this.stuffDef;
		this.stuffDef = stuffDef;
		this.styleOverridden = styleOverridden;
		UpdateIcon();
	}
}
