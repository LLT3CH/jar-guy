namespace HumanGlassWatcher.Character.Model
{
    public enum ActionVerb
    {
        Observe,
        Approach,
        Avoid,
        Grab,
        Eat,
        Drink,
        Throw,
        Strike,
        Cut,
        Clean,
        Wear,
        Rest,
        Play,
        Signal,
        AttemptEscape,
        Speak
    }

    public enum ItemCapability
    {
        Grabbable,
        Throwable,
        Bouncy,
        Edible,
        Drinkable,
        SwingTool,
        SharpEdge,
        FlexibleLine,
        Absorbent,
        CleaningAgent,
        LightSource,
        Comfort,
        Wearable,
        Container,
        Lever,
        Adhesive,
        Flammable,
        Dirty,
        Toxic,
        Fragile,
        Entertainment
    }

    public enum CharacterEmotion
    {
        Neutral,
        Joy,
        Curiosity,
        Sadness,
        Fear,
        Anger,
        Disgust,
        Surprise,
        Contempt,
        Relief
    }

    public enum ResidentEventType
    {
        ItemIntroduced,
        GiftReceived,
        ItemUsed,
        PlayerCausedMess,
        Harmed,
        ComfortProvided,
        PromiseMade,
        PromiseKept,
        PromiseBroken,
        Conversation,
        EscapeProgress
    }
}
