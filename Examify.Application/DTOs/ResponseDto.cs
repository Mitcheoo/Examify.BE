using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examify.Application.DTOs
{
    public class ResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public object? Data { get; set; }

        public IEnumerable<string>? Errors { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
