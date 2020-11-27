﻿//-----------------------------------------------------------------------
// <copyright file="PinchGestureRecognizer.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//
// Modified 2020 by Unity Technologies Inc.
//
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem.Controls;

namespace UnityEngine.Reflect.Viewer.Input
{
    /// <summary>
    /// Gesture Recognizer for when the user performs a two-finger pinch motion on the touch screen.
    /// </summary>
    public class PinchGestureRecognizer : GestureRecognizer<PinchGesture>
    {
        const float k_SlopInches = 0.2f;
        const float k_SlopMotionDirectionDegrees = 30.0f;

        internal float m_SlopInches => k_SlopInches;

        internal float m_SlopMotionDirectionDegrees => k_SlopMotionDirectionDegrees;

        private TouchControl m_Touch1;
        private TouchControl m_Touch2;

        internal PinchGestureRecognizer(TouchControl touch1, TouchControl touch2)
        {
            m_Touch1 = touch1;
            m_Touch2 = touch2;
        }

        /// <summary>
        /// Creates a Pinch gesture with the given touches.
        /// </summary>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        /// <returns>The created Tap gesture.</returns>
        internal PinchGesture CreateGesture(TouchControl touch1, TouchControl touch2)
        {
            return new PinchGesture(this, touch1, touch2);
        }

        /// <summary>
        /// Tries to create a Pinch Gesture.
        /// </summary>
        protected internal override void TryCreateGestures()
        {
            TryCreateGestureTwoFingerGestureOnTouchBeganForTouchControls(m_Touch1, m_Touch2, CreateGesture);
        }
    }
}
