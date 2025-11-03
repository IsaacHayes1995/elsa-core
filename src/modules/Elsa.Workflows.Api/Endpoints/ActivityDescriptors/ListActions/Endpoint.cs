using Elsa.Abstractions;
using Elsa.Workflows.Management;
using JetBrains.Annotations;

namespace Elsa.Workflows.Api.Endpoints.ActivityDescriptors.ListActions;

[PublicAPI]
internal class List(IActivityRegistry registry, IActivityRegistryPopulator registryPopulator) : ElsaEndpointWithoutRequest<ListTriggers.Response>
{
    public override void Configure()
    {
        Get("/descriptors/actions");
        ConfigurePermissions("read:*", "read:activity-descriptors");
    }

    public override async Task<ListTriggers.Response> ExecuteAsync(CancellationToken cancellationToken)
    {
        var forceRefresh = Query<bool>("refresh", false);

        if (forceRefresh)
            await registryPopulator.PopulateRegistryAsync(cancellationToken);

        var descriptors = registry.ListAll().Where(x => !x.Namespace.StartsWith("Elsa") && x.Kind == ActivityKind.Action).ToList();
        var response = new ListTriggers.Response(descriptors);

        return response;
    }
}