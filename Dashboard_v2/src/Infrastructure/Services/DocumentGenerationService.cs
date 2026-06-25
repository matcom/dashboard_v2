using ClosedXML.Report;
using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Infrastructure.Services;

public sealed class DocumentRenderer : IDocumentRenderer
{
    private const string ResourcePrefix = "Dashboard_v2.Infrastructure.Templates.";
    private readonly IReadOnlyDictionary<string, ICustomDocumentRenderer> _customRenderers;

    public DocumentRenderer(IEnumerable<ICustomDocumentRenderer> customRenderers)
    {
        _customRenderers = customRenderers.ToDictionary(r => r.TemplateName, StringComparer.OrdinalIgnoreCase);
    }

    public byte[] Render(string templateName, IReadOnlyDictionary<string, object> variables)
    {
        using var templateStream = LoadEmbeddedTemplate(templateName);

        if (_customRenderers.TryGetValue(templateName, out var custom))
            return custom.Render(templateStream, variables);

        var template = new XLTemplate(templateStream);
        foreach (var (name, value) in variables)
            template.AddVariable(name, value);
        template.Generate();

        using var output = new MemoryStream();
        template.SaveAs(output);
        return output.ToArray();
    }

    private static Stream LoadEmbeddedTemplate(string templateName)
    {
        var resourceName = $"{ResourcePrefix}{templateName}.xlsx";
        var assembly = typeof(DocumentRenderer).Assembly;
        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Plantilla embebida '{resourceName}' no encontrada. " +
                $"Verifica que Infrastructure/Templates/{templateName}.xlsx exista " +
                "y esté marcada como EmbeddedResource en Infrastructure.csproj.");
    }
}
