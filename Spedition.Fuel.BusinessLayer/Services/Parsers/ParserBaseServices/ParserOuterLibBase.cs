using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Helpers;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.Shared.Attributes;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

namespace Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;

public abstract class ParserOuterLibBase<TParsed, TSuccess, TNotSuccess, TSearch> 
    : MapperBase<TParsed, TSuccess, TNotSuccess, TSearch>
    where TParsed : class, IParsedItem
{
    private readonly OuterLibrary outerLibrary;

    public ParserOuterLibBase(
        IWebHostEnvironment env,
        FuelRepositories fuelRepositories,
        IConfiguration configuration,
        IMapper mapper,
        OuterLibrary outerLibrary)
        : base(env, fuelRepositories, configuration, mapper)
    {
        this.outerLibrary = outerLibrary;
    }

    protected Type TItemType { get; set; } = typeof(TParsed);

    protected OuterLibrary OuterLibraryValue => outerLibrary;

    /// <summary>
    /// Библиотека из ParserOuterLibBase может конвертировать в эти форматы.
    /// </summary>
    protected virtual List<string> FilesFormatsRequired =>
        outerLibrary switch
        {
            OuterLibrary.EPPlus => new List<string> { "." + FilesExtension.xlsx.ToString() },
            OuterLibrary.EPPlusShort => new List<string> { "." + FilesExtension.xlsx.ToString() },
            OuterLibrary.NPOI => new List<string> { "." + FilesExtension.xls.ToString(), "." + FilesExtension.xlsx.ToString() },
            _ => new()
        };

    /// <summary>
    /// Число страниц в книге excel.
    /// </summary>
    protected int NumberWorksheets { get; set; } = 0;

    /// <summary>
    /// Разновидность отчета - ежедневный / ежемесячный.
    /// </summary>
    private ReportKind ReportKindValue => IsMonthly.HasValue ? (IsMonthly.Value ? ReportKind.monthly : ReportKind.daily) : ReportKind.daily;

    /// <summary>
    /// Коллекция информации по свойствам Т-модели, к которым применен атрибут NameAttribute.
    /// </summary>
    protected List<PropertyInfo> ReportsItemsNameAttrProps =>
        TItemType?.IsAbstract ?? false
        ? TItemType.Assembly?.GetTypes()?.Where(ttype => ttype.IsSubclassOf(TItemType) && ttype.IsClass && !ttype.IsAbstract)?
        .SelectMany(ttype => ttype.GetProperties()?.Where(y => y.CustomAttributes.Any(z => z.AttributeType == typeof(NameAttribute))))?.Distinct()?.ToList() ?? new()
        : TItemType?.GetProperties()?.Where(_ => _.CustomAttributes.Any(_ => _.AttributeType == typeof(NameAttribute)))?.ToList() ?? new();

    /// <summary>
    /// Коллекция, сопоставляющая свойство модели c наименованием столбца в заголовке таблицы в отчете.
    /// </summary>
    protected virtual Dictionary<PropertyInfo, string> ComparisonPropertyToColumnsName =>
        ReportsItemsNameAttrProps?.ToDictionary(prop => prop, prop => TItemType.GetNameAttributeValue(ReportKindValue, prop.Name));

    protected List<string> ColumnsNames => ComparisonPropertyToColumnsName?.Select(prop => prop.Value)?.ToList() ?? new();

    /// <summary>
    /// Коллекция-словарь, сопоставляющая свойство модели с номером столбца из таблицы в отчете.
    /// </summary>
    protected Dictionary<PropertyInfo, int> ComparisonPropertyToColumnsNumber { get; set; } = new();

    public abstract Task ParseFile(UploadedContent content, CancellationToken token = default);

    protected delegate void ChoiceOuterLibDelegate();

    protected ChoiceOuterLibDelegate choiceOuterLib;

    protected abstract void ChoiceOuterLib();

    protected virtual void ProcessingFile()
    {
        choiceOuterLib?.Invoke();
    }
}
