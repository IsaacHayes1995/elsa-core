﻿using Elsa.Extensions;
using Elsa.Mediator.Contracts;
using Elsa.Telnyx.Attributes;
using Elsa.Telnyx.Bookmarks;
using Elsa.Telnyx.Events;
using Elsa.Telnyx.Extensions;
using Elsa.Telnyx.Payloads.Abstract;
using Elsa.Workflows.Core.Contracts;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Runtime.Contracts;
using JetBrains.Annotations;

namespace Elsa.Telnyx.Handlers;

/// <summary>
/// Resumes all workflows blocked on activities that are waiting for a given webhook.
/// </summary>
[PublicAPI]
internal class TriggerWebhookDrivenActivities : INotificationHandler<TelnyxWebhookReceived>
{
    private readonly IWorkflowRuntime _workflowRuntime;
    private readonly IActivityRegistry _activityRegistry;

    public TriggerWebhookDrivenActivities(IWorkflowRuntime workflowRuntime, IActivityRegistry activityRegistry)
    {
        _workflowRuntime = workflowRuntime;
        _activityRegistry = activityRegistry;
    }

    public async Task HandleAsync(TelnyxWebhookReceived notification, CancellationToken cancellationToken)
    {
        var webhook = notification.Webhook;
        var eventType = webhook.Data.EventType;
        var eventPayload = webhook.Data.Payload;
        var input = new Dictionary<string, object>().AddInput(eventPayload.GetType().Name, eventPayload);
        var activityDescriptors = FindActivityDescriptors(eventType).ToList();
        var correlationId = ((Payload)webhook.Data.Payload).GetCorrelationId();
        var bookmarkPayload = new WebhookEventBookmarkPayload(eventType);

        foreach (var activityDescriptor in activityDescriptors)
            await _workflowRuntime.ResumeWorkflowsAsync(activityDescriptor.TypeName, bookmarkPayload, new TriggerWorkflowsRuntimeOptions(correlationId, Input: input), cancellationToken);
    }

    private IEnumerable<ActivityDescriptor> FindActivityDescriptors(string eventType) =>
        _activityRegistry.FindMany(descriptor => descriptor.GetAttribute<WebhookDrivenAttribute>()?.EventTypes.Contains(eventType) == true);
}