using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WillSoss.Data.Tests
{
    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture Fixture;

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            Fixture = fixture;
        }
    }
}
