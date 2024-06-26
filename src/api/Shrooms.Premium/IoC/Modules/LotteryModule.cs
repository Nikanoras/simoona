﻿using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.DomainServiceValidators.Lotteries;
using Shrooms.Premium.Domain.Services.Email.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;

namespace Shrooms.Premium.IoC.Modules
{
    public class LotteryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LotteryService>()
                .As<ILotteryService>()
                .InstancePerRequest()
                .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<LotteryParticipantService>()
                .As<ILotteryParticipantService>()
                .InstancePerRequest()
                .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<LotteryExportService>()
                 .As<ILotteryExportService>()
                 .InstancePerRequest()
                 .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<LotteryAbortJob>()
                .As<ILotteryAbortJob>()
                .EnableInterfaceTelemetryInterceptor();

            builder.RegisterType<LotteryNotificationService>()
                .As<ILotteryNotificationService>()
                .InstancePerRequest();

            builder.RegisterType<LotteryValidator>()
                .As<ILotteryValidator>()
                .InstancePerRequest();
        }
    }
}
