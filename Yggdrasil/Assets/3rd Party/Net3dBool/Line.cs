/*
The MIT License (MIT)

Copyright (c) 2014 Sebastian Loncar

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

See:
D. H. Laidlaw, W. B. Trumbore, and J. F. Hughes.
"Constructive Solid Geometry for Polyhedral Objects"
SIGGRAPH Proceedings, 1986, p.161.

original author: Danilo Balby Silva Castanheira (danbalby@yahoo.com)

Ported from Java to C# by Sebastian Loncar, Web: http://www.loncar.de
Project: https://github.com/Arakis/Net3dBool

Optimized and refactored by: Lars Brubaker (larsbrubaker@matterhackers.com)
Project: https://github.com/MatterHackers/agg-sharp (an included library)
*/

using System;
using UnityEngine;

namespace Net3dBool
{
    /// <summary>
    /// Representation of a 3d line or a ray(represented by a direction and a point).
    /// </summary>
    public class Line
    {
        /// <summary>
        /// tolerance value to test equalities
        /// </summary>
        private readonly static float EqualityTolerance = 1e-10f;
        private static System.Random Rnd = new System.Random();
        private Vector3 StartPoint;

        /// <summary>
        /// Constructor for a line.The line created is the intersection between two planes
        /// </summary>
        /// <param name="face1">face representing one of the planes</param>
        /// <param name="face2">face representing one of the planes</param>
        public Line(Face face1, Face face2)
        {
            Vector3 normalFace1 = face1.GetNormal();
            Vector3 normalFace2 = face2.GetNormal();

            //direction: cross product of the faces normals
            Direction = Vector3.Cross(normalFace1, normalFace2);

            //if direction lenght is not zero (the planes aren't parallel )...
            if (!(Direction.magnitude < EqualityTolerance))
            {
                //getting a line point, zero is set to a coordinate whose direction
                //component isn't zero (line intersecting its origin plan)
                StartPoint = new Vector3();
                float d1 = -(normalFace1.x * face1.V1._Position.x + normalFace1.y * face1.V1._Position.y + normalFace1.z * face1.V1._Position.z);
                float d2 = -(normalFace2.x * face2.V1._Position.x + normalFace2.y * face2.V1._Position.y + normalFace2.z * face2.V1._Position.z);
                if (Mathf.Abs(Direction.x) > EqualityTolerance)
                {
                    StartPoint.x = 0;
                    StartPoint.y = (d2 * normalFace1.z - d1 * normalFace2.z) / Direction.x;
                    StartPoint.z = (d1 * normalFace2.y - d2 * normalFace1.y) / Direction.x;
                }
                else if (Mathf.Abs(Direction.y) > EqualityTolerance)
                {
                    StartPoint.x = (d1 * normalFace2.z - d2 * normalFace1.z) / Direction.y;
                    StartPoint.y = 0;
                    StartPoint.z = (d2 * normalFace1.x - d1 * normalFace2.x) / Direction.y;
                }
                else
                {
                    StartPoint.x = (d2 * normalFace1.y - d1 * normalFace2.y) / Direction.z;
                    StartPoint.y = (d1 * normalFace2.x - d2 * normalFace1.x) / Direction.z;
                    StartPoint.z = 0;
                }
            }

            Direction.Normalize();
        }

        /// <summary>
        /// Constructor for a ray
        /// </summary>
        /// <param name="direction">direction ray</param>
        /// <param name="point">beginning of the ray</param>
        public Line(Vector3 direction, Vector3 point)
        {
            Direction = direction;
            StartPoint = point;
            direction.Normalize();
        }

        private Line()
        {
        }

        /// <summary>
        /// line direction
        /// </summary>
        public Vector3 Direction { get; private set; }

        public Line Clone()
        {
            Line clone = new Line();
            clone.Direction = Direction;
            clone.StartPoint = StartPoint;
            return clone;
        }

        /// <summary>
        /// Computes the point resulting from the intersection with another line
        /// </summary>
        /// <param name="otherLine">the other line to apply the intersection. The lines are supposed to intersect</param>
        /// <returns>point resulting from the intersection. If the point coundn't be obtained, return null</returns>
        public Vector3 ComputeLineIntersection(Line otherLine)
        {
            //x = x1 + a1*t = x2 + b1*s
            //y = y1 + a2*t = y2 + b2*s
            //z = z1 + a3*t = z2 + b3*s

            Vector3 linePoint = otherLine.GetPoint();
            Vector3 lineDirection = otherLine.Direction;

            float t;
            if (Mathf.Abs(Direction.y * lineDirection.x - Direction.x * lineDirection.y) > EqualityTolerance)
            {
                t = (-StartPoint.y * lineDirection.x + linePoint.y * lineDirection.x + lineDirection.y * StartPoint.x - lineDirection.y * linePoint.x) / (Direction.y * lineDirection.x - Direction.x * lineDirection.y);
            }
            else if (Mathf.Abs(-Direction.x * lineDirection.z + Direction.z * lineDirection.x) > EqualityTolerance)
            {
                t = -(-lineDirection.z * StartPoint.x + lineDirection.z * linePoint.x + lineDirection.x * StartPoint.z - lineDirection.x * linePoint.z) / (-Direction.x * lineDirection.z + Direction.z * lineDirection.x);
            }
            else if (Mathf.Abs(-Direction.z * lineDirection.y + Direction.y * lineDirection.z) > EqualityTolerance)
            {
                t = (StartPoint.z * lineDirection.y - linePoint.z * lineDirection.y - lineDirection.z * StartPoint.y + lineDirection.z * linePoint.y) / (-Direction.z * lineDirection.y + Direction.y * lineDirection.z);
            }
            else
            {
#if DEBUG
                throw new InvalidOperationException();
#else
				return Vector3.zero;
#endif
            }

            float x = StartPoint.x + Direction.x * t;
            float y = StartPoint.y + Direction.y * t;
            float z = StartPoint.z + Direction.z * t;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Compute the point resulting from the intersection with a plane
        /// </summary>
        /// <param name="normal">the plane normal</param>
        /// <param name="planePoint">a plane point.</param>
        /// <returns>intersection point.If they don't intersect, return null</returns>
        public Vector3 ComputePlaneIntersection(Plane plane)
        {
            float distanceToStartFromOrigin = Vector3.Dot(plane.PlaneNormal, StartPoint);

            float distanceFromPlane = distanceToStartFromOrigin - plane.DistanceToPlaneFromOrigin;
            float denominator = Vector3.Dot(plane.PlaneNormal, Direction);

            if (Mathf.Abs(denominator) < EqualityTolerance)
            {
                //if line is paralel to the plane...
                if (Mathf.Abs(distanceFromPlane) < EqualityTolerance)
                {
                    //if line is contained in the plane...
                    return StartPoint;
                }
                else
                {
                    return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                }
            }
            else // line intercepts the plane...
            {
                float t = -distanceFromPlane / denominator;
                Vector3 resultPoint = new Vector3();
                resultPoint.x = StartPoint.x + t * Direction.x;
                resultPoint.y = StartPoint.y + t * Direction.y;
                resultPoint.z = StartPoint.z + t * Direction.z;

                return resultPoint;
            }
        }

        /// <summary>
        /// Computes the distance from the line point to another point
        /// </summary>
        /// <param name="otherPoint">the point to compute the distance from the line point. The point is supposed to be on the same line.</param>
        /// <returns>points distance. If the point submitted is behind the direction, the distance is negative</returns>
        public float ComputePointToPointDistance(Vector3 otherPoint)
        {
            float distance = (otherPoint - StartPoint).magnitude;
            Vector3 vec = new Vector3(otherPoint.x - StartPoint.x, otherPoint.y - StartPoint.y, otherPoint.z - StartPoint.z);
            vec.Normalize();
            if (Vector3.Dot(vec, Direction) < 0)
            {
                return -distance;
            }
            else
            {
                return distance;
            }
        }

        public Vector3 GetPoint()
        {
            return StartPoint;
        }

        /// <summary>
        /// Changes slightly the line direction
        /// </summary>
        public void PerturbDirection()
        {
            Vector3 perturbedDirection = Direction;
            perturbedDirection.x += 1e-5f * Random();
            perturbedDirection.y += 1e-5f * Random();
            perturbedDirection.z += 1e-5f * Random();

            Direction = perturbedDirection;
        }

        public void SetDirection(Vector3 direction)
        {
            Direction = direction;
        }

        public void SetPoint(Vector3 point)
        {
            StartPoint = point;
        }

        public string toString()
        {
            return "Direction: " + Direction.ToString() + "\nPoint: " + StartPoint.ToString();
        }

        private static float Random()
        {
            return (float)Rnd.NextDouble();
        }
    }
}