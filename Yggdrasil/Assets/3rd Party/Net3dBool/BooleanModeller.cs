﻿/*
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

using System.Collections.Generic;
// :)ematics;

namespace Net3dBool {
    /// <summary>
    /// Class used to apply boolean operations on solids.
    /// Two 'Solid' objects are submitted to this class constructor. There is a methods for
    /// each boolean operation. Each of these return a 'Solid' resulting from the application
    /// of its operation into the submitted solids.
    /// </summary>
    public class BooleanModeller {
        /** solid where boolean operations will be applied */
        private Object3D Object1;
        private Object3D Object2;

        //--------------------------------CONSTRUCTORS----------------------------------//

        /**
        * Constructs a BooleanModeller object to apply boolean operation in two solids.
        * Makes preliminary calculations
        *
        * @param solid1 first solid where boolean operations will be applied
        * @param solid2 second solid where boolean operations will be applied
        */

        public BooleanModeller(Solid solid1, Solid solid2) {
            //representation to apply boolean operations
            Object1 = new Object3D(solid1);
            Object2 = new Object3D(solid2);

            //split the faces so that none of them intercepts each other
            Object1.SplitFaces(Object2);
            Object2.SplitFaces(Object1);

            //classify faces as being inside or outside the other solid
            Object1.ClassifyFaces(Object2);
            Object2.ClassifyFaces(Object1);
        }

        private BooleanModeller() {
        }

        //----------------------------------OVERRIDES-----------------------------------//

        /**
        * Clones the BooleanModeller object
        *
        * @return cloned BooleanModeller object
        */

        public BooleanModeller Clone() {
            BooleanModeller clone = new BooleanModeller();
            clone.Object1 = Object1.Clone();
            clone.Object2 = Object2.Clone();
            return clone;
        }

        //-------------------------------BOOLEAN_OPERATIONS-----------------------------//

        /**
        * Gets the solid generated by the union of the two solids submitted to the constructor
        *
        * @return solid generated by the union of the two solids submitted to the constructor
        */

        public Solid GetDifference() {
            Object2.InvertInsideFaces();
            Solid result = ComposeSolid(Status.OUTSIDE, Status.OPPOSITE, Status.INSIDE);
            Object2.InvertInsideFaces();

            return result;
        }

        public Solid GetIntersection() {
            return ComposeSolid(Status.INSIDE, Status.SAME, Status.INSIDE);
        }

        public Solid GetUnion() {
            return ComposeSolid(Status.OUTSIDE, Status.SAME, Status.OUTSIDE);
        }

        /**
        * Gets the solid generated by the intersection of the two solids submitted to the constructor
        *
        * @return solid generated by the intersection of the two solids submitted to the constructor.
        * The generated solid may be empty depending on the solids. In this case, it can't be used on a scene
        * graph. To check this, use the Solid.isEmpty() method.
        */
        /** Gets the solid generated by the difference of the two solids submitted to the constructor.
        * The fist solid is substracted by the second.
        *
        * @return solid generated by the difference of the two solids submitted to the constructor
        */

        //--------------------------PRIVATES--------------------------------------------//

        /**
        * Composes a solid based on the faces status of the two operators solids:
        * Status.INSIDE, Status.OUTSIDE, Status.SAME, Status.OPPOSITE
        *
        * @param faceStatus1 status expected for the first solid faces
        * @param faceStatus2 other status expected for the first solid faces
        * (expected a status for the faces coincident with second solid faces)
        * @param faceStatus3 status expected for the second solid faces
        */

        private Solid ComposeSolid(Status faceStatus1, Status faceStatus2, Status faceStatus3) {
            var vertices = new List<Vertex>();
            var indices = new List<int>();

            //group the elements of the two solids whose faces fit with the desired status
            GroupObjectComponents(Object1, vertices, indices, faceStatus1, faceStatus2);
            GroupObjectComponents(Object2, vertices, indices, faceStatus3, faceStatus3);

            //turn the arrayLists to arrays
            Vector3d[] verticesArray = new Vector3d[vertices.Count];
            for (int i = 0; i < vertices.Count; i++) {
                verticesArray[i] = vertices[i].Position;
            }
            int[] indicesArray = new int[indices.Count];
            for (int i = 0; i < indices.Count; i++) {
                indicesArray[i] = indices[i];
            }

            //returns the solid containing the grouped elements
            return new Solid(verticesArray, indicesArray);
        }

        /**
        * Fills solid arrays with data about faces of an object generated whose status
        * is as required
        *
        * @param object3d solid object used to fill the arrays
        * @param vertices vertices array to be filled
        * @param indices indices array to be filled
        * @param faceStatus1 a status expected for the faces used to to fill the data arrays
        * @param faceStatus2 a status expected for the faces used to to fill the data arrays
        */

        private void GroupObjectComponents(Object3D obj, List<Vertex> vertices, List<int> indices, Status faceStatus1, Status faceStatus2) {
            Face face;
            //for each face..
            for (int i = 0; i < obj.NumFaces; i++) {
                face = obj.GetFace(i);
                //if the face status fits with the desired status...
                if (face.GetStatus() == faceStatus1 || face.GetStatus() == faceStatus2) {
                    //adds the face elements into the arrays
                    Vertex[] faceVerts = { face.V1, face.V2, face.V3 };
                    for (int j = 0; j < faceVerts.Length; j++) {
                        if (vertices.Contains(faceVerts[j])) {
                            indices.Add(vertices.IndexOf(faceVerts[j]));
                        } else {
                            indices.Add(vertices.Count);
                            vertices.Add(faceVerts[j]);
                        }
                    }
                }
            }
        }
    }
}