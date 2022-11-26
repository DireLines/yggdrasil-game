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

using System;
using UnityEngine;

namespace Net3dBool
{
    public class AxisAlignedBoundingBox
    {
        public Vector3 MinXYZ;
        public Vector3 MaxXYZ;

        public AxisAlignedBoundingBox(Vector3 minXYZ, Vector3 maxXYZ)
        {
            if (maxXYZ.x < minXYZ.x || maxXYZ.y < minXYZ.y || maxXYZ.z < minXYZ.z)
            {
                throw new ArgumentException("All values of min must be less than all values in max.");
            }

            MinXYZ = minXYZ;
            MaxXYZ = maxXYZ;
        }

        public Vector3 Size
        {
            get
            {
                return MaxXYZ - MinXYZ;
            }
        }

        public float XSize
        {
            get
            {
                return MaxXYZ.x - MinXYZ.x;
            }
        }

        public float YSize
        {
            get
            {
                return MaxXYZ.y - MinXYZ.y;
            }
        }

        public float ZSize
        {
            get
            {
                return MaxXYZ.z - MinXYZ.z;
            }
        }

        /// <summary>
        /// Geth the corners by quadrant of the bottom
        /// </summary>
        /// <param name="quadrantIndex"></param>
        public Vector3 GetBottomCorner(int quadrantIndex)
        {
            switch (quadrantIndex)
            {
                case 0:
                    return new Vector3(MaxXYZ.x, MaxXYZ.y, MinXYZ.z);

                case 1:
                    return new Vector3(MinXYZ.x, MaxXYZ.y, MinXYZ.z);

                case 2:
                    return new Vector3(MinXYZ.x, MinXYZ.y, MinXYZ.z);

                case 3:
                    return new Vector3(MaxXYZ.x, MinXYZ.y, MinXYZ.z);
            }

            return Vector3.zero;
        }

        public Vector3 Center
        {
            get
            {
                return (MinXYZ + MaxXYZ) / 2;
            }
        }

        /// <summary>
        /// This is the computation cost of doing an intersection with the given type.
        /// Attempt to give it in average CPU cycles for the intersecton.
        /// </summary>
        /// <returns></returns>
        public static float GetIntersectCost()
        {
            // it would be great to try and measure this more accurately.  This is a guess from looking at the intersect function.
            return 132;
        }

        public Vector3 GetCenter()
        {
            return (MinXYZ + MaxXYZ) * .5f;
        }

        public float GetCenterX()
        {
            return (MinXYZ.x + MaxXYZ.x) * .5f;
        }

        private float volumeCache = 0;

        public float GetVolume()
        {
            if (volumeCache == 0)
            {
                volumeCache = (MaxXYZ.x - MinXYZ.x) * (MaxXYZ.y - MinXYZ.y) * (MaxXYZ.z - MinXYZ.z);
            }

            return volumeCache;
        }

        private float SurfaceAreaCache = 0;

        public float GetSurfaceArea()
        {
            if (SurfaceAreaCache == 0)
            {
                float frontAndBack = (MaxXYZ.x - MinXYZ.x) * (MaxXYZ.z - MinXYZ.z) * 2;
                float leftAndRight = (MaxXYZ.y - MinXYZ.y) * (MaxXYZ.z - MinXYZ.z) * 2;
                float topAndBottom = (MaxXYZ.x - MinXYZ.x) * (MaxXYZ.y - MinXYZ.y) * 2;
                SurfaceAreaCache = frontAndBack + leftAndRight + topAndBottom;
            }

            return SurfaceAreaCache;
        }

        public Vector3 this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return MinXYZ;
                }
                else if (index == 1)
                {
                    return MaxXYZ;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public static AxisAlignedBoundingBox operator +(AxisAlignedBoundingBox A, AxisAlignedBoundingBox B)
        {
#if true
            return Union(A, B);
#else
            Vector3 calcMinXYZ = new Vector3();
            calcMinXYZ.x = Mathf.Min(A.minXYZ.x, B.minXYZ.x);
            calcMinXYZ.y = Mathf.Min(A.minXYZ.y, B.minXYZ.y);
            calcMinXYZ.z = Mathf.Min(A.minXYZ.z, B.minXYZ.z);

            Vector3 calcMaxXYZ = new Vector3();
            calcMaxXYZ.x = Mathf.Max(A.maxXYZ.x, B.maxXYZ.x);
            calcMaxXYZ.y = Mathf.Max(A.maxXYZ.y, B.maxXYZ.y);
            calcMaxXYZ.z = Mathf.Max(A.maxXYZ.z, B.maxXYZ.z);

            AxisAlignedBoundingBox combinedBounds = new AxisAlignedBoundingBox(calcMinXYZ, calcMaxXYZ);

            return combinedBounds;
#endif
        }

        public static AxisAlignedBoundingBox Union(AxisAlignedBoundingBox boundsA, AxisAlignedBoundingBox boundsB)
        {
            Vector3 minXYZ = Vector3.zero;
            minXYZ.x = Mathf.Min(boundsA.MinXYZ.x, boundsB.MinXYZ.x);
            minXYZ.y = Mathf.Min(boundsA.MinXYZ.y, boundsB.MinXYZ.y);
            minXYZ.z = Mathf.Min(boundsA.MinXYZ.z, boundsB.MinXYZ.z);

            Vector3 maxXYZ = Vector3.zero;
            maxXYZ.x = Mathf.Max(boundsA.MaxXYZ.x, boundsB.MaxXYZ.x);
            maxXYZ.y = Mathf.Max(boundsA.MaxXYZ.y, boundsB.MaxXYZ.y);
            maxXYZ.z = Mathf.Max(boundsA.MaxXYZ.z, boundsB.MaxXYZ.z);

            return new AxisAlignedBoundingBox(minXYZ, maxXYZ);
        }

        public static AxisAlignedBoundingBox Intersection(AxisAlignedBoundingBox boundsA, AxisAlignedBoundingBox boundsB)
        {
            Vector3 minXYZ = Vector3.zero;
            minXYZ.x = Mathf.Max(boundsA.MinXYZ.x, boundsB.MinXYZ.x);
            minXYZ.y = Mathf.Max(boundsA.MinXYZ.y, boundsB.MinXYZ.y);
            minXYZ.z = Mathf.Max(boundsA.MinXYZ.z, boundsB.MinXYZ.z);

            Vector3 maxXYZ = Vector3.zero;
            maxXYZ.x = Mathf.Max(minXYZ.x, Mathf.Min(boundsA.MaxXYZ.x, boundsB.MaxXYZ.x));
            maxXYZ.y = Mathf.Max(minXYZ.y, Mathf.Min(boundsA.MaxXYZ.y, boundsB.MaxXYZ.y));
            maxXYZ.z = Mathf.Max(minXYZ.z, Mathf.Min(boundsA.MaxXYZ.z, boundsB.MaxXYZ.z));

            return new AxisAlignedBoundingBox(minXYZ, maxXYZ);
        }

        public static AxisAlignedBoundingBox Union(AxisAlignedBoundingBox bounds, Vector3 vertex)
        {
            Vector3 minXYZ = Vector3.zero;
            minXYZ.x = Mathf.Min(bounds.MinXYZ.x, vertex.x);
            minXYZ.y = Mathf.Min(bounds.MinXYZ.y, vertex.y);
            minXYZ.z = Mathf.Min(bounds.MinXYZ.z, vertex.z);

            Vector3 maxXYZ = Vector3.zero;
            maxXYZ.x = Mathf.Max(bounds.MaxXYZ.x, vertex.x);
            maxXYZ.y = Mathf.Max(bounds.MaxXYZ.y, vertex.y);
            maxXYZ.z = Mathf.Max(bounds.MaxXYZ.z, vertex.z);

            return new AxisAlignedBoundingBox(minXYZ, maxXYZ);
        }

        public void Clamp(ref Vector3 positionToClamp)
        {
            if (positionToClamp.x < MinXYZ.x)
            {
                positionToClamp.x = MinXYZ.x;
            }
            else if (positionToClamp.x > MaxXYZ.x)
            {
                positionToClamp.x = MaxXYZ.x;
            }

            if (positionToClamp.y < MinXYZ.y)
            {
                positionToClamp.y = MinXYZ.y;
            }
            else if (positionToClamp.y > MaxXYZ.y)
            {
                positionToClamp.y = MaxXYZ.y;
            }

            if (positionToClamp.z < MinXYZ.z)
            {
                positionToClamp.z = MinXYZ.z;
            }
            else if (positionToClamp.z > MaxXYZ.z)
            {
                positionToClamp.z = MaxXYZ.z;
            }
        }

        public bool Contains(AxisAlignedBoundingBox bounds)
        {
            if (MinXYZ.x <= bounds.MinXYZ.x
                && MaxXYZ.x >= bounds.MaxXYZ.x
                && MinXYZ.y <= bounds.MinXYZ.y
                && MaxXYZ.y >= bounds.MaxXYZ.y
                && MinXYZ.z <= bounds.MinXYZ.z
                && MaxXYZ.z >= bounds.MaxXYZ.z)
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("min {0} - max {1}", MinXYZ, MaxXYZ);
        }
    }
}