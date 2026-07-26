using HumanGlassWatcher.Character.Model;
using NUnit.Framework;
using UnityEngine;

namespace HumanGlassWatcher.Character.Presentation.Tests
{
    public sealed class ResidentPresentationTests
    {
        [Test]
        public void FactoryBuildsRecognizableArticulatedAdultWithinMobileBudget()
        {
            var anchor = new GameObject("Resident Anchor");
            anchor.transform.localScale = new Vector3(0.72f, 1f, 0.72f);

            try
            {
                var rig = ResidentPresentationFactory.Build(anchor.transform);

                Assert.That(rig, Is.Not.Null);
                Assert.That(rig.HeadPivot, Is.Not.Null);
                Assert.That(rig.TorsoVisual, Is.Not.Null);
                Assert.That(rig.LeftShoulder, Is.Not.Null);
                Assert.That(rig.RightShoulder, Is.Not.Null);
                Assert.That(rig.LeftElbow, Is.Not.Null);
                Assert.That(rig.RightElbow, Is.Not.Null);
                Assert.That(rig.LeftHip, Is.Not.Null);
                Assert.That(rig.RightHip, Is.Not.Null);
                Assert.That(rig.LeftKnee, Is.Not.Null);
                Assert.That(rig.RightKnee, Is.Not.Null);
                Assert.That(rig.LeftHand, Is.Not.Null);
                Assert.That(rig.RightHand, Is.Not.Null);
                Assert.That(rig.LeftEye, Is.Not.Null);
                Assert.That(rig.RightEye, Is.Not.Null);
                Assert.That(rig.MouthCenter, Is.Not.Null);
                Assert.That(rig.HeadPivot.localPosition.y, Is.GreaterThan(rig.TorsoVisual.localPosition.y));
                Assert.That(rig.LeftHip.localPosition.y, Is.LessThan(rig.TorsoVisual.localPosition.y));
                Assert.That(rig.RendererCount, Is.InRange(26, 34));
                Assert.That(rig.MaterialCount, Is.LessThanOrEqualTo(9));
                Assert.That(rig.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(rig.transform.lossyScale.x, Is.EqualTo(1f).Within(0.001f));
                Assert.That(rig.transform.lossyScale.y, Is.EqualTo(1f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(anchor);
            }
        }

        [Test]
        public void EmotionPosesProduceReadableFaceAndBodyDifferences()
        {
            var anchor = new GameObject("Resident Anchor");

            try
            {
                var rig = ResidentPresentationFactory.Build(anchor.transform);
                var controller = rig.gameObject.AddComponent<ResidentPresentationController>();
                controller.Initialize(rig);

                controller.SetEmotion(CharacterEmotion.Joy, 1f);
                controller.SnapToPose(1f);
                var joyfulMouth = rig.MouthLeft.localPosition.y;
                var joyfulShoulder = rig.LeftShoulder.localEulerAngles;

                controller.SetEmotion(CharacterEmotion.Sadness, 1f);
                controller.SnapToPose(1f);
                var sadMouth = rig.MouthLeft.localPosition.y;
                var sadShoulder = rig.LeftShoulder.localEulerAngles;

                Assert.That(joyfulMouth, Is.GreaterThan(sadMouth + 0.07f));
                Assert.That(
                    Quaternion.Angle(Quaternion.Euler(joyfulShoulder), Quaternion.Euler(sadShoulder)),
                    Is.GreaterThan(15f));

                controller.SetReaction(ResidentReaction.Celebrate, 1f, 10f);
                controller.SnapToPose(1f);
                Assert.That(
                    Quaternion.Angle(Quaternion.identity, rig.LeftShoulder.localRotation),
                    Is.GreaterThan(100f));
            }
            finally
            {
                Object.DestroyImmediate(anchor);
            }
        }

        [Test]
        public void ExistingResidentMoodDrivesPresentationEmotion()
        {
            var anchor = new GameObject("Resident Anchor");

            try
            {
                var state = ResidentState.Create(42);
                state.Mood.Apply(
                    new AppraisalResult(-1f, 1f, -0.4f, CharacterEmotion.Disgust),
                    1f);
                var rig = ResidentPresentationFactory.Build(anchor.transform);
                var controller = rig.gameObject.AddComponent<ResidentPresentationController>();
                controller.Initialize(rig, state);
                controller.SnapToPose(0.8f);

                Assert.That(controller.BoundState, Is.SameAs(state));
                Assert.That(controller.CurrentEmotion, Is.EqualTo(CharacterEmotion.Disgust));
                Assert.That(
                    Quaternion.Angle(Quaternion.identity, rig.HeadPivot.localRotation),
                    Is.GreaterThan(10f));
                Assert.That(rig.LeftEye.localScale.y, Is.LessThan(0.8f));
            }
            finally
            {
                Object.DestroyImmediate(anchor);
            }
        }

        [Test]
        public void InstallerHidesGrayboxPreservesColliderAndIsIdempotent()
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            target.name = ResidentPresentationInstaller.GameplayTargetName;
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Facing Marker";
            marker.transform.SetParent(target.transform, false);

            try
            {
                var collider = target.GetComponent<Collider>();
                var first = ResidentPresentationInstaller.Install(target);
                var second = ResidentPresentationInstaller.Install(target);

                Assert.That(first, Is.SameAs(second));
                Assert.That(target.GetComponent<Renderer>().enabled, Is.False);
                Assert.That(marker.GetComponent<Renderer>().enabled, Is.False);
                Assert.That(target.GetComponent<Collider>(), Is.SameAs(collider));
                Assert.That(
                    target.transform.Find(ResidentPresentationFactory.PresentationRootName),
                    Is.Not.Null);
                Assert.That(
                    first.transform.localPosition.y,
                    Is.EqualTo(ResidentPresentationInstaller.StandingLocalYOffset).Within(0.001f));
                Assert.That(
                    target.GetComponentsInChildren<ResidentPresentationController>(true).Length,
                    Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
