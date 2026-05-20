using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Designator_Paint : DesignatorWithEyedropper
{
	protected ColorDef colorDef;

	private string cachedAttachmentString;

	protected abstract Texture2D IconTopTex { get; }

	public override Color IconDrawColor => colorDef.color;

	public override bool DragDrawMeasurements => true;

	public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Paint;

	protected virtual IEnumerable<ColorDef> Colors => from x in DefDatabase<ColorDef>.AllDefs
		where x.colorType == ColorType.Structure
		orderby x.displayOrder
		select x;

	private string AttachmentString
	{
		get
		{
			if (cachedAttachmentString == null)
			{
				cachedAttachmentString = "Paint".Translate() + ": " + colorDef.LabelCap + "\n" + KeyBindingDefOf.ShowEyedropper.MainKeyLabel + ": " + "GrabExistingColor".Translate();
			}
			return cachedAttachmentString;
		}
	}

	public Designator_Paint()
	{
		colorDef = Colors.FirstOrDefault();
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		useMouseIcon = true;
		soundSucceeded = SoundDefOf.Designate_Paint;
		hotKey = KeyBindingDefOf.Misc6;
		eyedropper = new Designator_Eyedropper(delegate(ColorDef newCol)
		{
			colorDef = newCol;
			cachedAttachmentString = null;
			if (!eyedropMode)
			{
				Find.DesignatorManager.Select(this);
			}
		}, "SelectAPaintedBuilding".Translate(), "DesignatorEyeDropperDesc_Paint".Translate());
	}

	public override void ProcessInput(Event ev)
	{
		if (!CheckCanInteract())
		{
			return;
		}
		List<FloatMenuGridOption> list = new List<FloatMenuGridOption>();
		Texture2D eyeDropperTex = Designator_Eyedropper.EyeDropperTex;
		Action action = delegate
		{
			base.ProcessInput(ev);
			Find.DesignatorManager.Select(eyedropper);
		};
		TipSignal? tooltip = "DesignatorEyeDropperDesc_Paint".Translate();
		list.Add(new FloatMenuGridOption(eyeDropperTex, action, null, tooltip));
		foreach (ColorDef color in Colors)
		{
			ColorDef newCol = color;
			list.Add(new FloatMenuGridOption(BaseContent.WhiteTex, delegate
			{
				base.ProcessInput(ev);
				Find.DesignatorManager.Select(this);
				colorDef = newCol;
				cachedAttachmentString = null;
			}, newCol.color, newCol.LabelCap));
		}
		Find.WindowStack.Add(new FloatMenuGrid(list));
		Find.DesignatorManager.Select(this);
	}

	public override void DrawMouseAttachments()
	{
		eyedropMode = KeyBindingDefOf.ShowEyedropper.IsDown;
		if (eyedropMode)
		{
			eyedropper.DrawMouseAttachments();
			return;
		}
		if (useMouseIcon)
		{
			Texture iconTex = icon;
			string attachmentString = AttachmentString;
			float angle = iconAngle;
			Vector2 offset = iconOffset;
			Color? iconColor = colorDef.color;
			GenUI.DrawMouseAttachment(iconTex, attachmentString, angle, offset, null, null, drawTextBackground: false, default(Color), iconColor, delegate(Rect r)
			{
				GUI.DrawTexture(r, IconTopTex);
			});
		}
		if (Find.DesignatorManager.Dragger.Dragging)
		{
			Vector2 vector = Event.current.mousePosition + Designator_Place.PlaceMouseAttachmentDrawOffset;
			if (useMouseIcon)
			{
				vector.y += 32f + Text.LineHeight * 2f;
			}
			Widgets.ThingIcon(new Rect(vector.x, vector.y, 27f, 27f), ThingDefOf.Dye);
			int num = NumHighlightedCells();
			string text = num.ToStringCached();
			if (base.Map.resourceCounter.GetCount(ThingDefOf.Dye) < num)
			{
				GUI.color = Color.red;
				text += " (" + "NotEnoughStoredLower".Translate() + ")";
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(new Rect(vector.x + 29f, vector.y, 999f, 29f), text);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}
	}

	public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
	{
		base.DrawIcon(rect, buttonMat, parms);
		Widgets.DrawTextureFitted(rect, IconTopTex, iconDrawScale * 0.85f, iconProportions, iconTexCoords, iconAngle, buttonMat);
	}

	protected abstract int NumHighlightedCells();
}
