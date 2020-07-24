using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class Need : IExposable
	{
		public NeedDef def;

		protected Pawn pawn;

		protected float curLevelInt;

		protected List<float> threshPercents;

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

		public Need()
		{
		}

		public Need(Pawn newPawn)
		{
			pawn = newPawn;
			SetInitialLevel();
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref curLevelInt, "curLevel", 0f);
		}

		public abstract void NeedInterval();

		public virtual string GetTipString()
		{
			return LabelCap + ": " + CurLevelPercentage.ToStringPercent() + "\n" + def.description;
		}

		public virtual void SetInitialLevel()
		{
			CurLevelPercentage = 0.5f;
		}

		public virtual void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (rect.height > 70f)
			{
				float num = (rect.height - 70f) / 2f;
				rect.height = 70f;
				rect.y += num;
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (doTooltip && Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, new TipSignal(() => GetTipString(), rect.GetHashCode()));
			}
			float num2 = 14f;
			float num3 = (customMargin >= 0f) ? customMargin : (num2 + 15f);
			if (rect.height < 50f)
			{
				num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
			}
			Text.Font = ((rect.height > 55f) ? GameFont.Small : GameFont.Tiny);
			Text.Anchor = TextAnchor.LowerLeft;
			Widgets.Label(new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f), LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
			rect2 = new Rect(rect2.x + num3, rect2.y, rect2.width - num3 * 2f, rect2.height - num2);
			Rect rect3 = rect2;
			float num4 = 1f;
			if (def.scaleBar && MaxLevel < 1f)
			{
				num4 = MaxLevel;
			}
			rect3.width *= num4;
			Rect barRect = Widgets.FillableBar(rect3, CurLevelPercentage);
			if (drawArrows)
			{
				Widgets.FillableBarChangeArrows(rect3, GUIChangeArrow);
			}
			if (threshPercents != null)
			{
				for (int i = 0; i < Mathf.Min(threshPercents.Count, maxThresholdMarkers); i++)
				{
					DrawBarThreshold(barRect, threshPercents[i] * num4);
				}
			}
			if (def.scaleBar)
			{
				for (int j = 1; (float)j < MaxLevel; j++)
				{
					DrawBarDivision(barRect, (float)j / MaxLevel * num4);
				}
			}
			float curInstantLevelPercentage = CurInstantLevelPercentage;
			if (curInstantLevelPercentage >= 0f)
			{
				DrawBarInstantMarkerAt(rect2, curInstantLevelPercentage * num4);
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
				Log.ErrorOnce(string.Concat(def, " drawing bar percent > 1 : ", pct), 6932178);
			}
			float num = 12f;
			if (barRect.width < 150f)
			{
				num /= 2f;
			}
			Vector2 vector = new Vector2(barRect.x + barRect.width * pct, barRect.y + barRect.height);
			GUI.DrawTexture(new Rect(vector.x - num / 2f, vector.y, num, num), BarInstantMarkerTex);
		}

		private void DrawBarThreshold(Rect barRect, float threshPct)
		{
			float num = (!(barRect.width > 60f)) ? 1 : 2;
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
}
