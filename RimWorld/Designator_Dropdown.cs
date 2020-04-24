using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Designator_Dropdown : Designator
	{
		private List<Designator> elements = new List<Designator>();

		private Designator activeDesignator;

		private bool activeDesignatorSet;

		public static readonly Texture2D PlusTex = ContentFinder<Texture2D>.Get("UI/Widgets/PlusOptions");

		private const float ButSize = 16f;

		private const float ButPadding = 1f;

		public override string Label => activeDesignator.Label + (activeDesignatorSet ? "" : "...");

		public override string Desc => activeDesignator.Desc;

		public override Color IconDrawColor => activeDesignator.IconDrawColor;

		public override bool Visible
		{
			get
			{
				for (int i = 0; i < elements.Count; i++)
				{
					if (elements[i].Visible)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<Designator> Elements => elements;

		public override float PanelReadoutTitleExtraRightMargin => activeDesignator.PanelReadoutTitleExtraRightMargin;

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
			DrawExtraOptionsIcon(topLeft, GetWidth(maxWidth));
			return result;
		}

		public static void DrawExtraOptionsIcon(Vector2 topLeft, float width)
		{
			GUI.DrawTexture(new Rect(topLeft.x + width - 16f - 1f, topLeft.y + 1f, 16f, 16f), PlusTex);
		}

		public void Add(Designator des)
		{
			elements.Add(des);
			if (activeDesignator == null)
			{
				SetActiveDesignator(des, explicitySet: false);
			}
		}

		public void SetActiveDesignator(Designator des, bool explicitySet = true)
		{
			activeDesignator = des;
			icon = des.icon;
			iconDrawScale = des.iconDrawScale;
			iconProportions = des.iconProportions;
			iconTexCoords = des.iconTexCoords;
			iconAngle = des.iconAngle;
			iconOffset = des.iconOffset;
			if (explicitySet)
			{
				activeDesignatorSet = true;
			}
		}

		public override void DrawMouseAttachments()
		{
			activeDesignator.DrawMouseAttachments();
		}

		public override void ProcessInput(Event ev)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			for (int i = 0; i < elements.Count; i++)
			{
				Designator des = elements[i];
				if (des.Visible)
				{
					Action action = delegate
					{
						base.ProcessInput(ev);
						Find.DesignatorManager.Select(des);
						SetActiveDesignator(des);
					};
					ThingDef designatorCost = GetDesignatorCost(des);
					if (designatorCost != null)
					{
						list.Add(new FloatMenuOption(des.LabelCap, action, designatorCost));
					}
					else
					{
						list.Add(new FloatMenuOption(des.LabelCap, action));
					}
				}
			}
			FloatMenu floatMenu = new FloatMenu(list);
			floatMenu.vanishIfMouseDistant = true;
			floatMenu.onCloseCallback = delegate
			{
				activeDesignatorSet = true;
			};
			Find.WindowStack.Add(floatMenu);
			Find.DesignatorManager.Select(activeDesignator);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 loc)
		{
			return activeDesignator.CanDesignateCell(loc);
		}

		public override void SelectedUpdate()
		{
			activeDesignator.SelectedUpdate();
		}

		public override void DrawPanelReadout(ref float curY, float width)
		{
			activeDesignator.DrawPanelReadout(ref curY, width);
		}

		private ThingDef GetDesignatorCost(Designator des)
		{
			Designator_Place designator_Place = des as Designator_Place;
			if (designator_Place != null)
			{
				BuildableDef placingDef = designator_Place.PlacingDef;
				if (placingDef.costList.Count > 0)
				{
					return placingDef.costList.MaxBy((ThingDefCountClass c) => c.thingDef.BaseMarketValue * (float)c.count).thingDef;
				}
			}
			return null;
		}
	}
}
