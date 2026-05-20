## Migration verification evidence

Closes part of CxODEV-1475.

### Local verification (run before opening this PR)

- [ ] Build green on `dotnet build --configuration Release`
- [ ] Migration tests green (paste tail of `dotnet test ... --filter "FullyQualifiedName~Differential|SqlSnapshot|EnumMapping"`)
- [ ] Full test suite green (paste tail of `dotnet test ${SOLUTION}`)
- [ ] CTK Newman suite green via `bash .tests/compliance/run.local.sh` (if applicable)
- [ ] Coverage report HTML attached to this PR (drag-drop `TestResults/CoverageReport/index.html` or paste `SummaryGithub.md`)
- [ ] Coverage % maintained or improved vs. `main` (paste `SummaryGithub.md` table)
- [ ] AutoMapper removed: `grep -rE 'AutoMapper' src/ --include "*.cs" --include "*.csproj"` returns no matches

### Special cases handled (if any)
<!-- List any patterns from Section 6 encountered and how each was resolved -->

### Profiles migrated
<!-- Bulleted list of every Profile class that existed and the corresponding new Expression-first mapper -->

### Call-site updates
<!-- Approximate count of _mapper.Map<>, .ProjectTo<>, .MapToResult<> replacements -->

---

## Reviewer checklist (senior, required before merge to `main`)

- [ ] All verification-evidence checkboxes in the PR description are checked
- [ ] Coverage HTML in PR shows coverage maintained or improved
- [ ] `grep` output in PR description shows zero AutoMapper matches
- [ ] All three mapper-pattern artefacts present per migrated aggregate: `<Aggregate>Expressions.cs`, `<Aggregate>Mapper.cs`, `<Aggregate>MappingExtensions.cs`, plus DI registration in `ServiceCollectionExtensions.cs`
- [ ] Special-case sections (if claimed) match the resolution patterns in CxODEV-1475
- [ ] CTK Newman log shows no failed scenarios (TMF services only)
- [ ] No new external NuGet dependencies (no Mapster, no Riok.Mapperly, no AutoMapper replacement library; the recommendation is hand-written mappers only)
- [ ] Sample-check at least 3 call-sites manually to confirm the pattern is applied consistently