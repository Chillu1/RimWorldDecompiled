using System;
using System.Collections.Generic;

namespace Verse;

public abstract class FleckSystem : IFleckCreator, IExposable, ILoadReferenceable
{
	public List<FleckDef> handledDefs = new List<FleckDef>();

	public FleckManager parent;

	public FleckSystem(FleckManager parent)
	{
		this.parent = parent;
	}

	public abstract void Update(float deltaTime);

	public abstract void Tick();

	public abstract void ForceDraw(DrawBatch drawBatch);

	public virtual void Draw(DrawBatch drawBatch)
	{
		if (!WorldComponent_GravshipController.GravshipRenderInProgess)
		{
			ForceDraw(drawBatch);
		}
	}

	public virtual void OnGUI()
	{
	}

	public virtual void Prewarm(float prewarmSeconds, Action<float> onUpdate, Action onTick)
	{
		float num = 0f;
		float num2 = 0f;
		while (num < prewarmSeconds)
		{
			Update(1f / 15f);
			onUpdate?.Invoke(1f / 15f);
			while (num2 >= 1f / 60f)
			{
				Tick();
				onTick?.Invoke();
				num2 -= 1f / 60f;
			}
			num += 1f / 15f;
			num2 += 1f / 15f;
		}
	}

	public abstract void CreateFleck(FleckCreationData fleckData);

	public abstract void MergeWith(FleckSystem system);

	public abstract IEnumerable<IFleck> EnumerateFlecks();

	public abstract void RemoveAllFlecks(Predicate<IFleck> shouldRemove);

	public abstract void ExposeData();

	public string GetUniqueLoadID()
	{
		return parent.parent.GetUniqueLoadID() + "_FleckSystem_" + GetType().FullName;
	}
}
