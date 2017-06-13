using StudioX.Dependency;
using Shouldly;
using Xunit;

namespace StudioX.Tests.Dependency
{
    public class CircularPropertyDependencyTests : TestBaseWithLocalIocManager
    {
        [Fact]
        public void ShouldSuccessCircularPropertyInjectionTransient()
        {
            InitializeTest(DependencyLifeStyle.Transient);

            var obj1 = LocalIocManager.Resolve<MyClass1>();
            obj1.Obj2.ShouldNotBe(null);
            obj1.Obj3.ShouldNotBe(null);
            obj1.Obj2.Obj3.ShouldNotBe(null);

            var obj2 = LocalIocManager.Resolve<MyClass2>();
            obj2.Obj1.ShouldNotBe(null);
            obj2.Obj3.ShouldNotBe(null);
            obj2.Obj1.Obj3.ShouldNotBe(null);

            MyClass1.CreateCount.ShouldBe(2);
            MyClass2.CreateCount.ShouldBe(2);
            MyClass3.CreateCount.ShouldBe(4);
        }

        [Fact]
        public void ShouldSuccessCircularPropertyInjectionSingleton()
        {
            InitializeTest(DependencyLifeStyle.Singleton);

            var obj1 = LocalIocManager.Resolve<MyClass1>();
            obj1.Obj2.ShouldNotBe(null);
            obj1.Obj3.ShouldNotBe(null);
            obj1.Obj2.Obj3.ShouldNotBe(null);

            var obj2 = LocalIocManager.Resolve<MyClass2>();
            obj2.Obj1.ShouldBe(null); //!!!Notice: It's null
            obj2.Obj3.ShouldNotBe(null);

            MyClass1.CreateCount.ShouldBe(1);
            MyClass2.CreateCount.ShouldBe(1);
            MyClass3.CreateCount.ShouldBe(1);
        }

        private void InitializeTest(DependencyLifeStyle lifeStyle)
        {
            MyClass1.CreateCount = 0;
            MyClass2.CreateCount = 0;
            MyClass3.CreateCount = 0;

            LocalIocManager.Register<MyClass1>(lifeStyle);
            LocalIocManager.Register<MyClass2>(lifeStyle);
            LocalIocManager.Register<MyClass3>(lifeStyle);
        }

        public class MyClass1
        {
            public static int CreateCount { get; set; }

            public MyClass2 Obj2 { get; set; }

            public MyClass3 Obj3 { get; set; }

            public MyClass1()
            {
                CreateCount++;
            }
        }

        public class MyClass2
        {
            public static int CreateCount { get; set; }

            public MyClass1 Obj1 { get; set; }

            public MyClass3 Obj3 { get; set; }

            public MyClass2()
            {
                CreateCount++;
            }
        }

        public class MyClass3
        {
            public static int CreateCount { get; set; }

            public MyClass3()
            {
                CreateCount++;
            }
        }
    }
}