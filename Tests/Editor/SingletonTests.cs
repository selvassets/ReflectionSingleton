using NUnit.Framework;

namespace ReflectionSingleton.Tests
{
    public class SingletonTests
    {
        [Test]
        public void SingletonResolveExistingClass()
        {
            SingletonManager manager = new SingletonManager();
            TestResolveClassA testClass = new TestResolveClassA();

            manager.Bind(testClass);

            var testResolveInstance = manager.Resolve<TestResolveClassA>();

            Assert.IsNotNull(testResolveInstance);
        }

        [Test]
        public void SingletonResolveNonExistingClass()
        {
            SingletonManager manager = new SingletonManager();

            var testResolveInstance = manager.Resolve<TestResolveClassB>();

            Assert.IsNull(testResolveInstance);
        }
    }
}