namespace Dashboard_v2.Domain.Entities;

public enum ScientificCategory
{
    None = 0,
    Licenciado = 1,
    Master = 2,
    Doctor = 3,
    //Other = 4
}

public static class ScientificCategoryExtensions
{
    public static string ToDisplayString(this ScientificCategory category)
    {
        return category switch
        {
            ScientificCategory.None => "Sin categoría científica",
            ScientificCategory.Licenciado => "Licenciado",
            ScientificCategory.Master => "Master",
            ScientificCategory.Doctor => "Doctor",
            //ScientificCategory.Other => "Otro",
            _ => "Desconocido"
        };
    }
}