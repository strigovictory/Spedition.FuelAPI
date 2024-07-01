using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.RequestModels.Print;
using Spedition.Fuel.Shared.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Services.Print;

public class PrintExcelCards<CardPrintRequest> : PrintExcelBase<IPrint>
{
    public PrintExcelCards(IWebHostEnvironment env)
       : base(env)
    {
    }

    public override UriSegment UriSegment => UriSegment.fuelcards;
}
