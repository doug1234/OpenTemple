using System;
using System.Diagnostics;
using System.Numerics;

namespace OpenTemple.Core.AAS;

/// <summary>
/// Represents an edge (between two vertices) and the distance (squared).
/// </summary>
struct EdgeDistance {
    public short To;
    public short From;
    public float DistSquared;
};

internal class AasClothStuff
{

    private readonly Vector4[] _vertexPosBuffer;

    public readonly int clothVertexCount;
    public Memory<Vector4> clothVertexPos1;
    public Memory<Vector4> clothVertexPos2;
    public Memory<Vector4> clothVertexPos3;
    public readonly byte[] bytePerVertex;
    public readonly int boneDistancesCount;
    public readonly int boneDistancesCountDelta;
    public readonly EdgeDistance[] boneDistances;
    public readonly int boneDistances2Count;
    public readonly int boneDistances2CountDelta;
    public readonly EdgeDistance[] boneDistances2;
    public readonly CollisionSphere spheres;
    public readonly CollisionCylinder cylinders;
    public float field_34_time;

    internal AasClothStuff(
        ReadOnlySpan<Vector4> clothVertexPos,
        ReadOnlySpan<byte> bytePerClothVertex,
        ReadOnlySpan<EdgeDistance> edges,
        ReadOnlySpan<EdgeDistance> indirectEdges,
        CollisionSphere spheres,
        CollisionCylinder cylinders
    )
    {

        this.spheres = spheres;
        this.cylinders = cylinders;

        clothVertexCount = clothVertexPos.Length;
        _vertexPosBuffer = new Vector4[2 * clothVertexPos.Length];

        // separate the buffer into two parts (first half, second half)
        clothVertexPos1 = _vertexPosBuffer.AsMemory(0, clothVertexPos.Length);
        clothVertexPos2 = _vertexPosBuffer.AsMemory(0, clothVertexPos.Length);
        clothVertexPos3 = _vertexPosBuffer.AsMemory(clothVertexPos.Length, clothVertexPos.Length);

        clothVertexPos.CopyTo(clothVertexPos1.Span);
        clothVertexPos.CopyTo(clothVertexPos3.Span);

        Trace.Assert(bytePerClothVertex.Length == clothVertexPos.Length);
        bytePerVertex = bytePerClothVertex.ToArray();
        
        if (!edges.IsEmpty)
        {
            boneDistances = edges.ToArray();

            SplitEdges(
                boneDistances,
                bytePerClothVertex,
                out boneDistancesCount,
                out boneDistancesCountDelta);
        }
        else
        {
            boneDistancesCount = 0;
            boneDistancesCountDelta = 0;
            boneDistances = Array.Empty<EdgeDistance>();
        }

        if (!indirectEdges.IsEmpty)
        {
            boneDistances2 = indirectEdges.ToArray();

            SplitEdges(
                boneDistances2,
                bytePerClothVertex,
                out boneDistances2Count,
                out boneDistances2CountDelta);
        }
        else
        {
            boneDistances2Count = 0;
            boneDistances2CountDelta = 0;
            boneDistances2 = Array.Empty<EdgeDistance>();
        }
        field_34_time = 0.0f;
    }

    public void UpdateBoneDistances()
    {
        clothVertexPos2.CopyTo(clothVertexPos3);

        var positions = clothVertexPos2.Span;

        for (var i = 0; i < boneDistancesCountDelta + boneDistancesCount; i++) {
            ref var distance = ref boneDistances[i];
            var pos1 = positions[distance.To];
            var pos2 = positions[distance.From];

            var deltaX = pos1.X - pos2.X;
            var deltaY = pos1.Y - pos2.Y;
            var deltaZ = pos1.Z - pos2.Z;
            distance.DistSquared = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        }

        for (var i = 0; i < boneDistances2CountDelta + boneDistances2Count; i++) {
            ref var distance = ref boneDistances2[i];
            var pos1 = positions[distance.To];
            var pos2 = positions[distance.From];

            var deltaX = pos1.X - pos2.X;
            var deltaY = pos1.Y - pos2.Y;
            var deltaZ = pos1.Z - pos2.Z;
            distance.DistSquared = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        }
    }

    public void Simulate(float time)
    {

        int stepCount = (int)MathF.Ceiling(time * 200.0f);
        float timePerStep;
        if (stepCount <= 5) {
            timePerStep = time / stepCount;
        }
        else {
            stepCount = 5;
            timePerStep = 0.005f;
        }

        for (int i = 0; i < stepCount; i++) {
            SimulateStep(timePerStep);
            ApplySprings();
            ApplySprings();
            HandleCollisions();
        }
    }

    private void SimulateStep(float stepTime)
    {

        if (field_34_time <= 0.0 || stepTime <= 0.0)
        {
            field_34_time = stepTime;
        }
        else
        {
            var factor = (MathF.Pow(2.0f, -stepTime) * stepTime + field_34_time) / field_34_time;

            var oldState = clothVertexPos2.Span;
            var newState = clothVertexPos3.Span;

            for (var i = 0; i < clothVertexCount; i++) {
                if (bytePerVertex[i] == 1) {
                    newState[i] = oldState[i];
                } else {
                    ref var newPos = ref newState[i];
                    ref var oldPos = ref oldState[i];

                    newPos.X += (oldPos.X - newPos.X) * factor;
                    newPos.Y += (oldPos.Y - newPos.Y) * factor;
                    newPos.Z += (oldPos.Z - newPos.Z) * factor;
                    newState[i].Y -= stepTime * stepTime * 2400.0f;
                }
            }

            Swap(ref clothVertexPos2, ref clothVertexPos3);
            field_34_time = stepTime;
        }

    }

    private void ApplySprings()
    {

        var positions = clothVertexPos2.Span;

        for (var i = 0; i < boneDistancesCount; i++) {
            ref var distance = ref boneDistances[i];

            ref var pos1 = ref positions[distance.To];
            ref var pos2 = ref positions[distance.From];

            var deltaX = pos1.X - pos2.X;
            var deltaY = pos1.Y - pos2.Y;
            var deltaZ = pos1.Z - pos2.Z;

            var factor = 2 * (distance.DistSquared / (deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + distance.DistSquared) - 0.5f);

            pos2.X -= deltaX * factor;
            pos2.Y -= deltaY * factor;
            pos2.Z -= deltaZ * factor;
        }

        for (var i = 0; i < boneDistancesCountDelta; i++) {

            ref var distance = ref boneDistances[boneDistancesCount + i];

            ref var pos1 = ref positions[distance.To];
            ref var pos2 = ref positions[distance.From];

            var deltaX = pos1.X - pos2.X;
            var deltaY = pos1.Y - pos2.Y;
            var deltaZ = pos1.Z - pos2.Z;

            var factor = (distance.DistSquared / (deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + distance.DistSquared) - 0.5f);

            pos1.X += deltaX * factor;
            pos1.Y += deltaY * factor;
            pos1.Z += deltaZ * factor;
            pos2.X -= deltaX * factor;
            pos2.Y -= deltaY * factor;
            pos2.Z -= deltaZ * factor;
        }
    }

    private void HandleCollisions()
    {

        var positions = clothVertexPos2.Span;

        for (var i = 0; i < clothVertexCount; i++) {

            if (bytePerVertex[i] == 0) {
                ref var pos = ref positions[i];
                // Collision with (assumed) ground plane
                if (pos.Y < 0.0f) {
                    pos.Y = 0.0f;
                }

                for (var sphere = spheres; sphere != null; sphere = sphere.Next) {
                    var posInSphere = Matrix3x4.transformPosition(sphere.WorldMatrixInverse, pos);
                    var distanceFromOriginSq = posInSphere.LengthSquared();
                    if (distanceFromOriginSq < sphere.Radius * sphere.Radius) {
                        var ratio = sphere.Radius / MathF.Sqrt(distanceFromOriginSq);
                        posInSphere.X *= ratio;
                        posInSphere.Y *= ratio;
                        posInSphere.Z *= ratio;
                        pos = Matrix3x4.transformPosition(sphere.WorldMatrix, posInSphere);
                    }
                }

                for (var cylinder = cylinders; cylinder != null; cylinder = cylinder.Next) {
                    var posInCylinder = Matrix3x4.transformPosition(cylinder.WorldMatrixInverse, pos);

                    if (posInCylinder.Z >= 0.0 && posInCylinder.Z <= cylinder.Height) {
                        var distanceFromOriginSq = posInCylinder.X * posInCylinder.X + posInCylinder.Y * posInCylinder.Y;
                        if (distanceFromOriginSq < cylinder.Radius * cylinder.Radius) {
                            var ratio = cylinder.Radius / MathF.Sqrt(distanceFromOriginSq);
                            posInCylinder.X *= ratio;
                            posInCylinder.Y *= ratio;

                            pos = Matrix3x4.transformPosition(cylinder.WorldMatrix, posInCylinder);
                        }
                    }
                }
            }

        }
    }

    private void SplitEdges(
        Span<EdgeDistance> edges,
        ReadOnlySpan<byte> bytePerClothVertex,
        out int boneCountOut,
        out int boneCountOutDelta
    )
    {

        var firstBucketCount = 0;
        var edgesProcessed = 0;
        if (!edges.IsEmpty)
        {
            var edgesItB = 0;
            var edgesBack = edges.Length - 1;
            var edgesItA = edgesItB;
            while (edgesProcessed < edges.Length)
            {
                if (bytePerClothVertex[edges[edgesItB].To] == 0) {
                    if (bytePerClothVertex[edges[edgesItB].From] == 1)
                    {
                        // Swap vertices of edgesitb so that the first vertex is the one with flag=1
                        Swap(ref edges[edgesItB].To, ref edges[edgesItB].From);

                        // Then swap edgesItB with edgesItA
                        Swap(ref edges[edgesItA], ref edges[edgesItB]);
                        ++firstBucketCount;
                        ++edgesItA;
                    }
                    ++edgesProcessed;
                    ++edgesItB;
                    continue;
                }
                if (bytePerClothVertex[edges[edgesItB].From] == 0)
                {
                    // Swap EdgesitA with EdgesitB
                    Swap(ref edges[edgesItA], ref edges[edgesItB]);

                    ++firstBucketCount;
                    ++edgesItA;
                    ++edgesProcessed;
                    ++edgesItB;
                    continue;
                }
                // Removes the edge at edge iterator position B
                edges[edgesItB] = edges[edgesBack];
                edges = edges.Slice(0, edges.Length - 1);
                --edgesBack;
            }
        }

        boneCountOut = firstBucketCount;
        boneCountOutDelta = edges.Length - firstBucketCount;
    }

    private static void Swap<T>(ref T a, ref T b)
    {
        (a, b) = (b, a);
    }
}