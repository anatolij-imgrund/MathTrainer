using System;
using MathTrainer.Model.Enums;

namespace MathTrainer.Model
{
    public class RewardRequest
    {
        public Guid Id { get; set; }
        public Guid ChildId { get; set; }
        public Guid RewardId { get; set; }
        public DateTime RequestDate { get; set; }
        public RequestStatus Status { get; set; }
        public string ChildName { get; set; } = string.Empty;
        public string RewardName { get; set; } = string.Empty;

        public RewardRequest()
        {
            Id = Guid.NewGuid();
            RequestDate = DateTime.Now;
            Status = RequestStatus.Pendung;
        }
    }
}