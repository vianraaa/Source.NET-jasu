using Source.Common.Formats.BSP;
using Source.Common.Mathematics;
using Source.Common.Physics;

using System.Numerics;

namespace Source.Common.Engine;

public enum TraceType
{
	Everything = 0,
	WorldOnly,
	EntitiesOnly,
	EverythingFilterProps,
}

public interface ITraceFilter
{
	bool ShouldHitEntity(IHandleEntity entity, Contents contentsMask);
	TraceType GetTraceType();
}

public class TraceFilter : ITraceFilter
{
	public virtual bool ShouldHitEntity(IHandleEntity entity, Contents contentsMask) => throw new NotImplementedException();
	public TraceType GetTraceType() => TraceType.Everything;
}

public class TraceFilterEntitiesOnly : ITraceFilter
{
	public bool ShouldHitEntity(IHandleEntity entity, Contents contentsMask) => throw new NotImplementedException();
	public TraceType GetTraceType() => TraceType.EntitiesOnly;
}

public class TraceFilterWorldOnly : ITraceFilter
{
	public bool ShouldHitEntity(IHandleEntity entity, Contents contentsMask) => false;
	public TraceType GetTraceType() => TraceType.WorldOnly;
}

public class TraceFilterWorldAndPropsOnly : ITraceFilter
{
	public bool ShouldHitEntity(IHandleEntity entity, Contents contentsMask) => false;
	public TraceType GetTraceType() => TraceType.Everything;
}

public class TraceFilterHitAll : TraceFilter
{
	public override bool ShouldHitEntity(IHandleEntity entity, Contents contentsMask) => true;
}

public interface IEntityEnumerator; // todo
public interface IEngineTrace
{
	int GetPointContents(in Vector3 vecAbsPosition, out IHandleEntity? entity);
	int GetPointContents_Collideable(ICollideable? collide, in Vector3 vecAbsPosition);
	void ClipRayToEntity(in Ray ray, uint mask, IHandleEntity ent, ref Trace pTrace);
	void ClipRayToCollideable(in Ray ray, uint mask, ICollideable collide, ref Trace pTrace);
	void TraceRay(in Ray ray, uint mask, ITraceFilter traceFilter, ref Trace pTrace);
	void SetupLeafAndEntityListRay(in Ray ray, TraceListData traceData );
	void SetupLeafAndEntityListBox(in Vector3 vecBoxMin, in Vector3 vecBoxMax, TraceListData traceData );
	void TraceRayAgainstLeafAndEntityList(in Ray ray, TraceListData traceData, uint mask, ITraceFilter? traceFilter, ref Trace trace);
	void SweepCollideable(ICollideable? collide, in Vector3 vecAbsStart, in Vector3 vecAbsEnd, in QAngle vecAngles, uint mask, ITraceFilter? traceFilter, ref Trace trace);
	void EnumerateEntities(in Ray ray, bool triggers, IEntityEnumerator enumerator);
	void EnumerateEntities(in Vector3 vecAbsMins, in Vector3 vecAbsMaxs, IEntityEnumerator enumerator);
	ICollideable? GetCollideable(IHandleEntity? entity);
	int GetStatByIndex(int index, bool bClear);
	void GetBrushesInAABB(in Vector3 mins, in Vector3 maxs, List<int> output, Contents contentsMask = unchecked((Contents)0xFFFFFFFF));
	IPhysCollide? GetCollidableFromDisplacementsInAABB(in Vector3 mins, in Vector3 maxs);
	bool GetBrushInfo(int iBrush, ref List<Vector4> planesOut, out Contents contents);
	int GetLeafContainingPoint(in Vector3 test);
}