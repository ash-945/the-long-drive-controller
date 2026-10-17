using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace LongDrive.Controller
{
    internal sealed class SaveMenuNavigation
    {
        private newSaveScreenScript screen;
        private GameObject scope, confirmation;
        private TMP_Text hintLabel;
        private Text legacyHint;
        private string original, marked;
        internal bool Active => screen != null && screen.isActiveAndEnabled;
        internal GameObject Scope => scope;

        internal bool UpdateScope()
        {
            var menu = menuhandler.s;
            var next = menu != null && menu.saveScreen != null && menu.saveScreen.isActiveAndEnabled ? menu.saveScreen : null;
            if (next == null) { Release(); return false; }
            screen = next;
            var dialog = Dialog();
            var nextScope = dialog != null ? dialog : screen.gameObject;
            bool changed = nextScope != scope;
            scope = nextScope; confirmation = dialog;
            return changed;
        }
        private GameObject Dialog()
        {
            foreach (var root in new[] { screen.ButtonSaveYesNo, screen.ButtonLoadYesNo, screen.ButtonDeleteYesNo, screen.ButtonLoadLastYesNo })
                if (root != null && root.activeInHierarchy) return root;
            return null;
        }
        internal bool Allows(Selectable target)
        {
            if (!Active) return true;
            if (scope == null || !target.transform.IsChildOf(scope.transform)) return false;
            // These input components display metadata/copyable paths, not actions.
            var input = target as TMP_InputField;
            return input == null || input != screen.TextScreenshotPath && input != screen.TextInfoName && input != screen.TextInfoDate &&
                input != screen.TextInfoSeed && input != screen.TextInfoDriven && input != screen.TextInfoVersion;
        }
        private static bool Under(Selectable target, GameObject root) => root != null && target.transform.IsChildOf(root.transform);
        internal int Priority(Selectable target)
        {
            if (confirmation != null)
            {
                var button = target as Button;
                if (button != null)
                    for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    {
                        string method = button.onClick.GetPersistentMethodName(i);
                        if (method == "PressedSaveNo" || method == "PressedLoadNo" || method == "PressedDeleteNo" || method == "PressedLoadLastNo") return 0;
                    }
                return 1;
            }
            if (Under(target, screen.ButtonSave)) return 0;
            foreach (var root in screen.ButtonGameObjects) if (Under(target, root)) return 1;
            foreach (var root in screen.ButtonAutoSaveGameObject) if (Under(target, root)) return 2;
            if (Under(target, screen.ButtonLoadLast)) return 3;
            return target is TMP_InputField || target is InputField ? 5 : 4;
        }
        internal bool FocusSave(EventSystem system)
        {
            if (!Active || confirmation != null || screen.ButtonSave == null) return false;
            var button = screen.ButtonSave.GetComponentInChildren<Button>(true);
            if (button == null || !button.isActiveAndEnabled || !button.IsInteractable()) return false;
            DeactivateInput(system.currentSelectedGameObject);
            system.SetSelectedGameObject(button.gameObject);
            return true;
        }
        internal bool FocusSlots(EventSystem system)
        {
            if (!Active || confirmation != null) return false;
            foreach (var root in screen.ButtonGameObjects)
            {
                var button = root == null ? null : root.GetComponentInChildren<Button>(true);
                if (button == null || !button.isActiveAndEnabled || !button.IsInteractable()) continue;
                DeactivateInput(system.currentSelectedGameObject);
                system.SetSelectedGameObject(button.gameObject);
                return true;
            }
            return false;
        }
        internal bool Cancel(EventSystem system)
        {
            if (!Active) return false;
            if (confirmation != null)
            {
                if (confirmation == screen.ButtonSaveYesNo) screen.PressedSaveNo();
                else if (confirmation == screen.ButtonLoadYesNo) screen.PressedLoadNo();
                else if (confirmation == screen.ButtonDeleteYesNo) screen.PressedDeleteNo();
                else screen.PressedLoadLastNo();
            }
            else if (screen.InputFieldSaveName != null && screen.InputFieldSaveName.isFocused)
            {
                screen.InputFieldSaveName.DeactivateInputField();
                FocusSave(system);
            }
            else screen.PressedBack();
            return true;
        }
        internal static void DeactivateInput(GameObject selected)
        {
            if (selected == null) return;
            selected.GetComponent<TMP_InputField>()?.DeactivateInputField();
            selected.GetComponent<InputField>()?.DeactivateInputField();
        }
        internal void UpdateHint()
        {
            if (!Active || confirmation != null || screen.ButtonSave == null || !screen.ButtonSave.activeInHierarchy) return;
            // Prefer the native legacy label when present, rather than selecting
            // the TMP visual child that our glyph bridge creates underneath it.
            var legacy = screen.ButtonSave.GetComponentInChildren<Text>(true);
            var tmp = legacy == null ? screen.ButtonSave.GetComponentInChildren<TMP_Text>(true) : null;
            if (tmp == null && legacy == null) return;
            if (tmp != hintLabel || legacy != legacyHint)
            { RestoreHint(); hintLabel = tmp; legacyHint = legacy; }
            string current = hintLabel != null ? hintLabel.text : legacyHint.text;
            if (current == marked) return;
            original = current;
            marked = original + (hintLabel != null ? " <size=60%>" : " <size=16>") + ControllerGlyphs.Tmp("[RB: focus]") + "</size>";
            if (hintLabel != null) hintLabel.text = marked;
            else legacyHint.text = marked;
        }
        private void RestoreHint()
        {
            if (hintLabel != null && hintLabel.text == marked) hintLabel.text = original;
            if (legacyHint != null && legacyHint.text == marked) legacyHint.text = original;
            hintLabel = null; legacyHint = null; original = marked = null;
        }
        internal void Release() { RestoreHint(); screen = null; scope = confirmation = null; }
    }
}
