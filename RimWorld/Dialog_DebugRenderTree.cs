using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_DebugRenderTree : Window
{
	public Pawn pawn;

	private PawnRenderTree tree;

	private Vector2 scrollPosition;

	private float scrollHeight;

	private PawnRenderNode currentNode;

	private float alpha = 1f;

	private PawnDrawParms drawParms;

	private Dictionary<PawnRenderNode, bool> nodeExpanded = new Dictionary<PawnRenderNode, bool>();

	private bool showAll;

	private AnimationDef currentAnimation;

	private const float NodeHeight = 25f;

	private const float IndentWidth = 12f;

	private const float IndentHeightOffset = 6.5f;

	private static readonly FloatRange AngleRange = new FloatRange(-180f, 180f);

	private static readonly FloatRange ScaleRange = new FloatRange(0.01f, 10f);

	private static readonly FloatRange OffsetRange = new FloatRange(-10f, 10f);

	private static readonly FloatRange LayerRange = new FloatRange(-100f, 100f);

	public override Vector2 InitialSize => new Vector2(600f, 600f);

	protected override float Margin => 10f;

	public Dialog_DebugRenderTree(Pawn pawn)
	{
		doCloseX = true;
		preventCameraMotion = false;
		closeOnAccept = false;
		draggable = true;
		Init(pawn);
	}

	public void Init(Pawn pawn)
	{
		currentNode = null;
		scrollPosition = Vector2.zero;
		scrollHeight = 0f;
		nodeExpanded.Clear();
		this.pawn = pawn;
		tree = pawn.Drawer.renderer.renderTree;
		currentAnimation = tree.currentAnimation;
		optionalTitle = pawn.LabelShortCap + " (" + pawn.RaceProps.renderTree.defName + ")";
		drawParms = new PawnDrawParms
		{
			pawn = pawn,
			rotDrawMode = RotDrawMode.Fresh,
			flags = (PawnRenderFlags.Headgear | PawnRenderFlags.Clothes)
		};
		PawnRenderNode rootNode = tree.rootNode;
		if (rootNode != null)
		{
			AddNode(rootNode, null);
		}
	}

	private void AddNode(PawnRenderNode node, PawnRenderNode parent)
	{
		if (parent != null && !nodeExpanded.ContainsKey(parent))
		{
			nodeExpanded.Add(parent, value: true);
		}
		if (!node.children.NullOrEmpty())
		{
			PawnRenderNode[] children = node.children;
			foreach (PawnRenderNode node2 in children)
			{
				AddNode(node2, node);
			}
		}
	}

	public override void WindowUpdate()
	{
		base.WindowUpdate();
		Pawn pawn = Find.Selector.SelectedPawns.FirstOrDefault();
		if (pawn != null && pawn != this.pawn)
		{
			Init(pawn);
		}
		drawParms.facing = this.pawn.Rotation;
		drawParms.posture = this.pawn.GetPosture();
		drawParms.bed = this.pawn.CurrentBed();
		drawParms.coveredInFoam = this.pawn.Drawer.renderer.FirefoamOverlays.coveredInFoam;
		drawParms.carriedThing = this.pawn.carryTracker?.CarriedThing;
		drawParms.dead = this.pawn.Dead;
		drawParms.rotDrawMode = this.pawn.Drawer.renderer.CurRotDrawMode;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		LeftRect(inRect.LeftHalf());
		RightRect(inRect.RightHalf());
	}

	private void LeftRect(Rect inRect)
	{
		if (pawn == null || tree == null || !tree.Resolved)
		{
			Close();
			return;
		}
		Widgets.DrawMenuSection(inRect);
		inRect = inRect.ContractedBy(Margin / 2f);
		Widgets.HorizontalSlider(new Rect(inRect.x + Margin, inRect.y, inRect.width - Margin * 2f, 25f), ref alpha, FloatRange.ZeroToOne, "Alpha " + alpha.ToStringPercent(), 0.01f);
		pawn.Drawer.renderer.renderTree.debugTint = Color.white.ToTransparent(alpha);
		inRect.yMin += 25f + Margin;
		Rect rect = new Rect(inRect.x + Margin, inRect.y, "Animation".GetWidthCached() + 4f, 25f);
		using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft))
		{
			Widgets.Label(rect, "Animation");
		}
		Rect rect2 = new Rect(rect.xMax + 4f, rect.y, inRect.width - rect.width - Margin * 2f, 25f);
		Widgets.DrawLightHighlight(rect2);
		Widgets.DrawHighlightIfMouseover(rect2);
		using (new TextBlock(TextAnchor.MiddleCenter))
		{
			Widgets.Label(rect2, (currentAnimation == null) ? "None" : currentAnimation.defName);
		}
		if (Widgets.ButtonInvisible(rect2))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("None", delegate
			{
				currentAnimation = null;
				pawn.Drawer.renderer.SetAnimation(null);
			}));
			foreach (AnimationDef item in DefDatabase<AnimationDef>.AllDefsListForReading)
			{
				AnimationDef def = item;
				list.Add(new FloatMenuOption(item.defName, delegate
				{
					currentAnimation = def;
					pawn.Drawer.renderer.SetAnimation(def);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		inRect.yMin += 25f + Margin;
		using (new TextBlock(GameFont.Tiny))
		{
			Widgets.CheckboxLabeled(new Rect(inRect.x + Margin, inRect.y, inRect.width - Margin * 2f, 25f), "Show all nodes", ref showAll);
		}
		inRect.yMin += 25f + Margin;
		Widgets.BeginScrollView(inRect, ref scrollPosition, new Rect(0f, 0f, inRect.width - 16f, scrollHeight));
		float curY = 0f;
		ListNode(tree.rootNode, 0, ref curY, inRect.width - 16f);
		if (Event.current.type == EventType.Layout)
		{
			scrollHeight = curY;
		}
		Widgets.EndScrollView();
	}

	private void RightRect(Rect inRect)
	{
		Widgets.DrawMenuSection(inRect);
		inRect = inRect.ContractedBy(Margin / 2f);
		Widgets.BeginGroup(inRect);
		Rect rect = new Rect(0f, 0f, inRect.width, Text.LineHeight);
		Rect rect2 = new Rect(Margin, rect.height + Margin, inRect.width - Margin * 2f, 30f);
		Widgets.DrawLightHighlight(rect);
		if (currentNode != null)
		{
			using (new TextBlock(TextAnchor.MiddleCenter))
			{
				Widgets.Label(rect, currentNode.ToString().Truncate(inRect.width));
			}
			Widgets.Label(rect2, currentNode.Worker.GetType().Name.Truncate(inRect.width));
			rect2.y += rect2.height;
			bool checkOn = currentNode.debugEnabled;
			Widgets.CheckboxLabeled(rect2, "Enabled", ref checkOn);
			if (checkOn != currentNode.debugEnabled)
			{
				currentNode.debugEnabled = checkOn;
				currentNode.requestRecache = true;
			}
			rect2.y += rect2.height;
			Vector3 pivot;
			Vector3 vector = currentNode.Worker.OffsetFor(currentNode, drawParms, out pivot);
			vector.y = currentNode.Worker.AltitudeFor(currentNode, drawParms);
			float num = currentNode.Props.baseLayer;
			if (currentNode.Props.drawData != null)
			{
				num = currentNode.Props.drawData.LayerForRot(drawParms.facing, num);
			}
			num += currentNode.debugLayerOffset;
			Widgets.Label(rect2, "Offset " + vector.ToString("F2") + " Layer " + num);
			rect2.y += rect2.height;
			Widgets.HorizontalSlider(rect2, ref currentNode.debugOffset.x, OffsetRange, "X: " + currentNode.debugOffset.x, 0.05f);
			rect2.y += rect2.height;
			Widgets.HorizontalSlider(rect2, ref currentNode.debugOffset.z, OffsetRange, "Z: " + currentNode.debugOffset.z, 0.05f);
			rect2.y += rect2.height;
			Widgets.HorizontalSlider(rect2, ref currentNode.debugLayerOffset, LayerRange, "Layer: " + currentNode.debugLayerOffset, 1f);
			rect2.y += rect2.height;
			rect2.y += Margin;
			Widgets.Label(rect2, "Pivot (" + pivot.x.ToStringPercent() + ", " + pivot.z.ToStringPercent() + ")");
			rect2.y += rect2.height;
			Widgets.HorizontalSlider(rect2, ref currentNode.debugPivotOffset.x, FloatRange.ZeroToOne, "X: " + currentNode.debugPivotOffset.x.ToStringPercent(), 0.01f);
			rect2.y += rect2.height;
			Widgets.HorizontalSlider(rect2, ref currentNode.debugPivotOffset.y, FloatRange.ZeroToOne, "Y: " + currentNode.debugPivotOffset.y.ToStringPercent(), 0.01f);
			rect2.y += rect2.height;
			rect2.y += Margin;
			float y = currentNode.Worker.RotationFor(currentNode, drawParms).eulerAngles.y;
			Widgets.Label(rect2, "Rotation " + y.ToString("F0") + "°");
			rect2.y += rect2.height;
			Widgets.HorizontalSlider(rect2, ref currentNode.debugAngleOffset, AngleRange, "Angle: " + currentNode.debugAngleOffset.ToString("F0") + "°", 1f);
			rect2.y += rect2.height;
			Vector3 vector2 = currentNode.Worker.ScaleFor(currentNode, drawParms);
			rect2.y += Margin;
			Rect rect3 = rect2;
			Vector3 vector3 = vector2;
			Widgets.Label(rect3, "Scale " + vector3.ToString());
			rect2.y += rect2.height;
			Widgets.HorizontalSlider(rect2, ref currentNode.debugScale, ScaleRange, "Scale: " + currentNode.debugScale.ToStringPercent("F0"), 0.01f);
			rect2.y += rect2.height;
			rect2.y += Margin;
			bool checkOn2 = currentNode.debugFlip;
			bool flag = currentNode.FlipGraphic(drawParms);
			Widgets.CheckboxLabeled(rect2, $"Flip ({flag})", ref checkOn2);
			if (checkOn2 != currentNode.debugFlip)
			{
				currentNode.debugFlip = checkOn2;
				currentNode.requestRecache = true;
			}
			rect2.y += rect2.height;
			if ((currentNode.debugAngleOffset != 0f || currentNode.debugScale != 1f || currentNode.debugOffset != Vector3.zero || currentNode.debugLayerOffset != 0f || !currentNode.debugEnabled || currentNode.debugPivotOffset != DrawData.PivotCenter) && Widgets.ButtonText(new Rect(0f, inRect.height - 25f, inRect.width, 25f), "Reset"))
			{
				currentNode.debugAngleOffset = 0f;
				currentNode.debugScale = 1f;
				currentNode.debugOffset = Vector3.zero;
				currentNode.debugLayerOffset = 0f;
				currentNode.debugEnabled = true;
				currentNode.requestRecache = true;
				currentNode.debugPivotOffset = DrawData.PivotCenter;
			}
		}
		else
		{
			using (new TextBlock(TextAnchor.MiddleCenter))
			{
				Widgets.Label(rect, "No node selected");
			}
		}
		Widgets.EndGroup();
	}

	private void ListNode(PawnRenderNode node, int indent, ref float curY, float width)
	{
		if (!showAll && !node.Worker.ShouldListOnGraph(node, drawParms))
		{
			return;
		}
		Rect rect = new Rect((float)(indent + 1) * 12f, curY, width, 25f);
		rect.xMax = width;
		Rect rect2 = rect.ContractedBy(2f);
		Widgets.DrawHighlight(rect2);
		Widgets.DrawHighlightIfMouseover(rect2);
		if (currentNode == node)
		{
			Widgets.DrawHighlight(rect2);
		}
		Rect rect3 = new Rect(rect2.x, rect2.y, rect2.height, rect2.height);
		Widgets.DrawLightHighlight(rect3);
		if (node.Props.useGraphic)
		{
			Texture texture = node.PrimaryGraphic?.MatAt(Rot4.South)?.mainTexture;
			if (texture != null)
			{
				GUI.DrawTexture(rect3, texture);
			}
		}
		Rect rect4 = new Rect(rect3.xMax + 4f, rect2.y, rect2.width - rect2.height - 4f, rect2.height);
		if (!node.Worker.CanDrawNow(node, drawParms) || (node.Props.useGraphic && node.PrimaryGraphic == null))
		{
			GUI.color = ColoredText.SubtleGrayColor;
		}
		Widgets.Label(rect4, node.ToString().Truncate(rect4.width));
		GUI.color = Color.white;
		if (Widgets.ButtonInvisible(rect2))
		{
			currentNode = node;
		}
		if (!node.children.NullOrEmpty())
		{
			Rect rect5 = new Rect((float)indent * 12f, curY + 6.5f, 12f, 12f);
			Widgets.DrawHighlightIfMouseover(rect5);
			if (Widgets.ButtonImage(rect5, nodeExpanded[node] ? TexButton.Minus : TexButton.Plus))
			{
				nodeExpanded[node] = !nodeExpanded[node];
			}
		}
		curY += 25f;
		if (!node.children.NullOrEmpty() && nodeExpanded[node])
		{
			PawnRenderNode[] children = node.children;
			foreach (PawnRenderNode node2 in children)
			{
				ListNode(node2, indent + 1, ref curY, width);
			}
		}
	}

	protected override void SetInitialSizeAndPosition()
	{
		Vector2 initialSize = InitialSize;
		windowRect = new Rect(5f, 5f, initialSize.x, initialSize.y).Rounded();
	}
}
