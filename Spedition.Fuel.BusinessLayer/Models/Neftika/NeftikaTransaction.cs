using NPOI.SS.Formula.Functions;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.Neftika;

public class NeftikaTransaction : NeftikaError, IParsedItem
{
    /// <summary>
    /// 1646991
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// "01323"
    /// </summary>
    public string ClientCode { get; set; }

    /// <summary>
    /// "433-Г"
    /// </summary>
    public string ClientFullName { get; set; }

    /// <summary>
    /// 3
    /// </summary>
    public int RegionID { get; set; }

    /// <summary>
    /// "Гомель"
    /// </summary>
    public string RegionName { get; set; }

    /// "000220366"
    public string Card { get; set; }

    /// <summary>
    /// -900.00
    /// </summary>
    public decimal Liters { get; set; }

    /// <summary>
    /// -43200.00 // общая сумма нетто
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// "RUB"
    /// </summary>
    public string Currency { get; set; }

    /// "RU36" // код азс - по нему находим страну
    public string UIDAZS { get; set; }

    /// <summary>
    /// "2022-05-05T23:07:33" местное время момента заправки (как в чеке)
    /// </summary>
    public DateTime Date { get; set; }

    ///  "2022-05-05T23:08:41.15"
    public DateTime DateAdd { get; set; }

    /// <summary>
    /// 6
    /// </summary>
    public int ServiceNT { get; set; }

    /// <summary>
    /// "ДТ Лето" // наименование услуги
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    /// 48.00 // цена нетто
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 154355
    /// </summary>
    public int Bill { get; set; }

    /// 52.37 // цена брутто (с учетов включенных налогов/наценок)
    public decimal PriceOriginal { get; set; }

    /// <summary>
    /// "RUB" // код валюты
    /// </summary>
    public string CurrencyOriginal { get; set; }

    /// <summary>
    /// -47133.0000  // общая сумма брутто (с учетов включенных налогов/наценок)
    /// </summary>
    public decimal AmountOriginal { get; set; }

    /// <summary>
    /// "433-Г"
    /// </summary>
    public string OfferID { get; set; }

    /// <summary>
    /// "АЗС №36 Смоленская область, Краснинский район, 454 км а/магистрали Москва-Минск"
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 1324
    /// </summary>
    public int ManagerID { get; set; }

    /// <summary>
    /// "Дундарова Эльвира"
    /// </summary>
    public string ManagerName { get; set; }

    /// <summary>
    ///  "2022-05-06T02:07:33"
    /// </summary>
    public DateTime DateGMT { get; set; }

    /// <summary>
    /// 3
    /// </summary>
    public int GMT { get; set; }

    /// 1
    public int CardTypeId { get; set; }

    /// <summary>
    /// 1
    /// </summary>
    public int StationTypeId { get; set; }

    /// <summary>
    /// null
    /// </summary>
    public string CardHolderInfo { get; set; }

    /// <summary>
    /// null
    /// </summary>
    public string CardDescription { get; set; }

    /// <summary>
    /// "1"
    /// </summary>
    public string ProvideFormatId { get; set; }

    /// <summary>
    ///  "CardPresent"
    /// </summary>
    public string ProvideFormatName { get; set; }

    public override string ToString()
    {
        return $"Транзакция " +
            $"от {Date}, " +
            $"количество: {Liters}, " +
            $"цена: {Price.FormatDecimalToString() ?? string.Empty}, " +
            $"сумма: {AmountOriginal.FormatDecimalToString() ?? string.Empty}, " +
            $"услуга: {ServiceName ?? string.Empty}, " +
            $"место заправки: {Address ?? string.Empty}, " +
            $"карта: {Card}, " +
            $"идентификатор: {ID}. ";
    }
}
