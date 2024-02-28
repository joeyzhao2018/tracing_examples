package main

import (
	"context"
	"fmt"

	// ddlambda "github.com/DataDog/datadog-lambda-go"
	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	"go.opentelemetry.io/otel/trace"

	// ddtracer "gopkg.in/DataDog/dd-trace-go.v1/ddtrace/tracer"
	"go.opentelemetry.io/otel"
	ddotel "gopkg.in/DataDog/dd-trace-go.v1/ddtrace/opentelemetry"
	ddtracer "gopkg.in/DataDog/dd-trace-go.v1/ddtrace/tracer"
)

var tracer trace.Tracer

func Handler(ctx context.Context, snsEvent events.SNSEvent) {
	// s, _ := ddtracer.StartSpanFromContext(ctx, "dd-serverless-tracer-span") // dd tracer
	// defer s.Finish() // dd tracer

	provider := ddotel.NewTracerProvider(
		ddtracer.WithService("aws.lambda"),
		ddtracer.WithLambdaMode(false),
		ddtracer.WithGlobalTag("_dd.origin","lambda"),
		ddtracer.WithSendRetries(2),
	)  //otel
	defer provider.Shutdown() // otel

	otel.SetTracerProvider(provider) //otel
	tracer = otel.Tracer("otel-tracer-provider") //otel

	_, span := tracer.Start(ctx, "hello-span")  //otel

	for _, record := range snsEvent.Records {
        snsRecord := record.SNS
        fmt.Printf("[%s %s] Message = %s", record.EventSource, snsRecord.Timestamp, snsRecord.Message)
    }

	span.End()  //otel
	// provider.ForceFlush(time.Minute, func(ok bool) {fmt.Printf(" ForceFlush = %v", ok)});  //otel
}

// func main() {
//     lambda.Start(ddlambda.WrapHandler(Handler,nil))
// }

func main() {
    lambda.Start(Handler)
}
