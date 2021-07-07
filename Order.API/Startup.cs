using Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.API.Data;
using Order.API.EventBus.Idempotence;
using Order.API.EventBus.Steps;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Serialization.Json;
using Rebus.ServiceProvider;

namespace Order.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkNpgsql();
            services.AddDbContextPool<OrderDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("OrderDatabase"));
                options.UseInternalServiceProvider(serviceProvider);
                options.EnableSensitiveDataLogging();
            });
            services.AddScoped<OrderDbContext>();

            services.AddTransient<IMessageTracker, MessageTracker>();

            services.AddRebus(x => x
                .Transport(t => t
                    .UseRabbitMq(Configuration.GetSection("RabbitMQConfig").GetValue<string>("BrokerConnectionString"), Configuration.GetSection("RabbitMQConfig").GetValue<string>("SubscriberName")))
                .Routing(r =>
                {
                    r.TypeBased()
                        .Map<OnNewOrder>("Orders")
                        .Map<ApproveOrder>("Orders")
                        .Map<RejectOrder>("Orders")
                        .Map<OrderingOlaBreached>("Orders")
                        .MapAssemblyDerivedFrom<WarehouseMessage>("Warehouse");
                })
                .Options(o =>
                {
                    o.SimpleRetryStrategy(errorQueueAddress: "OrdersErrorQueue");
                    o.Decorate<IPipeline>(x =>
                    {
                        var pipeline = x.Get<IPipeline>();
                        var stepToInject = new DbTransactionStep();

                        return new PipelineStepInjector(pipeline)
                            .OnReceive(stepToInject, PipelineRelativePosition.Before, typeof(DispatchIncomingMessageStep));
                    });
                })
                .Sagas(s => s.StoreInPostgres(Configuration.GetConnectionString("OrderDatabase"), "sagas", "sagaindex"))
                .Timeouts(s => s.StoreInPostgres(Configuration.GetConnectionString("OrderDatabase"), "timeouts"))
                .Serialization(x => x.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson)));

            services.AutoRegisterHandlersFromAssemblyOf<Startup>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ApplicationServices.UseRebus();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
