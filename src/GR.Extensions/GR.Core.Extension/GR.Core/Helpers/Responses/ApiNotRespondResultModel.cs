﻿using System.Collections.Generic;

namespace GR.Core.Helpers.Responses
{

    public class ApiNotRespondResultModel<T> : ResultModel<T>
    {
        public override ICollection<IErrorModel> Errors { get; set; } = new List<IErrorModel>
        {
            new ErrorModel(string.Empty, "The called API does not respond correctly or has been incorrectly called.")
        };
    }

    public class ApiNotRespondResultModel : ApiNotRespondResultModel<object>
    {

    }
}
