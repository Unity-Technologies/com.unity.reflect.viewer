using System;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;

/*
Radial Layout Group by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace Unity.Reflect.Viewer.UI
{
    public class RadialLayout : LayoutGroup
    {
        public float fDistance;
        [Range(0f, 360f)]
        public float MinAngle, MaxAngle, StartAngle;
        float m_VRDefaultStartAngle = 250f;
        IDisposable m_Disposable;
        IUISelector<bool> m_VREnableSelector;

        protected override void OnEnable()
        {
            base.OnEnable();
            bool VREnable = false;

            if (UIStateManager.current != null)
            {
                m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable));
                VREnable = m_VREnableSelector.GetValue();
            }

            CalculateRadial(VREnable);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_VREnableSelector?.Dispose();
        }

        public override void SetLayoutHorizontal() { }
        public override void SetLayoutVertical() { }

        public override void CalculateLayoutInputVertical()
        {
            bool VREnable = m_VREnableSelector != null ? m_VREnableSelector.GetValue() : false;
            CalculateRadial(VREnable);
        }

        public override void CalculateLayoutInputHorizontal()
        {
            bool VREnable = m_VREnableSelector != null ? m_VREnableSelector.GetValue() : false;
            CalculateRadial(VREnable);
        }

        void CalculateRadial(bool VREnable)
        {
            var childCount = ChildCountActive(transform);
            m_Tracker.Clear();
            if (childCount == 0)
                return;
            float fOffsetAngle = ((MaxAngle - MinAngle)) / (childCount - 1);

            float fAngle = VREnable ? m_VRDefaultStartAngle : StartAngle;
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = (RectTransform)transform.GetChild(i);
                if (child != null && child.gameObject.activeSelf)
                {
                    //Adding the elements to the tracker stops the user from modifying their positions via the editor.
                    m_Tracker.Add(this, child,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.Pivot);
                    Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                    child.localPosition = vPos * fDistance;

                    //Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
                    fAngle += fOffsetAngle;
                }
            }
        }

        int ChildCountActive(Transform t)
        {
            int k = 0;
            foreach (Transform c in t)
            {
                if (c.gameObject.activeSelf)
                    k++;
            }

            return k;
        }
    }
}
