using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Bill : IExposable, ILoadReferenceable
	{
		[Unsaved(false)]
		public BillStack billStack;

		private int loadID = -1;

		public RecipeDef recipe;

		public bool suspended;

		public ThingFilter ingredientFilter;

		public float ingredientSearchRadius = 999f;

		public IntRange allowedSkillRange = new IntRange(0, 20);

		public Pawn pawnRestriction;

		public bool deleted;

		public int lastIngredientSearchFailTicks = -99999;

		public const int MaxIngredientSearchRadius = 999;

		public const float ButSize = 24f;

		private const float InterfaceBaseHeight = 53f;

		private const float InterfaceStatusLineHeight = 17f;

		public Map Map => billStack.billGiver.Map;

		public virtual string Label => recipe.label;

		public virtual string LabelCap => Label.CapitalizeFirst(recipe);

		public virtual bool CheckIngredientsIfSociallyProper => true;

		public virtual bool CompletableEver => true;

		protected virtual string StatusString => null;

		protected virtual float StatusLineMinHeight => 0f;

		protected virtual bool CanCopy => true;

		public bool DeletedOrDereferenced
		{
			get
			{
				if (deleted)
				{
					return true;
				}
				Thing thing = billStack.billGiver as Thing;
				if (thing != null && thing.Destroyed)
				{
					return true;
				}
				return false;
			}
		}

		public Bill()
		{
		}

		public Bill(RecipeDef recipe)
		{
			this.recipe = recipe;
			ingredientFilter = new ThingFilter();
			ingredientFilter.CopyAllowancesFrom(recipe.defaultIngredientFilter);
			InitializeAfterClone();
		}

		public void InitializeAfterClone()
		{
			loadID = Find.UniqueIDsManager.GetNextBillID();
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_Defs.Look(ref recipe, "recipe");
			Scribe_Values.Look(ref suspended, "suspended", defaultValue: false);
			Scribe_Values.Look(ref ingredientSearchRadius, "ingredientSearchRadius", 999f);
			Scribe_Values.Look(ref allowedSkillRange, "allowedSkillRange");
			Scribe_References.Look(ref pawnRestriction, "pawnRestriction");
			if (Scribe.mode == LoadSaveMode.Saving && recipe.fixedIngredientFilter != null)
			{
				foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				{
					if (!recipe.fixedIngredientFilter.Allows(allDef))
					{
						ingredientFilter.SetAllow(allDef, allow: false);
					}
				}
			}
			Scribe_Deep.Look(ref ingredientFilter, "ingredientFilter");
		}

		public virtual bool PawnAllowedToStartAnew(Pawn p)
		{
			if (pawnRestriction != null)
			{
				return pawnRestriction == p;
			}
			if (recipe.workSkill != null)
			{
				int level = p.skills.GetSkill(recipe.workSkill).Level;
				if (level < allowedSkillRange.min)
				{
					JobFailReason.Is("UnderAllowedSkill".Translate(allowedSkillRange.min), Label);
					return false;
				}
				if (level > allowedSkillRange.max)
				{
					JobFailReason.Is("AboveAllowedSkill".Translate(allowedSkillRange.max), Label);
					return false;
				}
			}
			return true;
		}

		public virtual void Notify_PawnDidWork(Pawn p)
		{
		}

		public virtual void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
		}

		public abstract bool ShouldDoNow();

		public virtual void Notify_DoBillStarted(Pawn billDoer)
		{
		}

		protected virtual void DoConfigInterface(Rect rect, Color baseColor)
		{
			rect.yMin += 29f;
			float y = rect.center.y;
			Widgets.InfoCardButton(rect.xMax - (rect.yMax - y) - 12f, y - 12f, recipe);
		}

		public virtual void DoStatusLineInterface(Rect rect)
		{
		}

		public Rect DoInterface(float x, float y, float width, int index)
		{
			Rect rect = new Rect(x, y, width, 53f);
			float num = 0f;
			if (!StatusString.NullOrEmpty())
			{
				num = Mathf.Max(17f, StatusLineMinHeight);
			}
			rect.height += num;
			Color color = Color.white;
			if (!ShouldDoNow())
			{
				color = new Color(1f, 0.7f, 0.7f, 0.7f);
			}
			GUI.color = color;
			Text.Font = GameFont.Small;
			if (index % 2 == 0)
			{
				Widgets.DrawAltRect(rect);
			}
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 24f, 24f);
			if (billStack.IndexOf(this) > 0)
			{
				if (Widgets.ButtonImage(rect2, TexButton.ReorderUp, color))
				{
					billStack.Reorder(this, -1);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegionByKey(rect2, "ReorderBillUpTip");
			}
			if (billStack.IndexOf(this) < billStack.Count - 1)
			{
				Rect rect3 = new Rect(0f, 24f, 24f, 24f);
				if (Widgets.ButtonImage(rect3, TexButton.ReorderDown, color))
				{
					billStack.Reorder(this, 1);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegionByKey(rect3, "ReorderBillDownTip");
			}
			Widgets.Label(new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f), LabelCap);
			DoConfigInterface(rect.AtZero(), color);
			Rect rect4 = new Rect(rect.width - 24f, 0f, 24f, 24f);
			if (Widgets.ButtonImage(rect4, TexButton.DeleteX, color, color * GenUI.SubtleMouseoverColor))
			{
				billStack.Delete(this);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegionByKey(rect4, "DeleteBillTip");
			Rect rect5;
			if (!CanCopy)
			{
				rect5 = new Rect(rect4);
			}
			else
			{
				Rect rect6 = new Rect(rect4);
				rect6.x -= rect6.width + 4f;
				if (Widgets.ButtonImageFitted(rect6, TexButton.Copy, color))
				{
					BillUtility.Clipboard = Clone();
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegionByKey(rect6, "CopyBillTip");
				rect5 = new Rect(rect6);
			}
			rect5.x -= rect5.width + 4f;
			if (Widgets.ButtonImage(rect5, TexButton.Suspend, color))
			{
				suspended = !suspended;
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegionByKey(rect5, "SuspendBillTip");
			if (!StatusString.NullOrEmpty())
			{
				Text.Font = GameFont.Tiny;
				Rect rect7 = new Rect(24f, rect.height - num, rect.width - 24f, num);
				Widgets.Label(rect7, StatusString);
				DoStatusLineInterface(rect7);
			}
			GUI.EndGroup();
			if (suspended)
			{
				Text.Font = GameFont.Medium;
				Text.Anchor = TextAnchor.MiddleCenter;
				Rect rect8 = new Rect(rect.x + rect.width / 2f - 70f, rect.y + rect.height / 2f - 20f, 140f, 40f);
				GUI.DrawTexture(rect8, TexUI.GrayTextBG);
				Widgets.Label(rect8, "SuspendedCaps".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			}
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			return rect;
		}

		public bool IsFixedOrAllowedIngredient(Thing thing)
		{
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(thing))
				{
					return true;
				}
			}
			if (recipe.fixedIngredientFilter.Allows(thing))
			{
				return ingredientFilter.Allows(thing);
			}
			return false;
		}

		public bool IsFixedOrAllowedIngredient(ThingDef def)
		{
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(def))
				{
					return true;
				}
			}
			if (recipe.fixedIngredientFilter.Allows(def))
			{
				return ingredientFilter.Allows(def);
			}
			return false;
		}

		public static void CreateNoPawnsWithSkillDialog(RecipeDef recipe)
		{
			string str = "RecipeRequiresSkills".Translate(recipe.LabelCap);
			str += "\n\n";
			str += recipe.MinSkillString;
			Find.WindowStack.Add(new Dialog_MessageBox(str));
		}

		public virtual BillStoreModeDef GetStoreMode()
		{
			return BillStoreModeDefOf.BestStockpile;
		}

		public virtual Zone_Stockpile GetStoreZone()
		{
			return null;
		}

		public virtual void SetStoreMode(BillStoreModeDef mode, Zone_Stockpile zone = null)
		{
			Log.ErrorOnce("Tried to set store mode of a non-production bill", 27190980);
		}

		public virtual Bill Clone()
		{
			Bill obj = (Bill)Activator.CreateInstance(GetType());
			obj.recipe = recipe;
			obj.suspended = suspended;
			obj.ingredientFilter = new ThingFilter();
			obj.ingredientFilter.CopyAllowancesFrom(ingredientFilter);
			obj.ingredientSearchRadius = ingredientSearchRadius;
			obj.allowedSkillRange = allowedSkillRange;
			obj.pawnRestriction = pawnRestriction;
			return obj;
		}

		public virtual void ValidateSettings()
		{
			if (pawnRestriction != null && (pawnRestriction.Dead || pawnRestriction.Faction != Faction.OfPlayer || pawnRestriction.IsKidnapped()))
			{
				if (this != BillUtility.Clipboard)
				{
					Messages.Message("MessageBillValidationPawnUnavailable".Translate(pawnRestriction.LabelShortCap, Label, billStack.billGiver.LabelShort), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
				}
				pawnRestriction = null;
			}
		}

		public string GetUniqueLoadID()
		{
			return "Bill_" + recipe.defName + "_" + loadID;
		}

		public override string ToString()
		{
			return GetUniqueLoadID();
		}
	}
}
