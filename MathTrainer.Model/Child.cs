using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace MathTrainer.Model
{
    public class Child
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AvatarImagePath { get; set; }
        public string Backgroundcolor { get; set; } = "#FFFFFFFF";
        public int TotalScore { get; set; } = 0;
        public int AnswersInRowForMedal { get; set; } = 0;
        public int Medals { get; set; } = 0;

        public Child()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            AvatarImagePath = string.Empty;
            Backgroundcolor = "#FFFFFFFF";
        }
    }
}
