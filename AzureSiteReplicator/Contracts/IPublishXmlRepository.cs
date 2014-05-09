﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AzureSiteReplicator.Data;

namespace AzureSiteReplicator.Contracts
{
    public interface IPublishXmlRepository
    {
        IReadOnlyCollection<Site> Sites { get; }
    }
}