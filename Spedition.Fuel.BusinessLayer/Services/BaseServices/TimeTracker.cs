namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class TimeTracker : TimerBase
{
    protected void ServiceStart()
    {
        Start();
        Log.Information($"Запуск сервиса «{GetType().Name}». ", this);
    }

    protected void ServiceFinish()
    {
        Finish();
        Log.Information($"Сервис «{GetType().Name}» завершил работу. " +
            $"Продолжительность {PeriodString} ");
    }
}
