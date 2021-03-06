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

    internal class PotentialTapGesture
        : PotentialGesture
    {
        const Single MaxHoldTimeForTap = 0.6f;
        const Single MaxDisplacementForTap = 0.005f;

        Single timer = 0f;

        internal PotentialTapGesture(InputEventSystem inputEventSystem, String[] touchIDs)
            : base(inputEventSystem, GestureType.Tap, touchIDs)
        {

        }

        internal override Gesture Update(float dt, List<TouchTracker> touchTrackers)
        {
            if( failedGesture )
                throw new Exception("wrong!");

            this.timer += dt;

            if( this.timer > MaxHoldTimeForTap)
                failedGesture = true;

            var touchTracker = inputEventSystem.GetTouchTracker(touchIDs[0]);

            if (touchTracker == null)
            {
                failedGesture = true;
                return null;
            }


            if( touchTracker.Phase == TouchPhase.JustReleased )
            {
                float distanceTravelled = touchTracker.GetDistanceTraveled(TouchPositionSpace.RealWorld);
                if (distanceTravelled <= MaxDisplacementForTap)
                {
                    completedGesture = true;
                    return new Gesture(this.inputEventSystem, this.type, this.touchIDs);
                }
                else
                {
                    failedGesture = true;
                }

            }

            return null;
        }
    }
}
