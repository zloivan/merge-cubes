using IKhom.EventBusSystem.Runtime;
using MergeCubes.Events;
using UnityEngine;
using UnityEngine.UI;

namespace MergeCubes.Utilities
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        private void Awake() =>
            GetComponent<Button>().onClick.AddListener(() =>
                EventBus<UIButtonClickedEvent>.Raise(new UIButtonClickedEvent()));
    }
}