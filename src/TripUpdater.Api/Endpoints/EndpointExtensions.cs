namespace TripUpdater.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpointGroup)) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpointGroup group)
            {
                group.Map(app);
            }
        }

        return app;
    }
}
