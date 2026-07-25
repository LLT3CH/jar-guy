using System.Threading;
using System.Threading.Tasks;
using HumanGlassWatcher.Core.Items;

namespace HumanGlassWatcher.Core.Services
{
    public enum ItemResolutionStatus
    {
        Resolved,
        UnknownFallback,
        Empty,
        Duplicate,
        Unsafe,
        Unsupported,
        OfflineFallback
    }

    public readonly struct ItemResolution
    {
        public ItemResolution(ItemResolutionStatus status, ItemDefinition definition, string feedback)
        {
            Status = status;
            Definition = definition;
            Feedback = feedback;
        }

        public ItemResolutionStatus Status { get; }
        public ItemDefinition Definition { get; }
        public string Feedback { get; }
        public bool CanSpawn => Definition != null &&
                                (Status == ItemResolutionStatus.Resolved ||
                                 Status == ItemResolutionStatus.UnknownFallback ||
                                 Status == ItemResolutionStatus.OfflineFallback);
    }

    public interface IItemResolver
    {
        Task<ItemResolution> ResolveAsync(string prompt, CancellationToken cancellationToken);
    }

    public readonly struct DialogueRequest
    {
        public DialogueRequest(string stateDigest, string[] legalIntentIds)
        {
            StateDigest = stateDigest;
            LegalIntentIds = legalIntentIds;
        }

        public string StateDigest { get; }
        public string[] LegalIntentIds { get; }
    }

    public readonly struct DialogueResponse
    {
        public DialogueResponse(string spokenLine, string emotion, string intentId)
        {
            SpokenLine = spokenLine;
            Emotion = emotion;
            IntentId = intentId;
        }

        public string SpokenLine { get; }
        public string Emotion { get; }
        public string IntentId { get; }
    }

    public interface IDialogueService
    {
        Task<DialogueResponse> GenerateAsync(DialogueRequest request, CancellationToken cancellationToken);
    }
}
