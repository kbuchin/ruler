using UnityEngine;

namespace Algo
{
    static class RectExtension
    {
        public static Rect Enlarge(this Rect a_rect, float a_amount)
        {
            return new Rect(a_rect.x - a_amount, a_rect.y - a_amount, a_rect.width + 2 * a_amount, a_rect.height + 2 * a_amount);
        }
    }
}
