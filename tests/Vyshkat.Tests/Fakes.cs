using Vyshkat.Domain;
using Vyshkat.Services;

namespace Vyshkat.Tests;

// Подставные службы техосмотра: решение не зависит от реальных правил.
public class AlwaysAcceptServiceCenter : IServiceCenter
{
    public InspectionResult Inspect(Transport transport) =>
        new(true, "Принято тестовой службой.");
}

public class AlwaysRejectServiceCenter : IServiceCenter
{
    public InspectionResult Inspect(Transport transport) =>
        new(false, "Отклонено: имитация брака.");
}
