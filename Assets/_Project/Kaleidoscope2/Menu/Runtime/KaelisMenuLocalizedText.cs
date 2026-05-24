using TMPro;
using UnityEngine;

namespace Kaleidoscope2.Menu
{
    [DisallowMultipleComponent]
    internal sealed class KaelisMenuLocalizedText : MonoBehaviour
    {
        private TMP_Text target;
        private string sourceText;
        private bool raw;

        public void Initialize(TMP_Text target, string sourceText, bool raw)
        {
            this.target = target;
            this.sourceText = sourceText;
            this.raw = raw;
            Refresh();
        }

        public void SetSource(string value, bool rawValue)
        {
            sourceText = value;
            raw = rawValue;
            Refresh();
        }

        private void OnEnable()
        {
            KaelisMenuLocalizationService.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            KaelisMenuLocalizationService.LanguageChanged -= Refresh;
        }

        public void Refresh()
        {
            if (target == null)
            {
                target = GetComponent<TMP_Text>();
            }

            if (target == null)
            {
                return;
            }

            target.text = raw ? sourceText : KaelisMenuLocalizationService.Translate(sourceText);
        }
    }
}
