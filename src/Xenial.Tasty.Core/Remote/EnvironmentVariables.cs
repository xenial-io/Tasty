namespace Xenial.Delicious.Remote
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "Cause they hit user land, we would break binary combat.")]
    public static class EnvironmentVariables
    {
        public static readonly string InteractiveMode = "TASTY_INTERACTIVE";
        public static readonly string InteractiveConnectionType = "TASTY_INTERACTIVE_CON_TYPE";
        public static readonly string InteractiveConnectionId = "TASTY_INTERACTIVE_CON_ID";
    }
}
