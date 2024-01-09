using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace sns_otel_1;

public class Function
{
    private const string serviceName = "sns-otel-1";
    private static SpanContext _previousSpanContext;

    private static Tracer? _tracer;
    private static readonly ActivitySource MyActivitySource = new(serviceName);

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {

    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SNS event object and can be used
    /// to respond to SNS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SNSEvent evnt, ILambdaContext context)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0"))
            .AddConsoleExporter()
            .Build();

        _tracer = tracerProvider.GetTracer(serviceName + "-dedup-postfix");
        using (var span = _tracer.StartActiveSpan("ActivityOne"))
        {
            var otherLibraryTracer = tracerProvider.GetTracer("OtherLibrary", version: "4.0.0");
            using (var otherSpan = otherLibraryTracer.StartActiveSpan("ActivityTwo"))
            {
                Console.WriteLine($"[Main] Started span with span_id: {span.Context.SpanId}");
            }
        }
        using (var childActivity = MyActivitySource.StartActivity("ActivityThree"))
        {
            foreach(var record in evnt.Records)
            {
                await ProcessRecordAsync(record, context);
            }
        }
    }


    private async Task ProcessRecordAsync(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processed record {record.Sns.Message}");

        // TODO: Do interesting work based on the new message
        await Task.CompletedTask;
    }
}
