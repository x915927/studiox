﻿using System.Transactions;
using StudioX.Domain.Uow;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Xunit;

namespace StudioX.Tests.Domain.Uow
{
    public class UnitOfWorkManagerTests : TestBaseWithLocalIocManager
    {
        [Fact]
        public void ShouldCallUowMethods()
        {
            var fakeUow = Substitute.For<IUnitOfWork>();

            LocalIocManager.IocContainer.Register(
                Component.For<IUnitOfWorkDefaultOptions>().ImplementedBy<UnitOfWorkDefaultOptions>().LifestyleSingleton(),
                Component.For<IUnitOfWorkManager>().ImplementedBy<UnitOfWorkManager>().LifestyleSingleton(),
                Component.For<IUnitOfWork>().Instance(fakeUow).LifestyleSingleton(),
#if NET46
                Component.For<ICurrentUnitOfWorkProvider>().ImplementedBy<CallContextCurrentUnitOfWorkProvider>().LifestyleSingleton()
#else
                Component.For<ICurrentUnitOfWorkProvider>().ImplementedBy<AsyncLocalCurrentUnitOfWorkProvider>().LifestyleSingleton()
#endif
                );

            var uowManager = LocalIocManager.Resolve<IUnitOfWorkManager>();

            //Starting the first uow
            using (var uow1 = uowManager.Begin())
            {
                //so, begin will be called
                fakeUow.Received(1).Begin(Arg.Any<UnitOfWorkOptions>());

                //trying to begin a uow (not starting a new one, using the outer)
                using (var uow2 = uowManager.Begin())
                {
                    //Since there is a current uow, begin is not called
                    fakeUow.Received(1).Begin(Arg.Any<UnitOfWorkOptions>());

                    uow2.Complete();

                    //complete has no effect since outer uow should complete it
                    fakeUow.DidNotReceive().Complete();
                }

                //trying to begin a uow (forcing to start a NEW one)
                using (var uow2 = uowManager.Begin(TransactionScopeOption.RequiresNew))
                {
                    //So, begin is called again to create an inner uow
                    fakeUow.Received(2).Begin(Arg.Any<UnitOfWorkOptions>());

                    uow2.Complete();

                    //And the inner uow should be completed
                    fakeUow.Received(1).Complete();
                }

                //complete the outer uow
                uow1.Complete();
            }

            fakeUow.Received(2).Complete();
            fakeUow.Received(2).Dispose();
        }
    }
}
