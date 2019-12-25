using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryPropSwitch.Tests
{
    static class TestData
    {
        public static readonly string[] EnableData = new[] {
                "<Project>",
                "  <PropertyGroup>",
                "    <RepoRoot>$([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)'))</RepoRoot>",
                "    <PathMap>$(RepoRoot)=.</PathMap>",
                "  </PropertyGroup>",
                "</Project>",
            };
        public static readonly string[] DisableData = new[] {
                "<Project>",
                "  <PropertyGroup>",
                "    <RepoRoot>$([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)'))</RepoRoot>",
                "    <!-- <PathMap>$(RepoRoot)=.</PathMap> -->",
                "  </PropertyGroup>",
                "</Project>",
            };

    }
}
