using UnityEngine;
using UnityEngine.UI;

namespace GemGrid.Audio
{
    /// <summary>Plays <see cref="SfxId.ButtonClick"/> whenever the sibling <see cref="Button"/> is clicked.</summary>
    [RequireComponent(typeof(Button))]
    public class ButtonClickSfx : MonoBehaviour
    {
        private void Awake() =>
            GetComponent<Button>().onClick.AddListener(() => SharedAudioService.Instance.PlaySfx(SfxId.ButtonClick));
    }
}
