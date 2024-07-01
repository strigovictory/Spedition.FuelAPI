using Microsoft.AspNetCore.Hosting;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Services.Print;

public class PrintExcelNotFoundCards<CardNotFoundPrintRequest> : PrintExcelBase<IPrint>
{
    public PrintExcelNotFoundCards(IWebHostEnvironment env)
       : base(env)
    {
    }

    public override UriSegment UriSegment => UriSegment.fuelcardsnotfound;
}
