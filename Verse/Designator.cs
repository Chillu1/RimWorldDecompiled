using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

public abstract class Designator : Command
{
	protected bool useMouseIcon;

	public bool isOrder;

	public SoundDef soundDragSustain;

	public SoundDef soundDragChanged;

	public SoundDef soundSucceeded;

	protected SoundDef soundFailed = SoundDefOf.Designate_Failed;

	protected bool hasDesignateAllFloatMenuOption;

	protected string designateAllLabel;

	protected bool showReverseDesignatorDisabledReason;

	protected DesignationDef[] removeAllOtherDesignationDefs;

	private string cachedTutorTagSelect;

	private string cachedTutorTagDesignate;

	protected string cachedHighlightTag;

	private List<Designation> tmpAllDesignations = new List<Designation>();

	private const float IconButtonSize = 48f;

	private const float IconButtonGap = 16f;

	private const float StyleButtonSize = 60f;

	private const float StyleButtonLabelOffset = 27f;

	public Map Map => Find.CurrentMap;

	public virtual bool DragDrawMeasurements => false;

	public virtual bool DrawHighlight => true;

	protected override bool DoTooltip => false;

	public virtual bool AlwaysDoGuiControls => false;

	protected virtual DesignationDef Designation => null;

	public virtual float PanelReadoutTitleExtraRightMargin => 0f;

	public virtual DrawStyleCategoryDef DrawStyleCategory => null;

	public override string TutorTagSelect
	{
		get
		{
			if (tutorTag == null)
			{
				return null;
			}
			return cachedTutorTagSelect ?? (cachedTutorTagSelect = "SelectDesignator-" + tutorTag);
		}
	}

	public string TutorTagDesignate
	{
		get
		{
			if (tutorTag == null)
			{
				return null;
			}
			return cachedTutorTagDesignate ?? (cachedTutorTagDesignate = "Designate-" + tutorTag);
		}
	}

	public override string HighlightTag
	{
		get
		{
			if (cachedHighlightTag == null && tutorTag != null)
			{
				cachedHighlightTag = "Designator-" + tutorTag;
			}
			return cachedHighlightTag;
		}
	}

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			foreach (FloatMenuOption rightClickFloatMenuOption in base.RightClickFloatMenuOptions)
			{
				yield return rightClickFloatMenuOption;
			}
			if (hasDesignateAllFloatMenuOption)
			{
				int num = 0;
				List<Thing> things = Map.listerThings.AllThings;
				for (int i = 0; i < things.Count; i++)
				{
					Thing t = things[i];
					if (!t.Fogged() && CanDesignateThing(t).Accepted)
					{
						num++;
					}
				}
				if (num > 0)
				{
					yield return new FloatMenuOption(designateAllLabel + " (" + "CountToDesignate".Translate(num) + ")", delegate
					{
						for (int j = 0; j < things.Count; j++)
						{
							Thing t2 = things[j];
							if (!t2.Fogged() && CanDesignateThing(t2).Accepted)
							{
								DesignateThing(things[j]);
							}
						}
					});
				}
				else
				{
					yield return new FloatMenuOption(designateAllLabel + " (" + "NoneLower".Translate() + ")", null);
				}
			}
			DesignationDef designation = Designation;
			if (Designation == null)
			{
				yield break;
			}
			tmpAllDesignations.Clear();
			tmpAllDesignations.AddRange(Map.designationManager.designationsByDef[designation]);
			if (removeAllOtherDesignationDefs != null)
			{
				DesignationDef[] array = removeAllOtherDesignationDefs;
				foreach (DesignationDef def in array)
				{
					tmpAllDesignations.AddRange(Map.designationManager.designationsByDef[def]);
				}
			}
			int num3 = 0;
			foreach (Designation tmpAllDesignation in tmpAllDesignations)
			{
				if (RemoveAllDesignationsAffects(tmpAllDesignation.target))
				{
					num3++;
				}
			}
			if (num3 > 0)
			{
				yield return new FloatMenuOption(string.Concat("RemoveAllDesignations".Translate() + " (", num3.ToString(), ")"), delegate
				{
					for (int num4 = tmpAllDesignations.Count - 1; num4 >= 0; num4--)
					{
						Map.designationManager.RemoveDesignation(tmpAllDesignations[num4]);
					}
				});
			}
			else
			{
				yield return new FloatMenuOption("RemoveAllDesignations".Translate() + " (" + "NoneLower".Translate() + ")", null);
			}
		}
	}

	public Designator()
	{
		activateSound = SoundDefOf.Tick_Tiny;
		designateAllLabel = "DesignateAll".Translate();
	}

	protected bool CheckCanInteract()
	{
		if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(TutorTagSelect))
		{
			return false;
		}
		return true;
	}

	public override void ProcessInput(Event ev)
	{
		if (CheckCanInteract())
		{
			base.ProcessInput(ev);
			Find.DesignatorManager.Select(this);
		}
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
		if (DebugViewSettings.showArchitectMenuOrder)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			Widgets.Label(new Rect(topLeft.x, topLeft.y + 5f, GetWidth(maxWidth), 15f), Order.ToString());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
		}
		return result;
	}

	public Command_Action CreateReverseDesignationGizmo(Thing t)
	{
		AcceptanceReport acceptanceReport = CanDesignateThing(t);
		float angle;
		Vector2 offset;
		if (acceptanceReport.Accepted || (showReverseDesignatorDisabledReason && !acceptanceReport.Reason.NullOrEmpty()))
		{
			return new Command_Action
			{
				defaultLabel = LabelCapReverseDesignating(t),
				icon = IconReverseDesignating(t, out angle, out offset),
				iconAngle = angle,
				iconOffset = offset,
				defaultDesc = (acceptanceReport.Reason.NullOrEmpty() ? DescReverseDesignating(t) : acceptanceReport.Reason),
				Order = ((this is Designator_Uninstall) ? (-11f) : (-20f)),
				Disabled = !acceptanceReport.Accepted,
				action = delegate
				{
					if (TutorSystem.AllowAction(TutorTagDesignate) && (Designation == null || Designation.targetType != TargetType.Thing || Map.designationManager.DesignationOn(t, Designation) == null))
					{
						DesignateThing(t);
						Finalize(somethingSucceeded: true);
					}
				},
				hotKey = hotKey,
				groupKeyIgnoreContent = groupKeyIgnoreContent,
				groupKey = groupKey
			};
		}
		return null;
	}

	public virtual AcceptanceReport CanDesignateThing(Thing t)
	{
		return AcceptanceReport.WasRejected;
	}

	public virtual void DesignateThing(Thing t)
	{
		throw new NotImplementedException();
	}

	public abstract AcceptanceReport CanDesignateCell(IntVec3 loc);

	public virtual void DesignateMultiCell(IEnumerable<IntVec3> cells)
	{
		if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(TutorTagDesignate, cells)))
		{
			return;
		}
		bool somethingSucceeded = false;
		bool flag = false;
		foreach (IntVec3 cell in cells)
		{
			if (CanDesignateCell(cell).Accepted)
			{
				DesignateSingleCell(cell);
				somethingSucceeded = true;
				if (!flag)
				{
					flag = ShowWarningForCell(cell);
				}
			}
		}
		Finalize(somethingSucceeded);
		if (TutorSystem.TutorialMode)
		{
			TutorSystem.Notify_Event(new EventPack(TutorTagDesignate, cells));
		}
	}

	public virtual void DesignateSingleCell(IntVec3 c)
	{
		throw new NotImplementedException();
	}

	public virtual bool ShowWarningForCell(IntVec3 c)
	{
		return false;
	}

	public void Finalize(bool somethingSucceeded)
	{
		if (somethingSucceeded)
		{
			FinalizeDesignationSucceeded();
		}
		else
		{
			FinalizeDesignationFailed();
		}
	}

	protected virtual void FinalizeDesignationSucceeded()
	{
		if (soundSucceeded != null)
		{
			soundSucceeded.PlayOneShotOnCamera();
		}
	}

	protected virtual void FinalizeDesignationFailed()
	{
		if (soundFailed != null)
		{
			soundFailed.PlayOneShotOnCamera();
		}
		if (Find.DesignatorManager.Dragger.FailureReason != null)
		{
			Messages.Message(Find.DesignatorManager.Dragger.FailureReason, MessageTypeDefOf.RejectInput, historical: false);
		}
	}

	public virtual string LabelCapReverseDesignating(Thing t)
	{
		return LabelCap;
	}

	public virtual string DescReverseDesignating(Thing t)
	{
		return Desc;
	}

	public virtual Texture2D IconReverseDesignating(Thing t, out float angle, out Vector2 offset)
	{
		angle = iconAngle;
		offset = iconOffset;
		return (Texture2D)icon;
	}

	protected virtual bool RemoveAllDesignationsAffects(LocalTargetInfo target)
	{
		return true;
	}

	public virtual void DrawMouseAttachments()
	{
		if (useMouseIcon)
		{
			GenUI.DrawMouseAttachment(hideMouseIcon ? null : icon, mouseText, iconAngle, iconOffset);
		}
	}

	public virtual void DrawPanelReadout(ref float curY, float width)
	{
	}

	public virtual void DoExtraGuiControls(float leftX, float bottomY)
	{
		Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
		DrawStyleDef style = Find.DesignatorManager.SelectedStyle;
		List<DrawStyleDef> list = DrawStyleCategory?.styles;
		if (style == null || list == null || list.Count <= 1)
		{
			return;
		}
		Find.WindowStack.ImmediateWindow(415111, winRect, WindowLayer.GameUI, delegate
		{
			using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter))
			{
				Rect rect = winRect;
				rect.x = 0f;
				rect.y = 0f;
				Rect rect2 = rect.MiddlePartPixels(48f, 48f);
				if (Widgets.ButtonImage(rect2, style.uiIcon))
				{
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					foreach (DrawStyleDef style2 in DrawStyleCategory.styles)
					{
						DrawStyleDef lDef = style2;
						list2.Add(new FloatMenuOption(style2.LabelCap, delegate
						{
							Find.DesignatorManager.SelectedStyle = lDef;
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list2));
				}
				Rect rect3 = rect.MiddlePartPixels(60f, 60f);
				rect3.x = rect2.xMin - 16f - rect3.width;
				Rect rect4 = rect2;
				rect4.x = rect3.xMin + 27f;
				rect4.xMax = rect3.xMax + 27f;
				Rect butRect = rect3;
				butRect.x = rect2.xMax + 16f;
				Rect rect5 = rect2;
				rect5.x = butRect.xMin - 27f;
				rect5.xMax = butRect.xMax - 27f;
				if (Widgets.ButtonImage(rect3, TexUI.ConcaveArrowTexLeft))
				{
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
					Find.DesignatorManager.PreviousDrawStyle();
					Event.current.Use();
				}
				if (!SteamDeck.IsSteamDeck)
				{
					Widgets.Label(rect4, KeyBindingDefOf.Designator_PreviousDrawStyle.MainKeyLabel);
				}
				if (Widgets.ButtonImage(butRect, TexUI.ConcaveArrowTexRight))
				{
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
					Find.DesignatorManager.AdvanceDrawStyle();
					Event.current.Use();
				}
				if (!SteamDeck.IsSteamDeck)
				{
					Widgets.Label(rect5, KeyBindingDefOf.Designator_NextDrawStyle.MainKeyLabel);
				}
			}
		});
	}

	public virtual void SelectedUpdate()
	{
	}

	public virtual void SelectedProcessInput(Event ev)
	{
		if (KeyBindingDefOf.Designator_NextDrawStyle.KeyDownEvent)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			Find.DesignatorManager.AdvanceDrawStyle();
		}
		if (KeyBindingDefOf.Designator_PreviousDrawStyle.KeyDownEvent)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			Find.DesignatorManager.PreviousDrawStyle();
		}
	}

	public virtual bool CanRemainSelected()
	{
		return true;
	}

	public virtual void Selected()
	{
	}

	public virtual void Deselected()
	{
	}

	public virtual void RenderHighlight(List<IntVec3> dragCells)
	{
		DesignatorUtility.RenderHighlightOverSelectableThings(this, dragCells);
	}
}
