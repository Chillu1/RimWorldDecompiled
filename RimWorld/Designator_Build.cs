using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Build : Designator_Place
	{
		protected BuildableDef entDef;

		private ThingDef stuffDef;

		private bool writeStuff;

		private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);

		private const float DragPriceDrawNumberX = 29f;

		public override BuildableDef PlacingDef => entDef;

		public override string Label
		{
			get
			{
				ThingDef thingDef = entDef as ThingDef;
				if (thingDef != null && writeStuff)
				{
					return GenLabel.ThingLabel(thingDef, stuffDef);
				}
				if (thingDef != null && thingDef.MadeFromStuff)
				{
					return entDef.label + "...";
				}
				return entDef.label;
			}
		}

		public override string Desc => entDef.description;

		public override Color IconDrawColor
		{
			get
			{
				if (stuffDef != null)
				{
					return entDef.GetColorForStuff(stuffDef);
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
				if (entDef.minTechLevelToBuild != 0 && (int)Faction.OfPlayer.def.techLevel < (int)entDef.minTechLevelToBuild)
				{
					return false;
				}
				if (entDef.maxTechLevelToBuild != 0 && (int)Faction.OfPlayer.def.techLevel > (int)entDef.maxTechLevelToBuild)
				{
					return false;
				}
				if (!entDef.IsResearchFinished)
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
				return true;
			}
		}

		public override int DraggableDimensions => entDef.placingDraggableDimensions;

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

		public Designator_Build(BuildableDef entDef)
		{
			this.entDef = entDef;
			icon = entDef.uiIcon;
			iconAngle = entDef.uiIconAngle;
			iconOffset = entDef.uiIconOffset;
			hotKey = entDef.designationHotKey;
			tutorTag = entDef.defName;
			order = 20f;
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null)
			{
				iconProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
				iconDrawScale = GenUI.IconDrawScale(thingDef);
			}
			else
			{
				iconProportions = new Vector2(1f, 1f);
				iconDrawScale = 1f;
			}
			if (entDef is TerrainDef)
			{
				iconTexCoords = Widgets.CroppedTerrainTextureRect(icon);
			}
			ResetStuffToDefault();
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null && thingDef.MadeFromStuff)
			{
				Designator_Dropdown.DrawExtraOptionsIcon(topLeft, GetWidth(maxWidth));
			}
			return result;
		}

		protected override void DrawIcon(Rect rect, Material buttonMat = null)
		{
			Widgets.DefIcon(rect, PlacingDef, stuffDef, 0.85f);
		}

		public Texture2D ResolvedIcon()
		{
			Graphic_Appearances graphic_Appearances;
			if (stuffDef != null && (graphic_Appearances = (entDef.graphic as Graphic_Appearances)) != null)
			{
				return (Texture2D)graphic_Appearances.SubGraphicFor(stuffDef).MatAt(entDef.defaultPlacingRot).mainTexture;
			}
			return icon;
		}

		public void ResetStuffToDefault()
		{
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null && thingDef.MadeFromStuff)
			{
				stuffDef = GenStuff.DefaultStuffFor(thingDef);
			}
		}

		public override void DrawMouseAttachments()
		{
			base.DrawMouseAttachments();
			if (ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				return;
			}
			DesignationDragger dragger = Find.DesignatorManager.Dragger;
			int num = (!dragger.Dragging) ? 1 : dragger.DragCells.Count();
			float num2 = 0f;
			Vector2 vector = Event.current.mousePosition + DragPriceDrawOffset;
			List<ThingDefCountClass> list = entDef.CostListAdjusted(stuffDef);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = list[i];
				float y = vector.y + num2;
				Widgets.ThingIcon(new Rect(vector.x, y, 27f, 27f), thingDefCountClass.thingDef);
				Rect rect = new Rect(vector.x + 29f, y, 999f, 29f);
				int num3 = num * thingDefCountClass.count;
				string text = num3.ToString();
				if (base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < num3)
				{
					GUI.color = Color.red;
					text += " (" + "NotEnoughStoredLower".Translate() + ")";
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect, text);
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				num2 += 29f;
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (!CheckCanInteract())
			{
				return;
			}
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef == null || !thingDef.MadeFromStuff)
			{
				base.ProcessInput(ev);
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (ThingDef key in base.Map.resourceCounter.AllCountedAmounts.Keys)
			{
				if (key.IsStuff && key.stuffProps.CanMake(thingDef) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(key).Count > 0))
				{
					ThingDef localStuffDef = key;
					FloatMenuOption floatMenuOption = new FloatMenuOption(GenLabel.ThingLabel(entDef, localStuffDef).CapitalizeFirst(), delegate
					{
						base.ProcessInput(ev);
						Find.DesignatorManager.Select(this);
						stuffDef = localStuffDef;
						writeStuff = true;
					}, key);
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
			floatMenu.vanishIfMouseDistant = true;
			floatMenu.onCloseCallback = delegate
			{
				writeStuff = true;
			};
			Find.WindowStack.Add(floatMenu);
			Find.DesignatorManager.Select(this);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			return GenConstruct.CanPlaceBlueprintAt(entDef, c, placingRot, base.Map, DebugSettings.godMode, null, null, stuffDef);
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
			{
				return;
			}
			if (DebugSettings.godMode || entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffDef) == 0f)
			{
				if (entDef is TerrainDef)
				{
					base.Map.terrainGrid.SetTerrain(c, (TerrainDef)entDef);
				}
				else
				{
					Thing thing = ThingMaker.MakeThing((ThingDef)entDef, stuffDef);
					thing.SetFactionDirect(Faction.OfPlayer);
					GenSpawn.Spawn(thing, c, base.Map, placingRot);
				}
			}
			else
			{
				GenSpawn.WipeExistingThings(c, placingRot, entDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
				GenConstruct.PlaceBlueprintForBuild(entDef, c, base.Map, placingRot, Faction.OfPlayer, stuffDef);
			}
			MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, placingRot, entDef.Size), base.Map);
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null && thingDef.IsOrbitalTradeBeacon)
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
			if (entDef.costStuffCount <= 0 && stuffDef != null)
			{
				stuffDef = null;
			}
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null)
			{
				Widgets.InfoCardButton(width - 24f - 2f, 6f, thingDef, stuffDef);
			}
			else
			{
				Widgets.InfoCardButton(width - 24f - 2f, 6f, entDef);
			}
			Text.Font = GameFont.Small;
			List<ThingDefCountClass> list = entDef.CostListAdjusted(stuffDef, errorOnNullStuff: false);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = list[i];
				Color color = GUI.color;
				Widgets.ThingIcon(new Rect(0f, curY, 20f, 20f), thingDefCountClass.thingDef);
				GUI.color = color;
				if (thingDefCountClass.thingDef != null && thingDefCountClass.thingDef.resourceReadoutPriority != 0 && base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < thingDefCountClass.count)
				{
					GUI.color = Color.red;
				}
				Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingDefCountClass.count.ToString());
				GUI.color = Color.white;
				string text = (thingDefCountClass.thingDef != null) ? ((string)thingDefCountClass.thingDef.LabelCap) : ((string)("(" + "UnchosenStuff".Translate() + ")"));
				float width2 = width - 60f;
				float num = Text.CalcHeight(text, width2) - 5f;
				Widgets.Label(new Rect(60f, curY + 2f, width2, num + 5f), text);
				curY += num;
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
				TaggedString taggedString = "NoColonistWithAllSkillsForConstructing".Translate(Faction.OfPlayer.def.pawnsPlural);
				Rect rect = new Rect(0f, curY + 2f, width, Text.CalcHeight(taggedString, width));
				GUI.color = Color.red;
				Widgets.Label(rect, taggedString);
				GUI.color = Color.white;
				curY += rect.height;
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
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
