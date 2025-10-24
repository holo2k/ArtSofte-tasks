using MassTransit;
using OrderService.Application.Activities;
using OrderService.Application.Messages;

namespace OrderService.API.Activities;

public class ReserveInventoryActivity : IExecuteActivity<ReserveInventoryArguments>
{
    public async Task<ExecutionResult> Execute(ExecuteContext<ReserveInventoryArguments> context)
    {
        Console.WriteLine("Reserve");

        await context.Publish(new InventoryReserved(
            context.Arguments.OrderId,
            context.Arguments.ProductId,
            context.Arguments.Quantity));

        return context.Completed();
    }
}