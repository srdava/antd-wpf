using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System.Runtime.InteropServices;
using FlaUI.UIA3;

namespace Antd.Wpf.UiTests
{
    /// <summary>
    /// Clicks the real window. The other suites set properties directly, which is why "the Select
    /// never opened on click" stayed green for three releases; this suite exists to make that class
    /// of defect fail.
    ///
    /// Real input goes to whatever window is foreground, so every interaction starts by proving the
    /// host really is in front - otherwise the click lands in another app and the assertion becomes
    /// meaningless (that is exactly how the first run failed).
    /// </summary>
    internal static class Native
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        public static IntPtr Foreground() { return GetForegroundWindow(); }

        public static void SetForeground(IntPtr handle) { SetForegroundWindow(handle); }
    }
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
                    app.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(15));
                    Window window = app.GetMainWindow(automation, TimeSpan.FromSeconds(15));
                    if (window == null)
                    {
                        Console.WriteLine("FAIL could not find the main window");
                        return 1;
                    }

                    Check("host window comes to the foreground", Activate(window, app), "still background");

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

        /// <summary>Brings the host to the front and waits until Windows agrees it is there.</summary>
        private static bool Activate(Window window, Application app)
        {
            IntPtr handle = app.MainWindowHandle;

            for (int attempt = 0; attempt < 12; attempt++)
            {
                try
                {
                    window.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Normal);
                    window.SetForeground();
                    window.Focus();
                }
                catch
                {
                    // Patterns are not always available right after launch; the retry covers it.
                }

                Native.SetForeground(handle);
                Thread.Sleep(200);

                if (Native.Foreground() == handle && !window.IsOffscreen)
                {
                    Thread.Sleep(150);
                    return true;
                }
            }

            return false;
        }

        private static void CheckSelectClicks(Window window, UIA3Automation automation)
        {
            AutomationElement element = window.FindFirstDescendant(cf => cf.ByAutomationId("StatusSelect"));
            Check("host exposes the select", element != null, element == null ? "missing" : "ok");
            if (element == null) return;

            // A real click: this is what users do and what was broken.
            element.Click();
            Thread.Sleep(400);

            bool opened = WaitFor(delegate
            {
                return automation.GetDesktop().FindFirstDescendant(cf => cf.ByName("已停止")) != null;
            }, 2000);

            Check("clicking the field opens the dropdown", opened, opened ? "open" : "no popup on screen");
            if (!opened) return;

            AutomationElement item = automation.GetDesktop().FindFirstDescendant(cf => cf.ByName("运行中"));
            Check("dropdown exposes its items", item != null, item == null ? "missing" : "ok");
            if (item == null) return;

            item.Click();
            Thread.Sleep(350);

            AutomationElement value = window.FindFirstDescendant(cf => cf.ByAutomationId("StatusValue"));
            string text = value == null ? "<missing>" : value.Name;
            Check("picking an item updates the field", text == "运行中", text);
        }

        private static void CheckModalConfirm(Window window, UIA3Automation automation)
        {
            AutomationElement button = window.FindFirstDescendant(cf => cf.ByAutomationId("ConfirmButton"));
            Check("host exposes the confirm button", button != null, button == null ? "missing" : "ok");
            if (button == null) return;

            button.Click();
            Thread.Sleep(600);

            AutomationElement ok = automation.GetDesktop().FindFirstDescendant(cf => cf.ByName("确定"));
            Check("confirm dialog shows an OK button", ok != null, ok == null ? "missing" : "ok");
            if (ok == null) return;

            ok.Click();
            Thread.Sleep(400);

            AutomationElement result = window.FindFirstDescendant(cf => cf.ByAutomationId("ResultText"));
            string text = result == null ? "<missing>" : result.Name;
            Check("confirm callback runs after a real click", text == "ok", text);
        }

        private static bool WaitFor(Func<bool> condition, int timeoutMs)
        {
            int waited = 0;
            while (waited < timeoutMs)
            {
                if (condition()) return true;
                System.Threading.Thread.Sleep(100);
                waited += 100;
            }
            return false;
        }

        private static string ResolveHost(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0])) return args[0];

            DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
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
