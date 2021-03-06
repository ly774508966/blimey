// ┌────────────────────────────────────────────────────────────────────────┐ \\
// │ __________.__  .__                                                     │ \\
// │ \______   \  | |__| _____   ____ ___.__.                               │ \\
// │  |    |  _/  | |  |/     \_/ __ <   |  |                               │ \\
// │  |    |   \  |_|  |  Y Y  \  ___/\___  |                               │ \\
// │  |______  /____/__|__|_|  /\___  > ____|                               │ \\
// │         \/              \/     \/\/                                    │ \\
// │                                                                        │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Copyright © 2012 - 2015 ~ Blimey Engine (http://www.blimey.io)         │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Authors:                                                               │ \\
// │ ~ Ash Pook (http://www.ajpook.com)                                     │ \\
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

namespace Blimey.Engine
{
    using System;
    using System.Runtime.InteropServices;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Fudge;
    using global::Blimey.Platform;
    using global::Blimey.Asset;
    using Abacus.SinglePrecision;

    using System.Linq;

    // ────────────────────────────────────────────────────────────────────────────────────────────────────────────── //

    /// <summary>
    /// Every object in a scene has a Transform. It's used to store and manipulate
    /// the position, rotation and scale of the object. Every Transforms can have a
    /// parent, which allows you to apply position, rotation and scale hierarchically.
    /// This is the hierarchy seen in the Hierarchy pane. They also support
    /// enumerators so you can loop through children using:
    /// </summary>
    public sealed class Transform
        : IEnumerable
    {

        public static Transform Origin = new Transform ();

        // DATA --------------------------------------------------
        Vector3 _localPosition = Vector3.Zero;
        Vector3 _localScale = new Vector3 (1, 1, 1);
        Quaternion _localRotation = Quaternion.Identity;
        List<Transform> _children = new List<Transform> ();
        List<Transform> _cachedHierarchyToRootParent = new List<Transform> (0);
        Transform _parent = null;
        //--------------------------------------------------------



        // The parent of the transform.
        public Transform Parent {
            get {
                return _parent;
            }
            set {
                _parent = value;
                _cachedHierarchyToRootParent.Clear ();
                Transform temp = this.Parent;
                while (temp != null) {
                    _cachedHierarchyToRootParent.Add (temp);
                    temp = temp.Parent;
                }
            }
        }

        // Returns the topmost transform in the hierarchy.
        public Transform Root {
            get {
                Transform temp = this;
                while (temp.Parent != null) {
                    temp = temp.Parent;
                }

                return temp;
            }
        }

        // How many child transforms?
        public int ChildCount {
            get {
                return _children.Count;
            }
        }

        // Matrix44 that transforms a point from local space into world space.
        internal Matrix44 LocalToWorld {
            get {
                Matrix44 trans = Matrix44.Identity;
                Transform temp = this.Parent;
                while (temp != null) {
                    trans = trans * temp.LocalLocation;
                    temp = temp.Parent;
                }
                return trans;
            }
        }

        // Matrix44 that transforms a point from world space into local space.
        internal Matrix44 WorldToLocal {
            get {
                // why doesn't this work
                //Matrix44 trans = Matrix44.Identity;
                //for (int i = _cachedHierarchyToRootParent.Count - 1; i > -1; --i)
                //{
                //    trans = _cachedHierarchyToRootParent[i].LocalLocation * trans;
                //}

                //use this for now
                Matrix44 loc2World = LocalToWorld;

                Matrix44 trans; Matrix44.Invert(ref loc2World, out trans);
                return trans;
            }
        }

        // In world space.
        public Vector3 Forward { get { return Location.Forward; } }

        public Vector3 Up { get { return Location.Up; } }

        public Vector3 Right { get { return Location.Right; } }

        public Vector3 Position {
            get
            {
                Vector3 localPos = LocalPosition;
                Matrix44 location; Matrix44.CreateTranslation(ref localPos, out location);
                Transform temp = this.Parent;
                while (temp != null)
                {
                    Matrix44 rotMat;
                    Quaternion lr = temp.LocalRotation;
                    Matrix44.CreateFromQuaternion(ref lr, out rotMat);
                    Matrix44.Transform(ref location, ref lr, out location);
                    //rotMat * location;


                    location.Translation += temp.LocalPosition;
                    temp = temp.Parent;
                }
                return location.Translation;
            }

            set
            {
                Matrix44 trans;
                Matrix44.CreateTranslation(ref value, out trans);

                Matrix44 newMat;
                Matrix44 w2l = WorldToLocal;
                Matrix44.Multiply(ref trans, ref w2l, out newMat);

                LocalPosition = newMat.Translation;
            }
        }

        public Quaternion Rotation {
            get {
                Quaternion rotation = LocalRotation;
                Transform temp = this.Parent;
                while (temp != null) {
                    rotation = rotation * temp.LocalRotation;
                    temp = temp.Parent;
                }
                return rotation;
            }
            set
            {
                Quaternion q = value;
                q.Normalise ();

                if (WorldToLocal != Matrix44.Identity)
                {
                    Matrix44 mat;
                    Matrix44.CreateFromQuaternion (ref q, out mat);

                    Matrix44 r = WorldToLocal * mat;

                    Quaternion newRot;
                    Quaternion.CreateFromRotationMatrix (ref r, out newRot);
                    LocalRotation = newRot;
                }
                else
                {
                    LocalRotation = q;
                }
            }
        }

        public Vector3 Scale {
            get {
                Vector3 scale = this.LocalScale;
                Transform temp = this.Parent;
                while (temp != null) {
                    scale = scale * temp.LocalScale;
                    temp = temp.Parent;
                }
                return scale;
            }
        }

        public Vector3 EulerAngles { get { return MathsUtils.QuaternionToEuler (Rotation); } }

        public Matrix44 Location {
            get {
                return LocalLocation * LocalToWorld;
            }
        }


        // Relative to the parent transform.
        public Vector3 LocalPosition { get { return _localPosition; } set { _localPosition = value; } }

        public Quaternion LocalRotation { get { return _localRotation; } set { _localRotation = value; _localRotation.Normalise(); } }

        public Vector3 LocalScale { get { return _localScale; } set { _localScale = value; } }

        public Vector3 LocalEulerAngles
        {
            get { return MathsUtils.QuaternionToEuler(_localRotation); }
            set { _localRotation = MathsUtils.EulerToQuaternion(value); }
        }

        public Matrix44 LocalLocation
        {
            get
            {
                Matrix44 scale;
                Matrix44.CreateScale(ref _localScale, out scale);

                Matrix44 rotation;
                Matrix44.CreateFromQuaternion(ref _localRotation, out rotation);

                Matrix44 translation;
                Matrix44.CreateTranslation(ref _localPosition, out translation);

                Matrix44 result = scale * rotation * translation;
                return result;
            }
        }


        // Moves the transform in the direction and distance of translation.
        //
        // If relativeTo is left out or set to Space.Self the movement is applied
        // relative to the transform's local axes. (the x, y and z axes shown when
        // selecting the object inside the Scene View.) If relativeTo is Space.World
        // the movement is applied relative to the world coordinate system.
        public void Translate (Vector3 translation)
        {
            Position += translation;
        }

        public void Translate (Vector3 translation, Space relativeTo)
        {
            if (relativeTo == Space.World)
                Position += translation;
            else
                LocalPosition += translation;
        }

        public void Translate (Vector3 translation, Transform relativeTo)
        {
            Vector3 pointInWorld = relativeTo.TransformPoint (translation);
            Vector3 worldTrans = pointInWorld - relativeTo.Position;
            this.Position += worldTrans;
        }

        public void Translate (Single x, Single y, Single z)
        {
            this.Translate (new Vector3 (x, y, z));
        }

        public void Translate (Single x, Single y, Single z, Space relativeTo)
        {
            this.Translate (new Vector3 (x, y, z), relativeTo);
        }

        public void Translate (Single x, Single y, Single z, Transform relativeTo)
        {
            this.Translate (new Vector3 (x, y, z), relativeTo);
        }

        // Applies a rotation of eulerAngles.z degrees around the z axis,
        // eulerAngles.x degrees around the x axis, and eulerAngles.y
        // degrees around the y axis (in that order).
        //
        // If relativeTo is left out or set to Space.Self the rotation is applied
        // around the transform's local axes. (The x, y and z axes shown when
        // selecting the object inside the Scene View.) If relativeTo is
        // Space.World the rotation is applied around the world x, y, z axes.
        public void Rotate (Vector3 eulerAngles)
        {
            throw new System.NotImplementedException();
        }

        public void Rotate (Vector3 axis, Single angle)
        {
            throw new System.NotImplementedException();
        }

        public void Rotate (Vector3 eulerAngles, Space relativeTo)
        {
            throw new System.NotImplementedException();
        }

        public void Rotate (Single xAngle, Single yAngle, Single zAngle)
        {
            throw new System.NotImplementedException();
        }

        public void Rotate (Vector3 axis, Single angle, Space relativeTo)
        {
            throw new System.NotImplementedException();
        }

        public void Rotate (Single xAngle, Single yAngle, Single zAngle, Space relativeTo)
        {
            throw new System.NotImplementedException();
        }

        // Rotates the transform about axis passing through point
        // in world coordinates by angle degrees.
        // This modifies both the position and the rotation of the transform.
        public void RotateAround (Vector3 axis, Single angle)
        {
            throw new System.NotImplementedException();
        }

        public void RotateAround (Vector3 point, Vector3 axis, Single angle)
        {
            throw new System.NotImplementedException();
        }

        public void RotateAroundLocal (Vector3 axis, Single angle)
        {
            throw new System.NotImplementedException();
        }

        //Rotates the transform so the forward vector points at /target/'s current position.
        //
        // Then it rotates the transform to point its up direction vector in the direction
        // hinted at by the worldUp vector. If you leave out the worldUp parameter, the
        // function will use the world y axis. worldUp is only a hint vector. The up
        // vector of the rotation will only match the worldUp vector if the forward
        // direction is perpendicular to worldUp
        public void LookAt (Transform target)
        {
            LookAt (target, Vector3.Up);
        }

        public void LookAt (Vector3 worldPosition)
        {
            LookAt (worldPosition, Vector3.Up);
        }

        public void LookAt (Transform target, Vector3 worldUp)
        {
            LookAt (target.Position, worldUp);
        }

        public void LookAt (Vector3 worldPosition, Vector3 worldUp)
        {
            Vector3 lookAtVector = worldPosition - this.Position;
            Vector3.Normalise(ref lookAtVector, out lookAtVector);

            Matrix44 newOrientation = Matrix44.Identity;

            newOrientation.Forward = lookAtVector;

            Vector3 newRight;
            Vector3.Cross(ref lookAtVector, ref worldUp, out newRight);

            Single epsilon; Maths.Epsilon(out epsilon);

            Single newRightLengthSquared =
                (newRight.X * newRight.X) +
                (newRight.Y * newRight.Y) +
                (newRight.Z * newRight.Z);

            if (newRightLengthSquared <= epsilon ||
                Single.IsInfinity (newRightLengthSquared)) {
                newRight = Vector3.Zero;
            } else {
                Vector3.Normalise(ref newRight, out newRight);

            }

            newOrientation.Right = newRight;

            Vector3 newUp;
            Vector3.Cross(ref newRight, ref lookAtVector, out newUp);

            Single newUpLengthSquared =
                (newUp.X * newUp.X) +
                (newUp.Y * newUp.Y) +
                (newUp.Z * newUp.Z);


            if (newUpLengthSquared <= epsilon ||
                Single.IsInfinity (newUpLengthSquared)) {
                newUp = Vector3.Zero;
            } else {
                Vector3.Normalise(ref newUp, out newUp);

            }

            newOrientation.Up = newUp;

            Quaternion rotation;
            Quaternion.CreateFromRotationMatrix(ref newOrientation, out rotation);

            this.Rotation = rotation;

            /*

            // A vector going from our parent game object to our Subject
            Vector3 lookAtVector = Subject.Position - this.Parent.Transform.Position;

            // A direction from our parent game object to our Subject
            Vector3.Normalise(ref lookAtVector, out lookAtVector);

            // Build a new orientation matrix
            Matrix44 newOrientation = Matrix44.Identity;

            Vector3 t1;
            Vector3.Normalise(ref lookAtVector, out t1);
            newOrientation.Forward = t1;

            if (LockToY)
            {
                Vector3 t2 = Vector3.Up;
                Vector3.Normalise(ref t2, out t2);
                newOrientation.Up = t2;

                Vector3 b = newOrientation.Backward;
                Vector3 u = newOrientation.Up;

                Vector3 r;
                Vector3.Cross(ref b, ref u, out r);
                Vector3.Normalise(ref r, out r);
                newOrientation.Right = r;
            }
            else
            {
                Vector3 f = newOrientation.Forward;
                Vector3 u = Vector3.Up;
                Vector3 r;
                Vector3.Cross(ref f, ref u, out r);
                Vector3.Normalise(ref r, out r);
                newOrientation.Right = r;

                Vector3.Cross(ref r, ref f, out u);
                Vector3.Normalise(ref u, out u);
                newOrientation.Up = u;
            }

            Quaternion rotation;
            Quaternion.CreateFromRotationMatrix(ref newOrientation, out rotation);
            this.Parent.Transform.Rotation = rotation;
        }

            */

        }


        // Transforms direction from local space to world space.
        // This operation is not affected by scale or position of the transform.
        // The returned vector has the same length as direction.
        public Vector3 TransformDirection (Vector3 direction)
        {
            Single length = direction.Length ();
            Vector3.Normalise(ref direction, out direction);
            var t = TransformPoint (direction);
            Vector3.Normalise(ref t, out t);

            return t * length;
        }

        public Vector3 TransformDirection (Single x, Single y, Single z)
        {
            return TransformDirection (new Vector3 (x, y, z));
        }

        // Transforms a direction from world space to local space.
        // The opposite of Transform.TransformDirection.
        // This operation is unaffected by scale.
        public Vector3 InverseTransformDirection (Vector3 direction)
        {
            Single length = direction.Length ();
            Vector3.Normalise(ref direction, out direction);
            var t = InverseTransformPoint(direction);
            Vector3.Normalise(ref t, out t);
            return t * length;
        }

        public Vector3 InverseTransformDirection (Single x, Single y, Single z)
        {
            return InverseTransformDirection (new Vector3 (x, y, z));
        }

        // Transforms position from local space to world space.
        // Note that the returned position is affected by scale.
        // Use Transform.TransformDirection if you are dealing with directions.
        public Vector3 TransformPoint (Vector3 position)
        {
            Matrix44 trans;
            Matrix44.CreateTranslation(ref position, out trans);

            return (trans * LocalToWorld).Translation;
        }

        public Vector3 TransformPoint (Single x, Single y, Single z)
        {
            return TransformPoint (new Vector3 (x, y, z));
        }

        // Transforms position from world space to local space. The
        // opposite of Transform.TransformPoint.
        // Note that the returned position is affected by scale. Use
        // Transform.InverseTransformDirection if you are dealing with directions.
        public Vector3 InverseTransformPoint (Vector3 position)
        {
            Matrix44 trans;
            Matrix44.CreateTranslation(ref position, out trans);

            return (trans * WorldToLocal).Translation;
        }

        public Vector3 InverseTransformPoint (Single x, Single y, Single z)
        {
            return InverseTransformPoint (new Vector3 (x, y, z));
        }

        // Unparents all children.
        // Useful if you want to destroy the root of a hierarchy without destroying the children.
        public void DetachChildren ()
        {
            while (_children.Count > 0) {
                _children [0].Parent = null;
                _children.RemoveAt (0);
            }
        }

        // Not sure if we want this?
        public Transform GetChild (int index)
        {
            if (_children.Count > index)
                return _children [index];

            return null;
        }



        // Returns a Booleanean value that indicates whether the transform
        // is a child of a given transform. true if this transform is a child,
        // deep child (child of a child...) or identical to this transform, otherwise false.
        public Boolean IsChildOf (Transform parent)
        {
            Transform temp = this;
            while (temp != null) {
                if (temp == parent)
                    return true;

                temp = temp.Parent;
            }

            return false;
        }

        public override String ToString ()
        {
            return
                "LOCAL \n" +
                string.Format (" - Position: |{0} {1} {2}|\n", LocalPosition.X, LocalPosition.Y, LocalPosition.Z) +
                string.Format (" - Rotation: |{0} {1} {2}|\n", LocalEulerAngles.X, LocalEulerAngles.Y, LocalEulerAngles.Z) +
                string.Format (" - Scale:    |{0} {1} {2}|\n", LocalScale.X, LocalScale.Y, LocalScale.Z) +

                "WORLD \n" +
                string.Format (" - Position: |{0} {1} {2}|\n", Position.X, Position.Y, Position.Z) +
                string.Format (" - Rotation: |{0} {1} {2}|\n", EulerAngles.X, EulerAngles.Y, EulerAngles.Z) +
                string.Format (" - Scale:    |{0} {1} {2}|\n", Scale.X, Scale.Y, Scale.Z);

        }
        //--------------------------------------------------------------------------

        // IEnumerable implementation
        public IEnumerator GetEnumerator ()
        {
            return new Enumerator (this);
        }

        // Nested Types
        sealed class Enumerator
            : IEnumerator
        {
            // Fields
            int currentIndex = -1;
            Transform outer;

            // Methods
            internal Enumerator (Transform outer)
            {
                this.outer = outer;
            }

            public Boolean MoveNext ()
            {
                int childCount = this.outer.ChildCount;
                return (++this.currentIndex < childCount);
            }

            public void Reset ()
            {
                this.currentIndex = -1;
            }

            // Properties
            public object Current {
                get {
                    return this.outer.GetChild (this.currentIndex);
                }
            }
        }
    }
}
