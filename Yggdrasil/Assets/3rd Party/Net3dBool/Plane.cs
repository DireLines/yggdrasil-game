/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/


using UnityEngine;

namespace Net3dBool
{
    public class Plane
    {
        public float DistanceToPlaneFromOrigin;
        public Vector3 PlaneNormal;
        private const float TreatAsZero = .000000001f;

        public Plane(Vector3 planeNormal, float distanceFromOrigin)
        {
            PlaneNormal = planeNormal.normalized;
            DistanceToPlaneFromOrigin = distanceFromOrigin;
        }

        public Plane(Vector3 point0, Vector3 point1, Vector3 point2)
        {
            PlaneNormal = Vector3.Cross(point1 - point0, point2 - point0).normalized;
            DistanceToPlaneFromOrigin = Vector3.Dot(PlaneNormal, point0);
        }

        public Plane(Vector3 planeNormal, Vector3 pointOnPlane)
        {
            PlaneNormal = planeNormal.normalized;
            DistanceToPlaneFromOrigin = Vector3.Dot(planeNormal, pointOnPlane);
        }

        public float GetDistanceFromPlane(Vector3 positionToCheck)
        {
            float distanceToPointFromOrigin = Vector3.Dot(positionToCheck, PlaneNormal);
            return distanceToPointFromOrigin - DistanceToPlaneFromOrigin;
        }

        public float GetDistanceToIntersection(Ray ray, out bool inFront)
        {
            inFront = false;
            float normalDotRayDirection = Vector3.Dot(PlaneNormal, ray.DirectionNormal);
            if (normalDotRayDirection < TreatAsZero && normalDotRayDirection > -TreatAsZero) // the ray is parallel to the plane
            {
                return float.PositiveInfinity;
            }

            if (normalDotRayDirection < 0)
            {
                inFront = true;
            }

            return (DistanceToPlaneFromOrigin - Vector3.Dot(PlaneNormal, ray.Origin)) / normalDotRayDirection;
        }

        public float GetDistanceToIntersection(Vector3 pointOnLine, Vector3 lineDirection)
        {
            float normalDotRayDirection = Vector3.Dot(PlaneNormal, lineDirection);
            if (normalDotRayDirection < TreatAsZero && normalDotRayDirection > -TreatAsZero) // the ray is parallel to the plane
            {
                return float.PositiveInfinity;
            }

            float planeNormalDotPointOnLine = Vector3.Dot(PlaneNormal, pointOnLine);
            return (DistanceToPlaneFromOrigin - planeNormalDotPointOnLine) / normalDotRayDirection;
        }

        public bool RayHitPlane(Ray ray, out float distanceToHit, out bool hitFrontOfPlane)
        {
            distanceToHit = float.PositiveInfinity;
            hitFrontOfPlane = false;

            float normalDotRayDirection = Vector3.Dot(PlaneNormal, ray.DirectionNormal);
            if (normalDotRayDirection < TreatAsZero && normalDotRayDirection > -TreatAsZero) // the ray is parallel to the plane
            {
                return false;
            }

            if (normalDotRayDirection < 0)
            {
                hitFrontOfPlane = true;
            }

            float distanceToRayOriginFromOrigin = Vector3.Dot(PlaneNormal, ray.Origin);

            float distanceToPlaneFromRayOrigin = DistanceToPlaneFromOrigin - distanceToRayOriginFromOrigin;

            bool originInFrontOfPlane = distanceToPlaneFromRayOrigin < 0;

            bool originAndHitAreOnSameSide = originInFrontOfPlane == hitFrontOfPlane;
            if (!originAndHitAreOnSameSide)
            {
                return false;
            }

            distanceToHit = distanceToPlaneFromRayOrigin / normalDotRayDirection;
            return true;
        }

        public bool LineHitPlane(Vector3 start, Vector3 end, out Vector3 intersectionPosition)
        {
            float distanceToStartFromOrigin = Vector3.Dot(PlaneNormal, start);
            if (distanceToStartFromOrigin == 0)
            {
                intersectionPosition = start;
                return true;
            }

            float distanceToEndFromOrigin = Vector3.Dot(PlaneNormal, end);
            if (distanceToEndFromOrigin == 0)
            {
                intersectionPosition = end;
                return true;
            }

            if ((distanceToStartFromOrigin < 0 && distanceToEndFromOrigin > 0)
                || (distanceToStartFromOrigin > 0 && distanceToEndFromOrigin < 0))
            {
                Vector3 direction = (end - start).normalized;

                float startDistanceFromPlane = distanceToStartFromOrigin - DistanceToPlaneFromOrigin;
                float endDistanceFromPlane = distanceToEndFromOrigin - DistanceToPlaneFromOrigin;
                float lengthAlongPlanNormal = endDistanceFromPlane - startDistanceFromPlane;

                float ratioToPlanFromStart = startDistanceFromPlane / lengthAlongPlanNormal;
                intersectionPosition = start + direction * ratioToPlanFromStart;

                return true;
            }

            intersectionPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            return false;
        }
    }
}