using System;
using System.Collections.Generic;
using Antrea.Windows.ProfileProvider;
using Newtonsoft.Json.Schema;

namespace SampleConnector
{
    public class AuditError: IAuditError
    {
        public AuditError(ValidationError e, IAudience audience)
        {
            Message = e.Message;
            Value = e.Value;
            ErrorType = e.ErrorType;
            Path = e.Path;
            SchemaId = e.SchemaId;
            LineNumber = e.LineNumber;
            LinePosition = e.LinePosition;

            this.Audience = new List<IAudience>() { audience };
        }

        public string Message { get; set; }

        public ErrorType ErrorType { get; set; }

        public object Value { get; set; }

        public IList<IAudience> Audience { get; set; }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }

        public string Path { get; set; }

        public Uri SchemaId { get; set; }

}

}
