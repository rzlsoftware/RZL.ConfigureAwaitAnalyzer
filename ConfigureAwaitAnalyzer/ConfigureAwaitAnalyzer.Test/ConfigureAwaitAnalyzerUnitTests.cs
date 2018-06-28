using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConfigureAwaitAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void When_ConfigureAwait_false_is_provided_validation_should_pass()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Domain.Services
{
    public class Service
    {
        public async Task DoAsyncStuff()
            => await Task.Delay(30).ConfigureAwait(false);

        public async Task DoSomeOtherAsyncStuff()
        {
            await Task.Delay(30).ConfigureAwait(false);
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void When_ConfigureAwait_false_is_missing_validation_should_return_an_error()
        {
            var test = @"
using System.Threading.Tasks;

namespace Domain.Services
{
    public class Service
    {
        public async Task DoAsyncStuff()
            => await Task.Delay(30);

        public async Task DoSomeOtherAsyncStuff()
        {
            await Task.Delay(30);
        }
    }
}";
            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "ConfigureAwaitAnalyzer",
                    Message = "Missing ConfigureAwait(false)",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", line: 9, column: 16) }
                },
                new DiagnosticResult
                {
                    Id = "ConfigureAwaitAnalyzer",
                    Message = "Missing ConfigureAwait(false)",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", line: 13, column: 13) }
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System.Threading.Tasks;

namespace Domain.Services
{
    public class Service
    {
        public async Task DoAsyncStuff()
            => await Task.Delay(30).ConfigureAwait(false);

        public async Task DoSomeOtherAsyncStuff()
        {
            await Task.Delay(30).ConfigureAwait(false);
        }
    }
}";

            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void When_ConfigureAwait_true_is_provided_validation_should_return_an_error()
        {
            var test = @"
using System.Threading.Tasks;

namespace Domain.Services
{
    public class Service
    {
        public async Task DoAsyncStuff()
            => await Task.Delay(30).ConfigureAwait(true);

        public async Task DoSomeOtherAsyncStuff()
        {
            await Task.Delay(30).ConfigureAwait(true);
        }
    }
}";
            var expected = new[]
            {
                new DiagnosticResult
                {
                    Id = "ConfigureAwaitAnalyzer",
                    Message = "ConfigureAwait must be set to false",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", line: 9, column: 16) }
                },
                new DiagnosticResult
                {
                    Id = "ConfigureAwaitAnalyzer",
                    Message = "ConfigureAwait must be set to false",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", line: 13, column: 13) }
                }
            };

            VerifyCSharpDiagnostic(test, expected);

//            var fixtest = @"
//using System.Threading.Tasks;

//namespace Domain.Services
//{
//    public class Service
//    {
//        public async Task DoAsyncStuff()
//            => await Task.Delay(30).ConfigureAwait(false);

//        public async Task DoSomeOtherAsyncStuff()
//        {
//            await Task.Delay(30).ConfigureAwait(false);
//        }
//    }
//}";

//            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
            => new ConfigureAwaitAnalyzerCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ConfigureAwaitAnalyzerAnalyzer();
    }
}
