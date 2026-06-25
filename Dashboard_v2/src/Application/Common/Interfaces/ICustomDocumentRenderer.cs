namespace Dashboard_v2.Application.Common.Interfaces;

public interface ICustomDocumentRenderer
{
    string TemplateName { get; }
    byte[] Render(Stream templateStream, IReadOnlyDictionary<string, object> variables);
}
