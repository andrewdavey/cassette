using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Should;
using Xunit;


namespace Cassette.Configuration
{
    public class DiskBackedBundleCache_Tests
    {
        Mock<IFileHelper> moqDiskBacker;
        Mock<Bundle> moqBundle;
        public DiskBackedBundleCache_Tests()
        {
            moqDiskBacker = new Mock<IFileHelper>();
            //diskBacker.Setup()
        }
    }
}
