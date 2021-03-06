﻿using Plunder.Compoment;
using System.Threading.Tasks;

namespace Plunder.Pipeline
{
    public interface IResultPipelineModule
    {
        string ModuleName { get; }

        string ModuleDescription { get; }

        void Init(object context);

        Task ProcessAsync(PageResult result);

    }
}
