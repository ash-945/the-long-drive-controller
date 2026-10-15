using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace LongDrive.Controller
{
    internal sealed class MenuControls
    {
        private EventSystem system;
        private bool originalNavigation, ownsNavigation, pointerMode;
        private Vector2 position, pressPosition;
        private PointerEventData pointer;
        private GameObject hover, pressed, dragged;
        private bool dragging;
        private float repeatAt;
        private Vector2 lastDirection;
        private readonly List<RaycastResult> hits = new List<RaycastResult>();
        private readonly Vector3[] corners = new Vector3[4];
        private BaseInputModule[] modalModules;

        public bool Cancel()
        {
            foreach (var dropdown in UnityEngine.Object.FindObjectsOfType<TMP_Dropdown>())
                if (dropdown.IsExpanded) { dropdown.Hide(); return true; }
            foreach (var dropdown in UnityEngine.Object.FindObjectsOfType<Dropdown>())
                if (HarmonyLib.AccessTools.Field(typeof(Dropdown), "m_Dropdown")?.GetValue(dropdown) is GameObject list && list.activeInHierarchy)
                { dropdown.Hide(); return true; }
            return false;
        }

        private void KeepSelectionVisible()
        {
            if (system == null || system.currentSelectedGameObject == null) return;
            var selected = system.currentSelectedGameObject.transform as RectTransform;
            var scroll = selected == null ? null : selected.GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.content == null) return;
            var viewport = scroll.viewport != null ? scroll.viewport : scroll.transform as RectTransform;
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, selected);
            var rect = viewport.rect;
            var offset = Vector2.zero;
            if (scroll.vertical)
                offset.y = bounds.min.y < rect.yMin ? rect.yMin - bounds.min.y : bounds.max.y > rect.yMax ? rect.yMax - bounds.max.y : 0;
            if (scroll.horizontal)
                offset.x = bounds.min.x < rect.xMin ? rect.xMin - bounds.min.x : bounds.max.x > rect.xMax ? rect.xMax - bounds.max.x : 0;
            scroll.content.anchoredPosition += offset;
            scroll.velocity = Vector2.zero;
        }

        public void SetModal(bool active)
        {
            if (active && modalModules == null && system != null)
            {
                var enabled = new List<BaseInputModule>();
                foreach (var module in system.GetComponents<BaseInputModule>())
                    if (module.enabled) { enabled.Add(module); module.enabled = false; }
                modalModules = enabled.ToArray();
            }
            else if (!active && modalModules != null)
            {
                foreach (var module in modalModules) if (module != null) module.enabled = true;
                modalModules = null;
            }
        }

        public bool Prepare(PadState pad)
        {
            if (EventSystem.current == null || !pad.Connected)
            {
                Release();
                return false;
            }
            if (system != EventSystem.current)
            {
                Release();
                system = EventSystem.current;
                originalNavigation = system.sendNavigationEvents;
                position = new Vector2(Screen.width * .5f, Screen.height * .5f);
                pointer = new PointerEventData(system) { pointerId = -100, button = PointerEventData.InputButton.Left };
            }
            // Keep native mouse interaction and restore keyboard navigation when used.
            bool keyboard = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Tab) || Input.GetMouseButtonDown(0);
            if (keyboard) { system.sendNavigationEvents = originalNavigation; ownsNavigation = false; pointerMode = false; }
            if (!ownsNavigation && !keyboard) { system.sendNavigationEvents = false; ownsNavigation = true; }
            return true;
        }

        public void Tick(Plugin plugin, PadState pad, PadState previous, float speed)
        {
            if (!Prepare(pad)) return;
            bool confirmDown = pad.Held(Buttons.A) && !previous.Held(Buttons.A);
            bool confirmUp = !pad.Held(Buttons.A) && previous.Held(Buttons.A);
            var direction = new Vector2(pad.LX, pad.LY);
            if (pad.Held(Buttons.Up)) direction.y = 1;
            if (pad.Held(Buttons.Down)) direction.y = -1;
            if (pad.Held(Buttons.Left)) direction.x = -1;
            if (pad.Held(Buttons.Right)) direction.x = 1;
            if (direction.sqrMagnitude > .25f)
            {
                pointerMode = false;
                Vector2 cardinal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? new Vector2(Mathf.Sign(direction.x), 0) : new Vector2(0, Mathf.Sign(direction.y));
                bool first = cardinal != lastDirection;
                if (first || Time.unscaledTime >= repeatAt)
                {
                    EnsureSelection();
                    var selected = system.currentSelectedGameObject;
                    if (selected != null)
                    {
                        var move = new AxisEventData(system) { moveVector = cardinal, moveDir = cardinal.x < 0 ? MoveDirection.Left : cardinal.x > 0 ? MoveDirection.Right : cardinal.y > 0 ? MoveDirection.Up : MoveDirection.Down };
                        ExecuteEvents.Execute(selected, move, ExecuteEvents.moveHandler);
                        if (system.currentSelectedGameObject == selected && !(selected.GetComponent<Slider>() != null && cardinal.x != 0))
                            GeometricMove(selected, cardinal);
                    }
                    repeatAt = Time.unscaledTime + (first ? .35f : .13f);
                    KeepSelectionVisible();
                }
                lastDirection = cardinal;
            }
            else lastDirection = Vector2.zero;

            var delta = new Vector2(pad.RX, pad.RY) * speed * Mathf.Min(Time.unscaledDeltaTime, .05f);
            if (delta.sqrMagnitude > 0) pointerMode = true;
            if (pointerMode)
            {
                position += delta;
                position.x = Mathf.Clamp(position.x, 0, Screen.width - 1);
                position.y = Mathf.Clamp(position.y, 0, Screen.height - 1);
                pointer.position = position;
                pointer.delta = delta;
                hits.Clear();
                system.RaycastAll(pointer, hits);
                pointer.pointerCurrentRaycast = hits.Count > 0 ? hits[0] : default;
                GameObject hit = hits.Count > 0 ? hits[0].gameObject : null;
                GameObject nextHover = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(hit);
                if (nextHover != hover)
                {
                    if (hover != null) ExecuteEvents.Execute(hover, pointer, ExecuteEvents.pointerExitHandler);
                    hover = nextHover;
                    pointer.pointerEnter = hover;
                    if (hover != null) ExecuteEvents.Execute(hover, pointer, ExecuteEvents.pointerEnterHandler);
                }
                if (confirmDown && hit != null)
                {
                    pressPosition = position;
                    pointer.pressPosition = position;
                    pointer.pointerPressRaycast = pointer.pointerCurrentRaycast;
                    pointer.eligibleForClick = true;
                    pointer.clickCount = 1;
                    pointer.clickTime = Time.unscaledTime;
                    pressed = ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.pointerDownHandler);
                    if (pressed == null) pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit);
                    pointer.pointerPress = pressed;
                    pointer.rawPointerPress = hit;
                    dragged = ExecuteEvents.GetEventHandler<IDragHandler>(hit);
                    pointer.pointerDrag = dragged;
                    if (dragged != null) ExecuteEvents.Execute(dragged, pointer, ExecuteEvents.initializePotentialDrag);
                }
                if (pressed != null && pad.Held(Buttons.A) && dragged != null)
                {
                    if (!dragging && Vector2.Distance(position, pressPosition) > system.pixelDragThreshold)
                    {
                        dragging = pointer.dragging = true;
                        pointer.eligibleForClick = false;
                        ExecuteEvents.Execute(dragged, pointer, ExecuteEvents.beginDragHandler);
                    }
                    if (dragging) ExecuteEvents.Execute(dragged, pointer, ExecuteEvents.dragHandler);
                }
                if (confirmUp)
                {
                    bool activated = pressed != null;
                    PointerUp(true);
                    if (activated) plugin.MenuActivated();
                }
                if (pad.RT != pad.LT && hit != null)
                {
                    pointer.scrollDelta = new Vector2(0, (pad.LT - pad.RT) * 12f * Time.unscaledDeltaTime);
                    ExecuteEvents.ExecuteHierarchy(hit, pointer, ExecuteEvents.scrollHandler);
                }
            }
            else
            {
                if (pressed != null) PointerUp(false);
                EnsureSelection();
                if (confirmDown && system.currentSelectedGameObject != null)
                {
                    ExecuteEvents.Execute(system.currentSelectedGameObject, new BaseEventData(system), ExecuteEvents.submitHandler);
                    plugin.MenuActivated();
                }
            }
        }

        private bool Usable(Selectable selectable) => selectable != null && selectable.isActiveAndEnabled && selectable.IsInteractable();
        private Vector2 Center(Selectable selectable)
        {
            var rect = selectable.transform as RectTransform;
            var canvas = selectable.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        }
        private void EnsureSelection()
        {
            if (system.currentSelectedGameObject != null && Usable(system.currentSelectedGameObject.GetComponent<Selectable>())) return;
            Selectable best = null;
            float score = float.PositiveInfinity;
            foreach (var item in Selectable.allSelectablesArray)
            {
                if (!Usable(item)) continue;
                Vector2 center = Center(item);
                if (center.x < 0 || center.x > Screen.width || center.y < 0 || center.y > Screen.height) continue;
                float candidate = (Screen.height - center.y) * Screen.width + center.x;
                if (candidate < score) { best = item; score = candidate; }
            }
            if (best != null) system.SetSelectedGameObject(best.gameObject);
        }
        private void GeometricMove(GameObject selected, Vector2 direction)
        {
            var from = selected.GetComponent<Selectable>();
            if (from == null) return;
            var origin = Center(from);
            Selectable best = null;
            float score = float.PositiveInfinity;
            foreach (var item in Selectable.allSelectablesArray)
            {
                if (!Usable(item) || item == from) continue;
                var offset = Center(item) - origin;
                float forward = Vector2.Dot(offset, direction);
                if (forward <= 1) continue;
                float sideways = Mathf.Abs(Vector2.Dot(offset, new Vector2(-direction.y, direction.x)));
                float candidate = forward + sideways * 3;
                if (candidate < score) { score = candidate; best = item; }
            }
            if (best != null) system.SetSelectedGameObject(best.gameObject);
        }
        private void PointerUp(bool allowClick)
        {
            if (pointer == null) return;
            if (pressed != null)
            {
                ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerUpHandler);
                if (allowClick && !dragging && pointer.eligibleForClick &&
                    ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointer.pointerCurrentRaycast.gameObject) == pressed)
                    ExecuteEvents.Execute(pressed, pointer, ExecuteEvents.pointerClickHandler);
            }
            if (dragging && dragged != null) ExecuteEvents.Execute(dragged, pointer, ExecuteEvents.endDragHandler);
            pressed = dragged = pointer.pointerPress = pointer.rawPointerPress = pointer.pointerDrag = null;
            dragging = pointer.dragging = pointer.eligibleForClick = false;
        }
        public void Release()
        {
            SetModal(false);
            PointerUp(false);
            if (hover != null && pointer != null) ExecuteEvents.Execute(hover, pointer, ExecuteEvents.pointerExitHandler);
            hover = null;
            if (system != null && ownsNavigation) system.sendNavigationEvents = originalNavigation;
            system = null;
            ownsNavigation = pointerMode = false;
            lastDirection = Vector2.zero;
        }
        public void Draw()
        {
            if (pointerMode)
            {
                GUI.color = Color.yellow;
                GUI.DrawTexture(new Rect(position.x - 8, Screen.height - position.y - 1, 16, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(position.x - 1, Screen.height - position.y - 8, 2, 16), Texture2D.whiteTexture);
                GUI.color = Color.white;
                return;
            }
            if (system == null || system.currentSelectedGameObject == null) return;
            var rect = system.currentSelectedGameObject.transform as RectTransform;
            if (rect == null) return;
            var canvas = rect.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            rect.GetWorldCorners(corners);
            var min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            var box = new Rect(min.x - 3, Screen.height - max.y - 3, max.x - min.x + 6, max.y - min.y + 6);
            GUI.color = Color.yellow;
            GUI.DrawTexture(new Rect(box.x, box.y, box.width, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(box.x, box.yMax - 3, box.width, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(box.x, box.y, 3, box.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(box.xMax - 3, box.y, 3, box.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
