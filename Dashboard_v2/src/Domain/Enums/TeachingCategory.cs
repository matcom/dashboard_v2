namespace Dashboard_v2.Domain.Entities;

public enum TeachingCategory
{
    None = 0,
    Titular = 1,
    Auxiliar = 2,
    Asistente = 3,
    Instructor = 4,
}

public static class TeachingCategoryExtensions
{
    public static string ToDisplayString(this TeachingCategory category)
    {
        return category switch
        {
            TeachingCategory.None => "Sin categoría docente",
            TeachingCategory.Titular => " Profesor Titular",
            TeachingCategory.Auxiliar => "Profesor Auxiliar",
            TeachingCategory.Asistente => "Profesor Asistente",
            TeachingCategory.Instructor => "Instructor",
            _ => "Desconocida"
        };
    }
}