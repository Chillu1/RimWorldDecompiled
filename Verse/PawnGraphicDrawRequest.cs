using UnityEngine;

namespace Verse;

public struct PawnGraphicDrawRequest
{
	public readonly PawnRenderNode node;

	public readonly Mesh mesh;

	public Material material;

	public Matrix4x4 preDrawnComputedMatrix;

	public PawnGraphicDrawRequest(PawnRenderNode node, Mesh mesh = null, Material material = null)
	{
		this.node = node;
		this.mesh = mesh;
		this.material = material;
		preDrawnComputedMatrix = default(Matrix4x4);
	}
}
