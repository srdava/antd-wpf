using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

namespace Antd.Wpf.UiTests
{
    /// <summary>
    /// Clicks the real window. Assertions in the other suites set properties directly, which is why
    /// "the Select never opened on click" stayed green for three releases; this suite exists to make
    /// that class of defect fail.
    /// </summary>
    internal static class Program
    {
        private static int _checks;
        private static int _failures;

        private static int Main(string[] args)
        {
            string exe = ResolveHost(args);
            if (exe == null)
            {
                Console.WriteLine("RESULT: SKIP (host exe not built)");
                return 2;
            }

            Console.WriteLine("host: " + exe);

            Application app = Application.Launch(exe);
            try
            {
                using (var automation = new UIA3Automation())
                {
                    Window window = app.GetMainWindow(automation, TimeSpan.FromSeconds(15));
                    if (window == null)
                    {
                        Console.WriteLine("FAIL could not find the main window");
                        return 1;
                    }

                    window.SetForeground();
                    Thread.Sleep(400);

                    if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "--dump")
                    {
                        window.FindFirstDescendant(cf => cf.ByAutomationId("StatusSelect")).Click();
                        Thread.Sleep(500);
                        Console.WriteLine("--- tree after select click ---");
                        foreach (AutomationElement e in window.FindAllDescendants())
                        {
                            string id = e.Properties.AutomationId.IsSupported ? e.Properties.AutomationId.Value : null;
                            Console.WriteLine("  [" + e.ControlType + "] id=" + id + " name=" + e.Name);
                        }
                        using (var shot = window.Capture()) shot.Save("artifacts/ui-after-click.png");

                        var popItem = automation.GetDesktop().FindFirstDescendant(cf => cf.ByName("运行中"));
                        Console.WriteLine("desktop has item 运行中: " + (popItem != null));
                        Console.WriteLine("--- desktop windows ---");
                        foreach (Window w in automation.GetDesktop().FindAllChildren().Select(x => x.AsWindow()))
                        {
                            Console.WriteLine("  win: " + w.Title + " / " + w.Name);
                        }
                        return 0;
                    }
                    CheckSelectClicks(window, automation);
                    CheckModalConfirm(window, automation);
                }
            }
            finally
            {
                try { app.Close(); } catch { }
            }

            Console.WriteLine("checks: " + _checks + ", failures: " + _failures);
            Console.WriteLine("RESULT: " + (_failures == 0 ? "PASS" : "FAIL"));
            return _failures == 0 ? 0 : 1;
        }

        private static void CheckSelectClicks(Window window, UIA3Automation automation)
        {
            AutomationElement element = window.FindFirstDescendant(cf => cf.ByAutomationId("StatusSelect"));
            Check("host exposes the select", element != null, element == null ? "missing" : "ok");
            if (element == null) return;

            // A real click, through the input pipeline: this is what users do and what was broken.
            element.Click();
            Thread.Sleep(350);

            Keyboard.Press(VirtualKeyShort.DOWN);
            Thread.Sleep(120);
            Keyboard.Press(VirtualKeyShort.ENTER);
            Thread.Sleep(350);

            AutomationElement value = window.FindFirstDescendant(cf => cf.ByAutomationId("StatusValue"));
            string text = value == null ? "<missing>" : value.Name;
            Check("clicking the field opens the list and picks an item", text == "运行中", text);
        }

        private static void CheckModalConfirm(Window window, UIA3Automation automation)
        {
            AutomationElement button = window.FindFirstDescendant(cf => cf.ByAutomationId("ConfirmButton"));
            Check("host exposes the confirm button", button != null, button == null ? "missing" : "ok");
            if (button == null) return;

            button.Click();
            Thread.Sleep(500);

            AutomationElement ok = automation.GetDesktop().FindFirstDescendant(cf => cf.ByName("确定"));
            Check("confirm dialog shows an OK button", ok != null, ok == null ? "missing" : "ok");
            if (ok == null) return;

            ok.Click();
            Thread.Sleep(400);

            AutomationElement result = window.FindFirstDescendant(cf => cf.ByAutomationId("ResultText"));
            string text = result == null ? "<missing>" : result.Name;
            Check("confirm callback runs after a real click", text == "ok", text);
        }

        private static string ResolveHost(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0])) return args[0];

            string local = AppContext.BaseDirectory;
            DirectoryInfo dir = new DirectoryInfo(local);
            for (int i = 0; i < 6 && dir != null; i++)
            {
                string candidate = Path.Combine(dir.FullName, "tests", "Antd.Wpf.UiHost", "bin", "Debug", "net8.0-windows", "Antd.Wpf.UiHost.exe");
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            return null;
        }

        private static void Check(string name, bool ok, string actual)
        {
            _checks++;
            if (!ok) _failures++;

            Console.WriteLine((ok ? "  . " : "  x ") + name + (ok ? string.Empty : "  (actual: " + actual + ")"));
            if (!ok) Console.WriteLine("  FAIL " + name + " (actual: " + actual + ")");
        }
    }
}
