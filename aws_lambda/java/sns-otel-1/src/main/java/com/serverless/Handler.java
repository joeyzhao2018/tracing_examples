package com.serverless;

import io.opentelemetry.api.OpenTelemetry;
import io.opentelemetry.api.trace.Span;
import io.opentelemetry.api.trace.Tracer;
import io.opentelemetry.context.Scope;
import io.opentelemetry.sdk.OpenTelemetrySdk;
import io.opentelemetry.sdk.resources.Resource;
import io.opentelemetry.semconv.resource.attributes.ResourceAttributes;
import org.apache.log4j.Logger;
import io.opentelemetry.sdk.trace.SdkTracerProvider;
//import io.opentelemetry.sdk.autoconfigure.AutoConfiguredOpenTelemetrySdk;

import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.amazonaws.services.lambda.runtime.events.SNSEvent;

public class Handler implements RequestHandler<SNSEvent, Void> {

	private Resource resource = Resource.getDefault().toBuilder().put(ResourceAttributes.SERVICE_NAME, "sns-otel-1").put(ResourceAttributes.SERVICE_VERSION, "0.1.0").build();

//	private  OpenTelemetrySdk sdk = AutoConfiguredOpenTelemetrySdk.initialize()
//			.getOpenTelemetrySdk();
	private SdkTracerProvider sdkTracerProvider = SdkTracerProvider.builder()
			.setResource(resource)
			.build();

	private OpenTelemetry otel = OpenTelemetrySdk.builder().setTracerProvider(sdkTracerProvider).buildAndRegisterGlobal();
	private static final Logger LOG = Logger.getLogger(Handler.class);
	private Tracer tracer = otel.getTracer("instrumentation-scope-test", "instrumentation-scope-version");
	@Override
	public Void handleRequest(SNSEvent event, Context context) {
		Span span = this.tracer.spanBuilder("sns-handling").startSpan();

		try (Scope scope = span.makeCurrent()) {
			LOG.info("received: " + event);
			return null;
		} catch(Throwable t) {
			span.recordException(t);
			throw t;
		} finally {
			span.end();
		}
	}
}
