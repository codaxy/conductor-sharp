﻿using ConductorSharp.Toolkit.Models;
using ConductorSharp.Toolkit.Service;

namespace ConductorSharp.Toolkit.Commands
{
    public class ScaffoldCommand : IWFECommand
    {
        private readonly IScaffoldingService scaffoldingService;
        public ScaffoldCommand(IScaffoldingService scaffoldingService)
        {
            this.scaffoldingService = scaffoldingService;
        }

        public string GetName() => "scaffold";
        public async Task Execute(WFECommandInput input)
        {
            await scaffoldingService.Scaffold();
        }
    }
}
