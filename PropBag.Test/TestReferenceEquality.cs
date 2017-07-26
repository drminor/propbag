using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

using DRM.ReferenceEquality;

//using PropBagLib;
using DRM.PropBag;

namespace PropBagLib.Tests
{

    [TestFixtureAttribute]
    public class TestReferenceEquality
    {

        private RefEqualityModel mod1;


        [OneTimeSetUp]
        public void Create()
        {
            // Create
            mod1 = new RefEqualityModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);

            mod1.SubscribeToPropChanged<string>(DoWhenUpdatedExt, "PropString");
        }

        [Test]
        public void ShouldSetAndGetStringLiteral()
        {

            // Set it using string literal.
            string before = "Test";
            mod1.PropString = before;

            // Reset to false, so that we can test that PropBag treat the new value as different.
            mod1.ItGotUpdated = false;

            // Create new string, to avoid interning.
            string after = before;  UnitTestHelpers.GetNewString(before);
            mod1.PropString = after;

            string testAfter = mod1.PropString;

            // Using string comparison
            Assert.That(testAfter, Is.EqualTo("Test"));

            // Now use reference comparison
            object a = (object) before;
            object b = (object) after;
            object c = (object) testAfter;

            // .Net is going to compare strings if possible.
            Assert.That(a, Is.EqualTo(b));

            // Force using reference comparison
            // The string literal and the string created are stored at different locations.
            Assert.That(ReferenceEqualityComparer.Default.Equals(a, "Test"), Is.EqualTo(true));

            // The reference that was used originaly is not the same as the reference of the current value.
            Assert.That(ReferenceEqualityComparer.Default.Equals(a, c), Is.EqualTo(true));

            // The current value is the same reference as the value used to set it last.
            Assert.That(ReferenceEqualityComparer.Default.Equals("Test", c), Is.EqualTo(true));

            // PropBag is using reference comparison and did discover a change, and did call our callback method.
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(false));
        }

        [Test]
        public void ShouldSetAndGetStringInterned()
        {
            // TODO: Run test to demonstrate that string interning produced strings at the same reference location
            // and that this implementation does treat them as equal.

            // Set it using string literal.
            string before = "Test";
            string after = "Test";

            mod1.PropString = before;

            // Reset to false, so that we can test that PropBag treat the new value as different.
            mod1.ItGotUpdated = false;
            mod1.PropString = after;

            string testAfter = mod1.PropString;

            // Now use reference comparison
            string a = before;
            string b = after;
            string c = testAfter;

            // .Net is going to compare strings if possible.
            Assert.That(a, Is.EqualTo(b));
            Assert.That(b, Is.EqualTo(c));

            // Force using reference comparison
            // The string literal and the string created are stored at different locations.
            Assert.That(ReferenceEqualityComparer.Default.Equals(a, b), Is.EqualTo(true));

            // The reference that was used originaly is not the same as the reference of the current value.
            Assert.That(ReferenceEqualityComparer.Default.Equals(a, c), Is.EqualTo(true));

            // The current value is the same reference as the value used to set it last.
            Assert.That(ReferenceEqualityComparer.Default.Equals(b, c), Is.EqualTo(true));

            // PropBag is using reference comparison and did discover a change, and did call our callback method.
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(false));
        }

        [Test]
        public void ShouldSetAndGetStringBuilt()
        {
            string before = mod1.PropString;

            string after = UnitTestHelpers.GetNewString("Test");

            mod1.ItGotUpdated = false;
            mod1.PropString = after;

            string testAfter = mod1.PropString;


            //.Net is going to use string comparison
            Assert.That((object)mod1.PropString, Is.EqualTo((object)before));

            // The reference that was used originaly is not the same as the reference of the current value.
            Assert.That(ReferenceEqualityComparer.Default.Equals(before, after), Is.EqualTo(false));

            // The current value is the same reference as the value used to set it last.
            Assert.That(ReferenceEqualityComparer.Default.Equals(after, testAfter), Is.EqualTo(true), "After and testAfter refer to different addresses.");

            // PropBag is using reference comparison and did discover a change, and did call our callback method.
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(true));
        }

        [OneTimeTearDown]
        public void destroy()
        {
            mod1 = null;
        }

        private void DoWhenUpdatedExt(string oldVal, string newVal)
        {
            //ItGotUpdated = true;
        }

    }

}
