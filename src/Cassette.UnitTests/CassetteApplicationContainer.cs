using System;
using System.IO;
using System.Threading;
using Cassette.Configuration;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class CassetteApplicationContainer_Tests : IDisposable
    {
        public CassetteApplicationContainer_Tests()
        {
            rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(rootPath);
            applicationInstances = new[] {CreateApplication(), CreateApplication()};
        }

        [Fact]
        public void WhenCreateContainer_ThenApplicationPropertyReturnsApplicationCreatedByFactoryMethod()
        {
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                container.Application.ShouldBeSameAs(applicationInstances[0].Object);
            }
        }

        [Fact]
        public void WhenFileCreated_TheApplicationFactoryCalledAgain()
        {
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                var first = container.Application;
                File.WriteAllText(Path.Combine(rootPath, "test.txt"), "");
                Thread.Sleep(500); // File system events are asynchronously fired on different thread. Hack a brief pause to catch the event!
                var getItAgain = container.Application;
            }
            factoryCallCount.ShouldEqual(2);
        }

        [Fact]
        public void WhenFileCreated_ApplicationPropertyReturnsSecondApplication()
        {
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                var first = container.Application;
                File.WriteAllText(Path.Combine(rootPath, "test.txt"), "");
                Thread.Sleep(500); // File system events are asynchronously fired on different thread. Hack a brief pause to catch the event!
                container.Application.ShouldBeSameAs(applicationInstances[1].Object);
            }
        }

        [Fact]
        public void WhenFileDeleted_TheApplicationFactoryCalledAgain()
        {
            var filename = Path.Combine(rootPath, "test.txt");
            File.WriteAllText(filename, "");
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                var first = container.Application;
                File.Delete(filename);
                Thread.Sleep(500); // File system events are asynchronously fired on different thread. Hack a brief pause to catch the event!
                var getItAgain = container.Application;
            }
            factoryCallCount.ShouldEqual(2);
        }

        [Fact]
        public void WhenFileChanged_TheApplicationFactoryCalledAgain()
        {
            var filename = Path.Combine(rootPath, "test.txt");
            File.WriteAllText(filename, "");
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                var first = container.Application;
                File.WriteAllText(filename, "changed");
                Thread.Sleep(500); // File system events are asynchronously fired on different thread. Hack a brief pause to catch the event!
                var getItAgain = container.Application;
            }
            factoryCallCount.ShouldEqual(2);
        }

        [Fact]
        public void WhenFileRenamed_TheApplicationFactoryCalledAgain()
        {
            var filename = Path.Combine(rootPath, "test.txt");
            File.WriteAllText(filename, "");
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                var first = container.Application;
                File.Move(filename, filename + ".new");
                Thread.Sleep(500); // File system events are asynchronously fired on different thread. Hack a brief pause to catch the event!
                var getItAgain = container.Application;
            }
            factoryCallCount.ShouldEqual(2);
        }

        [Fact]
        public void WhenCreatingANewApplication_ThenOldApplicationIsDisposed()
        {
            var filename = Path.Combine(rootPath, "test.txt");
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                var first = container.Application;
                File.WriteAllText(filename, "");
                Thread.Sleep(500);
                    // File system events are asynchronously fired on different thread. Hack a brief pause to catch the event!
                var getItAgain = container.Application;
            }
            applicationInstances[0].Verify(a => a.Dispose());
        }

        [Fact]
        public void WhenDispose_ThenCurrentApplicationInstanceDisposed()
        {
            using (var container = new CassetteApplicationContainer(StubApplicationFactory, rootPath))
            {
                var first = container.Application;                
            }
            applicationInstances[0].Verify(a => a.Dispose());
        }

        [Fact]
        public void GivenFirstCreateCallsThrowsException_WhenFileSystemUpdated_ThenCreateIsCalledNextTimeApplicationRequested()
        {
            Func<ICassetteApplication> create;
            var exception = new Exception();
            var createCalledSecondTime = false;
            var successfulCreate = new Func<ICassetteApplication>(() =>
            {
                createCalledSecondTime = true;
                return Mock.Of<ICassetteApplication>();
            });
            var failingCreate = new Func<ICassetteApplication>(() =>
            {
                create = successfulCreate;
                throw exception;
            });
            create = failingCreate;

            var container = new CassetteApplicationContainer(() => create(), rootPath);
            var actualException = Assert.Throws<Exception>(delegate
            {
                var app1 = container.Application;
            });
            actualException.ShouldBeSameAs(exception);

            // Touch the file system.
            File.WriteAllText(Path.Combine(rootPath, "test.txt"), "");
            Thread.Sleep(500);

            var app2 = container.Application;
            createCalledSecondTime.ShouldBeTrue();
        }

        readonly Mock<ICassetteApplication>[] applicationInstances;
        readonly string rootPath;
        int factoryCallCount;

        Mock<ICassetteApplication> CreateApplication()
        {
            var app = new Mock<ICassetteApplication>();
            var settings = new CassetteSettings("") { SourceDirectory = Mock.Of<IDirectory>() };
            app.SetupGet(a => a.Settings).Returns(settings);
            return app;
        }

        ICassetteApplication StubApplicationFactory()
        {
            return applicationInstances[factoryCallCount++].Object;
        }

        public void Dispose()
        {
            Directory.Delete(rootPath, true);
        }
    }
}

