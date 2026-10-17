using System;
using LongDrive.Controller;

internal static class MenuFocusChecks
{
    internal static void Run(Action<bool, string> check)
    {
        var save = new[]
        {
            new MenuFocusTarget(50, 700, 0, false), // metadata/path is excluded
            new MenuFocusTarget(200, 450, 1, true),
            new MenuFocusTarget(600, 450, 1, true),
            new MenuFocusTarget(1000, 80, 0, true), // actual Save action, bottom right
            new MenuFocusTarget(1000, 160, 5, true) // editable save name
        };
        check(MenuFocus.Initial(save) == 3, "save default prioritizes actionable Save over path, slots, and name field");
        save[3].Eligible = false;
        check(MenuFocus.Initial(save) == 1, "load-only screen defaults to first visible save slot");
        save[1].Eligible = false;
        check(MenuFocus.Initial(save) == 2, "hidden save entries are skipped");
        save[1].Eligible = save[3].Eligible = true;
        check(MenuFocus.Move(save, 1, 1, 0) == 2, "right from left save slot reaches right save slot");
        check(MenuFocus.Move(save, 3, 0, 1) == 4, "up from Save can reach editable name");
        check(MenuFocus.Move(save, 4, 0, -1) == 3, "down from name returns to Save without a keyboard overlay");
        check(MenuFocus.Move(save, 3, -1, 0) == 2, "left from Save returns to visible slots");
        check(MenuFocus.Move(save, 3, 1, 0) == -1, "save navigation does not wrap to unrelated controls");
        var dialog = new[]
        {
            new MenuFocusTarget(1000, 80, 0, false), // background Save outside dialog
            new MenuFocusTarget(900, 80, 1, true),  // Yes
            new MenuFocusTarget(1100, 80, 0, true)  // No
        };
        check(MenuFocus.Initial(dialog) == 2, "overwrite/delete confirmation defaults to No");
        check(MenuFocus.Move(dialog, 2, -1, 0) == 1, "confirmation allows explicit move from No to Yes");
        dialog[1].Eligible = false;
        check(MenuFocus.Move(dialog, 2, -1, 0) == -1, "confirmation cannot move to controls behind the dialog");
        check(MenuFocus.Initial(Array.Empty<MenuFocusTarget>()) == -1, "empty menu has no invented selection");
    }
}
