using Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Serialization.Json;
using Rebus.ServiceProvider;
using Warehouse.Data;
using Warehouse.EventBus.Idempotence;
using Warehouse.EventBus.Steps;

namespace Warehouse
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

            services.AddDbContextPool<WarehouseDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("OrderDatabase"));
                options.UseInternalServiceProvider(serviceProvider);
                options.EnableSensitiveDataLogging();
            });

            services.AddScoped<WarehouseDbContext>();
            
            services.AddTransient<IMessageTracker, MessageTracker>();

            services.AddRebus(x => x
                .Routing(r =>
                {
                    r.TypeBased().MapAssemblyDerivedFrom<OrderMessage>("Orders");
                })
                .Transport(t => t
                    .UseRabbitMq(Configuration.GetSection("RabbitMQConfig").GetValue<string>("BrokerConnectionString"), Configuration.GetSection("RabbitMQConfig").GetValue<string>("SubscriberName"))
                    .ExchangeNames(Configuration.GetSection("RabbitMQConfig").GetValue<string>("DirectName"), Configuration.GetSection("RabbitMQConfig").GetValue<string>("TopicName")))
                .Options(o =>
                {
                    o.SimpleRetryStrategy(errorQueueAddress: "WarehouseErrorQueue");
                    o.Decorate<IPipeline>(x =>
                    {
                        var pipeline = x.Get<IPipeline>();
                        var stepToInject = new DbTransactionStep();

                        return new PipelineStepInjector(pipeline)
                            .OnReceive(stepToInject, PipelineRelativePosition.Before, typeof(DispatchIncomingMessageStep));
                    });
                })
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
