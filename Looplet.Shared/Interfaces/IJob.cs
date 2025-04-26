using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Looplet.Shared.Interfaces;

public interface IJob
{
    Task ExecuteAsync(BsonDocument? parameters, CancellationToken cancellationToken);
}
