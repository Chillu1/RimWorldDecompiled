using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class Need : IExposable
{
	public NeedDef def;

	protected readonly Pawn pawn;

	protected float curLevelInt;

	protected List<float> threshPercents;

	private CompCanBeDormant intDormant;

	public const float MaxDrawHeight = 70f;

	private static readonly Texture2D BarInstantMarkerTex = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarker");

	private static readonly Texture2D NeedUnitDividerTex = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider");

	private const float BarInstantMarkerSize = 12f;

	public string LabelCap => def.LabelCap;

	public float CurInstantLevelPercentage => CurInstantLevel / MaxLevel;

	public virtual int GUIChangeArrow => 0;

	public virtual float CurInstantLevel => -1f;

	public virtual float MaxLevel => 1f;

	public virtual float CurLevel
	{
		get
		{
			return curLevelInt;
		}
		set
		{
			curLevelInt = Mathf.Clamp(value, 0f, MaxLevel);
		}
	}

	public float CurLevelPercentage
	{
		get
		{
			return CurLevel / MaxLevel;
		}
		set
		{
			CurLevel = value * MaxLevel;
		}
	}

	protected virtual bool IsFrozen
	{
		get
		{
			if (pawn.Suspended)
			{
				return true;
			}
			if (def.freezeWhileSleeping && !pawn.Awake())
			{
				return true;
			}
			if (def.freezeInMentalState && pawn.InMentalState)
			{
				return true;
			}
			if (NeedFrozenFromDormanancy())
			{
				return true;
			}
			return !IsPawnInteractableOrVisible;
		}
	}

	private bool IsPawnInteractableOrVisible
	{
		get
		{
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				return true;
			}
			if (pawn.IsCaravanMember())
			{
				return true;
			}
			if (PawnUtility.IsTravelingInTransportPodWorldObject(pawn))
			{
				return true;
			}
			return false;
		}
	}

	public virtual bool ShowOnNeedList => def.showOnNeedList;

	private bool NeedFrozenFromDormanancy()
	{
		if (intDormant == null)
		{
			return false;
		}
		if (intDormant.Awake)
		{
			return false;
		}
		return intDormant.Props.freezeNeeds.Contains(def);
	}

	public Need(Pawn newPawn)
	{
		pawn = newPawn;
		SetInitialLevel();
		intDormant = pawn.TryGetComp<CompCanBeDormant>();
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref curLevelInt, "curLevel", 0f);
	}

	public abstract void NeedInterval();

	public virtual string GetTipString()
	{
		string text = (LabelCap + ": " + CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + "\n" + def.description;
		Gene gene;
		Hediff hediff;
		if (pawn.story?.traits != null && pawn.story.traits.TryGetNeedEnablingTrait(def, out var trait))
		{
			text += string.Format("\n\n{0}: {1}", "ComesFromTrait".Translate(), trait.LabelCap);
		}
		else if (pawn.genes != null && pawn.genes.TryGetNeedEnablingGene(def, out gene))
		{
			text += string.Format("\n\n{0}: {1}", "ComesFromGene".Translate(), gene.LabelCap);
		}
		else if (pawn.Ideo != null && pawn.Ideo.EnablesNeed(def))
		{
			text += string.Format("\n\n{0}: {1}", "ComesFromIdeo".Translate(), pawn.Ideo.name.CapitalizeFirst());
		}
		else if (pawn.health != null && pawn.health.hediffSet.TryGetNeedEnablingHediff(def, out hediff))
		{
			text += string.Format("\n\n{0}: {1}", "ComesFromHediff".Translate(), hediff.LabelCap);
		}
		return text;
	}

	public virtual void SetInitialLevel()
	{
		CurLevelPercentage = 0.5f;
	}

	public virtual void OnNeedRemoved()
	{
	}

	protected virtual void OffsetDebugPercent(float offsetPercent)
	{
		CurLevelPercentage += offsetPercent;
	}

	public virtual void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
	{
		if (rect.height > 70f)
		{
			float num = (rect.height - 70f) / 2f;
			rect.height = 70f;
			rect.y += num;
		}
		Rect rect2 = rectForTooltip ?? rect;
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
		}
		if (doTooltip && Mouse.IsOver(rect2))
		{
			TooltipHandler.TipRegion(rect2, new TipSignal(() => GetTipString(), rect2.GetHashCode()));
		}
		float num2 = 14f;
		float num3 = ((customMargin >= 0f) ? customMargin : (num2 + 15f));
		if (rect.height < 50f)
		{
			num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
		}
		if (drawLabel)
		{
			Text.Font = ((rect.height > 55f) ? GameFont.Small : GameFont.Tiny);
			Text.Anchor = TextAnchor.LowerLeft;
			Widgets.Label(new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f), LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
		}
		Rect rect3 = rect;
		if (drawLabel)
		{
			rect3.y += rect.height / 2f;
			rect3.height -= rect.height / 2f;
		}
		rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
		if (DebugSettings.ShowDevGizmos)
		{
			float lineHeight = Text.LineHeight;
			Rect rect4 = new Rect(rect3.xMax - lineHeight, rect3.y - lineHeight, lineHeight, lineHeight);
			if (Widgets.ButtonImage(rect4.ContractedBy(4f), TexButton.Plus))
			{
				OffsetDebugPercent(0.1f);
			}
			if (Mouse.IsOver(rect4))
			{
				TooltipHandler.TipRegion(rect4, "+ 10%");
			}
			Rect rect5 = new Rect(rect4.xMin - lineHeight, rect3.y - lineHeight, lineHeight, lineHeight);
			if (Widgets.ButtonImage(rect5.ContractedBy(4f), TexButton.Minus))
			{
				OffsetDebugPercent(-0.1f);
			}
			if (Mouse.IsOver(rect5))
			{
				TooltipHandler.TipRegion(rect5, "- 10%");
			}
		}
		Rect rect6 = rect3;
		float num4 = 1f;
		if (def.scaleBar && MaxLevel < 1f)
		{
			num4 = MaxLevel;
		}
		rect6.width *= num4;
		Rect barRect = Widgets.FillableBar(rect6, CurLevelPercentage);
		if (drawArrows)
		{
			Widgets.FillableBarChangeArrows(rect6, GUIChangeArrow);
		}
		if (threshPercents != null)
		{
			for (int num5 = 0; num5 < Mathf.Min(threshPercents.Count, maxThresholdMarkers); num5++)
			{
				DrawBarThreshold(barRect, threshPercents[num5] * num4);
			}
		}
		if (def.showUnitTicks)
		{
			for (int num6 = 1; (float)num6 < MaxLevel; num6++)
			{
				DrawBarDivision(barRect, (float)num6 / MaxLevel * num4);
			}
		}
		float curInstantLevelPercentage = CurInstantLevelPercentage;
		if (curInstantLevelPercentage >= 0f)
		{
			DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage * num4);
		}
		if (!def.tutorHighlightTag.NullOrEmpty())
		{
			UIHighlighter.HighlightOpportunity(rect, def.tutorHighlightTag);
		}
		Text.Font = GameFont.Small;
	}

	protected void DrawBarInstantMarkerAt(Rect barRect, float pct)
	{
		if (pct > 1f)
		{
			Log.ErrorOnce(def?.ToString() + " drawing bar percent > 1 : " + pct, 6932178);
		}
		float num = 12f;
		if (barRect.width < 150f)
		{
			num /= 2f;
		}
		Vector2 vector = new Vector2(barRect.x + barRect.width * pct, barRect.y + barRect.height);
		GUI.DrawTexture(new Rect(vector.x - num / 2f, vector.y, num, num), BarInstantMarkerTex);
	}

	protected void DrawBarThreshold(Rect barRect, float threshPct)
	{
		float num = ((!(barRect.width > 60f)) ? 1 : 2);
		Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
		Texture2D image;
		if (threshPct < CurLevelPercentage)
		{
			image = BaseContent.BlackTex;
			GUI.color = new Color(1f, 1f, 1f, 0.9f);
		}
		else
		{
			image = BaseContent.GreyTex;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
		}
		GUI.DrawTexture(position, image);
		GUI.color = Color.white;
	}

	private void DrawBarDivision(Rect barRect, float threshPct)
	{
		float num = 5f;
		Rect rect = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y, num, barRect.height);
		if (threshPct < CurLevelPercentage)
		{
			GUI.color = new Color(0f, 0f, 0f, 0.9f);
		}
		else
		{
			GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		}
		Rect position = rect;
		position.yMax = position.yMin + 4f;
		GUI.DrawTextureWithTexCoords(position, NeedUnitDividerTex, new Rect(0f, 0.5f, 1f, 0.5f));
		Rect position2 = rect;
		position2.yMin = position2.yMax - 4f;
		GUI.DrawTextureWithTexCoords(position2, NeedUnitDividerTex, new Rect(0f, 0f, 1f, 0.5f));
		Rect position3 = rect;
		position3.yMin = position.yMax;
		position3.yMax = position2.yMin;
		if (position3.height > 0f)
		{
			GUI.DrawTextureWithTexCoords(position3, NeedUnitDividerTex, new Rect(0f, 0.4f, 1f, 0.2f));
		}
		GUI.color = Color.white;
	}
}
