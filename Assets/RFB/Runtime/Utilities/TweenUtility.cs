/****************************************
 * 
 * TweenUtility.cs
 * by: Ryan F. Bailey
 * 
 * description: A utility for simple
 * tween animations.
 * 
 ****************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFB.Utilities
{
    // Tween eases
    public enum TweenEase
    {
        linear,
        easeInQuad,
        easeOutQuad,
        easeInOutQuad,
        easeInCubic,
        easeOutCubic,
        easeInOutCubic,
        easeInQuart,
        easeOutQuart,
        easeInOutQuart,
        easeInQuint,
        easeOutQuint,
        easeInOutQuint,
        easeInSine,
        easeOutSine,
        easeInOutSine,
        easeInBack,
        easeOutBack,
        easeInOutBack,
        easeInExpo,
        easeOutExpo,
        easeInOutExpo,
        easeInCirc,
        easeOutCirc,
        easeInOutCirc,
        easeInElastic,
        easeOutElastic,
        easeInOutElastic,
        easeInBounce,
        easeOutBounce,
        easeInOutBounce
    }

    // The tween
    public static class TweenUtility
    {
        #region STATIC
        // Log
        private static void Log(string comment, LogType type = LogType.Log)
        {
            LogUtility.Log(comment, "Tween Utility", type);
        }

        // Get tween
        public static TweenHelper GetTween(GameObject gameObject, string tweenID)
        {
            // Check for gameobject
            if (gameObject != null)
            {
                // Check for helpers
                TweenHelper[] helpers = gameObject.GetComponents<TweenHelper>();
                if (helpers != null)
                {
                    // Check for tween id
                    foreach (TweenHelper helper in helpers)
                    {
                        if (string.Equals(tweenID, helper.data.tweenID, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return helper;
                        }
                    }
                }
            }

            // None found
            return null;
        }
        // Stop tween
        public static void StopTween(GameObject gameObject, string tweenID)
        {
            // Find helper
            TweenHelper helper = GetTween(gameObject, tweenID);
            if (helper != null)
            {
                // Cancel tween
                helper.Cancel();
            }
        }
        // Perform tween
        public static TweenHelper StartTween(GameObject gameObject, string tweenID, float startValue, float endValue, float duration, TweenEase ease, Action<GameObject, string, float> onUpdate, Action<GameObject, string, bool> onComplete)
        {
            // Create data
            TweenData newData = new TweenData();
            newData.tweenID = tweenID;
            newData.startValue = startValue;
            newData.endValue = endValue;
            newData.duration = duration;
            newData.ease = ease;
            newData.onUpdate = onUpdate;
            newData.onComplete = onComplete;

            // Tween
            return StartTween(gameObject, newData);
        }
        // Tween
        public static TweenHelper StartTween(GameObject gameObject, TweenData newData)
        {
            // No id
            if (string.IsNullOrEmpty(newData.tweenID))
            {
                Log("Tween Failed - No Tween ID Passed", LogType.Error);
                return null;
            }
            // No gameobject
            if (gameObject == null)
            {
                Log("Tween Failed - No GameObject Passed\nTween ID: " + newData.tweenID, LogType.Error);
                return null;
            }

            // Stop existing tween
            StopTween(gameObject, newData.tweenID);

            // Tween
            TweenHelper helper = gameObject.AddComponent<TweenHelper>();
            helper.Tween(newData);

            // Return
            return helper;
        }
        #endregion

        #region INSTANCE
        // Tween
        [Serializable]
        public struct TweenData
        {
            // Unique identifier
            public string tweenID;

            // Start value
            public float startValue;
            // End value
            public float endValue;
            // Duration
            public float duration;
            // Ease
            public TweenEase ease;

            // Update callback (float = value)
            public Action<GameObject, string, float> onUpdate;
            // Complete callback (bool = cancelled)
            public Action<GameObject, string, bool> onComplete;
        }

        // Tween helper
        public class TweenHelper : MonoBehaviour
        {
            // Tween data
            public TweenData data;

            // Animate
            public bool isAnimating { get; private set; }
            // Elapsed time
            public float elapsed { get; private set; }

            // Function
            private Func<float, float, float, float> _function;
            // Coroutine
            private Coroutine _coroutine;

            // Tween
            public void Tween(TweenData newData)
            {
                // Set data
                data = newData;
                // Get function
                _function = GetEaseFunction(data.ease);

                // Animating
                _coroutine = StartCoroutine(Perform());
            }
            // Cancel on disable
            private void OnDisable()
            {
                Cancel();
            }
            // Perform
            private IEnumerator Perform()
            {
                // Animating
                isAnimating = true;
                elapsed = 0f;

                // Immediate
                if (data.duration > 0f)
                {
                    // Perform
                    float start = Time.realtimeSinceStartup;
                    while (elapsed < data.duration)
                    {
                        // Call
                        float progress = Mathf.Clamp01(elapsed / data.duration);
                        float newValue = _function(data.startValue, data.endValue, progress);
                        data.onUpdate(gameObject, data.tweenID, newValue);

                        // Wait a frame & update elapsed
                        yield return new WaitForEndOfFrame();
                        elapsed = Time.realtimeSinceStartup - start;
                    }
                }

                // Final value
                data.onUpdate(gameObject, data.tweenID, data.endValue);

                // Complete
                Complete(false);
            }
            // Cancel
            public void Cancel()
            {
                Complete(true);
            }
            // Complete
            private void Complete(bool wasCancelled)
            {
                // Ignore if not animating
                if (!isAnimating)
                {
                    return;
                }

                // Done
                isAnimating = false;

                // Stop coroutine
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                    _coroutine = null;
                }

                // Complete
                if (data.onComplete != null)
                {
                    data.onComplete(gameObject, data.tweenID, wasCancelled);
                }

                // Destroy this script
                Destroy(this);
            }
        }
        #endregion

        #region METHODS
        public static Func<float, float, float, float> GetEaseFunction(TweenEase ease)
        {
            Func<float, float, float, float> del = null;
            switch (ease)
            {
                case TweenEase.linear:
                    del = linear;
                    break;
                case TweenEase.easeInQuad:
                    del = easeInQuad;
                    break;
                case TweenEase.easeOutQuad:
                    del = easeOutQuad;
                    break;
                case TweenEase.easeInOutQuad:
                    del = easeInOutQuad;
                    break;
                case TweenEase.easeInCubic:
                    del = easeInCubic;
                    break;
                case TweenEase.easeOutCubic:
                    del = easeOutCubic;
                    break;
                case TweenEase.easeInOutCubic:
                    del = easeInOutCubic;
                    break;
                case TweenEase.easeInQuart:
                    del = easeInQuart;
                    break;
                case TweenEase.easeOutQuart:
                    del = easeOutQuart;
                    break;
                case TweenEase.easeInOutQuart:
                    del = easeInOutQuart;
                    break;
                case TweenEase.easeInQuint:
                    del = easeInQuint;
                    break;
                case TweenEase.easeOutQuint:
                    del = easeOutQuint;
                    break;
                case TweenEase.easeInOutQuint:
                    del = easeInOutQuint;
                    break;
                case TweenEase.easeInSine:
                    del = easeInSine;
                    break;
                case TweenEase.easeOutSine:
                    del = easeOutSine;
                    break;
                case TweenEase.easeInOutSine:
                    del = easeInOutSine;
                    break;
                case TweenEase.easeInBack:
                    del = easeInBack;
                    break;
                case TweenEase.easeOutBack:
                    del = easeOutBack;
                    break;
                case TweenEase.easeInOutBack:
                    del = easeInOutBack;
                    break;
                case TweenEase.easeInExpo:
                    del = easeInExpo;
                    break;
                case TweenEase.easeOutExpo:
                    del = easeOutExpo;
                    break;
                case TweenEase.easeInOutExpo:
                    del = easeInOutExpo;
                    break;
                case TweenEase.easeInCirc:
                    del = easeInCirc;
                    break;
                case TweenEase.easeOutCirc:
                    del = easeOutCirc;
                    break;
                case TweenEase.easeInOutCirc:
                    del = easeInOutCirc;
                    break;
                case TweenEase.easeInElastic:
                    del = easeInElastic;
                    break;
                case TweenEase.easeOutElastic:
                    del = easeOutElastic;
                    break;
                case TweenEase.easeInOutElastic:
                    del = easeInOutElastic;
                    break;
                case TweenEase.easeInBounce:
                    del = easeInBounce;
                    break;
                case TweenEase.easeOutBounce:
                    del = easeOutBounce;
                    break;
                case TweenEase.easeInOutBounce:
                    del = easeInOutBounce;
                    break;
                default:
                    del = linear;
                    break;
            }
            return del;
        }

        public static float easeOutQuadOpt(float start, float diff, float ratioPassed)
        {
            return -diff * ratioPassed * (ratioPassed - 2) + start;
        }

        public static float easeInQuadOpt(float start, float diff, float ratioPassed)
        {
            return diff * ratioPassed * ratioPassed + start;
        }

        public static float easeInOutQuadOpt(float start, float diff, float ratioPassed)
        {
            ratioPassed /= .5f;
            if (ratioPassed < 1) return diff / 2 * ratioPassed * ratioPassed + start;
            ratioPassed--;
            return -diff / 2 * (ratioPassed * (ratioPassed - 2) - 1) + start;
        }

        public static Vector3 easeInOutQuadOpt(Vector3 start, Vector3 diff, float ratioPassed)
        {
            ratioPassed /= .5f;
            if (ratioPassed < 1) return diff / 2 * ratioPassed * ratioPassed + start;
            ratioPassed--;
            return -diff / 2 * (ratioPassed * (ratioPassed - 2) - 1) + start;
        }

        public static float linear(float start, float end, float val)
        {
            return Mathf.Lerp(start, end, val);
        }

        public static float clerp(float start, float end, float val)
        {
            float min = 0.0f;
            float max = 360.0f;
            float half = Mathf.Abs((max - min) / 2.0f);
            float retval = 0.0f;
            float diff = 0.0f;
            if ((end - start) < -half)
            {
                diff = ((max - start) + end) * val;
                retval = start + diff;
            }
            else if ((end - start) > half)
            {
                diff = -((max - end) + start) * val;
                retval = start + diff;
            }
            else retval = start + (end - start) * val;
            return retval;
        }

        public static float spring(float start, float end, float val)
        {
            val = Mathf.Clamp01(val);
            val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
            return start + (end - start) * val;
        }

        public static float easeInQuad(float start, float end, float val)
        {
            end -= start;
            return end * val * val + start;
        }

        public static float easeOutQuad(float start, float end, float val)
        {
            end -= start;
            return -end * val * (val - 2) + start;
        }

        public static float easeInOutQuad(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val + start;
            val--;
            return -end / 2 * (val * (val - 2) - 1) + start;
        }


        public static float easeInOutQuadOpt2(float start, float diffBy2, float val, float val2)
        {
            val /= .5f;
            if (val < 1) return diffBy2 * val2 + start;
            val--;
            return -diffBy2 * ((val2 - 2) - 1f) + start;
        }

        public static float easeInCubic(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val + start;
        }

        public static float easeOutCubic(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val + 1) + start;
        }

        public static float easeInOutCubic(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val + 2) + start;
        }

        public static float easeInQuart(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val + start;
        }

        public static float easeOutQuart(float start, float end, float val)
        {
            val--;
            end -= start;
            return -end * (val * val * val * val - 1) + start;
        }

        public static float easeInOutQuart(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val + start;
            val -= 2;
            return -end / 2 * (val * val * val * val - 2) + start;
        }

        public static float easeInQuint(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val * val + start;
        }

        public static float easeOutQuint(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val * val * val + 1) + start;
        }

        public static float easeInOutQuint(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val * val * val + 2) + start;
        }

        public static float easeInSine(float start, float end, float val)
        {
            end -= start;
            return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
        }

        public static float easeOutSine(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
        }

        public static float easeInOutSine(float start, float end, float val)
        {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
        }

        public static float easeInExpo(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
        }

        public static float easeOutExpo(float start, float end, float val)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
        }

        public static float easeInOutExpo(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
            val--;
            return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
        }

        public static float easeInCirc(float start, float end, float val)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
        }

        public static float easeOutCirc(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * Mathf.Sqrt(1 - val * val) + start;
        }

        public static float easeInOutCirc(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
            val -= 2;
            return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
        }

        public static float easeInBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            return end - easeOutBounce(0, end, d - val) + start;
        }

        public static float easeOutBounce(float start, float end, float val)
        {
            val /= 1f;
            end -= start;
            if (val < (1 / 2.75f))
            {
                return end * (7.5625f * val * val) + start;
            }
            else if (val < (2 / 2.75f))
            {
                val -= (1.5f / 2.75f);
                return end * (7.5625f * (val) * val + .75f) + start;
            }
            else if (val < (2.5 / 2.75))
            {
                val -= (2.25f / 2.75f);
                return end * (7.5625f * (val) * val + .9375f) + start;
            }
            else
            {
                val -= (2.625f / 2.75f);
                return end * (7.5625f * (val) * val + .984375f) + start;
            }
        }

        public static float easeInOutBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            if (val < d / 2) return easeInBounce(0, end, val * 2) * 0.5f + start;
            else return easeOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
        }

        public static float easeInBack(float start, float end, float val)
        {
            return easeInBack(start, end, val, 1.0f);
        }

        public static float easeInBack(float start, float end, float val, float overshoot)
        {
            end -= start;
            val /= 1;
            float s = 1.70158f * overshoot;
            return end * (val) * val * ((s + 1) * val - s) + start;
        }

        public static float easeOutBack(float start, float end, float val)
        {
            return easeOutBack(start, end, val, 1.0f);
        }

        public static float easeOutBack(float start, float end, float val, float overshoot)
        {
            float s = 1.70158f * overshoot;
            end -= start;
            val = (val / 1) - 1;
            return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
        }

        public static float easeInOutBack(float start, float end, float val)
        {
            return easeInOutBack(start, end, val, 1.0f);
        }

        public static float easeInOutBack(float start, float end, float val, float overshoot)
        {
            float s = 1.70158f * overshoot;
            end -= start;
            val /= .5f;
            if ((val) < 1)
            {
                s *= (1.525f) * overshoot;
                return end / 2 * (val * val * (((s) + 1) * val - s)) + start;
            }
            val -= 2;
            s *= (1.525f) * overshoot;
            return end / 2 * ((val) * val * (((s) + 1) * val + s) + 2) + start;
        }

        public static float easeInElastic(float start, float end, float val)
        {
            return easeInElastic(start, end, val, 1.0f, 0.3f);
        }

        public static float easeInElastic(float start, float end, float val, float overshoot, float period)
        {
            end -= start;

            float p = period;
            float s = 0f;
            float a = 0f;

            if (val == 0f) return start;

            if (val == 1f) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4f;
            }
            else
            {
                s = p / (2f * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (overshoot > 1f && val > 0.6f)
                overshoot = 1f + ((1f - val) / 0.4f * (overshoot - 1f));
            // Debug.Log("ease in elastic val:"+val+" a:"+a+" overshoot:"+overshoot);

            val = val - 1f;
            return start - (a * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p)) * overshoot;
        }

        public static float easeOutElastic(float start, float end, float val)
        {
            return easeOutElastic(start, end, val, 1.0f, 0.3f);
        }

        public static float easeOutElastic(float start, float end, float val, float overshoot, float period)
        {
            end -= start;

            float p = period;
            float s = 0f;
            float a = 0f;

            if (val == 0f) return start;

            // Debug.Log("ease out elastic val:"+val+" a:"+a);
            if (val == 1f) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4f;
            }
            else
            {
                s = p / (2f * Mathf.PI) * Mathf.Asin(end / a);
            }
            if (overshoot > 1f && val < 0.4f)
                overshoot = 1f + (val / 0.4f * (overshoot - 1f));
            // Debug.Log("ease out elastic val:"+val+" a:"+a+" overshoot:"+overshoot);

            return start + end + a * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p) * overshoot;
        }

        public static float easeInOutElastic(float start, float end, float val)
        {
            return easeInOutElastic(start, end, val, 1.0f, 0.3f);
        }

        public static float easeInOutElastic(float start, float end, float val, float overshoot, float period)
        {
            end -= start;

            float p = period;
            float s = 0f;
            float a = 0f;

            if (val == 0f) return start;

            val = val / (1f / 2f);
            if (val == 2f) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4f;
            }
            else
            {
                s = p / (2f * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (overshoot > 1f)
            {
                if (val < 0.2f)
                {
                    overshoot = 1f + (val / 0.2f * (overshoot - 1f));
                }
                else if (val > 0.8f)
                {
                    overshoot = 1f + ((1f - val) / 0.2f * (overshoot - 1f));
                }
            }

            if (val < 1f)
            {
                val = val - 1f;
                return start - 0.5f * (a * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p)) * overshoot;
            }
            val = val - 1f;
            return end + start + a * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val - s) * (2f * Mathf.PI) / p) * 0.5f * overshoot;
        }
        #endregion
    }
}
