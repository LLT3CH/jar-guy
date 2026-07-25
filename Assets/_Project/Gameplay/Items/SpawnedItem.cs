using HumanGlassWatcher.Core.Items;
using UnityEngine;

namespace HumanGlassWatcher.Gameplay.Items
{
    public sealed class SpawnedItem : MonoBehaviour
    {
        public ItemDefinition Definition { get; private set; }

        public void Initialize(ItemDefinition definition)
        {
            Definition = definition;
            name = $"Item_{definition.CanonicalId}";
        }
    }
}
