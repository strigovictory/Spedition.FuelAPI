using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using NPOI.SS.Formula.Functions;
using Spedition.Fuel.BusinessLayer.Enums;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Shared;
using Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Transport;
using Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Office;
using Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Shared;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.DataAccess.Infrastructure.Repositories;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.FuelAPI.Controllers;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Spedition.Fuel.Test.ControllersTests;

public class FuelParserControllerTests : TestsHelper
{
    public FuelParserControllerTests(ITestOutputHelper output)
    : base(output)
    {
    }

    private static Dictionary<int, (string monthlyFileName, string dailyFileName)> ReportsSettings =>
        new Dictionary<int, (string monthlyFileName, string dailyFileName)>
        {
            { 7, ( "Diesel24_Rezon_Test.xls", "Diesel24_Rezon_Test.xlsx" ) },
            { 9, ( "GPN_Райзинг_Test.xlsx", string.Empty ) },
            { 11, ( "BP_RezonTrans_Test.xlsx", string.Empty ) },
            { 17, ( "Лидер_Райзинг_Test.xlsx", string.Empty ) },
            { 18, ( "UniversalScaffold_МДКОйл_Рутранс_Test.xlsx", string.Empty) },
            { 19, ( "Adnoc_Райзинг_Test.xlsx", "Adnoc_Рокат_Test.XLSX") },
            { 21, ( "Helios_Рокат_Test.xlsx", string.Empty ) },
            { 23, ( "ДизельТранс_Райзинг_Test.xlsx", string.Empty) },
            //{ 24, ( "Камп_Райзинг_Test.xls", string.Empty ) },
            { 24, ( "Камп_Фестина_Инвест_Test.xls", string.Empty ) },
        };

    private FuelParserController GetController(int providerId)
    {
        // Arrange
        var mockFuelRepositories = GetFuelRepositories();

        ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction> parser = providerId switch
        {
            7 => new ParserDiesel24(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            9 => new ParserGazProm(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            11 => new ParserBP(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            17 => new ParserLider(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            18 => new ParserUniversalScaffold(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            19 => new ParserAdnoc(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            21 => new ParserHelios(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            23 => new ParserDieselTrans(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
            24 => new ParserKamp(Environment, mockFuelRepositories, Configuration, Mapper, null, null, null, null, null),
        };

        parser.ProviderId = providerId;

        var parsers = new List<ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>>
        {
            parser as ITransactionsParserBase<FuelTransactionShortResponse, NotParsedTransaction>,
        };

        return new FuelParserController(parsers);
    }

    private async Task UploadReportsTest(int providerId, ReportKind reportType, string directoryPath = "origin")
    {
        var controller = GetController(providerId);
        var reportsSettings = ReportsSettings[providerId];
        var fileName = reportType == ReportKind.monthly ? ReportsSettings[providerId].monthlyFileName : ReportsSettings[providerId].dailyFileName;
        directoryPath = reportType == ReportKind.monthly ? $"testsfiles/parser/{directoryPath}/monthly": $"testsfiles/parser/{directoryPath}/daily";
        var content = ReedFile(fileName, directoryPath);
        if (content.Length == 0) return;

        var report = new FuelReport(
            providerId: providerId,
            isMonthly: reportType == ReportKind.monthly ? true : false,
            fileName: fileName,
            content: content,
            userName: string.Empty);

        // Act
        var action = async () => await controller.UploadReport(report, default);

        // Assert
        var actionResult = Assert.IsAssignableFrom<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>>(await action.Invoke());

        Output.WriteLine($"Parsed file «{fileName ?? string.Empty}». ");

        Output.WriteLine($"Result message «{actionResult?.Result ?? string.Empty}». ");

        Output.WriteLine($"Parsed and added to DB {actionResult?.SuccessItems?.Count ?? 0} successItems: ");
        actionResult?.SuccessItems?.ForEach(transaction => Output.WriteLine(transaction?.ToString() ?? string.Empty));

        Output.WriteLine($"Parsed and updated in DB {actionResult?.SecondarySuccessItems?.Count ?? 0} successItems: ");
        actionResult?.SecondarySuccessItems?.ForEach(transaction => Output.WriteLine(transaction?.ToString() ?? string.Empty));

        Output.WriteLine($"Parsed {actionResult?.NotSuccessItems?.Count ?? 0} notsuccessItems: ");
        actionResult?.NotSuccessItems?.ForEach(transaction => Output.WriteLine(
            $"{transaction?.NotSuccessItem?.ToString() ?? string.Empty} - {transaction?.Reason?.ToString() ?? string.Empty}"));

        Assert.True((actionResult?.SuccessItems?.Count ?? 0) > 0
            || (actionResult?.SecondarySuccessItems?.Count ?? 0) > 0
            || (actionResult?.NotSuccessItems?.Count ?? 0) > 0);
    }

    [Fact]
    public async Task UploadEmptyReportTest()
    {
        var controller = GetController(11);

        //Act
        var action = async () => await controller.UploadReport(default, default);

        //Assert
        var actionResult = Assert.IsAssignableFrom<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>>(await action.Invoke());

        Assert.NotNull(actionResult);

        Assert.True((actionResult?.SuccessItems?.Count ?? 0) == 0
            && (actionResult?.SecondarySuccessItems?.Count ?? 0) == 0
            && (actionResult?.NotSuccessItems?.Count ?? 0) == 0
            && (actionResult?.Result?.Equals("Ошибка! Файл не был прикреплен!") ?? false));
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestBP()
    {
        await UploadReportsTest(11, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestBP()
    {
        await UploadReportsTest(11, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestAdnoc()
    {
        await UploadReportsTest(19, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestAdnoc()
    {
        await UploadReportsTest(19, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportDailyTestAdnoc()
    {
        await UploadReportsTest(19, ReportKind.daily);
    }

    [Fact]
    public async Task UploadModifiedReportDailyTestAdnoc()
    {
        await UploadReportsTest(19, ReportKind.daily, "modified");
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestHelios()
    {
        await UploadReportsTest(21, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestHelios()
    {
        await UploadReportsTest(21, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportTestMonthlyDiesel24()
    {
        await UploadReportsTest(7, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportTestMonthlyDiesel24()
    {
        await UploadReportsTest(7, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportTestDailyDiesel24()
    {
        await UploadReportsTest(7, ReportKind.daily);
    }

    [Fact]
    public async Task UploadModifiedReportTestDailyDiesel24()
    {
        await UploadReportsTest(7, ReportKind.daily, "modified");
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestGazProm()
    {
        await UploadReportsTest(9, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestGazProm()
    {
        await UploadReportsTest(9, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestKamp()
    {
        await UploadReportsTest(24, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestKamp()
    {
        await UploadReportsTest(24, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestLider()
    {
        await UploadReportsTest(17, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestLider()
    {
        await UploadReportsTest(17, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestUniversalScaffold()
    {
        await UploadReportsTest(18, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestUniversalScaffold()
    {
        await UploadReportsTest(18, ReportKind.monthly, "modified");
    }

    [Fact]
    public async Task UploadOriginReportMonthlyTestDieselTrans()
    {
        await UploadReportsTest(23, ReportKind.monthly);
    }

    [Fact]
    public async Task UploadModifiedReportMonthlyTestDieselTrans()
    {
        await UploadReportsTest(23, ReportKind.monthly, "modified");
    }
}