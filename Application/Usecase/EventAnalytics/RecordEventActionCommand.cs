using MediatR;
using System;

namespace Application.Usecase.EventAnalytics
{
    public class RecordEventActionCommand : IRequest<bool>
    {
        public Guid EventId { get; set; }
        public string ActionType { get; set; } = string.Empty; // "share" or "timelapse"
    }
}
