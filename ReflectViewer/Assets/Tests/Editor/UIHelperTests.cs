using System.Collections.Generic;
using NUnit.Framework;
using Unity.Reflect.Viewer.UI;

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
                new ExpectedInitials() {fullName = "David Ménard", expected = "DM"}
            };

            foreach (var entry in ValidationList)
            {
                var initials = AccountUIController.GetInitials(entry.fullName);
                Assert.That(initials == entry.expected);
            }
        }
    }
}
