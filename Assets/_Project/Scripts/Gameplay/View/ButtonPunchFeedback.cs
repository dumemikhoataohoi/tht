using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GemGrid.Gameplay
{
    /// <summary>Tiny tactile feedback for touch-first UX: briefly scales the button down on click/tap.</summary>
    [RequireComponent(typeof(Button))]
    public class ButtonPunchFeedback : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => StartCoroutine(Punch()));
        }

        private IEnumerator Punch()
        {
            var t = transform;
            Vector3 original = t.localScale;
            const float duration = 0.08f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float k = elapsed / duration;
                t.localScale = Vector3.Lerp(original * 0.9f, original, k);
                yield return null;
            }
            t.localScale = original;
        }
    }
}
