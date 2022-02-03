using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Placement;
using Unity.Reflect.Markers.Storage;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.Examples
{
    public class BasicTransformUI : MonoBehaviour
    {
        [SerializeField] private Button xPosButton = null;
        [SerializeField] private Button xNegButton = null;

        [SerializeField] private Button yPosButton = null;
        [SerializeField] private Button yNegButton = null;

        [SerializeField] private Button zPosButton = null;
        [SerializeField] private Button zNegButton = null;

        [SerializeField] private Toggle posRotToggle = null;


        [SerializeField] private bool rotationMode = false;
        [SerializeField] private float positionIncrement = 0.01f;
        [SerializeField] private float rotationIncrement = 15f;

        private IMarker marker;
        private Transform model;

        public void Open(IMarker marker, Transform model)
        {
            if (isActiveAndEnabled)
                Close();

            gameObject.SetActive(true);
            this.marker = marker;
            this.model = model;
            posRotToggle.onValueChanged.AddListener(HandlePosRotToggle);
            xPosButton.onClick.AddListener(HandleXPos);
            xNegButton.onClick.AddListener(HandleXNeg);
            yPosButton.onClick.AddListener(HandleYPos);
            yNegButton.onClick.AddListener(HandleYNeg);
            zPosButton.onClick.AddListener(HandleZPos);
            zNegButton.onClick.AddListener(HandleZNeg);

        }

        public void Close()
        {
            posRotToggle.onValueChanged.RemoveListener(HandlePosRotToggle);
            xPosButton.onClick.RemoveListener(HandleXPos);
            xNegButton.onClick.RemoveListener(HandleXNeg);
            yPosButton.onClick.RemoveListener(HandleYPos);
            yNegButton.onClick.RemoveListener(HandleYNeg);
            zPosButton.onClick.RemoveListener(HandleZPos);
            zNegButton.onClick.RemoveListener(HandleZNeg);
        }

        private void OnDisable()
        {
            Close();
        }

        private void HandlePosRotToggle(bool value)
        {
            rotationMode = value;
        }

        private void Nudge(Vector3 amount)
        {
            if (rotationMode)
            {
                var origin = new TransformData(model);
                Pose pivot = Marker.GetWorldPose(origin, marker.RelativePosition, Quaternion.Euler(marker.RelativeRotationEuler));

                var result = PivotTransform.RotateAroundPivot(
                    origin,
                    pivot,
                    pivot.rotation * Quaternion.Euler(amount)
                );
                model.position = result.position;
                model.rotation = result.rotation;
                model.localScale = result.scale;
            }
            else
            {
                model.position += amount;
            }
        }

        private void HandleInput(Vector3 axis)
        {
            Nudge(axis * Increment());
        }

        /// <summary>
        /// </summary>
        /// <param name="positive"></param>
        /// <returns>Current mode's increment value in positive or negative values.</returns>
        private float Increment()
        {
            return (rotationMode) ? rotationIncrement : positionIncrement;
        }

        private void HandleXPos()
        {
            HandleInput(Vector3.right);
        }

        private void HandleXNeg()
        {
            HandleInput(Vector3.left);
        }

        private void HandleYPos()
        {
            HandleInput(Vector3.up);
        }

        private void HandleYNeg()
        {
            HandleInput(Vector3.down);
        }

        private void HandleZPos()
        {
            HandleInput(Vector3.forward);
        }

        private void HandleZNeg()
        {
            HandleInput(Vector3.back);
        }
    }
}
