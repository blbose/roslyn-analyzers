﻿// © 2019 Koninklijke Philips N.V. See License.md in the project root for license information........---

using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Philips.CodeAnalysis.Common;

namespace Philips.CodeAnalysis.MaintainabilityAnalyzers.Documentation
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CopyrightPresentAnalyzer : DiagnosticAnalyzer
	{
		private const string Title = @"Copyright Present";
		private const string MessageFormat = 
			@"File should start with a copyright statement, containing the company name, the year and either © or 'Copyright'.";
		private const string Description =
			@"File should start with a comment containing the company name, the year and either © or 'Copyright'.";
		private const string Category = Categories.Documentation;

		private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(Helper.ToDiagnosticId(DiagnosticIds.CopyrightPresent), Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);
		private static Regex yearRegex = new Regex(@"\d\d\d\d");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context)
		{
			CompilationUnitSyntax node = (CompilationUnitSyntax)context.Node;

			if (Helper.IsGeneratedCode(context) || Helper.IsAssemblyInfo(context) || Helper.HasAutoGeneratedComment(node))
			{
				return;
			}

			if (node.FindToken(0).IsKind(SyntaxKind.EndOfFileToken))
			{
				return;
			}

			var first = node.GetLeadingTrivia();

			if (!first.Any())
			{
				CreateDiagnostic(context, node.GetLocation());
				return;
			}

			SyntaxTrivia copyrightSyntax = first[0];

			if (first[0].IsKind(SyntaxKind.RegionDirectiveTrivia))
			{
				bool regionHeaderHasCopyright = CheckCopyrightStatement(context, first[0]);
				if (!regionHeaderHasCopyright && first.Count >= 2 && first[1].IsKind(SyntaxKind.SingleLineCommentTrivia))
				{
					copyrightSyntax = first[1];
				}
			}

			bool isCorrectStatement = CheckCopyrightStatement(context, copyrightSyntax);
			if (!isCorrectStatement)
			{
				CreateDiagnostic(context, copyrightSyntax.GetLocation());
				return;
			}

		}

		private static void CreateDiagnostic(SyntaxNodeAnalysisContext context, Location location)
		{
			Diagnostic diagnostic = Diagnostic.Create(Rule, location);
			context.ReportDiagnostic(diagnostic);
		}

		private static bool CheckCopyrightStatement(SyntaxNodeAnalysisContext context, SyntaxTrivia trivia) {
			var comment = trivia.ToFullString();
			// Check the copyright mar itself
			var hasCopyright = comment.Contains("©") || comment.Contains("Copyright");
			
			// Check the year
			bool hasYear = yearRegex.IsMatch(comment);

			// Check the company name, only if it is configured.
			var additionalFilesHelper = new AdditionalFilesHelper(context.Options, context.Compilation);
			var companyName = additionalFilesHelper.GetValueFromEditorConfig(Rule.Id, @"company_name");
			var hasCompanyName = string.IsNullOrEmpty(companyName) || comment.Contains(companyName);

			return hasCopyright && hasYear && hasCompanyName;
		}
	}
}
