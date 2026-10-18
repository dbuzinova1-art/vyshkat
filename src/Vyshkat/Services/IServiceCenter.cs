using Vyshkat.Domain;

namespace Vyshkat.Services;

public record InspectionResult(bool IsApproved, string Reason);

public interface IServiceCenter
{
    InspectionResult Inspect(Transport transport);
}

public class StrictServiceCenter : IServiceCenter
{
    public InspectionResult Inspect(Transport transport)
    {
        if (transport is IPowerConsumer consumer && consumer.DailyEnergyKwh <= 0.0)
        {
            return new InspectionResult(false, "Техосмотр провален: аккумулятор разряжен (0 кВт·ч).");
        }

        return new InspectionResult(true, "Техосмотр пройден успешно.");
    }
}
