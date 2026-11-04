namespace Antd.Wpf.Controls
{
    /// <summary>antd component size: small / middle / large.</summary>
    public enum AntdSize
    {
        Small = 0,
        Middle = 1,
        Large = 2
    }

    /// <summary>Validation feedback status.</summary>
    public enum AntdStatus
    {
        None = 0,
        Error = 1,
        Warning = 2
    }

    /// <summary>Button types, matching antd's `type` prop plus the text/link variants.</summary>
    public enum ButtonVariant
    {
        Default = 0,
        Primary = 1,
        Dashed = 2,
        Text = 3,
        Link = 4
    }

    /// <summary>Button shapes.</summary>
    public enum ButtonShape
    {
        Default = 0,
        Circle = 1,
        Round = 2
    }

    /// <summary>Where a floating panel sits relative to its trigger.</summary>
    public enum AntdPlacement
    {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3
    }

    /// <summary>When a floating panel opens.</summary>
    public enum AntdTrigger
    {
        /// <summary>Opens on pointer hover, like antd's Tooltip.</summary>
        Hover = 0,

        /// <summary>Opens on click, like antd's Dropdown and Popover.</summary>
        Click = 1
    }

    /// <summary>How an AutoComplete narrows its options while typing.</summary>
    public enum AntdFilterMode
    {
        /// <summary>Match anywhere in the option, antd's default.</summary>
        Contains = 0,

        /// <summary>Match only from the start of the option.</summary>
        StartsWith = 1,

        /// <summary>Ignore the typed text and always offer every option.</summary>
        None = 2
    }

    /// <summary>Where the icon sits relative to the label.</summary>
    public enum ButtonIconPlacement
    {
        Start = 0,
        End = 1
    }
}
