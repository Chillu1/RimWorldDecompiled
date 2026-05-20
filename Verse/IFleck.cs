using UnityEngine;

namespace Verse;

public interface IFleck
{
	void Setup(FleckCreationData creationData);

	bool TimeInterval(float deltaTime, Map map);

	void Draw(DrawBatch batch);

	Vector3 GetPosition();
}
