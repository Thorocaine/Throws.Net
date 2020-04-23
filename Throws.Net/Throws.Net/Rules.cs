using Microsoft.CodeAnalysis;

namespace Throws.Net
{
    internal static class Rules
    {
        const string Category = "Usage";

        static LocalizableResourceString GetString(string resourcesName)
        {
            var manager = Resources.ResourceManager;
            return new LocalizableResourceString(resourcesName, manager, typeof(Resources));
        }

        public static DiagnosticDescriptor CreateRule()
        {
            var title = GetString(nameof(Resources.AnalyzerTitle));
            var messageFormat = GetString(nameof(Resources.AnalyzerMessageFormat));
            var description = GetString(nameof(Resources.AnalyzerDescription));

            return new DiagnosticDescriptor(
                ThrowsNetAnalyzer.DiagnosticId,
                title,
                messageFormat,
                Category,
                DiagnosticSeverity.Error,
                true,
                description);
        }

        public static DiagnosticDescriptor CreateOverrideRule()
        {
            var title = GetString(nameof(Resources.OverrideAnalyzerTitle));
            var messageFormat = GetString(nameof(Resources.OverrideAnalyzerMessageFormat));
            var description = GetString(nameof(Resources.OverrideAnalyzerDescription));

            return new DiagnosticDescriptor(
                ThrowsNetAnalyzer.DiagnosticId,
                title,
                messageFormat,
                Category,
                DiagnosticSeverity.Error,
                true,
                description);
        }
    }
}