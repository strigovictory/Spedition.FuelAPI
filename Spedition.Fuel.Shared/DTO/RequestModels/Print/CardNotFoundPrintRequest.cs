using System.ComponentModel;
using Spedition.Fuel.Shared.Interfaces;

namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

[DisplayName("Не обнаруженные топливные карты")]
public class CardNotFoundPrintRequest : IPrint
{
    [DisplayName("Идентификатор в БД")]
    public int Id { get; set; }

    [DisplayName("Томер карты")]
    public string Number { get; set; }

    [DisplayName("Пользователь")]
    public string UserName { get; set; }

    [DisplayName("Дата импорта отчета")]
    public DateTime ImportDate { get; set; }

    [DisplayName("Провайдер")]
    public string FuelProviderName { get; set; }
}
