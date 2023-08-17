{
  description = "A simple flake for DougaAPI";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system};
      in {
        devShell = pkgs.mkShell {
          buildInputs = with pkgs; [ dotnet-sdk_7 openssl.dev ];
          DOTNET_CLI_TELEMETRY_OPTOUT = true;
          DOTNET_SKIP_FIRST_TIME_EXPERIENCE = true;
        };

        packages = rec {
          DougaAPI = pkgs.stdenv.mkDerivation {
            name = "DougaAPI";
            buildInputs = [ pkgs.dotnet-sdk_7 ];
            sourceRoot = ".";
            configurePhase = "export HOME=$TMPDIR";
            buildPhase = ''
              dotnet publish ${self.outPath}/DougaAPI/DougaAPI.csproj -c Release -o $out/
            '';
            installPhase = "mkdir -p $out/bin; cp -r * $out/bin";
          };
        };
      });
}
