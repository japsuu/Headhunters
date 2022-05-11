using UnityEngine;

namespace Helpers
{
    public static class CursorHelper
    {
        public static void SetCursorLockState(bool setLocked)
        {
            Cursor.lockState = setLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !setLocked;
        }
    }
}