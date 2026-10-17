using UnityEngine;
using UnityEngine.UI;

namespace LongDrive.Controller
{
    internal sealed class ExitMenuNavigation
    {
        private GameObject scope;
        internal bool Active => scope != null && scope.activeInHierarchy;
        internal bool UpdateScope()
        {
            var menu = menuhandler.s;
            var next = menu != null && menu.SureExit != null && menu.SureExit.activeInHierarchy ? menu.SureExit : null;
            bool changed = next != scope;
            scope = next;
            return changed;
        }
        internal bool Allows(Selectable target) => !Active || target.transform.IsChildOf(scope.transform);
        internal int Priority(Selectable target)
        {
            var button = target as Button;
            if (button != null)
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    if (button.onClick.GetPersistentMethodName(i) == "PressedExitNo") return 0;
            return 1;
        }
        internal bool Cancel()
        {
            if (!Active || menuhandler.s == null) return false;
            menuhandler.s.PressedExitNo();
            UpdateScope();
            return true;
        }
        internal void Release() { scope = null; }
    }
}
