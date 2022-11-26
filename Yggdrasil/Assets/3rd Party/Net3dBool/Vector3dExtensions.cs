

using UnityEngine;

namespace Net3dBool
{
    public static class Vector3Extensions 
    {
        /// <summary>
        /// Indicates whether this instance and a specified object are equal within an error range.
        /// </summary>
        /// <param name="OtherVector"></param>
        /// <param name="ErrorValue"></param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public static bool Equals(this Vector3 vector, Vector3 OtherVector, float ErrorValue)
        {
            if (vector.x < OtherVector.x + ErrorValue && vector.x > OtherVector.x - ErrorValue &&
                vector.y < OtherVector.y + ErrorValue && vector.y > OtherVector.y - ErrorValue &&
                vector.z < OtherVector.z + ErrorValue && vector.z > OtherVector.z - ErrorValue)
            {
                return true;
            }

            return false;
        }
    }
}