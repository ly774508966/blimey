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
using System.Collections.Generic;
using Sungiant.Cor;

namespace Sungiant.Blimey
{
	public class InputEventSystem
	{
		MultiTouchController controller;

		internal InputEventSystem(ICor engine)
		{
			this.engine = engine;

			this.controller = engine.Input.GetMultiTouchController();
		}

		ICor engine;

		public delegate void GestureDelegate(Gesture gesture);

		public event GestureDelegate Tap;
		public event GestureDelegate DoubleTap;
		public event GestureDelegate Flick;
		//public event GestureDelegate DragStart;
		//public event GestureDelegate DragUpdate;
		//public event GestureDelegate DragEnd;
		//public event GestureDelegate Pinch;


		internal TouchTracker GetTouchTracker(Int32 id)
		{
			var found = touchTrackers.Find(q => q.TouchID == id);

			return found;
		}

		List<TouchTracker> touchTrackers = new List<TouchTracker>();

		Queue<Gesture> gestureQueue = new Queue<Gesture>();

		List<PotentialGesture> potentialGestures = new List<PotentialGesture>();

		internal void Reset()
		{
			// release all listeners
			Tap = null;
			DoubleTap = null;
			Flick = null;
		}

		internal virtual void Update(AppTime time)
		{
			if( controller != null )
			{
				// before this the child should have updated this TouchCollection
				this.UpdateTouchTrackers(time);
				this.UpdateGestureDetection(time);
				this.InvokeGestureEvents(time);
			}
		}

		void UpdateTouchTrackers(AppTime time)
		{
			
			// delete all touch trackers that whose last touch was in the released state
			int num = touchTrackers.RemoveAll(x => (x.Phase == TouchPhase.JustReleased || x.Phase == TouchPhase.Invalid));			

			if( num > 0 )
			{
				Teletype.WriteLine("Blimey.Input", string.Format("Removing {0} touches.", num));
			}

			// go through all active touches
			foreach (var touch in controller.TouchCollection)
			{
				// find the corresponding tracker

				TouchTracker tracker = touchTrackers.Find(x => (x.TouchID == touch.ID));

				if (tracker == null)
				{
					tracker = new TouchTracker(
						this.engine, 
						this.engine.System.ScreenSpecification,
						this.engine.System.PanelSpecification, 
						touch.ID );

					touchTrackers.Add(tracker);
				}

				tracker.RegisterTouch(touch);
			}

			// assert if there are any trackers in the list that have not been updated this frame
			var problems = touchTrackers.FindAll(x => (x.LatestTouch.FrameNumber != time.FrameNumber));
			System.Diagnostics.Debug.Assert(problems.Count == 0);
		}

		void UpdateGestureDetection(AppTime time)
		{
			// Each frame we look for press combinations that could potentially
			// be the start of a gesture.
			foreach (var touchTracker in touchTrackers)
			{
				if( touchTracker.Phase == TouchPhase.JustPressed )
				{
					// this could be the start of a tap
					var potentialTapGesture = 
						new PotentialTapGesture(
							this,
							new Int32[]{touchTracker.TouchID} );

					var potentialDoubleTapGesture = 
						new PotentialDoubleTapGesture(
							this,
							new Int32[]{touchTracker.TouchID} );

					var potentialFlickGesture = 
						new PotentialFlickGesture(
							this,
							new Int32[]{touchTracker.TouchID} );

					potentialGestures.Add(potentialTapGesture);
					potentialGestures.Add(potentialDoubleTapGesture);
					potentialGestures.Add(potentialFlickGesture);

				}

				int enqueueCount = 0;

				foreach(var potentialGesture in potentialGestures)
				{
					var gesture = potentialGesture.Update(time.Delta, touchTrackers);

					if( gesture != null )
					{
						this.gestureQueue.Enqueue(gesture);
						enqueueCount++;
					}
				}

				int removeCount = potentialGestures.Count;
				potentialGestures.RemoveAll(x => x.Finished );
				removeCount -= potentialGestures.Count;

			}
		}

		void InvokeGestureEvents(AppTime time)
		{
			foreach (var gesture in gestureQueue)
			{
				string line = string.Format("({1}) {0}", gesture.Type, gesture.ID);
				switch (gesture.Type)
				{
					case GestureType.Tap:
						line += string.Format(", finishing position {0}", gesture.GetFinishingPosition(TouchPositionSpace.NormalisedEngine));
						
						if (this.Tap != null)
						{
							this.Tap(gesture);
						}
						break;

					case GestureType.DoubleTap:
						if (this.DoubleTap != null)
						{
							this.DoubleTap(gesture);
						}
						break;

					case GestureType.Flick:
						line += string.Format(", finishing position {0}", gesture.GetFinishingPosition(TouchPositionSpace.NormalisedEngine));
						if (this.Flick != null)
						{
							this.Flick(gesture);
						}
						break;

					default: throw new System.NotImplementedException();
				}

				Teletype.WriteLine("Blimey.Input", line);
			}

			gestureQueue.Clear();
		}
	}
	
}

