using TMPro;
using UnityEngine;

namespace Kaleidoscope2.Menu
{
    internal sealed class KaelisMenuLanguageSelector : MonoBehaviour
    {
        private KaelisMenuInteractiveRow row;
        private TMP_Text valueText;

        public void Initialize(KaelisMenuInteractiveRow row, TMP_Text valueText)
        {
            this.row = row;
            this.valueText = valueText;
            KaelisMenuLocalizationService.LanguageChanged -= Refresh;
            KaelisMenuLocalizationService.LanguageChanged += Refresh;
            Refresh();
        }

        public void CycleLanguage()
        {
            KaelisMenuLocalizationService.CycleLanguage();
            if (row != null)
            {
                row.Flash();
            }
        }

        private void OnDestroy()
        {
            KaelisMenuLocalizationService.LanguageChanged -= Refresh;
        }

        private void Refresh()
        {
            string displayName = KaelisMenuLocalizationService.GetCurrentLanguageDisplayName();
            if (valueText != null)
            {
                KaelisMenuLocalizationService.SetRawText(valueText, displayName);
            }

            if (row != null)
            {
                row.SetTooltipCurrent(displayName);
            }
        }
    }
}
