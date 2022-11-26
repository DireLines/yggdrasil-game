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
    /// Representation of a bound - the extremes of a 3d component for each coordinate.
    /// </summary>
    public class Bound
    {
        /** maximum from the x coordinate */
        private float XMax;
        /** minimum from the x coordinate */
        private float XMin;
        /** maximum from the y coordinate */
        private float YMax;
        /** minimum from the y coordinate */
        private float YMin;
        /** maximum from the z coordinate */
        private float ZMax;
        /** minimum from the z coordinate */
        private float ZMin;

        /** tolerance value to test equalities */
        private readonly static float EqualityTolerance = 1e-10f;

        //---------------------------------CONSTRUCTORS---------------------------------//

        /**
        * Bound constructor for a face
        * 
        * @param p1 point relative to the first vertex
        * @param p2 point relative to the second vertex
        * @param p3 point relative to the third vertex
        */
        public Bound(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            XMax = XMin = p1.x;
            YMax = YMin = p1.y;
            ZMax = ZMin = p1.z;

            CheckVertex(p2);
            CheckVertex(p3);
        }

        /**
        * Bound constructor for a object 3d
        * 
        * @param vertices the object vertices
        */
        public Bound(Vector3[] vertices)
        {
            XMax = XMin = vertices[0].x;
            YMax = YMin = vertices[0].y;
            ZMax = ZMin = vertices[0].z;

            for (int i = 1; i < vertices.Length; i++)
            {
                CheckVertex(vertices[i]);
            }
        }

        //----------------------------------OVERRIDES-----------------------------------//

        /**
        * Makes a string definition for the bound object
        * 
        * @return the string definition
        */
        public string toString()
        {
            return "x: " + XMin + " .. " + XMax + "\ny: " + YMin + " .. " + YMax + "\nz: " + ZMin + " .. " + ZMax;
        }

        //--------------------------------------OTHERS----------------------------------//

        /**
        * Checks if a bound overlaps other one
        * 
        * @param bound other bound to make the comparison
        * @return true if they insersect, false otherwise
        */
        public bool Overlap(Bound bound)
        {
            if ((XMin > bound.YMax + EqualityTolerance) || (XMax < bound.YMin - EqualityTolerance) || (YMin > bound.YMax + EqualityTolerance) || (YMax < bound.YMin - EqualityTolerance) || (ZMin > bound.ZMax + EqualityTolerance) || (ZMax < bound.ZMin - EqualityTolerance))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //-------------------------------------PRIVATES---------------------------------//

        /**
        * Checks if one of the coordinates of a vertex exceed the ones found before 
        * 
        * @param vertex vertex to be tested
        */
        private void CheckVertex(Vector3 vertex)
        {
            if (vertex.x > XMax)
            {
                XMax = vertex.x;
            }
            else if (vertex.x < XMin)
            {
                XMin = vertex.x;
            }

            if (vertex.y > YMax)
            {
                YMax = vertex.y;
            }
            else if (vertex.y < YMin)
            {
                YMin = vertex.y;
            }

            if (vertex.z > ZMax)
            {
                ZMax = vertex.z;
            }
            else if (vertex.z < ZMin)
            {
                ZMin = vertex.z;
            }
        }
    }
}

