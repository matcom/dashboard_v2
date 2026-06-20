using TemplateGen.Core.Base;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class Anexo1Template : ExcelTemplateBase
{
    protected override string OutputPath =>
        "../Dashboard_v2/src/Infrastructure/Templates/Anexo1.xlsx";

    protected override IEnumerable<ISheetTemplate> GetSheets()
    {
        yield return new Anexo1RedesSheet();
        yield return new PatentesRegistrosSheet();
        yield return new NuevosProductosSheet();
        yield return new Anexo1PremiosSheet();
        yield return new PonenciasEventosSheet();
        yield return new PonenciasActividadesSheet();
        yield return new CelebracionEventosSheet();
        yield return new ProyectosDLSheet();
        yield return new Anexo1PublicacionesSheet();
        yield return new PublicacionesIndicesSheet();
    }
}
