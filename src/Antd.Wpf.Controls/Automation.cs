using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;

namespace Antd.Wpf.Controls
{
    /// <summary>
    /// Automation peers for the controls that do not inherit a WPF control with a built-in peer.
    /// Without these a screen reader sees nothing at all: <see cref="UIElement.OnCreateAutomationPeer"/>
    /// returns null by default, so the whole custom control surface is silent.
    /// </summary>
    internal abstract class AntdAutomationPeer : FrameworkElementAutomationPeer
    {
        protected AntdAutomationPeer(FrameworkElement owner)
            : base(owner)
        {
        }

        /// <summary>Fallback spoken name when the host did not set AutomationProperties.Name.</summary>
        protected abstract string AutomationName { get; }

        protected override string GetNameCore()
        {
            string name = base.GetNameCore();
            if (!string.IsNullOrEmpty(name)) return name;

            string fallback = AutomationName;
            return fallback ?? string.Empty;
        }

        protected override bool IsEnabledCore()
        {
            var element = Owner as UIElement;
            return element == null || element.IsEnabled;
        }
    }

    /// <summary>Button: reports ControlType.Button and answers the Invoke pattern.</summary>
    internal sealed class AntdButtonAutomationPeer : AntdAutomationPeer, IInvokeProvider
    {
        public AntdButtonAutomationPeer(Button owner)
            : base(owner)
        {
        }

        protected override string AutomationName
        {
            get
            {
                var button = Owner as Button;
                if (button == null) return null;

                object content = button.Content;
                string text = content as string;
                if (text != null) return text;
                return content == null ? null : content.ToString();
            }
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        protected override string GetClassNameCore()
        {
            return "Button";
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Invoke
                ? this
                : base.GetPattern(patternInterface);
        }

        public void Invoke()
        {
            var button = Owner as Button;
            if (button == null || !button.IsEnabled) return;

            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        }
    }

    /// <summary>Alert announces like a status line, named after its message.</summary>
    internal sealed class AlertAutomationPeer : AntdAutomationPeer
    {
        public AlertAutomationPeer(Alert owner)
            : base(owner)
        {
        }

        protected override string AutomationName
        {
            get
            {
                var alert = Owner as Alert;
                if (alert == null) return null;
                return string.IsNullOrEmpty(alert.Message) ? alert.Description : alert.Message;
            }
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Text;
        }

        protected override string GetClassNameCore()
        {
            return "Alert";
        }
    }

    /// <summary>Result page: a text landmark named after its title.</summary>
    internal sealed class ResultAutomationPeer : AntdAutomationPeer
    {
        public ResultAutomationPeer(Result owner)
            : base(owner)
        {
        }

        protected override string AutomationName
        {
            get
            {
                var result = Owner as Result;
                if (result == null) return null;
                return string.IsNullOrEmpty(result.Title) ? result.SubTitle : result.Title;
            }
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Text;
        }

        protected override string GetClassNameCore()
        {
            return "Result";
        }
    }

    /// <summary>Tag reads its own content.</summary>
    internal sealed class TagAutomationPeer : AntdAutomationPeer
    {
        public TagAutomationPeer(Tag owner)
            : base(owner)
        {
        }

        protected override string AutomationName
        {
            get
            {
                var tag = Owner as Tag;
                return tag == null || tag.Content == null ? null : tag.Content.ToString();
            }
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Text;
        }

        protected override string GetClassNameCore()
        {
            return "Tag";
        }
    }

    /// <summary>Modal and Drawer: a pane announced by its title.</summary>
    internal sealed class DialogAutomationPeer : AntdAutomationPeer
    {
        public DialogAutomationPeer(FrameworkElement owner)
            : base(owner)
        {
        }

        protected override string AutomationName
        {
            get
            {
                var modal = Owner as Modal;
                if (modal != null) return modal.Title == null ? null : modal.Title.ToString();

                var drawer = Owner as Drawer;
                if (drawer != null) return drawer.Title == null ? null : drawer.Title.ToString();

                return null;
            }
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Pane;
        }

        protected override string GetClassNameCore()
        {
            var modal = Owner as Modal;
            return modal != null ? "Modal" : "Drawer";
        }
    }
}
