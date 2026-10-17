using UnityEngine;
using UnityEngine.UI;

namespace LongDrive.Controller
{
    internal sealed class TopMenuNavigation
    {
        private GameObject scope;
        internal bool Active => scope != null && scope.activeInHierarchy;
        internal bool UpdateScope(bool blocked)
        {
            var menu = menuhandler.s;
            var next = !blocked && menu != null && menu.Menu != null && menu.Menu.activeInHierarchy &&
                (mainscript.s == null || !mainscript.s.settingsOpen) ? menu.Menu : null;
            bool changed = next != scope;
            scope = next;
            return changed;
        }
        internal bool Allows(Selectable target) => !Active || target.transform.IsChildOf(scope.transform);
        internal int Priority(Selectable target)
        {
            var button = target as Button;
            if (button == null) return 3;
            int priority = 2;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                priority = System.Math.Min(priority, MenuFocus.TopLevelPriority(button.onClick.GetPersistentMethodName(i)));
            return priority;
        }
        internal void Release() { scope = null; }
    }
}
