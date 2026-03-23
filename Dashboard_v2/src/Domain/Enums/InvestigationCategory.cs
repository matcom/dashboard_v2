namespace Dashboard_v2.Domain.Entities;

public enum InvestigationCategory
{
    None = 0,
    Titular = 1,
    Auxiliar = 2,
    Agregado = 3,
    Asociado = 4,
}

public static class InvestigationCategoryExtensions
{
    public static string ToDisplayString(this InvestigationCategory category)
    {
        return category switch
        {
            InvestigationCategory.None => "Sin categoría de investigación",
            InvestigationCategory.Titular => "Investigador Titular",
            InvestigationCategory.Auxiliar => "Investigador Auxiliar",
            InvestigationCategory.Agregado => "Investigador Agregado",
            InvestigationCategory.Asociado => "Investigador Asociado",
            _ => "Desconocida"
        };
    }
}