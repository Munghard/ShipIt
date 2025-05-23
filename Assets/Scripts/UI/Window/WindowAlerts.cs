using Assets.Scripts.Core;
using System.Collections;
using UnityEngine;


namespace Assets.Scripts.UI.Window
{
    public static class WindowAlerts
    {
        public static void DoAlert(UIManager uIManager,WindowUI windowUI,string alert, int times,string sound)
        {
            if (windowUI.CurrentAlertCoroutine == null)
            {
                windowUI.CurrentAlertCoroutine = uIManager.StartCoroutine(Alert(windowUI, alert, times, sound));
            }
        }

        public static IEnumerator Alert(WindowUI windowUI, string alert,int times,string sound)
        {
            
            float speed = 0.5f;

            for (int i = 0; i < times; i++)
            {
                windowUI.AddToClassList(alert);
                AudioManager.Play(sound);
                yield return new WaitForSeconds(speed);
                windowUI.RemoveFromClassList(alert);
                yield return new WaitForSeconds(speed);
            }

            windowUI.CurrentAlertCoroutine = null;
        }

    }
}
