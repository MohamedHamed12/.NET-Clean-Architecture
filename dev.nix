{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  packages = with pkgs; [
    docker
    dotnet-sdk_9
  ];
  services.docker.enable = true;
}
nix
{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  packages = with pkgs; [
    docker
    dotnet-sdk_9
  ];
}