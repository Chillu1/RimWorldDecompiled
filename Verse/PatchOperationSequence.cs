using System.Collections.Generic;
using System.Xml;

namespace Verse;

public class PatchOperationSequence : PatchOperation
{
	private List<PatchOperation> operations;

	private PatchOperation lastFailedOperation;

	protected override bool ApplyWorker(XmlDocument xml)
	{
		foreach (PatchOperation operation in operations)
		{
			if (!operation.Apply(xml))
			{
				lastFailedOperation = operation;
				return false;
			}
		}
		return true;
	}

	public override void Complete(string modIdentifier)
	{
		base.Complete(modIdentifier);
		lastFailedOperation = null;
	}

	public override string ToString()
	{
		int num = ((operations != null) ? operations.Count : 0);
		string text = $"{base.ToString()}(count={num}";
		if (lastFailedOperation != null)
		{
			text = text + ", lastFailedOperation=" + lastFailedOperation;
		}
		return text + ")";
	}
}
