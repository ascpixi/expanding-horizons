using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

using System.Threading.Tasks;
 
[Preserve]
public class SplashScreenHandler
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void BeforeSplashScreen()
    {
#if UNITY_WEBGL
        Application.focusChanged += HandleFocusChange;
#else
        Task.Run(AsyncSkip);
#endif
    }
 
#if UNITY_WEBGL
    static void HandleFocusChange(bool obj)
    {
        Application.focusChanged -= HandleFocusChange;
        SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
    }
#else
    static void AsyncSkip()
    {
        SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
    }
#endif
}