using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect.Utils;

namespace ReflectViewerEditorTests
{
    public class Utils
    {
        // A Test behaves as an ordinary method

        internal struct ExpectedInitials
        {
            public string fullName;
            public string expected;
        }

        [Test]
        public void ValidationListInitialsPasses()
        {
            var ValidationList = new List<ExpectedInitials>()
            {
                new ExpectedInitials() {fullName = "Mason Zhwiti", expected = "MZ"},
                new ExpectedInitials() {fullName = "mason lowercase zhwiti", expected = "MZ"},
                new ExpectedInitials() {fullName = " Mason G Zhwiti", expected = "MZ"},
                new ExpectedInitials() {fullName = "Mason G. Zhwiti", expected = "MZ"},
                new ExpectedInitials() {fullName = "John Queue Public", expected = "JP"},
                new ExpectedInitials() {fullName = "John Q. Public, Jr.", expected = "JP"},
                new ExpectedInitials() {fullName = "John Q Public Jr.", expected = "JP"},
                new ExpectedInitials() {fullName = "Thurston Howell III", expected = "TH"},
                new ExpectedInitials() {fullName = "Thurston Howell, III", expected = "TH"},
                new ExpectedInitials() {fullName = "Malcolm X", expected = "MX"},
                new ExpectedInitials() {fullName = "A Ron", expected = "AR"},
                new ExpectedInitials() {fullName = "A A Ron", expected = "AR"},
                new ExpectedInitials() {fullName = "Madonna", expected = "M"},
                new ExpectedInitials() {fullName = "Chris O'Donnell", expected = "CO"},
                new ExpectedInitials() {fullName = "Malcolm McDowell", expected = "MM"},
                new ExpectedInitials() {fullName = "Robert \"Rocky\" Balboa, Sr.", expected = "RB"},
                new ExpectedInitials() {fullName = "1Bobby 2Tables", expected = "BT"},
                new ExpectedInitials() {fullName = "Éric Ígor", expected = "ÉÍ"},
                new ExpectedInitials() {fullName = "행운의 복숭아", expected = "행복"},
                new ExpectedInitials() {fullName = "David Ménard", expected = "DM"},
                new ExpectedInitials() {fullName = null, expected = ""}
            };

            foreach (var entry in ValidationList)
            {
                var initials = UIUtils.CreateInitialsFor(entry.fullName);
                Assert.That(initials == entry.expected);
            }
        }

        [Test]
        public void UIHelper_TestTimeIntervalStrings()
        {
            var dateTime = DateTime.Now;
            Assert.AreEqual(UIUtils.TimeIntervalJustNow, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddSeconds(-50);
            Assert.AreEqual(UIUtils.TimeIntervalJustNow, UIUtils.GetTimeIntervalSinceNow(dateTime));

            dateTime = DateTime.Now.AddMinutes(-1);
            Assert.AreEqual(UIUtils.TimeIntervalAMinute, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddMinutes(-10);
            Assert.AreEqual($"10 {UIUtils.TimeIntervalMinutes}", UIUtils.GetTimeIntervalSinceNow(dateTime));

            dateTime = DateTime.Now.AddHours(-1);
            Assert.AreEqual(UIUtils.TimeIntervalAnHour, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddHours(-10);
            Assert.AreEqual($"10 {UIUtils.TimeIntervalHours}", UIUtils.GetTimeIntervalSinceNow(dateTime));

            dateTime = DateTime.Now.AddDays(-1);
            Assert.AreEqual(UIUtils.TimeIntervalYesteday, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddDays(-3);
            Assert.AreEqual($"3 {UIUtils.TimeIntervalDays}", UIUtils.GetTimeIntervalSinceNow(dateTime));

            dateTime = DateTime.Now.AddDays(-7);
            Assert.AreEqual(UIUtils.TimeIntervalAWeek, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddDays(-22);
            Assert.AreEqual($"3 {UIUtils.TimeIntervalWeeks}", UIUtils.GetTimeIntervalSinceNow(dateTime));

            dateTime = DateTime.Now.AddDays(-28);
            Assert.AreEqual(UIUtils.TimeIntervalAMonth, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddDays(-29);
            Assert.AreEqual(UIUtils.TimeIntervalAMonth, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddDays(-30);
            Assert.AreEqual(UIUtils.TimeIntervalAMonth, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddDays(-31);
            Assert.AreEqual(UIUtils.TimeIntervalAMonth, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddMonths(-10);
            Assert.AreEqual($"10 {UIUtils.TimeIntervalMonths}", UIUtils.GetTimeIntervalSinceNow(dateTime));

            dateTime = DateTime.Now.AddYears(-1);
            Assert.AreEqual(UIUtils.TimeIntervalAYear, UIUtils.GetTimeIntervalSinceNow(dateTime));
            dateTime = DateTime.Now.AddYears(-10);
            Assert.AreEqual($"10 {UIUtils.TimeIntervalYears}", UIUtils.GetTimeIntervalSinceNow(dateTime));
        }

        [TestCase(1920, 1080, ScreenSizeQualifier.XLarge)]
        [TestCase(1200, 1920, ScreenSizeQualifier.XLarge)]
        [TestCase(1600, 1200, ScreenSizeQualifier.Large)]
        [TestCase(1440, 900, ScreenSizeQualifier.Large)]
        [TestCase(900, 1440, ScreenSizeQualifier.Large)]
        [TestCase(1024, 600, ScreenSizeQualifier.Medium)]
        [TestCase(600, 1024, ScreenSizeQualifier.Medium)]
        [TestCase(800, 600, ScreenSizeQualifier.Small)]
        [TestCase(600, 800, ScreenSizeQualifier.Small)]
        [TestCase(640, 480, ScreenSizeQualifier.Small)]
        [TestCase(480, 640, ScreenSizeQualifier.Small)]
        [TestCase(480, 320, ScreenSizeQualifier.XSmall)]
        public void UIHelper_TestScreenSizeQualifiers(int width, int height, ScreenSizeQualifier expectedQualifier)
        {
            //when
            var processedQualifier = UIUtils.QualifyScreenSize(new Vector2(width, height));

            //Then
            Assert.AreEqual(expectedQualifier.ToString(),processedQualifier.ToString());
        }

        [TestCase(2388, 1668, 264, DisplayType.Tablet, 2.31f)]
        [TestCase(2732, 2048, 264, DisplayType.Tablet,2.75f)]
        [TestCase(1600, 1200, 96, DisplayType.Desktop,1)]
        [TestCase(800, 600, 96, DisplayType.Desktop,1)]
        [TestCase(800, 600, 96, DisplayType.Tablet,0.83f)]
        [TestCase(480, 320, 96, DisplayType.Desktop,0.64f)] //Scale on desktop
        public void UIHelper_TestScaleFactor(int width, int height, float dpi, Unity.Reflect.Viewer.UI.DisplayType displayType, float expectedScaleFactor)
        {
            //when
            var scaleFactor = UIUtils.GetScaleFactor(width, height, dpi, displayType);

            //Then
            Assert.IsTrue(Math.Abs(expectedScaleFactor - scaleFactor) < 0.1f, $"Expected Scale Factor was {expectedScaleFactor.ToString()}, but {scaleFactor.ToString()} was found" , null);
        }
    }
}
