using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using ContactForm;
using ContactForm.Models;

using Xunit;

namespace ContactForm.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestOKReturned()
        {
            var function = new Function();
            var context = new TestLambdaContext();

            var contactRequest = new ContactRequest()
            {
                Name = "James Smith",
                Email = "hello@example.com",
                Phone = "01234567",
                Website = "https://www.example.com",
                Body = "This is a message"
            };

            var upperCase = function.FunctionHandler(contactRequest, context);

            Assert.Equal("OK", upperCase);
        }
    }
}