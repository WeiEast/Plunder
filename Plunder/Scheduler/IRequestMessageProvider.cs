﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plunder.Scheduler
{
    public interface IRequestMessageProvider
    {
        IEnumerable<RequestMessage> List(int number);
        IEnumerable<RequestMessage> PriorityList(int number);
    }
}
