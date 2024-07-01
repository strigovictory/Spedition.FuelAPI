namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class TimerBase
{
    private TimeSpan PeriodSeconds =>
        StartPeriod.HasValue && FinishPeriod.HasValue
        ? (FinishPeriod.Value - StartPeriod.Value)
        : default;

    protected string PeriodString =>
        PeriodSeconds != default
        ? (PeriodSeconds.Minutes > 0
        ? $"{PeriodSeconds.Minutes} мин. {PeriodSeconds.Seconds} сек."
        : $"{PeriodSeconds.Seconds} сек.")
        : string.Empty;

    private DateTime? StartPeriod { get; set; }

    protected string StartPeriodString =>
        StartPeriod?.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty;

    private DateTime? FinishPeriod { get; set; }

    protected string FinishPeriodString =>
        FinishPeriod?.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty;

    protected void Start()
    {
        StartPeriod = DateTime.Now;
    }

    protected void Finish()
    {
        FinishPeriod = DateTime.Now;
    }
}
