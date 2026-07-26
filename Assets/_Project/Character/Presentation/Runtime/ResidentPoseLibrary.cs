using HumanGlassWatcher.Character.Model;
using UnityEngine;

namespace HumanGlassWatcher.Character.Presentation
{
    public enum ResidentReaction
    {
        None,
        Recoil,
        Inspect,
        Celebrate,
        Disgust,
        Sleep,
        Comfort,
        EscapeStrain
    }

    public struct ResidentPose
    {
        public Vector3 BodyEuler;
        public Vector3 BodyOffset;
        public Vector3 HeadEuler;
        public Vector3 LeftShoulderEuler;
        public Vector3 RightShoulderEuler;
        public Vector3 LeftElbowEuler;
        public Vector3 RightElbowEuler;
        public Vector3 LeftHipEuler;
        public Vector3 RightHipEuler;
        public float EyeOpen;
        public float MouthOpen;
        public float MouthCurve;
        public float BrowTilt;
        public float BrowLift;
        public float IdleAmount;

        public static ResidentPose Neutral()
        {
            return new ResidentPose
            {
                EyeOpen = 1f,
                IdleAmount = 1f
            };
        }

        public static ResidentPose Lerp(ResidentPose from, ResidentPose to, float amount)
        {
            var blend = Mathf.Clamp01(amount);
            return new ResidentPose
            {
                BodyEuler = Vector3.Lerp(from.BodyEuler, to.BodyEuler, blend),
                BodyOffset = Vector3.Lerp(from.BodyOffset, to.BodyOffset, blend),
                HeadEuler = Vector3.Lerp(from.HeadEuler, to.HeadEuler, blend),
                LeftShoulderEuler = Vector3.Lerp(from.LeftShoulderEuler, to.LeftShoulderEuler, blend),
                RightShoulderEuler = Vector3.Lerp(from.RightShoulderEuler, to.RightShoulderEuler, blend),
                LeftElbowEuler = Vector3.Lerp(from.LeftElbowEuler, to.LeftElbowEuler, blend),
                RightElbowEuler = Vector3.Lerp(from.RightElbowEuler, to.RightElbowEuler, blend),
                LeftHipEuler = Vector3.Lerp(from.LeftHipEuler, to.LeftHipEuler, blend),
                RightHipEuler = Vector3.Lerp(from.RightHipEuler, to.RightHipEuler, blend),
                EyeOpen = Mathf.Lerp(from.EyeOpen, to.EyeOpen, blend),
                MouthOpen = Mathf.Lerp(from.MouthOpen, to.MouthOpen, blend),
                MouthCurve = Mathf.Lerp(from.MouthCurve, to.MouthCurve, blend),
                BrowTilt = Mathf.Lerp(from.BrowTilt, to.BrowTilt, blend),
                BrowLift = Mathf.Lerp(from.BrowLift, to.BrowLift, blend),
                IdleAmount = Mathf.Lerp(from.IdleAmount, to.IdleAmount, blend)
            };
        }
    }

    public static class ResidentPoseLibrary
    {
        public static ResidentPose ForEmotion(CharacterEmotion emotion)
        {
            var pose = ResidentPose.Neutral();
            switch (emotion)
            {
                case CharacterEmotion.Joy:
                    pose.BodyEuler = new Vector3(-3f, 0f, 0f);
                    pose.BodyOffset = new Vector3(0f, 0.035f, 0f);
                    pose.HeadEuler = new Vector3(-5f, 0f, 3f);
                    pose.LeftShoulderEuler = new Vector3(0f, 0f, -28f);
                    pose.RightShoulderEuler = new Vector3(0f, 0f, 28f);
                    pose.LeftElbowEuler = new Vector3(0f, 0f, -12f);
                    pose.RightElbowEuler = new Vector3(0f, 0f, 12f);
                    pose.EyeOpen = 0.88f;
                    pose.MouthCurve = 1f;
                    pose.BrowLift = 0.35f;
                    pose.IdleAmount = 1.2f;
                    break;
                case CharacterEmotion.Curiosity:
                    pose.BodyEuler = new Vector3(3f, -4f, -2f);
                    pose.HeadEuler = new Vector3(-3f, -13f, 12f);
                    pose.RightShoulderEuler = new Vector3(-18f, 0f, 23f);
                    pose.RightElbowEuler = new Vector3(-12f, 0f, 58f);
                    pose.EyeOpen = 1.12f;
                    pose.MouthOpen = 0.14f;
                    pose.MouthCurve = 0.25f;
                    pose.BrowLift = 0.65f;
                    break;
                case CharacterEmotion.Sadness:
                    pose.BodyEuler = new Vector3(8f, 0f, 0f);
                    pose.BodyOffset = new Vector3(0f, -0.045f, 0f);
                    pose.HeadEuler = new Vector3(16f, 0f, -3f);
                    pose.LeftShoulderEuler = new Vector3(0f, 0f, 10f);
                    pose.RightShoulderEuler = new Vector3(0f, 0f, -10f);
                    pose.EyeOpen = 0.68f;
                    pose.MouthCurve = -0.9f;
                    pose.BrowTilt = -0.45f;
                    pose.BrowLift = -0.2f;
                    pose.IdleAmount = 0.55f;
                    break;
                case CharacterEmotion.Fear:
                    pose.BodyEuler = new Vector3(-9f, 0f, 0f);
                    pose.BodyOffset = new Vector3(0f, 0f, 0.035f);
                    pose.HeadEuler = new Vector3(-4f, 0f, 0f);
                    pose.LeftShoulderEuler = new Vector3(-18f, 0f, -62f);
                    pose.RightShoulderEuler = new Vector3(-18f, 0f, 62f);
                    pose.LeftElbowEuler = new Vector3(0f, 0f, 70f);
                    pose.RightElbowEuler = new Vector3(0f, 0f, -70f);
                    pose.LeftHipEuler = new Vector3(-7f, 0f, 0f);
                    pose.RightHipEuler = new Vector3(-7f, 0f, 0f);
                    pose.EyeOpen = 1.26f;
                    pose.MouthOpen = 0.72f;
                    pose.MouthCurve = -0.2f;
                    pose.BrowTilt = -0.8f;
                    pose.BrowLift = 0.8f;
                    pose.IdleAmount = 1.5f;
                    break;
                case CharacterEmotion.Anger:
                    pose.BodyEuler = new Vector3(7f, 0f, 0f);
                    pose.HeadEuler = new Vector3(4f, 0f, 0f);
                    pose.LeftShoulderEuler = new Vector3(0f, 0f, -18f);
                    pose.RightShoulderEuler = new Vector3(0f, 0f, 18f);
                    pose.LeftElbowEuler = new Vector3(0f, 0f, -58f);
                    pose.RightElbowEuler = new Vector3(0f, 0f, 58f);
                    pose.EyeOpen = 0.76f;
                    pose.MouthCurve = -0.55f;
                    pose.BrowTilt = 1f;
                    pose.BrowLift = -0.25f;
                    pose.IdleAmount = 0.75f;
                    break;
                case CharacterEmotion.Disgust:
                    pose.BodyEuler = new Vector3(-5f, 13f, 3f);
                    pose.BodyOffset = new Vector3(0.03f, 0f, 0.02f);
                    pose.HeadEuler = new Vector3(-3f, -18f, 8f);
                    pose.RightShoulderEuler = new Vector3(-25f, 0f, 52f);
                    pose.RightElbowEuler = new Vector3(-18f, 0f, -82f);
                    pose.EyeOpen = 0.62f;
                    pose.MouthCurve = -0.7f;
                    pose.BrowTilt = 0.65f;
                    pose.BrowLift = 0.15f;
                    pose.IdleAmount = 0.65f;
                    break;
                case CharacterEmotion.Surprise:
                    pose.BodyEuler = new Vector3(-5f, 0f, 0f);
                    pose.HeadEuler = new Vector3(-6f, 0f, 0f);
                    pose.LeftShoulderEuler = new Vector3(0f, 0f, -48f);
                    pose.RightShoulderEuler = new Vector3(0f, 0f, 48f);
                    pose.EyeOpen = 1.32f;
                    pose.MouthOpen = 1f;
                    pose.BrowLift = 1f;
                    pose.IdleAmount = 1.3f;
                    break;
                case CharacterEmotion.Contempt:
                    pose.BodyEuler = new Vector3(0f, -7f, 0f);
                    pose.HeadEuler = new Vector3(-3f, 12f, -8f);
                    pose.LeftShoulderEuler = new Vector3(0f, 0f, -8f);
                    pose.RightShoulderEuler = new Vector3(0f, 0f, -2f);
                    pose.EyeOpen = 0.67f;
                    pose.MouthCurve = 0.25f;
                    pose.BrowTilt = 0.25f;
                    pose.BrowLift = -0.3f;
                    pose.IdleAmount = 0.7f;
                    break;
                case CharacterEmotion.Relief:
                    pose.BodyEuler = new Vector3(2f, 0f, 0f);
                    pose.BodyOffset = new Vector3(0f, -0.02f, 0f);
                    pose.HeadEuler = new Vector3(5f, 0f, 3f);
                    pose.LeftShoulderEuler = new Vector3(0f, 0f, 8f);
                    pose.RightShoulderEuler = new Vector3(0f, 0f, -8f);
                    pose.EyeOpen = 0.55f;
                    pose.MouthCurve = 0.55f;
                    pose.BrowLift = 0.15f;
                    pose.IdleAmount = 0.8f;
                    break;
            }

            return pose;
        }

        public static ResidentPose ForReaction(ResidentReaction reaction)
        {
            switch (reaction)
            {
                case ResidentReaction.Recoil:
                    var recoil = ForEmotion(CharacterEmotion.Fear);
                    recoil.BodyEuler = new Vector3(-18f, 0f, 0f);
                    recoil.BodyOffset = new Vector3(0f, 0.06f, 0.12f);
                    return recoil;
                case ResidentReaction.Inspect:
                    return ForEmotion(CharacterEmotion.Curiosity);
                case ResidentReaction.Celebrate:
                    var celebrate = ForEmotion(CharacterEmotion.Joy);
                    celebrate.LeftShoulderEuler = new Vector3(0f, 0f, -138f);
                    celebrate.RightShoulderEuler = new Vector3(0f, 0f, 138f);
                    celebrate.LeftElbowEuler = new Vector3(0f, 0f, -18f);
                    celebrate.RightElbowEuler = new Vector3(0f, 0f, 18f);
                    celebrate.BodyOffset = new Vector3(0f, 0.10f, 0f);
                    return celebrate;
                case ResidentReaction.Disgust:
                    return ForEmotion(CharacterEmotion.Disgust);
                case ResidentReaction.Sleep:
                    var sleep = ForEmotion(CharacterEmotion.Relief);
                    sleep.BodyEuler = new Vector3(4f, 0f, 7f);
                    sleep.HeadEuler = new Vector3(12f, 0f, -17f);
                    sleep.EyeOpen = 0.06f;
                    sleep.MouthOpen = 0.1f;
                    sleep.IdleAmount = 0.22f;
                    return sleep;
                case ResidentReaction.Comfort:
                    return ForEmotion(CharacterEmotion.Relief);
                case ResidentReaction.EscapeStrain:
                    var escape = ForEmotion(CharacterEmotion.Anger);
                    escape.LeftShoulderEuler = new Vector3(-8f, 0f, -148f);
                    escape.RightShoulderEuler = new Vector3(-8f, 0f, 148f);
                    escape.LeftElbowEuler = new Vector3(0f, 0f, -12f);
                    escape.RightElbowEuler = new Vector3(0f, 0f, 12f);
                    escape.BodyEuler = new Vector3(14f, 0f, 0f);
                    escape.MouthOpen = 0.45f;
                    return escape;
                default:
                    return ResidentPose.Neutral();
            }
        }
    }
}
