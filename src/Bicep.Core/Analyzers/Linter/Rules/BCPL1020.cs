// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Core.Analyzers.Interfaces;
using Bicep.Core.Configuration;
using Bicep.Core.Parsing;
using Bicep.Core.Semantics;
using Bicep.Core.Syntax;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bicep.Core.Analyzers.Linter.Rules
{
    internal class BCPL1020 : LinterRule
    {
        private readonly HashSet<string> DisallowedHosts;

        internal BCPL1020() : base(
            code: "BCPL1020",
            ruleName: "Environment() URL hardcoded",
            description: "Environment() URLs should not be hardcoded",
            diagnosticLevel: Diagnostics.DiagnosticLevel.Error,
            docUri: "https://bicep/linter/rules/BCPL1020")// TODO: setup up doc pages
        {
            // create a hashset lookup for hosts
            this.DisallowedHosts = GetConfiguration(nameof(this.DisallowedHosts), Array.Empty<string>())
                                    .Select( s => s.ToUpper())
                                    .ToHashSet();
        }

        protected override string GetFormattedMessage(params object[] values)
            => string.Format("{0} -- Found: [{1}]", this.Description, values.First());

        public override IEnumerable<IBicepAnalyzerDiagnostic> Analyze(SemanticModel model)
        {
            var spansToMark = new Dictionary<TextSpan, List<string>>();
            var visitor = new BCPL1020Visitor(spansToMark, this.DisallowedHosts);
            visitor.Visit(model.SyntaxTree.ProgramSyntax);

            foreach(var kvp in spansToMark)
            {
                var span = kvp.Key;
                foreach(var hosts in kvp.Value)
                {
                    yield return CreateDiagnosticForSpan(span, hosts);
                }
            }
        }

        private class BCPL1020Visitor : SyntaxVisitor
        {
            private readonly Dictionary<TextSpan, List<string>> hostsFound;
            private readonly HashSet<string> disallowedHosts;

            public BCPL1020Visitor(Dictionary<TextSpan, List<string>> hostsFound, HashSet<string> disallowedHosts)
            {
                this.hostsFound = hostsFound;
                this.disallowedHosts = disallowedHosts;
            }

            public override void VisitStringSyntax(StringSyntax syntax)
            {
                var disallowed = syntax.SegmentValues.Where(s => this.disallowedHosts.Contains(s.ToUpper()));
                if (disallowed.Any())
                {
                    this.hostsFound[syntax.Span] = disallowed.ToList();
                }
                base.VisitStringSyntax(syntax);
            }
        }

    }
}
