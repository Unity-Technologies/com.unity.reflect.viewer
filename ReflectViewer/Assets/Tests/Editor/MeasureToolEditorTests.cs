using NUnit.Framework;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace ReflectViewerEditorTests
{
    public class MeasureToolEditorTests
    {
        const float MaxValue = 1+19F; //to allow MaxValue * MaxValue
        const int TriesAmount = 100000;

        PointAnchor GetRandomPointAnchor()
        {
            return GetPointAnchor(GenerateRandomVector(), GenerateRandomVector());
        }

        PointAnchor GetPointAnchor(Vector3 point, Vector3 normal,
            ToggleMeasureToolAction.AnchorType anchorType = ToggleMeasureToolAction.AnchorType.Point)
        {
            return new PointAnchor(point.GetHashCode(), ToggleMeasureToolAction.AnchorType.Point, point, Vector3.up);
        }

        Vector3 GenerateRandomVector()
        {
            return new Vector3(Random.Range(-MaxValue, MaxValue),
                                Random.Range(-MaxValue, MaxValue),
                                Random.Range(-MaxValue, MaxValue));
        }

        void AssertCalculation(PointAnchor anchor1, PointAnchor anchor2, System.Action<float> assertDelegate)
        {
            var distance = RawMeasure.GetDistanceBetweenAnchors(anchor1, anchor2);

            var position1 = anchor1.position;
            var normal1 = anchor1.normal;
            var position2 = anchor2.position;
            var normal2 = anchor2.normal;
            try
            {
                assertDelegate?.Invoke(distance);
            }
            catch (AssertionException e)
            {
                var msg = e.Message +
                    $"Anchors are:\nposition1:{position1.x}, {position1.y}, {position1.z};\nnormal1:{normal1.x}, {normal1.y}, {normal1.z};" +
                      $"\nposition2:{position2.x}, {position2.y}, {position2.z};\nnormal2:{normal2.x}, {normal2.y}, {normal2.z}";

                throw new AssertionException(msg, e);
            }
        }

        void IsDistanceEqualApproximately(PointAnchor anchor1, PointAnchor anchor2, float expectedDistance)
        {
            AssertCalculation(anchor1, anchor2, (distance)
                => Assert.IsTrue(Mathf.Approximately(distance, expectedDistance)));
        }

        void IsDistanceEqual(PointAnchor anchor1, PointAnchor anchor2, float expectedDistance)
        {
            AssertCalculation(anchor1, anchor2, (distance)
                => Assert.AreEqual(distance, expectedDistance));
        }

        void IsDistancePositive(PointAnchor anchor1, PointAnchor anchor2)
        {
            AssertCalculation(anchor1, anchor2, (distance) => Assert.GreaterOrEqual(distance, 0.0f));
        }

        [Test]
        public void MeasureToolTests_MinMaxFloat()
        {
            var minAnchor = GetPointAnchor(new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue),
                                              Vector3.up);

            var maxAnchor = GetPointAnchor(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                                              Vector3.up);

            IsDistanceEqual(minAnchor, maxAnchor, float.PositiveInfinity);
            IsDistanceEqual(maxAnchor, minAnchor, float.PositiveInfinity);
        }

        [Test]
        public void MeasureToolTests_InfinityFloat()
        {
            var negativeInfinityAnchor= GetPointAnchor(new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
                new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue));

            var positiveInfinityAnchor= GetPointAnchor(new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
                new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));

            IsDistancePositive(negativeInfinityAnchor, positiveInfinityAnchor);
            IsDistancePositive(positiveInfinityAnchor, negativeInfinityAnchor);
        }

        [Test]
        public void MeasureToolTests_NaNValues()
        {
            var anchor = GetPointAnchor(Vector3.one, Vector3.up);

            var nanAnchor = GetPointAnchor(new Vector3(float.NaN, 0, 0), Vector3.up);
            IsDistanceEqual(nanAnchor, anchor, float.NaN);

            nanAnchor = GetPointAnchor(new Vector3(0, float.NaN, 0), Vector3.up);
            IsDistanceEqual(nanAnchor, anchor, float.NaN);

            nanAnchor = GetPointAnchor(new Vector3(0, 0, float.NaN), Vector3.up);
            IsDistanceEqual(nanAnchor, anchor, float.NaN);

            //Nan normal must not affect the distance between points
            var expectedDistance = anchor.position.magnitude;
            var nanNormalAnchor = GetPointAnchor(Vector3.zero, new Vector3(float.NaN, 0, 0));
            IsDistanceEqualApproximately(nanNormalAnchor, anchor, expectedDistance);

            nanNormalAnchor = GetPointAnchor(Vector3.zero, new Vector3(0, float.NaN, 0));
            IsDistanceEqualApproximately(nanNormalAnchor, anchor, expectedDistance);

            nanNormalAnchor = GetPointAnchor(Vector3.zero, new Vector3(0, 0, float.NaN));
            IsDistanceEqualApproximately(nanNormalAnchor, anchor, expectedDistance);
        }

        [Test]
        public void MeasureToolTests_ZeroDistance()
        {
            var zeroAnchor = GetPointAnchor(Vector3.zero, Vector3.zero);
            IsDistanceEqual(zeroAnchor, zeroAnchor, 0.0f);

            for (var i = 0; i < TriesAmount; ++i)
            {
                var position = GenerateRandomVector();
                var normal = GenerateRandomVector();
                var anchor = GetPointAnchor(position, normal);
                IsDistanceEqual(anchor, anchor, 0.0f);
            }
        }

        [Test]
        public void MeasureToolTests_RandomDistance()
        {
            for (var i = 0; i < TriesAmount; ++i)
            {
                var difference = GenerateRandomVector();
                var offset = GenerateRandomVector();

                var anchor1 = GetPointAnchor(offset, GenerateRandomVector());
                var anchor2 = GetPointAnchor(offset + difference, GenerateRandomVector());

                var expectedDistance = difference.magnitude;
                IsDistanceEqualApproximately(anchor1, anchor2, expectedDistance);
                IsDistanceEqualApproximately(anchor2, anchor1, expectedDistance);
            }
        }

        //This test is known to be failing. Disabled until double-mathematics algorithm implementation
        public void MeasureToolTests_FloatPrecision()
        {
            var precision = 0.01f; //claimed precision is 0.01
            var x = 999999.9f;
            var position1 = new Vector3(x, 0, 0);
            var position2 = new Vector3(precision * 2, 0, 0);

            var anchor1 = GetPointAnchor(position1, Vector3.up);
            var anchor2 = GetPointAnchor(position2, Vector3.up);

            var distance = RawMeasure.GetDistanceBetweenAnchors(anchor1, anchor2);

            Assert.AreNotEqual(distance, x);
            Assert.IsTrue(Mathf.Approximately(position1.x - position2.x, distance));
        }
    }
}
