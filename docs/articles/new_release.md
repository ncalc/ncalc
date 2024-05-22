# Publishing a new release

The project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) tool to manage versions.  
Each library build can be traced back to the original git commit. The installation is simple:

>dotnet tool install --global nbgv

## Preparing and publishing a new release

1. Make sure that `nbgv` dotnet CLI tool is installed and is up to date
2. Run `nbgv prepare-release` to create a stable branch for the upcoming release, i.e. release/v1.0
3. Switch to the release branch: `git checkout release/v1.0`
4. Execute the unit tests, update the README, release notes in csproj file, etc. Commit and push your changes.
5. Run `dotnet pack -c Release` and verify that it builds Nuget packages with the right version number.
6. Run `nbgv tag release/v1.0` to tag the last commit on the release branch with your current version number, i.e. v1.0.7.
7. Push tags as suggested by nbgv tool: `git push origin v1.0.7`
8. Go to github project page and create a release out of the last tag v1.0.7.
9. Verify that [github workflow for publishing the nuget package](https://github.com/ncalc/ncalc/actions/workflows/publish-nuget.yml) has completed.
10. Switch back to master and merge the release branch.

## Nuget package token

* Github actions publish all tagged releases as nuget packages automatically.
* Nuget API token is required for publishing new package versions.
* The token expires every year and should be regenerated upon expiration.
* Please contact Nuget package owners for regenerating the package token.
* API key: [Settings → Action secrets and variables](https://github.com/ncalc/ncalc/settings/secrets/actions) → `NUGET_NCALC_SYNC_API_TOKEN` secret.