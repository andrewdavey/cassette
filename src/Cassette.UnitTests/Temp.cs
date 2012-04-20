using System.Linq;
using Should;
using Xunit;

namespace Cassette
{
    public class Temp
    {
        [Fact]
        public void __()
        {
            var c = new TinyIoC.TinyIoCContainer();
            c.Register<ITest>(new A());
            c.RegisterMultiple<ITest>(new[]{typeof(B)});

            c.ResolveAll<ITest>().Count().ShouldEqual(2);
        }

        interface ITest
        {
             
        }
        class A:ITest
        {
            
        }
        class B:ITest
        {
            
        }
    }
}