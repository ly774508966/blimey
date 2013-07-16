﻿// ┌────────────────────────────────────────────────────────────────────────┐ \\
// │ Blimey - Fast, efficient, high level engine built upon Cor & Abacus    │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Brought to you by:                                                     │ \\
// │          _________                    .__               __             │ \\
// │         /   _____/__ __  ____    ____ |__|____    _____/  |_           │ \\
// │         \_____  \|  |  \/    \  / ___\|  \__  \  /    \   __\          │ \\
// │         /        \  |  /   |  \/ /_/  >  |/ __ \|   |  \  |            │ \\
// │        /_______  /____/|___|  /\___  /|__(____  /___|  /__|            │ \\
// │                \/           \//_____/         \/     \/                │ \\
// │                                                                        │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Copyright © 2013 A.J.Pook (http://sungiant.github.com)                 │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Permission is hereby granted, free of charge, to any person obtaining  │ \\
// │ a copy of this software and associated documentation files (the        │ \\
// │ "Software"), to deal in the Software without restriction, including    │ \\
// │ without limitation the rights to use, copy, modify, merge, publish,    │ \\
// │ distribute, sublicense, and/or sellcopies of the Software, and to      │ \\
// │ permit persons to whom the Software is furnished to do so, subject to  │ \\
// │ the following conditions:                                              │ \\
// │                                                                        │ \\
// │ The above copyright notice and this permission notice shall be         │ \\
// │ included in all copies or substantial portions of the Software.        │ \\
// │                                                                        │ \\
// │ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        │ \\
// │ EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     │ \\
// │ MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. │ \\
// │ IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   │ \\
// │ CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   │ \\
// │ TORT OR OTHERWISE, ARISING FROM,OUT OF OR IN CONNECTION WITH THE       │ \\
// │ SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 │ \\
// └────────────────────────────────────────────────────────────────────────┘ \\

using System;
using Sungiant.Abacus;
using Sungiant.Abacus.SinglePrecision;

namespace Sungiant.Blimey
{
    public static class BlimeyMathsHelper
    {
        public static Single Distance(Single value1, Single value2)
        {
            return Math.Abs((Single)(value1 - value2));
        }

        public static T Clamp<T>(T value, T min, T max)
            where T : System.IComparable<T>
        {
            T result = value;

            if (value.CompareTo(max) > 0)
                result = max;

            if (value.CompareTo(min) < 0)
                result = min;

            return result;
        }

        public static Single Limit(ref Single zItem, Single zLower, Single zUpper)
        {
            if (zItem < zLower)
            {
                zItem = zLower;
            }

            else if (zItem > zUpper)
            {
                zItem = zUpper;
            }

            return zItem;
        }

        public static Single Wrap(ref Single zItem, Single zLower, Single zUpper)
        {
            while (zItem < zLower)
            {
                zItem += (zUpper - zLower);
            }

            while (zItem >= zUpper)
            {
                zItem -= (zUpper - zLower);
            }

            return zItem;
        }

        public static Quaternion EulerToQuaternion(Vector3 e)
        {
            
            Single x = RealMaths.ToRadians(e.X);
            Single y = RealMaths.ToRadians(e.Y);
            Single z = RealMaths.ToRadians(e.Z);

            Quaternion result;
            Quaternion.CreateFromYawPitchRoll(x, y, z, out result);
            return result;
        }

        public static Vector3 QuaternionToEuler(Quaternion rotation)
        {
            // This bad boy works, taken from: 

            Single q0 = rotation.W;
            Single q1 = rotation.Y;
            Single q2 = rotation.X;
            Single q3 = rotation.Z;

            Vector3 angles = Vector3.Zero;

            // METHOD 1: http://forums.create.msdn.com/forums/p/28687/159870.aspx

            angles.X = (Single)Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (Math.Pow(q1, 2) + Math.Pow(q2, 2)));
            angles.Y = (Single)Math.Asin(2 * (q0 * q2 - q3 * q1));
            angles.Z = (Single)Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (Math.Pow(q2, 2) + Math.Pow(q3, 2)));


            // METHOD 2: http://forums.create.msdn.com/forums/p/4574/23763.aspx
            //angles.X = (Single)Math.Atan2(2 * q1 * q0 - 2 * q2 * q3, 1 - 2 * Math.Pow(q1, 2) - 2 * Math.Pow(q3, 2));
            //angles.Z = (Single)Math.Asin(2 * q2 * q1 + 2 * q3 * q0);
            //angles.Y = (Single)Math.Atan2(2 * q2 * q0 - 2 * q1 * q3, 1 - 2 * Math.Pow(q2, 2) - 2 * Math.Pow(q3, 2));
            //if (q2 * q1 + q3 * q0 == 0.5)
            //{
            //    angles.X = (Single)(2 * Math.Atan2(q2, q0));
            //    angles.Y = 0;
            //}
            //else if (q2 * q1 + q3 * q0 == -0.5)
            //{
            //    angles.X = (Single)(-2 * Math.Atan2(q2, q0));
            //    angles.Y = 0;
            //}

            // METHOD 3: http://forums.create.msdn.com/forums/p/4574/23763.aspx
            //const Single Epsilon = 0.0009765625f;
            //const Single Threshold = 0.5f - Epsilon;
            //Single XY = q2 * q1;
            //Single ZW = q3 * q0;
            //Single TEST = XY + ZW;
            //if (TEST < -Threshold || TEST > Threshold)
            //{
            //    int sign = Math.Sign(TEST);
            //    angles.X = sign * 2 * (Single)Math.Atan2(q2, q0);
            //    angles.Y = sign * MathHelper.PiOver2;
            //    angles.Z = 0;
            //}
            //else
            //{
            //    Single XX = q2 * q2;
            //    Single XZ = q2 * q3;
            //    Single XW = q2 * q0;
            //    Single YY = q1 * q1;
            //    Single YW = q1 * q0;
            //    Single YZ = q1 * q3;
            //    Single ZZ = q3 * q3;
            //    angles.X = (Single)Math.Atan2(2 * YW - 2 * XZ, 1 - 2 * YY - 2 * ZZ);
            //    angles.Y = (Single)Math.Atan2(2 * XW - 2 * YZ, 1 - 2 * XX - 2 * ZZ);
            //    angles.Z = (Single)Math.Asin(2 * TEST);
            //}


            angles.X = RealMaths.ToDegrees(angles.X);
            angles.Y = RealMaths.ToDegrees(angles.Y);
            angles.Z = RealMaths.ToDegrees(angles.Z);


            return angles;
        }



        public static Vector3 QuaternionToYawPitchRoll(Quaternion q)
        {

            const Single Epsilon = 0.0009765625f;
            const Single Threshold = 0.5f - Epsilon;

            Single yaw;
            Single pitch;
            Single roll;

            Single XY = q.X * q.Y;
            Single ZW = q.Z * q.W;

            Single TEST = XY + ZW;

            if (TEST < -Threshold || TEST > Threshold)
            {

                int sign = Math.Sign(TEST);

                yaw = sign * 2 * (Single)Math.Atan2(q.X, q.W);

                Single piOver2;
                RealMaths.Pi(out piOver2);
				piOver2 /= 2;

                pitch = sign * piOver2;

                roll = 0;

            }
            else
            {

                Single XX = q.X * q.X;
                Single XZ = q.X * q.Z;
                Single XW = q.X * q.W;

                Single YY = q.Y * q.Y;
                Single YW = q.Y * q.W;
                Single YZ = q.Y * q.Z;

                Single ZZ = q.Z * q.Z;

                yaw = (Single)Math.Atan2(2 * YW - 2 * XZ, 1 - 2 * YY - 2 * ZZ);

                pitch = (Single)Math.Atan2(2 * XW - 2 * YZ, 1 - 2 * XX - 2 * ZZ);

                roll = (Single)Math.Asin(2 * TEST);

            }

            return new Vector3(yaw, pitch, roll);

        }

        public static Boolean CheckThatAllComponentsAreValidNumbers(Vector3 zVec)
        {
            if (Single.IsNaN(zVec.X) || Single.IsNaN(zVec.Y) || Single.IsNaN(zVec.Z))
            {
                return false;
            }
            return true;
        }

        /// Return angle between two vectors. Used for visbility testing and
        /// for checking angles between vectors for the road sign generation.
        public static Single GetAngleBetweenVectors(Vector3 vec1, Vector3 vec2)
        {


            // See http://en.wikipedia.org/wiki/Vector_(spatial)
            // for help and check out the Dot Product section ^^
            // Both vectors are normalized so we can save deviding through the
            // lengths.

            Boolean isVec1Ok = CheckThatAllComponentsAreValidNumbers(vec1);
            Boolean isVec2Ok = CheckThatAllComponentsAreValidNumbers(vec2);
            System.Diagnostics.Debug.Assert(isVec1Ok && isVec2Ok);

			Vector3.Normalise(ref vec1, out vec1);
			Vector3.Normalise(ref vec2, out vec2);
            Single dot;
            Vector3.Dot(ref vec1, ref vec2, out dot);
            dot = Clamp(dot, -1.0f, 1.0f);
            Single result = (Single)System.Math.Acos(dot);
            System.Diagnostics.Debug.Assert(!Single.IsNaN(result));
            return result;
        }

        public static Single GetSignedAngleBetweenVectors(Vector3 vec1, Vector3 vec2)
        {
            // See http://en.wikipedia.org/wiki/Vector_(spatial)
            // for help and check out the Dot Product section ^^
            // Both vectors are normalized so we can save deviding through the
            // lengths.

            Single dot;
            Vector3.Dot(ref vec1, ref vec2, out dot);
            Single angle = RealMaths.ArcCos(dot);

            //check to see if the car->camera vector is to the left or right of the
            //inverse car.look vector using the cross product
            //to do this we can just check the sign of the y as we set the y
            //of the two input vector to zero
            Vector3 cross;
            Vector3.Cross(ref vec1, ref vec2, out cross);
            Single sign = 1.0f;

            if (cross.Y < 0.0f)
            {
                sign = -1.0f;
            }

            //check to see if the angle between the car->camera vector and the inverse car.look
            //vector is greater than our limiting angle
            angle *= sign;

            Single pi;
            RealMaths.Pi(out pi);

            Single tau;
            RealMaths.Tau(out tau);

            while (angle < -pi)
                angle += tau;
            while (angle >= pi)
                angle -= tau;

            return angle;
        }

        /// Distance from our point to the line described by linePos1 and linePos2.
        public static Single DistanceToLine(Vector3 point, Vector3 linePos1, Vector3 linePos2)
        {
            // For help check out this article:
            // http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
            Vector3 lineVec = linePos2 - linePos1;
            Vector3 pointVec = linePos1 - point;

            Vector3 cross;
            Vector3.Cross(ref lineVec, ref pointVec, out cross);

            return cross.Length() / lineVec.Length();
        }

        /// Signed distance to plane
        public static Single SignedDistanceToPlane(Vector3 point, Vector3 planePosition, Vector3 planeNormal)
        {
            Vector3 pointVec = planePosition - point;

            Single dot;

            Vector3.Dot(ref planeNormal, ref pointVec, out dot);

            return dot;
        }


        public static string NiceMatrixString(Matrix44 mat)
        {
            return string.Format(
                "| {0:+00000.00;-00000.00;} {1:+00000.00;-00000.00;} {2:+00000.00;-00000.00;} {3:+00000.00;-00000.00;} |\n" +
                "| {4:+00000.00;-00000.00;} {5:+00000.00;-00000.00;} {6:+00000.00;-00000.00;} {7:+00000.00;-00000.00;} |\n" +
                "| {8:+00000.00;-00000.00;} {9:+00000.00;-00000.00;} {10:+00000.00;-00000.00;} {11:+00000.00;-00000.00;} |\n" +
                "| {12:+00000.00;-00000.00;} {13:+00000.00;-00000.00;} {14:+00000.00;-00000.00;} {15:+00000.00;-00000.00;} |\n",
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44) +

            "Translation: " + mat.Translation + "\n";
        }

        public static Single FastInverseSquareRoot(Single val)
        {
            if (Single.IsNaN(val))
                throw new Exception("FastInverseSquareRoot only works on numbers!");

            if (Single.IsInfinity(val))
                return 0f;

            if (val == 0f)
                return val;

            unsafe
            {
                Single halfVal = 0.5f * val;
                Int32 i = *(Int32*)&val;    // evil floating point bit level hacking
                i = 0x5f3759df - (i >> 1);  // what the fuck?
                val = *(Single*)&i;
                val = val * (1.5f - (halfVal * val * val));
                //val = val * (1.5f - (halfVal * val * val));
                return val;
            }
        }
    }
}
