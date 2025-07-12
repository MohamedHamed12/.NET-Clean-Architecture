{
  description = "Development environment";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-parts.url = "github:hercules-ci/flake-parts";
  };

  outputs = { self, nixpkgs, flake-parts, ... }:
    flake-parts.lib.mkOutput {
      systems = [ "aarch64-darwin" "aarch64-linux" "x86_64-darwin" "x86_64-linux" ];
      perSystem = { config, pkgs, lib, system, ... }: {
        devShells.default = import ./dev.nix { pkgs = nixpkgs.legacyPackages.${nixpkgs.system}; };
      };
    };
}