using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Reflect.Markers.Placement;
using Unity.Reflect.Markers.Storage;
using UnityEngine;
using UnityEngine.TestTools;

namespace MarkersRuntimeTests
{
    public class MarkerTransformTests
    {
        [UnityTest]
        public IEnumerator PivotTransform_MoveWithPivot()
        {
            // Object A is a child of B
            // Object A is re-parented to C, with the same world position.
            // Process using ChangeRelativePosition should have equal effect as Transform.SetParent(newParent, worldPositionStats: true);

            Vector3 objectA_Positon = new Vector3(2324.32f, 928.23f, 284.134f);
            Vector3 objectB_Positon = new Vector3(243.2f, 332f, 5f);
            Vector3 objectC_Positon = new Vector3(-420.9f, -6f, 82f);


            // Setup test objects
            GameObject objectA_gameObject = new GameObject("objectA");
            objectA_gameObject.transform.position = objectA_Positon;

            yield return new WaitForSeconds(0.1f);
            PivotTransform.MoveWithPivot(objectA_gameObject.transform, objectB_Positon, objectC_Positon);

            yield return new WaitForSeconds(0.1f);
            Vector3 diff_objA = objectA_gameObject.transform.position - objectA_Positon;
            float diff_objA_magnitude = diff_objA.magnitude;

            Debug.Log($"ObjectA Difference: {diff_objA} magnitude: {diff_objA_magnitude}");

            Vector3 diff_OrigMovement = objectB_Positon - objectC_Positon;
            float diff_OrigMovement_magnitude = diff_OrigMovement.magnitude;

            Debug.Log($"B, C Difference: {diff_OrigMovement} magnitude: {diff_OrigMovement_magnitude}");

            Assert.True(Mathf.Abs(diff_objA_magnitude - diff_OrigMovement_magnitude) < float.Epsilon);
        }

        [UnityTest]
        public IEnumerator PivotTransform_RotateWithPivot()
        {

            yield return new WaitForSeconds(0.1f);
            //PivotTransform.RotateAroundPivot(targetObject, pivotPose, targetRotation, keepTargetUpright);
        }

        [UnityTest]
        public IEnumerator Marker_GetWorldPose()
        {
            Vector3 testPostion = Vector3.one;
            Vector3 testRotationEuler = new Vector3(0, 0, 90f);
            GameObject objectA = new GameObject();
            objectA.transform.position = new Vector3(43f, 7f, 2f);
            objectA.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90f));

            GameObject objectB = new GameObject();
            objectB.transform.SetParent(objectA.transform);
            objectB.transform.localPosition = testPostion;
            objectB.transform.localRotation = Quaternion.Euler(testRotationEuler);

            yield return new WaitForSeconds(0.1f);
            Pose result = Marker.GetWorldPose(objectA.transform, testPostion, Quaternion.Euler(testRotationEuler));

            Debug.Log($"Result pose pos: {result.position}  rot: {result.rotation}");
            Debug.Log($"VirtualObject transform: {objectB.transform.position} {objectB.transform.rotation}");
            Assert.True(objectB.transform.position == result.position);
            Assert.True(objectB.transform.rotation == result.rotation);
        }

        [UnityTest]
        public IEnumerator Marker_AlignObject()
        {
            Marker marker = new Marker()
            {
                RelativePosition = Vector3.one,
                RelativeRotationEuler = Vector3.zero,
                ObjectScale = Vector3.one
            };

            Vector3 modelPosition = Vector3.zero;
            Quaternion modelRotation = Quaternion.identity;
            Vector3 modelScale = Vector3.one;


            GameObject m_VirtualMarkerParent = new GameObject("VirtualMarkerParent");
            m_VirtualMarkerParent.transform.position = modelPosition;
            m_VirtualMarkerParent.transform.rotation = modelRotation;
            m_VirtualMarkerParent.transform.localScale = modelScale;

            GameObject m_VirtualMarker = new GameObject("Virtual Marker");
            m_VirtualMarker.transform.SetParent(m_VirtualMarkerParent.transform, false);
            m_VirtualMarker.transform.localPosition = marker.RelativePosition;
            m_VirtualMarker.transform.localRotation = Quaternion.Euler(marker.RelativeRotationEuler);
            m_VirtualMarkerParent.transform.localScale = marker.ObjectScale;

            GameObject testModel = new GameObject();
            testModel.transform.position = new Vector3(234f, 2f, 8f);
            testModel.transform.rotation = Quaternion.Euler(new Vector3(0f, 23f, 0f));
            testModel.transform.localScale = modelScale;

            yield return new WaitForSeconds(0.1f);

            // Test with a marker that which is attached to a wall and slightly tilted to the side but otherwise the Y should be the same.
            Pose testPose = new Pose(modelPosition + marker.RelativePosition, Quaternion.Euler(new Vector3(90f, 0f, 0f)));
            marker.AlignObject(testModel.transform, testPose);


            yield return new WaitForSeconds(0.1f);
            Vector3 positionDiffrence = testModel.transform.position - m_VirtualMarkerParent.transform.position;
            float positionDiffMagnitude = positionDiffrence.sqrMagnitude;
            Debug.Log($"Positon Test: {testModel.transform.position} Should match {m_VirtualMarkerParent.transform.position}");
            Debug.Log($"Position Difference of {positionDiffrence} magnitude {positionDiffMagnitude} epsilon {float.Epsilon}");

            Vector3 rotationDifference = testModel.transform.rotation.eulerAngles - m_VirtualMarkerParent.transform.rotation.eulerAngles;
            float rotationDiffMagnitude = rotationDifference.sqrMagnitude;
            Debug.Log($"Rotation Test: {testModel.transform.rotation.eulerAngles} Should match {m_VirtualMarkerParent.transform.rotation.eulerAngles}");
            Debug.Log($"Rotation Difference of {rotationDifference} magnitude {rotationDiffMagnitude}");

            Assert.True(positionDiffMagnitude < 0.01f);
            Assert.True(rotationDiffMagnitude < 0.01f);

        }
    }
}
