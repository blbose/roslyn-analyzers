﻿// © 2021 Koninklijke Philips N.V. See License.md in the project root for license information.


using System;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.CodeAnalysis.Common;
using Philips.CodeAnalysis.MaintainabilityAnalyzers.Readability;

namespace Philips.CodeAnalysis.Test.Maintainability.Readability
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class PreferTupleFieldNamesAnalyzerTest : DiagnosticVerifier
	{
		#region Non-Public Data Members

		#endregion

		#region Non-Public Properties/Methods

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new PreferTupleFieldNamesAnalyzer();
		}

		private string CreateFunction(string argument)
		{
			string baseline = @"
class Foo 
{{
  public void Foo((string, int num) data)
  {{
    _ = {0};
  }}
}}
";

			return string.Format(baseline, argument);
		}

		#endregion

		#region Public Interface

		[DataRow("data.Item1", false)]
		[DataRow("data.Item2", true)]
		[DataRow("data.num", false)]
		[DataTestMethod]
		public void NamedTuplesDontCauseErrors(string argument, bool isError)
		{
			DiagnosticResult[] results = Array.Empty<DiagnosticResult>();

			if (isError)
			{
				results = DiagnosticResultHelper.CreateArray(DiagnosticIds.PreferUsingNamedTupleField);
			}

			VerifyCSharpDiagnostic(CreateFunction(argument), results);
		}

		#endregion
	}
}
