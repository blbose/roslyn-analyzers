﻿// © 2019 Koninklijke Philips N.V. See License.md in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Philips.CodeAnalysis.Common;
using Philips.CodeAnalysis.MaintainabilityAnalyzers.Readability;

namespace Philips.CodeAnalysis.Test.Maintainability.Readability
{
	[TestClass]
	public class PreventUnnecessaryRangeChecksAnalyzerTest : CodeFixVerifier
	{
		#region Non-Public Data Members

		#endregion

		#region Non-Public Properties/Methods

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new PreventUnnecessaryRangeChecksAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new PreventUnnecessaryRangeChecksCodeFixProvider();
		}

		#endregion

		#region Public Interface

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckArrayRange(string declaration, string countLengthMethod)
		{
			const string template = @"
class Foo
{{
  public void test()
  {{
    {0};
    // comment
    if(data.{1} > 0)
    {{
      foreach (int i in data)
      {{
      }}
      //middle comment
    }}
    // end comment
  }}
}}
";

			const string fixedTemplate = @"
class Foo
{{
  public void test()
  {{
    {0};
    // comment
    foreach (int i in data)
    {{
    }}
    //middle comment
    // end comment
  }}
}}
";

			string errorCode = string.Format(template, declaration, countLengthMethod);

			VerifyCSharpDiagnostic(errorCode, DiagnosticResultHelper.Create(DiagnosticIds.PreventUncessaryRangeChecks));

			VerifyCSharpFix(errorCode, string.Format(fixedTemplate, declaration));
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckNestedRange(string declaration, string countLengthMethod)
		{
			const string template = @"
public class Container
{{
  public {0};
}}

class Foo
{{
  public void test()
  {{
    Container container = new Container();
    // comment
    if(container.data.{1} > 0)
    {{
      foreach (int i in container.data)
      {{
      }}
      //middle comment
    }}
    // end comment
  }}
}}
";

			const string fixedTemplate = @"
public class Container
{{
  public {0};
}}

class Foo
{{
  public void test()
  {{
    Container container = new Container();
    // comment
    foreach (int i in container.data)
    {{
    }}
    //middle comment
    // end comment
  }}
}}
";

			string errorCode = string.Format(template, declaration, countLengthMethod);

			VerifyCSharpDiagnostic(errorCode, DiagnosticResultHelper.Create(DiagnosticIds.PreventUncessaryRangeChecks));

			VerifyCSharpFix(errorCode, string.Format(fixedTemplate, declaration));
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckNestedRange2(string declaration, string countLengthMethod)
		{
			const string template = @"
public class Container
{{
  public {0};
}}

class Foo
{{
  public void test()
  {{
    Container container1 = new Container();
    Container container2 = new Container();
    // comment
    if(container1.data.{1} > 0)
    {{
      foreach (int i in container2.data)
      {{
      }}
      //middle comment
    }}
    // end comment
  }}
}}
";
			string errorCode = string.Format(template, declaration, countLengthMethod);

			VerifyCSharpDiagnostic(errorCode);
		}

		[TestMethod]
		public void CheckNestedRange2a()
		{
			const string template = @"
public class Container
{{
  public List<int> Data {{ get; set; }}
}}

class Foo
{{
  public void test()
  {{
    Container container1 = new Container();
    Container container2 = new Container();

    List<string> other = new List<string>();
    // comment
    if(other.Count > 0)
    {{
      foreach (int i in container2.data)
      {{
      }}
      //middle comment
    }}
    // end comment
  }}
}}
";
			string errorCode = string.Format(template);

			VerifyCSharpDiagnostic(errorCode);
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckNestedRange3(string declaration, string countLengthMethod)
		{
			const string template = @"
public class OuterContainer
{{
  public Container container;
}}

public class Container
{{
  public {0};
}}

class Foo
{{
  public void test()
  {{
    OuterContainer container1 = new OuterContainer();
    OuterContainer container2 = new OuterContainer();
    // comment
    if(container1.container.data.{1} > 0)
    {{
      foreach (int i in container2.container.data)
      {{
      }}
      //middle comment
    }}
    // end comment
  }}
}}
";
			string errorCode = string.Format(template, declaration, countLengthMethod);

			VerifyCSharpDiagnostic(errorCode);
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckArrayRangeBraces(string declaration, string countLengthMethod)
		{
			const string template = @"
class Foo
{{
  public void test()
  {{
    {0};
    // comment
    if(data.{1} > 0)
      foreach (int i in data)
      {{
      }}
    // end comment
  }}
}}
";

			const string fixedTemplate = @"
class Foo
{{
  public void test()
  {{
    {0};
    // comment
    foreach (int i in data)
    {{
    }}
    // end comment
  }}
}}
";

			string errorCode = string.Format(template, declaration, countLengthMethod);

			VerifyCSharpDiagnostic(errorCode, DiagnosticResultHelper.Create(DiagnosticIds.PreventUncessaryRangeChecks));

			VerifyCSharpFix(errorCode, string.Format(fixedTemplate, declaration));
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckArrayRangeNoBraces(string declaration, string countLengthMethod)
		{
			const string template = @"
class Foo
{{
  public void test()
  {{
    {0};
    // comment
    if(data.{1} > 0)
      foreach (int i in data)
        i.ToString();
    // end comment
  }}
}}
";

			const string fixedTemplate = @"
class Foo
{{
  public void test()
  {{
    {0};
    // comment
    foreach (int i in data)
      i.ToString();
    // end comment
  }}
}}
";

			string errorCode = string.Format(template, declaration, countLengthMethod);

			VerifyCSharpDiagnostic(errorCode, DiagnosticResultHelper.Create(DiagnosticIds.PreventUncessaryRangeChecks));

			VerifyCSharpFix(errorCode, string.Format(fixedTemplate, declaration));
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckArrayRange2(string declaration, string countLengthMethod)
		{
			const string template = @"
using System.Linq;
using System;
using System.Collections.Generic;

class Foo {{
public void test()
{{
	{1};
	if(data.{1} > 0)
	{{
		foreach(int i in data)
		{{
		}}

		Console.WriteLine(data.{1}.ToString());
	}}
}}
}}
";

			VerifyCSharpDiagnostic(string.Format(template, declaration, countLengthMethod));
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckArrayRange3(string declaration, string countLengthMethod)
		{
			const string template = @"
using System.Linq;
using System;
using System.Collections.Generic;

class Foo {{
public void test()
{{
	{1};
	if(data.{1} != 0)
	{{
		foreach(int i in data)
		{{
		}}
	}}
}}
}}
";

			VerifyCSharpDiagnostic(string.Format(template, declaration, countLengthMethod), DiagnosticResultHelper.Create(DiagnosticIds.PreventUncessaryRangeChecks));
		}

		[DataRow("int[] data = new int[0]", "Length")]
		[DataRow("int[] data = new int[0]", "Count()")]
		[DataRow("List<int> data = new List<int>()", "Count")]
		[DataRow("List<int> data = new List<int>()", "Count()")]
		[DataTestMethod]
		public void CheckArrayRange4(string declaration, string countLengthMethod)
		{
			const string template = @"
using System.Linq;
using System;
using System.Collections.Generic;

class Foo {{
public void test()
{{
	{1};
	if(data.{1} != 0)
	{{
		foreach(int i in data)
		{{
		}}

		Console.WriteLine(data.{1}.ToString());
	}}
}}
}}
";

			VerifyCSharpDiagnostic(string.Format(template, declaration, countLengthMethod));
		}

		#endregion
	}
}
