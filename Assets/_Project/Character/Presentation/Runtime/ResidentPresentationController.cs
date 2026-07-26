using HumanGlassWatcher.Character.Model;
using UnityEngine;

namespace HumanGlassWatcher.Character.Presentation
{
    public sealed class ResidentPresentationController : MonoBehaviour
    {
        [SerializeField] private float poseSmoothing = 11f;
        [SerializeField] private float idleSpeed = 1.7f;

        private ResidentState boundState;
        private ResidentVisualRig rig;
        private CharacterEmotion targetEmotion = CharacterEmotion.Neutral;
        private float targetIntensity = 0.65f;
        private ResidentReaction reaction;
        private float reactionIntensity;
        private float reactionEndsAt;
        private bool baselineCaptured;
        private Vector3 bodyPosition;
        private Quaternion bodyRotation;
        private Quaternion headRotation;
        private Quaternion leftShoulderRotation;
        private Quaternion rightShoulderRotation;
        private Quaternion leftElbowRotation;
        private Quaternion rightElbowRotation;
        private Quaternion leftHipRotation;
        private Quaternion rightHipRotation;
        private Vector3 torsoScale;
        private Vector3 leftEyeScale;
        private Vector3 rightEyeScale;
        private Vector3 mouthCenterScale;
        private Vector3 mouthLeftPosition;
        private Vector3 mouthRightPosition;
        private Vector3 leftBrowPosition;
        private Vector3 rightBrowPosition;

        public ResidentVisualRig Rig => rig;
        public ResidentState BoundState => boundState;
        public CharacterEmotion CurrentEmotion => targetEmotion;
        public ResidentReaction CurrentReaction => reaction;

        public void Initialize(ResidentVisualRig visualRig, ResidentState state = null)
        {
            rig = visualRig != null ? visualRig : GetComponent<ResidentVisualRig>();
            boundState = state;
            CaptureBaseline();
            RefreshFromState();
            SnapToPose(0f);
        }

        public void Bind(ResidentState state)
        {
            boundState = state;
            RefreshFromState();
        }

        public void SetEmotion(CharacterEmotion emotion, float intensity)
        {
            targetEmotion = emotion;
            targetIntensity = Mathf.Clamp01(intensity);
        }

        public void SetReaction(
            ResidentReaction nextReaction,
            float intensity = 1f,
            float durationSeconds = 1.1f)
        {
            reaction = nextReaction;
            reactionIntensity = Mathf.Clamp01(intensity);
            reactionEndsAt = Time.time + Mathf.Max(0f, durationSeconds);
        }

        public void ClearReaction()
        {
            reaction = ResidentReaction.None;
            reactionIntensity = 0f;
        }

        public void RefreshFromState()
        {
            if (boundState == null)
            {
                return;
            }

            var mood = boundState.Mood;
            targetEmotion = ResolveVisibleEmotion(mood);
            targetIntensity = Mathf.Clamp01(
                0.38f + (mood.Arousal * 0.42f) + (Mathf.Abs(mood.Valence) * 0.28f));
        }

        public void SnapToPose(float time)
        {
            if (!EnsureRig())
            {
                return;
            }

            RefreshFromState();
            ApplyPose(EvaluatePose(time), time, 1f);
        }

        public void TickPresentation(float time, float deltaTime)
        {
            if (!EnsureRig())
            {
                return;
            }

            RefreshFromState();
            if (reaction != ResidentReaction.None && time >= reactionEndsAt)
            {
                ClearReaction();
            }

            var blend = 1f - Mathf.Exp(-poseSmoothing * Mathf.Max(0f, deltaTime));
            ApplyPose(EvaluatePose(time), time, blend);
        }

        private void Awake()
        {
            EnsureRig();
        }

        private void Update()
        {
            TickPresentation(Time.time, Time.deltaTime);
        }

        private ResidentPose EvaluatePose(float time)
        {
            var neutral = ResidentPose.Neutral();
            if (reaction != ResidentReaction.None)
            {
                return ResidentPose.Lerp(
                    neutral,
                    ResidentPoseLibrary.ForReaction(reaction),
                    reactionIntensity);
            }

            return ResidentPose.Lerp(
                neutral,
                ResidentPoseLibrary.ForEmotion(targetEmotion),
                targetIntensity);
        }

        private void ApplyPose(ResidentPose pose, float time, float blend)
        {
            var breath = Mathf.Sin(time * idleSpeed) * 0.014f * pose.IdleAmount;
            var sway = Mathf.Sin((time * idleSpeed * 0.47f) + 0.8f) * 1.15f * pose.IdleAmount;
            var blink = BlinkAmount(time) * pose.EyeOpen;

            rig.BodyRoot.localPosition = Vector3.Lerp(
                rig.BodyRoot.localPosition,
                bodyPosition + pose.BodyOffset + new Vector3(0f, breath * 0.45f, 0f),
                blend);
            rig.BodyRoot.localRotation = Quaternion.Slerp(
                rig.BodyRoot.localRotation,
                bodyRotation * Quaternion.Euler(pose.BodyEuler + new Vector3(0f, 0f, sway)),
                blend);
            rig.HeadPivot.localRotation = Quaternion.Slerp(
                rig.HeadPivot.localRotation,
                headRotation * Quaternion.Euler(pose.HeadEuler + new Vector3(0f, sway * 0.35f, 0f)),
                blend);
            SetRotation(rig.LeftShoulder, leftShoulderRotation, pose.LeftShoulderEuler, blend);
            SetRotation(rig.RightShoulder, rightShoulderRotation, pose.RightShoulderEuler, blend);
            SetRotation(rig.LeftElbow, leftElbowRotation, pose.LeftElbowEuler, blend);
            SetRotation(rig.RightElbow, rightElbowRotation, pose.RightElbowEuler, blend);
            SetRotation(rig.LeftHip, leftHipRotation, pose.LeftHipEuler, blend);
            SetRotation(rig.RightHip, rightHipRotation, pose.RightHipEuler, blend);

            rig.TorsoVisual.localScale = Vector3.Lerp(
                rig.TorsoVisual.localScale,
                new Vector3(
                    torsoScale.x * (1f - (breath * 0.25f)),
                    torsoScale.y * (1f + breath),
                    torsoScale.z * (1f + (breath * 0.5f))),
                blend);
            rig.LeftEye.localScale = Vector3.Lerp(
                rig.LeftEye.localScale,
                new Vector3(leftEyeScale.x, leftEyeScale.y * blink, leftEyeScale.z),
                blend);
            rig.RightEye.localScale = Vector3.Lerp(
                rig.RightEye.localScale,
                new Vector3(rightEyeScale.x, rightEyeScale.y * blink, rightEyeScale.z),
                blend);

            var mouthOpenScale = new Vector3(
                mouthCenterScale.x * (1f - (pose.MouthOpen * 0.22f)),
                mouthCenterScale.y * (1f + (pose.MouthOpen * 5.5f)),
                mouthCenterScale.z);
            rig.MouthCenter.localScale = Vector3.Lerp(
                rig.MouthCenter.localScale,
                mouthOpenScale,
                blend);
            rig.MouthLeft.localPosition = Vector3.Lerp(
                rig.MouthLeft.localPosition,
                mouthLeftPosition + new Vector3(0f, pose.MouthCurve * 0.055f, 0f),
                blend);
            rig.MouthRight.localPosition = Vector3.Lerp(
                rig.MouthRight.localPosition,
                mouthRightPosition + new Vector3(0f, pose.MouthCurve * 0.055f, 0f),
                blend);

            rig.LeftBrow.localPosition = Vector3.Lerp(
                rig.LeftBrow.localPosition,
                leftBrowPosition + new Vector3(0f, pose.BrowLift * 0.045f, 0f),
                blend);
            rig.RightBrow.localPosition = Vector3.Lerp(
                rig.RightBrow.localPosition,
                rightBrowPosition + new Vector3(0f, pose.BrowLift * 0.045f, 0f),
                blend);
            rig.LeftBrow.localRotation = Quaternion.Slerp(
                rig.LeftBrow.localRotation,
                Quaternion.Euler(0f, 0f, pose.BrowTilt * -15f),
                blend);
            rig.RightBrow.localRotation = Quaternion.Slerp(
                rig.RightBrow.localRotation,
                Quaternion.Euler(0f, 0f, pose.BrowTilt * 15f),
                blend);
        }

        private bool EnsureRig()
        {
            if (rig == null)
            {
                rig = GetComponent<ResidentVisualRig>();
            }

            if (rig == null)
            {
                return false;
            }

            if (!baselineCaptured)
            {
                CaptureBaseline();
            }

            return baselineCaptured;
        }

        private void CaptureBaseline()
        {
            if (rig == null || rig.BodyRoot == null)
            {
                return;
            }

            bodyPosition = rig.BodyRoot.localPosition;
            bodyRotation = rig.BodyRoot.localRotation;
            headRotation = rig.HeadPivot.localRotation;
            leftShoulderRotation = rig.LeftShoulder.localRotation;
            rightShoulderRotation = rig.RightShoulder.localRotation;
            leftElbowRotation = rig.LeftElbow.localRotation;
            rightElbowRotation = rig.RightElbow.localRotation;
            leftHipRotation = rig.LeftHip.localRotation;
            rightHipRotation = rig.RightHip.localRotation;
            torsoScale = rig.TorsoVisual.localScale;
            leftEyeScale = rig.LeftEye.localScale;
            rightEyeScale = rig.RightEye.localScale;
            mouthCenterScale = rig.MouthCenter.localScale;
            mouthLeftPosition = rig.MouthLeft.localPosition;
            mouthRightPosition = rig.MouthRight.localPosition;
            leftBrowPosition = rig.LeftBrow.localPosition;
            rightBrowPosition = rig.RightBrow.localPosition;
            baselineCaptured = true;
        }

        private static void SetRotation(
            Transform target,
            Quaternion baseline,
            Vector3 euler,
            float blend)
        {
            target.localRotation = Quaternion.Slerp(
                target.localRotation,
                baseline * Quaternion.Euler(euler),
                blend);
        }

        private static float BlinkAmount(float time)
        {
            var cycle = Mathf.Repeat(time, 4.15f);
            if (cycle > 0.14f)
            {
                return 1f;
            }

            return Mathf.Lerp(0.08f, 1f, Mathf.Abs(cycle - 0.07f) / 0.07f);
        }

        private static CharacterEmotion ResolveVisibleEmotion(MoodState mood)
        {
            if (mood.Emotion != CharacterEmotion.Neutral)
            {
                return mood.Emotion;
            }

            if (mood.Valence > 0.18f)
            {
                return CharacterEmotion.Joy;
            }

            return mood.Valence < -0.18f
                ? CharacterEmotion.Sadness
                : CharacterEmotion.Neutral;
        }
    }
}
