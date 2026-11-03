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

    /// <summary>Where the icon sits relative to the label.</summary>
    public enum ButtonIconPlacement
    {
        Start = 0,
        End = 1
    }
}
